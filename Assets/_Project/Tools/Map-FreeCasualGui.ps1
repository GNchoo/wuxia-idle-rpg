# Maps Free Casual GUI (Asset Store) PNGs into FreePack/CasualGui slot names.
# Run AFTER importing the pack via Package Manager.
# Usage (from project root):
#   powershell -File Assets/_Project/Tools/Map-FreeCasualGui.ps1

$ErrorActionPreference = "Stop"
$root = Split-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) -Parent
if (-not (Test-Path (Join-Path $root "Assets"))) {
  $root = "H:\Game\IdleRPG\NewRPG"
}

$dstUi = Join-Path $root "Assets\_Project\Resources\FreePack\UI"
$dstCg = Join-Path $root "Assets\_Project\Resources\CasualGui"
$dstGa = Join-Path $root "Assets\_Project\Resources\GrowArt"
New-Item -ItemType Directory -Force -Path $dstUi, $dstCg | Out-Null

# Search common import folder names
$candidates = @(
  "Assets\Free Casual GUI",
  "Assets\FreeCasualGUI",
  "Assets\CasualGui",
  "Assets\UncoGames\Free Casual GUI",
  "Assets\_ThirdParty\Free Casual GUI"
) | ForEach-Object { Join-Path $root $_ }

$srcRoot = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $srcRoot) {
  $found = Get-ChildItem (Join-Path $root "Assets") -Directory -Recurse -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match 'Casual|Boba|Free.?GUI' } |
    Select-Object -First 1 -ExpandProperty FullName
  $srcRoot = $found
}

if (-not $srcRoot) {
  Write-Host @"
[Map-FreeCasualGui] Free Casual GUI folder not found under Assets/.

1) Browser: https://assetstore.unity.com/packages/2d/gui/free-casual-gui-332804
2) Add to My Assets (same Unity account)
3) Unity: Window > Package Manager > My Assets > Download > Import
4) Re-run this script.
"@
  exit 1
}

Write-Host "Source: $srcRoot"
$pngs = Get-ChildItem $srcRoot -Recurse -Filter *.png
Write-Host "Found $($pngs.Count) PNGs"

function Pick([string]$pattern) {
  $pngs | Where-Object { $_.Name -match $pattern } | Sort-Object Length -Descending | Select-Object -First 1
}

function CopySlot($file, [string]$slotName) {
  if (-not $file) { Write-Host "  skip $slotName"; return }
  Copy-Item $file.FullName (Join-Path $dstUi $slotName) -Force
  Copy-Item $file.FullName (Join-Path $dstCg $slotName) -Force
  Copy-Item $file.FullName (Join-Path $dstGa $slotName) -Force
  Write-Host "  $slotName <- $($file.Name)"
}

CopySlot (Pick 'panel|window|frame|card') "PanelFrame.png"
CopySlot (Pick 'modal|popup|dialog') "ModalFrame.png"
CopySlot (Pick 'button.*(orange|yellow|primary)|btn.*(orange|yellow)') "CtaButton.png"
CopySlot (Pick 'button.*(orange|yellow|primary)|btn.*(orange|yellow)') "UpgradeButton.png"
CopySlot (Pick 'bar.*(empty|bg|track)|progress.*bg|slider.*bg') "BarEmpty.png"
CopySlot (Pick 'bar.*(fill|fg)|progress.*fill') "BarFill.png"
CopySlot (Pick 'slot|inventory|item.?frame') "InvSlot.png"
CopySlot (Pick 'slot|card|shop') "ShopCard.png"
CopySlot (Pick 'close|x_icon|icon.?close') "IconClose.png"

Write-Host "Done. Reimport in Unity if needed, then Play."
