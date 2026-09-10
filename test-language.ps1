param([string]$Executable = "$PSScriptRoot/dist/SattoMDV/SattoMDV.exe")
$ErrorActionPreference = 'Stop'
$savedDataDir = $env:SATTOMDV_DATA_DIR
try {
    $env:SATTOMDV_DATA_DIR = Join-Path $PSScriptRoot ('.test-data/language-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Force $env:SATTOMDV_DATA_DIR | Out-Null
    $process = Start-Process $Executable -ArgumentList '--language-test' -WindowStyle Hidden -PassThru
    if (-not $process.WaitForExit(45000)) { Stop-Process -Id $process.Id; throw 'Language test timeout' }
    $errorFile = Join-Path $env:SATTOMDV_DATA_DIR 'language-error.txt'
    if (Test-Path $errorFile) { throw (Get-Content $errorFile -Raw) }
    $passFile = Join-Path $env:SATTOMDV_DATA_DIR 'language-pass.txt'
    if ($process.ExitCode -ne 0 -or -not (Test-Path $passFile)) { throw 'Language test failed' }
    Get-Content $passFile
    $restarted = Start-Process $Executable -ArgumentList '--smoke-test' -WindowStyle Hidden -PassThru
    if (-not $restarted.WaitForExit(20000)) { Stop-Process -Id $restarted.Id; throw 'Restart test timeout' }
    if ($restarted.ExitCode -ne 0) { throw 'Restart failed' }
    $result = Get-Content (Join-Path $env:SATTOMDV_DATA_DIR 'smoke.json') -Raw | ConvertFrom-Json
    $dom = $result.dom | ConvertFrom-Json
    if ($dom.heading -ne 'Read comfortably.' -or $dom.scripts -ne 0 -or $dom.overflow) { throw 'English restart/rendering failed' }
    Write-Host 'PASS: saved English language restored after process restart.'
} finally { $env:SATTOMDV_DATA_DIR = $savedDataDir }
