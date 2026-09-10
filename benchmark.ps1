param([string]$Executable = "$PSScriptRoot/dist/SattoMDV/SattoMDV.exe", [string]$Label = 'after', [int]$Runs = 5)
$ErrorActionPreference = 'Stop'
$saved = $env:SATTOMDV_DATA_DIR
$results = @()
try {
 foreach ($case in @('small','large')) {
  $env:SATTOMDV_DATA_DIR = "$PSScriptRoot/.test-data/benchmark-$Label-$case"
  New-Item -ItemType Directory -Force $env:SATTOMDV_DATA_DIR | Out-Null
  '{}' | Set-Content "$env:SATTOMDV_DATA_DIR/settings.json"
  $document = if ($case -eq 'small') { "$PSScriptRoot/samples/reading.md" } else { "$PSScriptRoot/.test-data/large.md" }
  for ($i=0; $i -le $Runs; $i++) {
   $p = Start-Process -FilePath $Executable -ArgumentList @('--benchmark',('"'+$document+'"')) -WindowStyle Hidden -PassThru
   if (-not $p.WaitForExit(20000)) { Stop-Process -Id $p.Id; throw 'Benchmark timeout' }
   if ($p.ExitCode -ne 0) { throw 'Benchmark process failed' }
   $data = Get-Content "$env:SATTOMDV_DATA_DIR/benchmark.json" -Raw | ConvertFrom-Json
   $results += [pscustomobject]@{ label=$Label; case=$case; run=$i; firstLaunch=($i -eq 0); processToPaintMs=$data.processToPaintMs; readyMs=$data.readyMs; privateBytes=$data.privateBytes }
  }
 }
 $results | ConvertTo-Json | Set-Content "$PSScriptRoot/.test-data/benchmark-$Label.json"
 $results | Format-Table -AutoSize
} finally { $env:SATTOMDV_DATA_DIR=$saved }

