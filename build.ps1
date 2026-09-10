param([string]$OutputDirectory = "$PSScriptRoot/dist/SattoMDV")
$ErrorActionPreference = 'Stop'
dotnet publish "$PSScriptRoot/src/MDV/MDV.csproj" -c Release -o $OutputDirectory
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
# Remove only known obsolete build products; leave user-added files untouched.
$publishRoot = [IO.Path]::GetFullPath($OutputDirectory)
foreach ($relative in @('Microsoft.Windows.SDK.NET.dll','WinRT.Runtime.dll','Microsoft.Web.WebView2.Core.xml','Microsoft.Web.WebView2.Wpf.xml','Microsoft.Web.WebView2.WinForms.dll','Microsoft.Web.WebView2.WinForms.xml','SattoMDV.pdb','PERFORMANCE.md','runtimes/win-x86/native/WebView2Loader.dll','runtimes/win-arm64/native/WebView2Loader.dll','runtimes/win-x64/native/WebView2Loader.dll')) {
    $obsolete = [IO.Path]::GetFullPath((Join-Path $publishRoot $relative))
    if (-not $obsolete.StartsWith($publishRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected cleanup path' }
    if (Test-Path -LiteralPath $obsolete) { Remove-Item -LiteralPath $obsolete }
}
Copy-Item "$PSScriptRoot/README.md" "$publishRoot/README.md" -Force
Copy-Item "$PSScriptRoot/LICENSE" "$publishRoot/LICENSE" -Force
Copy-Item "$PSScriptRoot/THIRD-PARTY-NOTICES.md" "$publishRoot/THIRD-PARTY-NOTICES.md" -Force
Copy-Item "$PSScriptRoot/licenses" "$publishRoot/licenses" -Recurse -Force
Copy-Item "$PSScriptRoot/samples" "$publishRoot/samples" -Recurse -Force
Write-Host "実行ファイル: $publishRoot/SattoMDV.exe"



