using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
namespace MDV;

public partial class MainWindow
{
    // Opt-in integration harness. Uses isolated MDV_DATA_DIR; never runs in normal use.
    async Task RunUiChecks() {
        if (Environment.GetCommandLineArgs().Contains("--menu-test")) { await RunMenuChecks(); return; }
        await RefreshOutline();
        static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
        static IEnumerable<T> Children<T>(DependencyObject parent) where T : DependencyObject {
            for (var i=0; i<VisualTreeHelper.GetChildrenCount(parent); i++) {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typed) yield return typed;
                foreach (var nested in Children<T>(child)) yield return nested;
            }
        }
        void Capture(FrameworkElement element, string name) {
            element.UpdateLayout();
            var bitmap = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(element);
            var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(Path.Combine(Preferences.Home, name)); encoder.Save(stream);
        }
        Check(Status.Text == DropHint, "Footer hint");
        Check(!Children<Button>(this).Any(), "Top toolbar was not removed");
        Check(OutlineList.Items.Count >= 3, "Outline extraction");
        Check(Title == "SattoMDV — さっと表示する Markdown Viewer", "New product title");
        Check(!CreateMenu().Items.OfType<MenuItem>().Single(m => m.Header.ToString() == "このファイルの場所を開く").IsEnabled, "Folder action disabled without file");

        // Dispatch a real browser input event, not a DOM contextmenu event.
        var contextRequested = new TaskCompletionSource();
        void ContextRequested(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2ContextMenuRequestedEventArgs e) => contextRequested.TrySetResult();
        Browser.CoreWebView2.ContextMenuRequested += ContextRequested;
        foreach (var type in new[] { "mousePressed", "mouseReleased" })
            await Browser.CoreWebView2.CallDevToolsProtocolMethodAsync("Input.dispatchMouseEvent", JsonSerializer.Serialize(new { type, x = 200, y = 100, button = "right", clickCount = 1 }));
        await contextRequested.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Browser.CoreWebView2.ContextMenuRequested -= ContextRequested;
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        Check(activeMenu?.IsOpen == true, "Browser right-click menu");
        var show = activeMenu!.Items.OfType<MenuItem>().Single(m => m.Header.ToString()!.StartsWith("アウトライン"));
        activeMenu.IsOpen = false;
        if (!prefs.ShowOutline) show.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Check(OutlinePanel.Visibility == Visibility.Visible && Preferences.Load().ShowOutline, "Outline show/persistence");
        OutlineList.SelectedIndex = OutlineList.Items.Count - 1;
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        var scroll = await Browser.CoreWebView2.ExecuteScriptAsync("scrollY");
        Check(double.Parse(scroll, System.Globalization.CultureInfo.InvariantCulture) > 0, "Outline heading jump");
        Capture(this, "outline-window.png");
        var hide = CreateMenu().Items.OfType<MenuItem>().Single(m => m.Header.ToString() == "アウトラインを非表示");
        hide.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Check(OutlinePanel.Visibility == Visibility.Collapsed && !Preferences.Load().ShowOutline, "Outline hide/persistence");

        var settings = new SettingsWindow(prefs) { Owner = this };
        Exception? settingsError = null;
        settings.Loaded += (_, _) => Dispatcher.BeginInvoke(() => {
            try {
                settings.UpdateLayout();
                var grid = Children<DataGrid>(settings).Single();
                Check(Children<ComboBox>(grid).Any(c => c.Items.Count > 30), "Installed font dropdown");
                var allFontNames = FontCatalog.GetNames();
                foreach (var family in Fonts.SystemFontFamilies)
                    Check(family.FamilyNames.Values.All(allFontNames.Contains), "Missing localized font names");
                File.WriteAllText(Path.Combine(Preferences.Home, "font-count.json"), JsonSerializer.Serialize(new { systemFamilies = Fonts.SystemFontFamilies.Count, selectableNames = allFontNames.Length - 1 }));
                var bulkSize = Children<ComboBox>(settings).First(c => c.Items.Contains("18px"));
                bulkSize.SelectedItem = "20px";
                var bulkFont = Children<ComboBox>(settings).First(c => c.Items.Count > 30);
                bulkFont.SelectedItem = allFontNames.Contains("游ゴシック") ? "游ゴシック" : "Arial";
                var marginInputs = Children<TextBox>(settings).Where(t => t.ToolTip?.ToString() == "0～500 px（上側だけ）").ToArray();
                marginInputs[0].Text = "72"; marginInputs[1].Text = "32";
                var size = Children<ComboBox>(grid).First(c => c.Items.Contains("18px"));
                size.SelectedItem = "22px";
                var field = Children<ColorField>(grid).First();
                Dispatcher.BeginInvoke(() => {
                    var picker = Application.Current.Windows.OfType<ColorPicker>().Single();
                    Capture(picker, "color-picker.png");
                    Children<Button>(picker).First(b => b.ToolTip?.ToString() == "#397467").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Children<Button>(picker).Single(b => b.Content?.ToString() == "決定").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                });
                field.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Check(field.Value == "#397467", "Color picker selection binding");
                Capture(settings, "settings-window.png");
                Children<Button>(settings).Single(b => b.Content?.ToString() == "保存して適用").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            } catch (Exception ex) { settingsError = ex; settings.Close(); }
        });
        settings.ShowDialog();
        if (settingsError is not null) throw settingsError;
        var saved = Preferences.Load();
        Check(saved.Elements[0].Color == "#397467" && saved.Elements[0].Size == "22px", "Settings save roundtrip");
        Check(saved.GlobalStyle.Size == "20px" && !string.IsNullOrEmpty(saved.GlobalStyle.Font), "Bulk save roundtrip");
        Check(saved.BodyTopMargin == 72 && saved.OutlineTopMargin == 32, "Margin save roundtrip");
        ApplyOutlineMargin();
        Check(OutlinePanel.Padding == new Thickness(12,32,12,12), "Only outline top padding changed");

        // Route file-drop events from both the reading surface and footer through WPF.
        var dropFile = Path.Combine(Preferences.Home, "drop.md");
        await File.WriteAllTextAsync(dropFile, "# Dropped document\n\n## Second heading\n\nReading.");
        foreach (UIElement surface in new UIElement[] { Browser, Status, OutlinePanel }) {
            var loaded = new TaskCompletionSource();
            void Completed(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args) => loaded.TrySetResult();
            Browser.CoreWebView2.NavigationCompleted += Completed;
            try {
                if (surface == Browser) {
                    foreach (var type in new[] { "dragEnter", "dragOver", "drop" })
                        await Browser.CoreWebView2.CallDevToolsProtocolMethodAsync("Input.dispatchDragEvent", JsonSerializer.Serialize(new { type, x=100, y=100, data=new { items=Array.Empty<object>(), files=new[] {dropFile}, dragOperationsMask=1 } }));
                } else {
                var ctor = typeof(DragEventArgs).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
                var args = (DragEventArgs)ctor.Invoke(new object[] { new DataObject(DataFormats.FileDrop, new[] { dropFile }), DragDropKeyStates.None, DragDropEffects.Copy, surface, new Point(20,20) });
                args.RoutedEvent = DragDrop.PreviewDropEvent;
                surface.RaiseEvent(args);
                Check(args.Handled, "Drop not intercepted");
                }
                await loaded.Task.WaitAsync(TimeSpan.FromSeconds(10));
                Check(file == dropFile, "Dropped path");
                Check(Title == $"SattoMDV — {dropFile}", "Full file path in title");
                // Open the menu created before a file was loaded, as WPF does on the footer.
                ContextMenu.PlacementTarget = Status;
                ContextMenu.IsOpen = true;
                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                Check(ContextMenu.Items.OfType<MenuItem>().Single(m => m.Header.ToString() == "このファイルの場所を開く").IsEnabled, "Previously created footer menu must reflect loaded file");
                ContextMenu.IsOpen = false;
                Check(CreateMenu().Items.OfType<MenuItem>().Single(m => m.Header.ToString() == "このファイルの場所を開く").IsEnabled, "Folder action enabled");
                var folderStart = FolderStartInfo(dropFile);
                Check(folderStart.FileName == "explorer.exe" && folderStart.ArgumentList.Single() == Path.GetDirectoryName(dropFile), "Explorer directory argument");
            } finally { Browser.CoreWebView2.NavigationCompleted -= Completed; }
        }
        await RefreshOutline();
        Check(OutlineList.Items.Count == 2, "Outline refresh after drop");
        await File.WriteAllTextAsync(Path.Combine(Preferences.Home, "ui-pass.txt"), "Context menu, outline toggle/jump/persistence, picker/dropdowns/save, browser/footer/outline routed drops: PASS");
    }
    async Task RunMenuChecks() {
        var menu = ContextMenu;
        var folder = menu.Items.OfType<MenuItem>().Single(m => m.Header.ToString() == "このファイルの場所を開く");
        if (folder.IsEnabled) throw new InvalidOperationException("Welcome page has no file");
        var path = Path.Combine(Preferences.Home, "menu-test.md");
        await File.WriteAllTextAsync(path, "# Menu test\n\nDocument.");
        await LoadFile(path);
        foreach (var target in new FrameworkElement[] { Status, OutlinePanel, Browser }) {
            menu.PlacementTarget = target;
            menu.IsOpen = true;
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            if (!folder.IsEnabled) throw new InvalidOperationException("Reused menu remains disabled after file open");
            menu.IsOpen = false;
        }
        await LoadFile(null);
        menu.IsOpen = true;
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        if (folder.IsEnabled) throw new InvalidOperationException("Folder menu remains enabled without file");
        menu.IsOpen = false;
        await File.WriteAllTextAsync(Path.Combine(Preferences.Home, "ui-pass.txt"), "Reused context menu: no file -> file open -> no file: PASS");
    }
}

