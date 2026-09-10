using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
namespace MDV;

public sealed class ColorField : Button
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(string), typeof(ColorField), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, _) => ((ColorField)d).UpdateSwatch()));
    public string Value { get => (string)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    readonly Border swatch = new() { Width = 18, Height = 18, BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1), Margin = new Thickness(0,0,6,0) };
    readonly TextBlock label = new();
    public ColorField() {
        Padding = new Thickness(6,4,6,4); HorizontalContentAlignment = HorizontalAlignment.Left;
        var row = new StackPanel { Orientation = Orientation.Horizontal }; row.Children.Add(swatch); row.Children.Add(label); Content = row;
        UpdateSwatch();
        Click += (_, _) => {
            var picker = new ColorPicker(Value) { Owner = Window.GetWindow(this) };
            if (picker.ShowDialog() == true) SetCurrentValue(ValueProperty, picker.Value);
        };
    }
    void UpdateSwatch() {
        label.Text = string.IsNullOrWhiteSpace(Value) ? "テーマを継承" : Value;
        try { swatch.Background = (Brush)new BrushConverter().ConvertFromString(Value)!; } catch { swatch.Background = Brushes.Transparent; }
    }
}

public sealed class ColorPicker : Window
{
    public string Value { get; private set; }
    public ColorPicker(string initial) {
        Value = initial; Title = "色を選択"; Width = 420; SizeToContent = SizeToContent.Height; ResizeMode = ResizeMode.NoResize; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var root = new StackPanel { Margin = new Thickness(20) }; Content = root;
        var preview = new Border { Height = 58, CornerRadius = new CornerRadius(4), BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1), Margin = new Thickness(0,0,0,14) }; root.Children.Add(preview);
        var palette = new WrapPanel { Margin = new Thickness(0,0,0,10) }; root.Children.Add(palette);
        var channels = new[] { new Slider(), new Slider(), new Slider() };
        var hex = new TextBox { Margin = new Thickness(0,12,0,12), Padding = new Thickness(8), ToolTip = "#RRGGBB または色名" };
        bool updating = false;
        void SetColor(Color color) {
            updating = true;
            channels[0].Value = color.R; channels[1].Value = color.G; channels[2].Value = color.B;
            hex.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}"; preview.Background = new SolidColorBrush(color);
            updating = false;
        }
        foreach (var code in new[] { "#000000", "#333333", "#666666", "#999999", "#CCCCCC", "#FFFFFF", "#F5F0E5", "#22262D", "#A13D3D", "#D97732", "#E5BE4A", "#71954B", "#397467", "#3984A0", "#4866AA", "#8654A1", "#C76B91", "#805234" }) {
            var color = (Color)ColorConverter.ConvertFromString(code);
            var button = new Button { Width = 34, Height = 30, Margin = new Thickness(3), Background = new SolidColorBrush(color), ToolTip = code };
            button.Click += (_, _) => SetColor(color); palette.Children.Add(button);
        }
        for (var i = 0; i < 3; i++) {
            var row = new DockPanel { Margin = new Thickness(0,4,0,4) };
            row.Children.Add(new TextBlock { Text = new[] { "赤 R", "緑 G", "青 B" }[i], Width = 48 });
            var slider = channels[i]; slider.Minimum = 0; slider.Maximum = 255; slider.TickFrequency = 1; slider.IsSnapToTickEnabled = true;
            slider.ValueChanged += (_, _) => { if (!updating) SetColor(Color.FromRgb((byte)channels[0].Value, (byte)channels[1].Value, (byte)channels[2].Value)); };
            row.Children.Add(slider); root.Children.Add(row);
        }
        root.Children.Add(hex);
        hex.TextChanged += (_, _) => { if (!updating) { try { preview.Background = (Brush)new BrushConverter().ConvertFromString(hex.Text)!; } catch { } } };
        var buttons = new WrapPanel(); root.Children.Add(buttons);
        void Add(string title, Action action) { var button = new Button { Content = title, Padding = new Thickness(10,7,10,7), Margin = new Thickness(0,0,6,0) }; button.Click += (_, _) => action(); buttons.Children.Add(button); }
        Add("テーマを継承", () => { Value = ""; DialogResult = true; });
        Add("キャンセル", () => DialogResult = false);
        Add("決定", () => {
            try { _ = ColorConverter.ConvertFromString(hex.Text.Trim()); Value = hex.Text.Trim(); DialogResult = true; }
            catch { MessageBox.Show(this, "#RRGGBB または有効な色名を指定してください。", "色を確認してください"); }
        });
        try { SetColor((Color)ColorConverter.ConvertFromString(initial)); } catch { SetColor(Colors.Black); }
    }
}

