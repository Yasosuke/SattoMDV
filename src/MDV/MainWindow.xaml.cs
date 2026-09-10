using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
namespace MDV;
public partial class MainWindow : Window
{
    readonly Preferences prefs = Preferences.Load();
    readonly Stopwatch startup = Stopwatch.StartNew();
    string? file;
    byte[] document = Array.Empty<byte>();
    bool ready;
    int revision;
    const string DropHint = "ファイルをひらくにはウィンドウへドロップしてください/右クリックでメニュー表示";
    string? pendingDrop;
    bool uiTesting;
    ContextMenu? activeMenu;
    public sealed class OutlineEntry {
        public string Text { get; set; } = "";
        public int Level { get; set; }
        public int Index { get; set; }
        public Thickness Indent => new((Level - 1) * 12, 0, 0, 0);
    }
    public MainWindow()
    {
        InitializeComponent();
        OutlinePanel.Visibility = prefs.ShowOutline ? Visibility.Visible : Visibility.Collapsed;
        ApplyOutlineMargin();
        ContextMenu = CreateMenu();
        Loaded += async (_, _) => {
            var startupStage = "文書の準備";
            try {
                var initialPath = pendingDrop ?? Environment.GetCommandLineArgs().Skip(1).FirstOrDefault(a => !a.StartsWith("--"));
                var prepared = PrepareDocument(initialPath);
                startupStage = "WebView2環境の作成";
                var env = await CoreWebView2Environment.CreateAsync(null, Preferences.BrowserDataDirectory);
                if (!IsLoaded) return;
                startupStage = "WebView2表示コントロールの作成";
                await Browser.EnsureCoreWebView2Async(env);
                startupStage = "WebView2の設定";
                var core = Browser.CoreWebView2;
                core.Settings.AreDefaultScriptDialogsEnabled = false;
                core.Settings.IsWebMessageEnabled = true;
                core.Settings.AreDevToolsEnabled = false;
                core.Settings.IsStatusBarEnabled = false;
                core.WebMessageReceived += (_, e) => {
                    // Only our current document and native File objects may request a drop.
                    if (e.Source != core.Source || !e.Source.StartsWith("https://mdv.invalid/", StringComparison.Ordinal)) return;
                    try {
                        if (e.TryGetWebMessageAsString() != "file-drop") return;
                        var dropped = e.AdditionalObjects?.OfType<CoreWebView2File>().FirstOrDefault()?.Path;
                        if (!string.IsNullOrEmpty(dropped) && Path.IsPathFullyQualified(dropped) && File.Exists(dropped)) _ = LoadFile(dropped);
                    } catch (ArgumentException) { }
                };
                await core.AddScriptToExecuteOnDocumentCreatedAsync("""
                    window.addEventListener('dragover', e => {
                        if (Array.from(e.dataTransfer.types).includes('Files')) { e.preventDefault(); e.dataTransfer.dropEffect='copy'; }
                    }, true);
                    window.addEventListener('drop', e => {
                        e.preventDefault(); e.stopPropagation();
                        if(e.dataTransfer.files.length) chrome.webview.postMessageWithAdditionalObjects('file-drop', [e.dataTransfer.files[0]]);
                    }, true);
                    """);
                core.ContextMenuRequested += (_, e) => {
                    e.Handled = true;
                    Dispatcher.BeginInvoke(() => {
                        var menu = activeMenu = CreateMenu();
                        menu.PlacementTarget = Browser;
                        menu.Placement = PlacementMode.MousePoint;
                        menu.IsOpen = true;
                    });
                };
                core.AddWebResourceRequestedFilter("https://mdv.invalid/*", CoreWebView2WebResourceContext.All);
                core.WebResourceRequested += (_, e) => e.Response = env.CreateWebResourceResponse(new MemoryStream(document, writable: false), 200, "OK", "Content-Type: text/html; charset=utf-8");
                core.NavigationStarting += (_, e) => {
                    if (e.Uri.StartsWith("https://mdv.invalid/")) return;
                    e.Cancel = true;
                    if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri) && uri.Host == "assets.mdv.invalid") {
                        if (uri.AbsolutePath == "/" && uri.Fragment.Length > 1) {
                            _ = core.ExecuteScriptAsync("document.getElementById(" + System.Text.Json.JsonSerializer.Serialize(Uri.UnescapeDataString(uri.Fragment[1..])) + ")?.scrollIntoView()");
                            return;
                        }
                        if (file is null) return;
                        var path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')).Replace('/', Path.DirectorySeparatorChar)));
                        if (Path.GetExtension(path).Equals(".md", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(path).Equals(".markdown", StringComparison.OrdinalIgnoreCase)) _ = LoadFile(path);
                    } else if (uri?.Scheme is "https" or "http" or "mailto") {
                        try { Process.Start(new ProcessStartInfo(e.Uri) { UseShellExecute = true }); } catch (Exception ex) { ShowError(ex); }
                    }
                };
                core.NewWindowRequested += (_, e) => e.Handled = true;
                core.PermissionRequested += (_, e) => e.State = CoreWebView2PermissionState.Deny;
                core.DownloadStarting += (_, e) => e.Cancel = true;
                core.NavigationCompleted += async (_, e) => {
                    if (Environment.GetCommandLineArgs().Contains("--startup-probe")) {
                        WriteStartupProbe(e.IsSuccess ? "ready" : e.WebErrorStatus.ToString());
                        Close(); return;
                    }
                    if (e.IsSuccess) {
                        Status.Text = DropHint;
                        Status.ToolTip = file;
                        if (prefs.ShowOutline) await RefreshOutline();
                    }
                    if (Environment.GetCommandLineArgs().Contains("--benchmark")) {
                        try { await RecordBenchmark(); } finally { Close(); }
                        return;
                    }
                    if (Environment.GetCommandLineArgs().Contains("--ui-test") || Environment.GetCommandLineArgs().Contains("--menu-test")) {
                        if (!uiTesting) {
                            uiTesting = true;
                            try { await RunUiChecks(); }
                            catch (Exception ex) { await File.WriteAllTextAsync(Path.Combine(Preferences.Home, "ui-error.txt"), ex.ToString()); }
                            finally { Close(); }
                        }
                        return;
                    }
                    if (Environment.GetCommandLineArgs().Contains("--smoke-test")) {
                        try {
                            var elapsed = startup.ElapsedMilliseconds;
                            var result = await core.ExecuteScriptAsync("JSON.stringify({heading:document.querySelector('h1')?.textContent,paragraphs:document.querySelectorAll('p').length,tables:document.querySelectorAll('table').length,color:getComputedStyle(document.body).color,h1Size:getComputedStyle(document.querySelector('h1')).fontSize,h1Color:getComputedStyle(document.querySelector('h1')).color,h1Font:getComputedStyle(document.querySelector('h1')).fontFamily,h1Background:getComputedStyle(document.querySelector('h1')).backgroundColor,pColor:getComputedStyle(document.querySelector('p')).color,pFont:getComputedStyle(document.querySelector('p')).fontFamily,pSize:getComputedStyle(document.querySelector('p')).fontSize,topPadding:getComputedStyle(document.querySelector('main')).paddingTop,leftPadding:getComputedStyle(document.querySelector('main')).paddingLeft,scripts:document.scripts.length,overflow:document.documentElement.scrollWidth>innerWidth})");
                            Directory.CreateDirectory(Preferences.Home);
                            await File.WriteAllTextAsync(Path.Combine(Preferences.Home, "smoke.json"), "{\"readyMs\":" + elapsed + ",\"dom\":" + result + "}");
                            using var shot = File.Create(Path.Combine(Preferences.Home, "preview.png"));
                            await core.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, shot);
                        } finally { Close(); }
                    }
                };
                ready = true;
                var selectedPath = pendingDrop ?? initialPath;
                await LoadFile(selectedPath, selectedPath == initialPath ? prepared : null);
            } catch (Exception ex) {
                if (Environment.GetCommandLineArgs().Contains("--startup-probe")) { WriteStartupProbe(startupStage + "\n" + ex); Close(); return; }
                Status.Text = "起動できませんでした"; ShowError(ex);
            }
        };
        Closed += (_, _) => Browser.Dispose();
        PreviewKeyDown += (_, e) => {
            if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control) { Dispatcher.BeginInvoke(() => OpenClick(this, new())); e.Handled = true; }
            if (e.Key == Key.F5) { Dispatcher.BeginInvoke(() => ReloadClick(this, new())); e.Handled = true; }
        };
    }
    static void WriteStartupProbe(string result) {
        var directory = Environment.GetEnvironmentVariable("SattoMDV_PROBE_DIR") ?? Preferences.Home;
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "startup-probe.txt"), result);
    }
    sealed record PreparedDocument(byte[] Bytes, string? Path, Exception? Error);
    Task<PreparedDocument> PrepareDocument(string? path) => Task.Run(async () => {
        try {
            var fullPath = path is null ? null : Path.GetFullPath(path);
            var markdown = fullPath is null ? Renderer.Welcome : await File.ReadAllTextAsync(fullPath);
            return new PreparedDocument(Encoding.UTF8.GetBytes(Renderer.Render(markdown, prefs)), fullPath, null);
        } catch (Exception ex) { return new PreparedDocument(Array.Empty<byte>(), null, ex); }
    });
    async Task LoadFile(string? path, Task<PreparedDocument>? prepared = null)
    {
        if (!ready) return;
        var ticket = ++revision;
        try {
            var result = await (prepared ?? PrepareDocument(path));
            if (ticket != revision || !IsLoaded) return;
            if (result.Error is not null) throw result.Error;
            file = result.Path;
            if (file is not null) Browser.CoreWebView2.SetVirtualHostNameToFolderMapping("assets.mdv.invalid", Path.GetDirectoryName(file)!, CoreWebView2HostResourceAccessKind.DenyCors);
            document = result.Bytes;
            OutlineList.ItemsSource = null;
            Title = file is null ? "SattoMDV — さっと表示する Markdown Viewer" : $"SattoMDV — {file}";
            Browser.CoreWebView2.Navigate("https://mdv.invalid/index.html?r=" + revision);
        } catch (Exception ex) { ShowError(ex); }
    }
    void ShowError(Exception ex) => MessageBox.Show(this, ex.Message, "SattoMDV", MessageBoxButton.OK, MessageBoxImage.Warning);
    void OpenClick(object sender, RoutedEventArgs e) {
        var dialog = new OpenFileDialog { Filter = "Markdown|*.md;*.markdown;*.txt|すべてのファイル|*.*" };
        if (dialog.ShowDialog(this) == true) _ = LoadFile(dialog.FileName);
    }
    void ReloadClick(object sender, RoutedEventArgs e) => _ = LoadFile(file);
    void SettingsClick(object sender, RoutedEventArgs e) {
        var window = new SettingsWindow(prefs) { Owner = this };
        if (window.ShowDialog() == true) { ApplyOutlineMargin(); _ = LoadFile(file); }
    }
    void ApplyOutlineMargin() => OutlinePanel.Padding = new Thickness(12, Preferences.NormalizeMargin(prefs.OutlineTopMargin, 20), 12, 12);
    static ProcessStartInfo FolderStartInfo(string path) {
        var start = new ProcessStartInfo("explorer.exe") { UseShellExecute = true };
        start.ArgumentList.Add(Path.GetDirectoryName(Path.GetFullPath(path))!);
        return start;
    }
    void OnDrop(object sender, DragEventArgs e) {
        e.Handled = true;
        if (e.Data.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } paths && File.Exists(paths[0])) {
            if (ready) _ = LoadFile(paths[0]); else pendingDrop = paths[0];
        }
    }
    void OnDragOver(object sender, DragEventArgs e) {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }
    ContextMenu CreateMenu() {
        var menu = new ContextMenu();
        void Add(string title, RoutedEventHandler action, string shortcut = "") {
            var item = new MenuItem { Header = title, InputGestureText = shortcut };
            item.Click += action; menu.Items.Add(item);
        }
        Add("ファイルを開く", OpenClick, "Ctrl+O");
        Add("再読込", ReloadClick, "F5");
        var folder = new MenuItem { Header = "このファイルの場所を開く", IsEnabled = file is not null };
        folder.Click += (_, _) => {
            if (file is null) return;
            try { Process.Start(FolderStartInfo(file)); } catch (Exception ex) { ShowError(ex); }
        };
        menu.Items.Add(folder);
        menu.Items.Add(new Separator());
        var outline = new MenuItem();
        outline.Click += (_, _) => {
            prefs.ShowOutline = !prefs.ShowOutline;
            OutlinePanel.Visibility = prefs.ShowOutline ? Visibility.Visible : Visibility.Collapsed;
            if (prefs.ShowOutline && ready) _ = RefreshOutline();
            try { prefs.Save(); } catch (Exception ex) { ShowError(ex); }
        };
        menu.Items.Add(outline);
        void RefreshState() {
            folder.IsEnabled = file is not null;
            folder.ToolTip = file is null ? "Markdownファイルを開くと使えます" : Path.GetDirectoryName(file);
            ToolTipService.SetShowOnDisabled(folder, true);
            outline.Header = prefs.ShowOutline ? "アウトラインを非表示" : "アウトラインを表示";
        }
        RefreshState();
        menu.Opened += (_, _) => RefreshState();
        Add("テーマ・表示設定", SettingsClick);
        menu.Items.Add(new Separator());
        Add("終了", (_, _) => Close());
        return menu;
    }
    async Task RefreshOutline() {
        var ticket = revision;
        try {
            var json = await Browser.CoreWebView2.ExecuteScriptAsync("Array.from(document.querySelectorAll('article h1,article h2,article h3,article h4,article h5,article h6'),(h,i)=>({Text:h.textContent,Level:Number(h.tagName[1]),Index:i}))");
            var entries = JsonSerializer.Deserialize<List<OutlineEntry>>(json) ?? new();
            if (ticket != revision || !IsLoaded) return;
            OutlineList.ItemsSource = entries;
            OutlineEmpty.Visibility = entries.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        } catch (Exception ex) { Status.Text = ex.Message; }
    }
    async void OutlineSelected(object sender, SelectionChangedEventArgs e) {
        if (ready && OutlineList.SelectedItem is OutlineEntry entry) {
            try { await Browser.CoreWebView2.ExecuteScriptAsync($"document.querySelectorAll('article h1,article h2,article h3,article h4,article h5,article h6')[{entry.Index}]?.scrollIntoView({{block:'start'}})"); }
            catch (Exception ex) { ShowError(ex); }
        }
    }
}


