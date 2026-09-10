param([string]$Executable = "$PSScriptRoot/dist/SattoMDV/SattoMDV.exe")
$ErrorActionPreference = 'Stop'
$testRoot = Join-Path $PSScriptRoot '.test-data'
$exePath = $Executable
function Test-Viewer($name, $settings, $filePath) {
    $env:SATTOMDV_DATA_DIR = Join-Path $testRoot $name
    New-Item -ItemType Directory -Force $env:SATTOMDV_DATA_DIR | Out-Null
    $settings | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $env:SATTOMDV_DATA_DIR 'settings.json') -Encoding utf8
    $arguments = @('--smoke-test')
    if ($filePath) { $arguments += ('"' + $filePath + '"') }
    $process = Start-Process -FilePath $exePath -ArgumentList $arguments -WindowStyle Hidden -PassThru
    if (-not $process.WaitForExit(20000)) { Stop-Process -Id $process.Id; throw "$name timed out" }
    if ($process.ExitCode -ne 0) { throw "$name failed: $($process.ExitCode)" }
    $result = Get-Content (Join-Path $env:SATTOMDV_DATA_DIR 'smoke.json') -Raw | ConvertFrom-Json
    $dom = $result.dom | ConvertFrom-Json
    if ($dom.scripts -ne 0 -or $dom.overflow) { throw "$name unsafe or overflowing" }
    Write-Host "$name ready: $($result.readyMs) ms"
    return $dom
}
$savedDataDir = $env:SATTOMDV_DATA_DIR
try {
    $default = Test-Viewer 'default' @{} $null
    if ($default.heading -ne '文章を、気持ちよく読む。' -or $default.tables -ne 1) { throw 'Default rendering failed' }
    $theme = Test-Viewer 'theme' @{ ThemePath = (Join-Path $PSScriptRoot 'samples/ink.theme.css'); Dark = $true; Elements = @(@{Selector='h1'; Size='42px'; Color='#123456'; Font='Consolas'; Background='#abcdef'}) } (Join-Path $PSScriptRoot 'samples/reading.md')
    if ($theme.color -ne 'rgb(215, 223, 235)' -or $theme.h1Size -ne '42px' -or $theme.h1Color -ne 'rgb(18, 52, 86)' -or $theme.h1Font -ne 'Consolas' -or $theme.h1Background -ne 'rgb(171, 205, 239)') { throw 'Theme or override failed' }
    $bulk = Test-Viewer 'bulk' @{ GlobalStyle = @{Color='#456789'; Size='21px'; Font='Meiryo'; Background='#abcdef'}; BodyTopMargin=80; Elements=@(@{Selector='h1';Color='#123456';Size='42px'}) } $null
    if ($bulk.pColor -ne 'rgb(69, 103, 137)' -or $bulk.pSize -ne '21px' -or $bulk.pFont -ne 'Meiryo' -or $bulk.h1Color -ne 'rgb(18, 52, 86)' -or $bulk.h1Size -ne '42px' -or $bulk.topPadding -ne '80px' -or $bulk.leftPadding -ne '36px') { throw 'Bulk override or top-only margin failed' }
    $largePath = Join-Path $testRoot 'large.md'
    ("# Large document`n`n<script>alert(1)</script>`n`n" + ("日本語の長い文章を読みます。`n`n" * 65000)) | Set-Content $largePath -Encoding utf8
    $large = Test-Viewer 'large' @{} $largePath
    if ($large.paragraphs -lt 65000) { throw 'Large document truncated' }
    $env:SATTOMDV_DATA_DIR = Join-Path $testRoot 'ui'
    New-Item -ItemType Directory -Force $env:SATTOMDV_DATA_DIR | Out-Null
    '{}' | Set-Content (Join-Path $env:SATTOMDV_DATA_DIR 'settings.json') -Encoding utf8
    foreach ($name in @('ui-pass.txt', 'ui-error.txt')) {
        $resultPath = Join-Path $env:SATTOMDV_DATA_DIR $name
        if (Test-Path -LiteralPath $resultPath) { Remove-Item -LiteralPath $resultPath }
    }
    $ui = Start-Process -FilePath $exePath -ArgumentList '--ui-test' -WindowStyle Hidden -PassThru
    if (-not $ui.WaitForExit(25000)) { Stop-Process -Id $ui.Id; throw 'UI test timed out' }
    if (Test-Path (Join-Path $env:SATTOMDV_DATA_DIR 'ui-error.txt')) { throw (Get-Content (Join-Path $env:SATTOMDV_DATA_DIR 'ui-error.txt') -Raw) }
    if ($ui.ExitCode -ne 0 -or -not (Test-Path (Join-Path $env:SATTOMDV_DATA_DIR 'ui-pass.txt'))) { throw 'UI test failed' }
    Get-Content (Join-Path $env:SATTOMDV_DATA_DIR 'ui-pass.txt')
    Write-Host 'All rendering and UI checks passed.'
} finally { $env:SATTOMDV_DATA_DIR = $savedDataDir }



