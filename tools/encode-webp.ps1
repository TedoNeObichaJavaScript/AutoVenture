<#
.SYNOPSIS
    Encodes the PNG fleet imagery in wwwroot/carsphoto to WebP (+ optional @2x).

.DESCRIPTION
    The _CarImage partial automatically serves a <source type="image/webp">
    whenever a matching file exists at carsphoto/webp/<name>.webp, falling back
    to the original PNG otherwise. Run this once (and after adding new car
    images) to generate those WebP variants.

    Requires the cwebp encoder (from Google's libwebp) on PATH:
        winget install Google.libwebp      # or: choco install webp

.EXAMPLE
    pwsh tools/encode-webp.ps1
    pwsh tools/encode-webp.ps1 -Quality 78
#>
[CmdletBinding()]
param(
    [int]$Quality = 80
)

$ErrorActionPreference = 'Stop'

$root   = Split-Path -Parent $PSScriptRoot
$srcDir = Join-Path $root 'AutoVenture/wwwroot/carsphoto'
$outDir = Join-Path $srcDir 'webp'

if (-not (Get-Command cwebp -ErrorAction SilentlyContinue)) {
    Write-Error "cwebp not found on PATH. Install libwebp (winget install Google.libwebp) and retry."
}
if (-not (Test-Path $srcDir)) {
    Write-Error "Source folder not found: $srcDir"
}
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

$pngs = Get-ChildItem -Path $srcDir -Filter *.png -File
if (-not $pngs) { Write-Host "No PNGs to encode."; return }

foreach ($png in $pngs) {
    $name = [IO.Path]::GetFileNameWithoutExtension($png.Name)
    $out  = Join-Path $outDir "$name.webp"
    cwebp -quiet -q $Quality $png.FullName -o $out
    Write-Host "encoded -> carsphoto/webp/$name.webp"
}

Write-Host "Done. $($pngs.Count) image(s) encoded at q$Quality."
