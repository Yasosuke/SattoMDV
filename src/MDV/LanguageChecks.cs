using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MDV;

public partial class MainWindow
{
    async Task RunLanguageChecks() {
        static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        static IEnumerable<T> Children<T>(DependencyObject parent) where T : DependencyObject {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typed) yield return typed;
                foreach (var nested in Children<T>(child)) yield return nested;
            }
        }
        async Task Idle() => await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        Check(prefs.Language == "ja", "Existing settings must default to Japanese");
        var settings = new SettingsWindow(prefs) { Owner = this };
        Exception? failure = null;
        settings.Loaded += (_, _) => Dispatcher.BeginInvoke(new Action(async () => {
            try {
                var size = Children<ComboBox>(settings).First(c => c.Items.Contains("18px"));
                size.Text = "24px";
                Children<RadioButton>(settings).Single(b => b.Content.ToString() == "English").IsChecked = true;
                await Idle();
                Check(settings.Title == "SattoMDV — Theme & appearance", "Live English title");
                Check(Children<TextBlock>(settings).Any(t => t.Text == "Make reading comfortable"), "English heading");
                var grid = Children<DataGrid>(settings).Single();
                Check(grid.Columns[0].Header.ToString() == "Element / selector", "English table headers");
                Check(size.Text == "24px", "Language change must preserve unsaved edits");
                var color = Children<ColorField>(settings).First();
                Check(Children<TextBlock>(color).Any(t => t.Text == "Inherit theme"), "English color field");
                var picker = new ColorPicker("") { Owner = settings };
                picker.Loaded += (_, _) => Dispatcher.BeginInvoke(new Action(() => {
                    try {
                        Check(picker.Title == "Choose a color", "English color picker title");
                        Check(Children<TextBlock>(picker).Any(t => t.Text == "Green G"), "English color channels");
                        Children<Button>(picker).Single(b => b.Content?.ToString() == "Cancel").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    } catch (Exception ex) { failure = ex; picker.Close(); }
                }));
                picker.ShowDialog();
                if (failure is not null) throw failure;
                Children<RadioButton>(settings).Single(b => b.Content.ToString() == "日本語").IsChecked = true;
                await Idle();
                Check(settings.Title == "SattoMDV — テーマ・表示設定", "Switch back to Japanese");
                Check(grid.Columns[0].Header.ToString() == "要素 / セレクター", "Japanese table headers");
                Children<RadioButton>(settings).Single(b => b.Content.ToString() == "English").IsChecked = true;
                await Idle();
                settings.UpdateLayout();
                var bitmap = new RenderTargetBitmap((int)settings.ActualWidth, (int)settings.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(settings);
                var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using (var image = File.Create(Path.Combine(Preferences.Home, "english-settings.png"))) encoder.Save(image);
                Children<Button>(settings).Single(b => b.Content?.ToString() == "Save & apply").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            } catch (Exception ex) { failure = ex; settings.Close(); }
        }));
        Check(settings.ShowDialog() == true, "Save English settings");
        if (failure is not null) throw failure;
        Check(Preferences.Load().Language == "en" && prefs.GlobalStyle.Size == "24px", "Saved language and edit roundtrip");
        ApplyLanguage(); ContextMenu = CreateMenu();
        Check(Status.Text.StartsWith("Drop a file"), "English footer");
        Check(OutlineHeading.Text == "Outline" && OutlineEmpty.Text == "No headings", "English outline captions");
        Check(ContextMenu.Items.OfType<MenuItem>().Any(m => m.Header.ToString() == "Open file location"), "English menu");
        var cancelled = new SettingsWindow(prefs) { Owner = this };
        cancelled.Loaded += (_, _) => Dispatcher.BeginInvoke(new Action(() => {
            Children<RadioButton>(cancelled).Single(b => b.Content.ToString() == "日本語").IsChecked = true;
            cancelled.Close();
        }));
        cancelled.ShowDialog();
        Check(UiText.Current.Language == "en" && Preferences.Load().Language == "en", "Cancel restores saved language");
        var loaded = new TaskCompletionSource();
        void Completed(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e) => loaded.TrySetResult();
        Browser.CoreWebView2.NavigationCompleted += Completed;
        await LoadFile(null);
        await loaded.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Browser.CoreWebView2.NavigationCompleted -= Completed;
        Check(await Browser.CoreWebView2.ExecuteScriptAsync("document.querySelector('h1').textContent") == "\"Read comfortably.\"", "English welcome page");
        Check(Renderer.Render("# 日本語の文書", prefs).Contains("日本語の文書"), "Document text must not be translated");
        File.WriteAllText(Path.Combine(Preferences.Home, "language-pass.txt"), "PASS: live switch, cancel, persistence, captions, color picker, welcome and unchanged document text");
    }
}
