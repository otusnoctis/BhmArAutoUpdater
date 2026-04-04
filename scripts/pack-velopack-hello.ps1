param(
    [string]$Version = "0.1.0"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "VelopackHello\VelopackHello.csproj"
$publishDir = Join-Path $repoRoot "artifacts\VelopackHello\publish"
$releaseDir = Join-Path $repoRoot "artifacts\VelopackHello\releases"
$propsPath = Join-Path $repoRoot "Directory.Build.props"
[xml]$props = Get-Content $propsPath
$velopackVersion = $props.Project.PropertyGroup.VelopackVersion

Write-Host "Publishing VelopackHello $Version..."
dotnet publish $projectPath `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:VelopackPackOnPublish=false `
    -o $publishDir

$env:DOTNET_ROLL_FORWARD = "Major"

Write-Host "Packing Velopack release..."
dnx --yes vpk --version $velopackVersion pack `
    --packId "VelopackHello" `
    --packVersion $Version `
    --packDir $publishDir `
    --mainExe "VelopackHello.exe" `
    --packTitle "Velopack Hello" `
    --packAuthors "otusnoctis" `
    --runtime "win-x64" `
    --outputDir $releaseDir

Write-Host "Velopack packages created in $releaseDir"
