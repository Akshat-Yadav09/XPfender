$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$scannerRoot = Join-Path $projectRoot 'Assets/Scripts/DesktopOverlay'
$sources = @(
    (Join-Path $PSScriptRoot 'UnityScannerStubs.cs'),
    (Join-Path $scannerRoot 'DesktopObject.cs'),
    (Join-Path $scannerRoot 'DesktopMatchRule.cs'),
    (Join-Path $scannerRoot 'DesktopCoordinateConverter.cs'),
    (Join-Path $scannerRoot 'WindowsShellInterop.cs'),
    (Join-Path $scannerRoot 'WindowsDesktopScanner.cs')
)
$unityData = 'C:/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Data'
$output = Join-Path $env:TEMP ('XPDefenderScanner-' + [Guid]::NewGuid().ToString('N') + '.dll')
$framework = [System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory()
$references = @('mscorlib.dll', 'System.dll', 'System.Core.dll') | ForEach-Object { '/r:' + (Join-Path $framework $_) }
& (Join-Path $unityData 'DotNetSdk/dotnet.exe') (Join-Path $unityData 'DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll') /nologo /target:library /out:$output @references @sources
if ($LASTEXITCODE -ne 0) { throw 'Scanner probe compilation failed.' }
Add-Type -Path $output
$objects = [WindowsDesktopScanner]::ScanDesktop()
Write-Output "TOTAL DESKTOP OBJECTS FOUND = $($objects.Count)"
foreach ($desktopObject in $objects) { Write-Output ($desktopObject.ToString() + ', Identity: ' + $desktopObject.shellIdentity + ', Source: ' + $desktopObject.sourcePath) }
$rules = @([DesktopObjectType]::ThisPC, [DesktopObjectType]::RecycleBin, [DesktopObjectType]::FileExplorer, [DesktopObjectType]::Shortcut) | ForEach-Object {
    $rule = New-Object DesktopMatchRule
    $rule.Type = $_
    $rule
}
$matchedCount = 0
foreach ($desktopObject in $objects) {
    foreach ($rule in $rules) { if ($rule.Matches($desktopObject)) { $matchedCount++; break } }
}
Write-Output "MATCHED OBJECTS = $matchedCount"
$browser = New-Object DesktopObject
$browser.type = [DesktopObjectType]::Browser
$browser.isShortcut = $true
if (!$rules[-1].Matches($browser)) { throw 'Browser shortcut matching failed' }
$point = [DesktopCoordinateConverter]::PhysicalToUnityScreen(
    (New-Object UnityEngine.Vector2 -ArgumentList -1820, 150),
    (New-Object UnityEngine.Rect -ArgumentList -1920, 50, 1920, 1080),
    (New-Object UnityEngine.Vector2 -ArgumentList 960, 540))
if ($point.x -ne 50 -or $point.y -ne 490) { throw 'Negative-monitor origin / scaled render conversion failed' }
Write-Output 'PASS: Browser shortcuts match Shortcut rules; negative monitor origin and scaled render conversion'
