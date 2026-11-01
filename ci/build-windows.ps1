param([string]$OutputDirectory, [switch]$SmokeTest, [int]$SmokeSeconds = 60, [string]$WindowsMetadata, [switch]$SpanishTest)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
foreach ($catalogName in @('es-ES.tsv', 'es-ES.fragments.tsv')) {
    $catalogKeys = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($catalogLine in [IO.File]::ReadAllLines((Join-Path $root ('locales\' + $catalogName)))) {
        if (-not $catalogLine -or $catalogLine.StartsWith('#')) { continue }
        $tabAt = $catalogLine.IndexOf("`t")
        if ($tabAt -lt 1) { throw "Invalid translation line in $catalogName" }
        if (-not $catalogKeys.Add($catalogLine.Substring(0, $tabAt))) { throw "Duplicate translation in ${catalogName}: $($catalogLine.Substring(0, $tabAt))" }
    }
}
$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$compiler = Join-Path $framework 'csc.exe'
$wpf = Join-Path $framework 'WPF'
$sdkMetadata = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\UnionMetadata'
$winmd = if ($WindowsMetadata) { (Resolve-Path -LiteralPath $WindowsMetadata).Path } elseif (Test-Path -LiteralPath $sdkMetadata) { Get-ChildItem -LiteralPath $sdkMetadata -Directory | Where-Object { $_.Name -match '^\d+(\.\d+)+$' } | Sort-Object { [version]$_.Name } -Descending | ForEach-Object { Join-Path $_.FullName 'Windows.winmd' } | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1 }
if (-not (Test-Path -LiteralPath $compiler)) { throw 'csc.exe not found. Capri needs the .NET Framework 4.8 that comes with Windows 10 and 11.' }
if (-not $winmd) { throw 'Windows.winmd not found. Install the Windows 10 or 11 SDK.' }
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) { $OutputDirectory = Join-Path $root 'out\windows' }
$out = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $out -Force | Out-Null
$app = Join-Path $out 'Capri.exe'
$setup = Join-Path $out 'Capri-Setup.exe'
$icon = Join-Path $root 'assets\chip.ico'
$shared = @('/nologo', '/target:winexe', '/platform:anycpu', '/optimize+', '/utf8output', '/warn:4', ('/win32manifest:' + (Join-Path $root 'app\app.manifest')), ('/win32icon:' + $icon), '/reference:System.dll', '/reference:System.Core.dll', '/reference:System.Drawing.dll', '/reference:System.Windows.Forms.dll')
$references = @('/reference:System.Runtime.Serialization.dll', '/reference:System.IO.Compression.dll', '/reference:System.IO.Compression.FileSystem.dll', '/reference:System.Security.dll', ('/reference:' + (Join-Path $wpf 'UIAutomationClient.dll')), ('/reference:' + (Join-Path $wpf 'UIAutomationTypes.dll')), ('/reference:' + (Join-Path $wpf 'WindowsBase.dll')), ('/reference:' + (Join-Path $wpf 'System.Speech.dll')), ('/reference:' + $winmd), ('/reference:' + (Join-Path $framework 'System.Runtime.dll')), ('/reference:' + (Join-Path $framework 'System.Runtime.WindowsRuntime.dll')), ('/reference:' + (Join-Path $framework 'System.Runtime.InteropServices.WindowsRuntime.dll')))
$resources = @(('/resource:' + $icon + ',Capri.Chip.ico'), ('/resource:' + (Join-Path $root 'assets\fonts\ShareTechMono-Regular.ttf') + ',Capri.Font.Mono.ttf'))
$resources += ('/resource:' + (Join-Path $root 'assets\cursor.png') + ',Capri.Cursor.png')
$localeResources = @('es-ES.tsv', 'es-ES.fragments.tsv') | ForEach-Object { '/resource:' + (Join-Path $root ('locales\' + $_)) + ',Capri.Locale.' + $_ }
$localeResources += '/resource:' + (Join-Path $root 'docs\es\ayuda.html') + ',Capri.Locale.Ayuda.html'
$resources += $localeResources
$resources += Get-ChildItem -LiteralPath (Join-Path $root 'assets\sprites') -Filter *.png | ForEach-Object { '/resource:' + $_.FullName + ',Capri.Sprite.' + $_.Name }
$resources += Get-ChildItem -LiteralPath (Join-Path $root 'assets\voice') -Filter *.wav | ForEach-Object { '/resource:' + $_.FullName + ',Capri.Voice.' + $_.Name }
$sources = @('Capri', 'Asides', 'Bolts', 'Commentary', 'Lookout', 'Brain', 'Talk', 'Mainframe', 'Neon', 'Notice', 'Engine', 'Ledges', 'Voice', 'Ears', 'Dictation', 'Agent', 'Hands', 'GameHud', 'Computer', 'Play', 'VoicePack', 'Startup', 'Presence', 'Player', 'Ocr', 'GameEars', 'GameNotes', 'GameKeys', 'Frames', 'Reminders', 'GameSense', 'GameWatch', 'SelfKnow', 'Care', 'Clips', 'Minis', 'Chess', 'ChessBoard', 'Files', 'FileFinder', 'Websites', 'Ink', 'FollowUps', 'WatchAlong', 'I18n', 'LocaleTests', 'AssemblyInfo') | ForEach-Object { Join-Path $root ('app\' + $_ + '.cs') }
& $compiler ($shared + $references + $resources + @('/unsafe+', ('/out:' + $app)) + $sources)
if ($LASTEXITCODE -ne 0) { throw 'Capri compilation failed.' }
& $compiler ($shared + $localeResources + @(('/out:' + $setup), '/reference:Microsoft.CSharp.dll', '/reference:System.IO.Compression.dll', '/reference:System.IO.Compression.FileSystem.dll', ('/resource:' + $app + ',Capri.App.exe'), ('/resource:' + $icon + ',Capri.Chip.ico'), ('/resource:' + (Join-Path $root 'assets\chip.png') + ',Capri.Chip.png'), (Join-Path $root 'app\Setup.cs'), (Join-Path $root 'app\Neon.cs'), (Join-Path $root 'app\Notice.cs'), (Join-Path $root 'app\VoicePack.cs'), (Join-Path $root 'app\Startup.cs'), (Join-Path $root 'app\I18n.cs'), (Join-Path $root 'app\AssemblyInfo.cs')))
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }
Write-Output "Built $app and $setup"
if ($SpanishTest) {
    $localeOutput = Join-Path $out 'spanish'
    $localeRun = Start-Process -FilePath $app -ArgumentList @('--test-spanish', ('"' + $localeOutput + '"')) -PassThru -WindowStyle Hidden
    if (-not $localeRun.WaitForExit(120000)) { $localeRun.Kill(); throw 'Spanish checks timed out.' }
    if (Test-Path (Join-Path $localeOutput 'resultado.txt')) { Get-Content (Join-Path $localeOutput 'resultado.txt') }
    if ($localeRun.ExitCode -ne 0) {
        if (Test-Path (Join-Path $localeOutput 'fallo.txt')) { Get-Content (Join-Path $localeOutput 'fallo.txt') }
        throw 'Spanish checks failed.'
    }
    $spanishPackage = Join-Path $out 'spanish-package'
    $packageRun = Start-Process -FilePath $setup -ArgumentList @('--verify', ('"' + $spanishPackage + '"')) -PassThru -WindowStyle Hidden
    if (-not $packageRun.WaitForExit(120000)) { $packageRun.Kill(); throw 'Installer verification timed out.' }
    if ($packageRun.ExitCode -ne 0) {
        if (Test-Path (Join-Path $spanishPackage 'failure.txt')) { Get-Content (Join-Path $spanishPackage 'failure.txt') }
        throw 'Installer verification failed.'
    }
    if ((Get-FileHash (Join-Path $spanishPackage 'Capri.exe')).Hash -ne (Get-FileHash $app).Hash) { throw 'Installer payload differs from the Spanish app.' }
    Write-Output 'PASS el instalador contiene exactamente la aplicación española'
}
if (-not $SmokeTest) { return }
$runtime = Join-Path $out 'smoke\runtime'
$package = Join-Path $out 'smoke\package'
$check = Start-Process -FilePath $app -ArgumentList @('--smoke-test', ('"' + $runtime + '"')) -PassThru -WindowStyle Hidden
if (-not $check.WaitForExit($SmokeSeconds * 1000)) { $check.Kill(); throw 'Runtime smoke test timed out.' }
if (Test-Path -LiteralPath (Join-Path $runtime 'smoke-test.txt')) { Get-Content -LiteralPath (Join-Path $runtime 'smoke-test.txt') }
elseif ($check.ExitCode -eq 0) { throw 'Runtime smoke test wrote no results. Another smoke test probably held the lock; rerun it when that one ends.' }
if ($check.ExitCode -ne 0) {
    if (Test-Path -LiteralPath (Join-Path $runtime 'failure.txt')) { Get-Content -LiteralPath (Join-Path $runtime 'failure.txt') }
    throw 'Runtime smoke test failed.'
}
$verify = Start-Process -FilePath $setup -ArgumentList @('--verify', ('"' + $package + '"')) -PassThru -WindowStyle Hidden
if (-not $verify.WaitForExit($SmokeSeconds * 1000)) { $verify.Kill(); throw 'Installer check timed out.' }
if ($verify.ExitCode -ne 0) {
    if (Test-Path -LiteralPath (Join-Path $package 'failure.txt')) { Get-Content -LiteralPath (Join-Path $package 'failure.txt') }
    throw 'Installer check failed.'
}
if ((Get-FileHash -LiteralPath (Join-Path $package 'Capri.exe')).Hash -ne (Get-FileHash -LiteralPath $app).Hash) { throw 'Installer payload differs from the app executable.' }
Get-Content -LiteralPath (Join-Path $package 'packaging-test.txt')
Write-Output 'PASS installer payload matches the app'

