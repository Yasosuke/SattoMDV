using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MDV;

// Only application captions are localized; document text, CSS and font names stay intact.
public sealed class UiText : INotifyPropertyChanged
{
    public static UiText Current { get; } = new();
    string language = "ja";
    public string Language {
        get => language;
        set {
            var normalized = value == "en" ? "en" : "ja";
            if (language == normalized) return;
            language = normalized;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public static string T(string japanese) => Current.Language == "en" && English.TryGetValue(japanese, out var text) ? text : japanese;
    static readonly Dictionary<string, string> English = new() {
        ["SattoMDV — テーマ・表示設定"] = "SattoMDV — Theme & appearance",
        ["SattoMDV — さっと表示する Markdown Viewer"] = "SattoMDV — A simple Markdown viewer",
        ["読み心地を整える"] = "Make reading comfortable",
        ["一括設定（本文のすべての要素）"] = "Global appearance (all document elements)",
        ["文字色"] = "Text color", ["背景色"] = "Background",
        ["文字サイズ"] = "Font size", ["フォント"] = "Font",
        ["空欄はテーマを継承。下の個別設定がある要素は、個別の指定を優先します。"] = "Blank values inherit the theme. Element-specific settings below take priority.",
        ["0～500 px（上側だけ）"] = "0–500 px (top only)",
        ["本文の上余白 (px)"] = "Document top margin (px)",
        ["アウトラインの上余白 (px)"] = "Outline top margin (px)",
        ["theme.css を選ぶ"] = "Choose theme.css",
        ["CSS テーマ|*.css"] = "CSS themes|*.css",
        ["標準に戻す"] = "Use default theme", ["ダークモード"] = "Dark mode",
        ["色のボタンでカラーピッカーを表示。文字サイズ・フォントはプルダウンから選択できます。\n空欄はテーマを継承。サイズ・フォントは直接入力も可能です。"] = "Click a color button to open the picker. Choose font sizes and fonts from the dropdowns.\nBlank values inherit the theme. You can also type a size or font name.",
        ["要素 / セレクター"] = "Element / selector",
        ["要素ごとの見た目"] = "Element appearance",
        ["追加 CSS（余白・行間・枠線など）"] = "Custom CSS (spacing, line height, borders)",
        ["キャンセル"] = "Cancel", ["保存して適用"] = "Save & apply",
        ["上余白は0～500の数値で指定してください。"] = "Enter a top margin between 0 and 500.",
        ["設定を保存できません"] = "Could not save settings",
        ["テーマを継承"] = "Inherit theme", ["色を選択"] = "Choose a color",
        ["#RRGGBB または色名"] = "#RRGGBB or a color name",
        ["赤 R"] = "Red R", ["緑 G"] = "Green G", ["青 B"] = "Blue B",
        ["決定"] = "OK",
        ["#RRGGBB または有効な色名を指定してください。"] = "Enter #RRGGBB or a valid color name.",
        ["色を確認してください"] = "Check the color",
        ["ファイルをひらくにはウィンドウへドロップしてください/右クリックでメニュー表示"] = "Drop a file anywhere in this window to open it / Right-click for the menu",
        ["アウトライン"] = "Outline", ["見出しがありません"] = "No headings",
        ["起動できませんでした"] = "Could not start the viewer",
        ["Markdown|*.md;*.markdown;*.txt|すべてのファイル|*.*"] = "Markdown|*.md;*.markdown;*.txt|All files|*.*",
        ["ファイルを開く"] = "Open file", ["再読込"] = "Reload",
        ["このファイルの場所を開く"] = "Open file location",
        ["Markdownファイルを開くと使えます"] = "Open a Markdown file to use this command",
        ["アウトラインを非表示"] = "Hide outline", ["アウトラインを表示"] = "Show outline",
        ["テーマ・表示設定"] = "Theme & appearance", ["終了"] = "Exit"
    };

    sealed class CaptionConverter(string caption) : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => T(caption);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
    static void Bind(DependencyObject element, DependencyProperty property) {
        if (element.GetValue(property) is not string caption || !English.ContainsKey(caption) || BindingOperations.IsDataBound(element, property)) return;
        BindingOperations.SetBinding(element, property, new Binding(nameof(Language)) { Source = Current, Converter = new CaptionConverter(caption), Mode = BindingMode.OneWay });
    }
    public static void Localize(DependencyObject element) {
        if (element is Window) Bind(element, Window.TitleProperty);
        if (element is TextBlock) Bind(element, TextBlock.TextProperty);
        if (element is ContentControl) Bind(element, ContentControl.ContentProperty);
        if (element is HeaderedContentControl) Bind(element, HeaderedContentControl.HeaderProperty);
        if (element is FrameworkElement) Bind(element, FrameworkElement.ToolTipProperty);
        if (element is DataGrid grid) foreach (var column in grid.Columns) Bind(column, DataGridColumn.HeaderProperty);
        // Do not walk editable values, font lists or document data.
        if (element is TextBox or ComboBox or DataGrid) return;
        foreach (var child in LogicalTreeHelper.GetChildren(element).OfType<DependencyObject>()) Localize(child);
    }
}
