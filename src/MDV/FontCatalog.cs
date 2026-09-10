using System.IO;
using System.Windows.Media;
using Microsoft.Win32;
namespace MDV;

public static class FontCatalog
{
    // Enumerated once on first settings open, never on the reader's startup path.
    static readonly Lazy<string[]> Names = new(EnumerateNames);
    public static string[] GetNames() => Names.Value;
    static string[] EnumerateNames() {
        var names = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
        void AddFamilies(IEnumerable<FontFamily> families) {
            foreach (var family in families) {
                try {
                    foreach (var name in family.FamilyNames.Values) if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
                    foreach (var face in family.GetTypefaces()) {
                        if (!face.TryGetGlyphTypeface(out var glyph)) continue;
                        foreach (var name in glyph.Win32FamilyNames.Values) if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
                    }
                } catch (Exception ex) when (ex is IOException or ArgumentException or NotSupportedException) { }
            }
        }
        AddFamilies(Fonts.SystemFontFamilies);
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Fonts")
        };
        // Registration can refer to fonts outside the usual per-user directory.
        foreach (var hive in new[] { Registry.CurrentUser, Registry.LocalMachine }) {
            using var key = hive.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts");
            if (key is null) continue;
            foreach (var name in key.GetValueNames()) {
                if (key.GetValue(name) is string path && Path.IsPathFullyQualified(path) && File.Exists(path)) paths.Add(path);
            }
        }
        foreach (var path in paths) {
            if (!File.Exists(path) && !Directory.Exists(path)) continue;
            try { AddFamilies(Fonts.GetFontFamilies(path)); }
            catch (Exception ex) when (ex is IOException or ArgumentException or NotSupportedException or UnauthorizedAccessException) { }
        }
        return names.OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase).Prepend("").ToArray();
    }
}

