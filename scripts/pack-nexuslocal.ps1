param(
    [string]$Version = "0.1.0"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "NexusLocal\NexusLocal.csproj"
$publishDir = Join-Path $repoRoot "artifacts\NexusLocal\publish"
$releaseDir = Join-Path $repoRoot "artifacts\NexusLocal\releases"
$propsPath = Join-Path $repoRoot "Directory.Build.props"
[xml]$props = Get-Content $propsPath
$velopackVersion = $props.Project.PropertyGroup.VelopackVersion

New-Item -ItemType Directory -Path $publishDir -Force | Out-Null
New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null
Get-ChildItem $releaseDir -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -in @("assets.win.json", "RELEASES", "releases.win.json", "NexusLocal-win-Setup.exe", "NexusLocal-win-Portable.zip") } |
    Remove-Item -Force

Write-Host "Publishing NexusLocal $Version..."
dotnet publish $projectPath `
    -c Release `
    -f net10.0-windows10.0.19041.0 `
    -r win-x64 `
    --self-contained true `
    -p:VelopackPackOnPublish=false `
    -o $publishDir

$env:DOTNET_ROLL_FORWARD = "Major"

Write-Host "Packing NexusLocal release..."
dnx --yes vpk --version $velopackVersion pack `
    --packId "NexusLocal" `
    --packVersion $Version `
    --packDir $publishDir `
    --mainExe "NexusLocal.exe" `
    --packTitle "NexusLocal" `
    --packAuthors "otusnoctis" `
    --runtime "win-x64" `
    --noPortable `
    --outputDir $releaseDir

Write-Host "Velopack packages created in $releaseDir"
