using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;
using Microsoft.Win32;
namespace MDV;
public class SettingsWindow : Window
{
    public SettingsWindow(Preferences target)
    {
        Title = "SattoMDV — テーマ・表示設定"; Width = 1060; Height = 850; MinWidth = 850; MinHeight = 720;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var root = new DockPanel { Margin = new Thickness(20) }; Content = root;
        var top = new StackPanel(); DockPanel.SetDock(top, Dock.Top); root.Children.Add(top);
        top.Children.Add(new TextBlock { Text = "読み心地を整える", FontSize = 24, Margin = new Thickness(0,0,0,16) });
        var fonts = FontCatalog.GetNames();
        var sizes = new[] { "", "12px", "14px", "16px", "18px", "20px", "22px", "24px", "28px", "32px", "36px", "42px", "48px" };
        var global = new ElementStyle { Color=target.GlobalStyle.Color, Background=target.GlobalStyle.Background, Size=target.GlobalStyle.Size, Font=target.GlobalStyle.Font };
        top.Children.Add(new TextBlock { Text = "一括設定（本文のすべての要素）", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0,0,0,8) });
        var bulk = new System.Windows.Controls.Primitives.UniformGrid { Columns = 4, DataContext = global, Margin = new Thickness(0,0,0,12) };
        void BulkField(string label, FrameworkElement control) {
            var column = new StackPanel { Margin = new Thickness(0,0,12,0) };
            column.Children.Add(new TextBlock { Text = label, Margin = new Thickness(0,0,0,4) }); column.Children.Add(control); bulk.Children.Add(column);
        }
        foreach (var (label, property) in new[] { ("文字色", "Color"), ("背景色", "Background") }) {
            var field = new ColorField(); field.SetBinding(ColorField.ValueProperty, new Binding(property) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged }); BulkField(label, field);
        }
        ComboBox Choice(string property, string[] values) {
            var combo = new ComboBox { ItemsSource = values, IsEditable = true, MaxDropDownHeight = 300 };
            combo.SetBinding(ComboBox.TextProperty, new Binding(property) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged }); return combo;
        }
        BulkField("文字サイズ", Choice("Size", sizes)); BulkField("フォント", Choice("Font", fonts)); top.Children.Add(bulk);
        top.Children.Add(new TextBlock { Text = "空欄はテーマを継承。下の個別設定がある要素は、個別の指定を優先します。", Margin = new Thickness(0,0,0,12) });
        var margins = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,0,0,16) };
        TextBox MarginField(string label, double value) {
            margins.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,0,8,0) });
            var input = new TextBox { Text = value.ToString(CultureInfo.CurrentCulture), Width = 65, Padding = new Thickness(6), Margin = new Thickness(0,0,20,0), ToolTip = "0～500 px（上側だけ）" };
            margins.Children.Add(input); return input;
        }
        var bodyMargin = MarginField("本文の上余白 (px)", target.BodyTopMargin);
        var outlineMargin = MarginField("アウトラインの上余白 (px)", target.OutlineTopMargin);
        top.Children.Add(margins);
        var path = new TextBox { Text = target.ThemePath, MinWidth = 300, VerticalContentAlignment = VerticalAlignment.Center };
        var browse = new Button { Content = "theme.css を選ぶ", Padding = new Thickness(12,6,12,6) };
        browse.Click += (_, _) => { var d = new OpenFileDialog { Filter = "CSS テーマ|*.css" }; if (d.ShowDialog(this) == true) path.Text = d.FileName; };
        var clear = new Button { Content = "標準に戻す", Padding = new Thickness(12,6,12,6), Margin = new Thickness(8,0,0,0) };
        clear.Click += (_, _) => path.Text = "";
        var themeRow = new DockPanel(); DockPanel.SetDock(clear, Dock.Right); DockPanel.SetDock(browse, Dock.Right);
        themeRow.Children.Add(clear); themeRow.Children.Add(browse); themeRow.Children.Add(path); top.Children.Add(themeRow);
        var dark = new CheckBox { Content = "ダークモード", IsChecked = target.Dark, Margin = new Thickness(0,14,0,14) }; top.Children.Add(dark);
        top.Children.Add(new TextBlock { Text = "色のボタンでカラーピッカーを表示。文字サイズ・フォントはプルダウンから選択できます。\n空欄はテーマを継承。サイズ・フォントは直接入力も可能です。", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,0,0,12) });
        var rows = new ObservableCollection<ElementStyle>(target.Elements.Select(s => new ElementStyle { Selector=s.Selector, Color=s.Color, Background=s.Background, Size=s.Size, Font=s.Font }));
        if (rows.Count == 0) foreach (var selector in new[] { "body", "p", "h1", "h2", "h3", "h4", "h5", "h6", "a", "strong", "em", "del", "mark", "blockquote", "ul", "ol", "li", "li::marker", "code", "pre", "table", "th", "td", "hr", "img", "input[type=checkbox]" }) rows.Add(new() { Selector = selector });
        var bottom = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0,14,0,0) };
        DockPanel.SetDock(bottom, Dock.Bottom); root.Children.Add(bottom);
        var tabs = new TabControl(); root.Children.Add(tabs);
        var grid = new DataGrid { ItemsSource = rows, AutoGenerateColumns = false, CanUserAddRows = true, CanUserDeleteRows = true, RowHeaderWidth = 22 };
        grid.Columns.Add(new DataGridTextColumn { Header = "要素 / セレクター", Binding = new Binding("Selector"), Width = 185, MinWidth = 150 });
        foreach (var (label, property) in new[] { ("文字色", "Color"), ("背景色", "Background") }) {
            var factory = new FrameworkElementFactory(typeof(ColorField));
            factory.SetBinding(ColorField.ValueProperty, new Binding(property) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            grid.Columns.Add(new DataGridTemplateColumn { Header = label, CellTemplate = new DataTemplate { VisualTree = factory }, Width = 160 });
        }
        AddDropdown(grid, "文字サイズ", "Size", new[] { "", "12px", "14px", "16px", "18px", "20px", "22px", "24px", "28px", "32px", "36px", "42px", "48px", "0.9em", "1em", "1.2em", "1.5em", "2em" }, 130);
        AddDropdown(grid, "フォント", "Font", fonts, 225);
        tabs.Items.Add(new TabItem { Header = "要素ごとの見た目", Content = grid });
        var css = new TextBox { Text = target.CustomCss, AcceptsReturn = true, AcceptsTab = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, FontFamily = new System.Windows.Media.FontFamily("Consolas"), Padding = new Thickness(10) };
        tabs.Items.Add(new TabItem { Header = "追加 CSS（余白・行間・枠線など）", Content = css });
        var cancel = new Button { Content = "キャンセル", IsCancel = true, Padding = new Thickness(18,8,18,8), Margin = new Thickness(0,0,8,0) }; bottom.Children.Add(cancel);
        var save = new Button { Content = "保存して適用", Padding = new Thickness(18,8,18,8) }; bottom.Children.Add(save);
        save.Click += (_, _) => {
            grid.CommitEdit(DataGridEditingUnit.Cell, true); grid.CommitEdit(DataGridEditingUnit.Row, true);
            try {
                if (!string.IsNullOrWhiteSpace(path.Text)) _ = File.ReadAllText(path.Text);
                double ReadMargin(TextBox input) {
                    if (!double.TryParse(input.Text, out var value) || !double.IsFinite(value) || value is < 0 or > 500) throw new ArgumentException("上余白は0～500の数値で指定してください。");
                    return value;
                }
                var updated = new Preferences { ThemePath = path.Text.Trim(), Dark = dark.IsChecked == true, ShowOutline = target.ShowOutline, GlobalStyle = global, BodyTopMargin = ReadMargin(bodyMargin), OutlineTopMargin = ReadMargin(outlineMargin), CustomCss = css.Text, Elements = rows.Where(s => !string.IsNullOrWhiteSpace(s.Selector)).ToList() };
                updated.Save();
                target.ThemePath=updated.ThemePath; target.Dark=updated.Dark; target.CustomCss=updated.CustomCss; target.Elements=updated.Elements;
                target.GlobalStyle=updated.GlobalStyle; target.BodyTopMargin=updated.BodyTopMargin; target.OutlineTopMargin=updated.OutlineTopMargin;
                DialogResult = true;
            } catch (Exception ex) { MessageBox.Show(this, ex.Message, "設定を保存できません", MessageBoxButton.OK, MessageBoxImage.Warning); }
        };
    }
    static void AddDropdown(DataGrid grid, string label, string property, string[] choices, double width) {
        var factory = new FrameworkElementFactory(typeof(ComboBox));
        factory.SetValue(ComboBox.ItemsSourceProperty, choices);
        factory.SetValue(ComboBox.IsEditableProperty, true);
        factory.SetValue(ComboBox.MaxDropDownHeightProperty, 300.0);
        factory.SetBinding(ComboBox.TextProperty, new Binding(property) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        grid.Columns.Add(new DataGridTemplateColumn { Header = label, CellTemplate = new DataTemplate { VisualTree = factory }, Width = width });
    }
}

