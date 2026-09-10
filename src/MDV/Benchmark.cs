using System.Diagnostics;
using System.IO;
using System.Text.Json;
namespace MDV;
public partial class MainWindow
{
    async Task RecordBenchmark() {
        // Same measurement in before/after binaries; actual browser first-contentful-paint.
        var result = await Browser.CoreWebView2.CallDevToolsProtocolMethodAsync("Runtime.evaluate", JsonSerializer.Serialize(new {
            expression = "new Promise(resolve => {const check=()=>{const p=performance.getEntriesByName('first-contentful-paint')[0]; if(p)resolve(performance.timeOrigin+p.startTime);else requestAnimationFrame(check)};check()})",
            awaitPromise = true, returnByValue = true
        }));
        using var json = JsonDocument.Parse(result);
        var paintEpoch = json.RootElement.GetProperty("result").GetProperty("value").GetDouble();
        using var current = Process.GetCurrentProcess();
        var startEpoch = new DateTimeOffset(current.StartTime.ToUniversalTime()).ToUnixTimeMilliseconds();
        long privateBytes = current.PrivateMemorySize64;
        foreach (var info in Browser.CoreWebView2.Environment.GetProcessInfos()) {
            try { using var process = Process.GetProcessById(info.ProcessId); privateBytes += process.PrivateMemorySize64; } catch (ArgumentException) { }
        }
        Directory.CreateDirectory(Preferences.Home);
        await File.WriteAllTextAsync(Path.Combine(Preferences.Home,"benchmark.json"), JsonSerializer.Serialize(new { processToPaintMs = paintEpoch-startEpoch, readyMs=startup.ElapsedMilliseconds, privateBytes }));
    }
}

