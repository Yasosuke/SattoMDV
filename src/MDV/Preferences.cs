using System.IO;
using System.Text.Json;
namespace MDV;
public class ElementStyle
{
    public string Selector { get; set; } = "";
    public string Color { get; set; } = "";
    public string Background { get; set; } = "";
    public string Size { get; set; } = "";
    public string Font { get; set; } = "";
}
public class Preferences
{
    static string? DataOverride => Environment.GetEnvironmentVariable("SATTOMDV_DATA_DIR") ?? Environment.GetEnvironmentVariable("SMDV_DATA_DIR") ?? Environment.GetEnvironmentVariable("MDV_DATA_DIR");
    public static string Home => DataOverride ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SattoMDV");
    // Native and composition controllers cannot share the same running browser environment.
    // Keep settings shared, but use a stable profile specific to this rendering backend.
    public static string BrowserDataDirectory => Path.Combine(Home, "WebView-Native-v1");
    public string Language { get; set; } = "ja";
    public string ThemePath { get; set; } = "";
    public bool Dark { get; set; }
    public bool ShowOutline { get; set; }
    public string CustomCss { get; set; } = "";
    public List<ElementStyle> Elements { get; set; } = new();
    public ElementStyle GlobalStyle { get; set; } = new();
    public double BodyTopMargin { get; set; } = 44;
    public double OutlineTopMargin { get; set; } = 20;
    public static Preferences Load() {
        try {
            var path = Path.Combine(Home, "settings.json");
            if (!File.Exists(path) && DataOverride is null) {
                foreach (var previous in new[] { "SMDV", "MDV" }) {
                    var candidate = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), previous, "settings.json");
                    if (File.Exists(candidate)) { path = candidate; break; }
                }
            }
            var prefs = JsonSerializer.Deserialize<Preferences>(File.ReadAllText(path)) ?? new();
            prefs.Language = prefs.Language == "en" ? "en" : "ja";
            prefs.BodyTopMargin = NormalizeMargin(prefs.BodyTopMargin, 44);
            prefs.OutlineTopMargin = NormalizeMargin(prefs.OutlineTopMargin, 20);
            return prefs;
        }
        catch { return new(); }
    }
    public static double NormalizeMargin(double value, double fallback) => double.IsFinite(value) && value is >= 0 and <= 500 ? value : fallback;
    public void Save() {
        Directory.CreateDirectory(Home);
        var path = Path.Combine(Home, "settings.json");
        File.WriteAllText(path + ".tmp", JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(path + ".tmp", path, true);
    }
}

