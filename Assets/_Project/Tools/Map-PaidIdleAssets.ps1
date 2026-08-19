# Maps Mobile Fantasy Idle UI Kit PNGs into GrowArt / CasualGui slot names.
# Run AFTER importing FantasyIdleGameGUI via Package Manager.
# Usage (from project root):
#   powershell -File Assets/_Project/Tools/Map-PaidIdleAssets.ps1

$ErrorActionPreference = "Stop"
$root = Split-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) -Parent
if (-not (Test-Path (Join-Path $root "Assets"))) {
  $root = "H:\Game\IdleRPG\NewRPG"
}

$srcRoot = Join-Path $root "Assets\FantasyIdleGameGUI\Resources\Sprites"
if (-not (Test-Path $srcRoot)) {
  Write-Host "[Map-PaidIdleAssets] FantasyIdleGameGUI not found at $srcRoot"
  exit 1
}

$dstGa = Join-Path $root "Assets\_Project\Resources\GrowArt"
$dstCg = Join-Path $root "Assets\_Project\Resources\CasualGui"
$dstUi = Join-Path $root "Assets\_Project\Resources\FreePack\UI"
New-Item -ItemType Directory -Force -Path $dstGa, $dstCg, $dstUi, (Join-Path $dstGa "Chars") | Out-Null

Write-Host "Source: $srcRoot"

function Rel([string]$rel) {
  $p = Join-Path $srcRoot $rel
  if (-not (Test-Path $p)) { Write-Host "  missing $rel"; return $null }
  return Get-Item $p
}

function CopySlot($file, [string]$slotName, [string]$sub = "") {
  if (-not $file) { Write-Host "  skip $slotName"; return }
  $targets = @($dstGa, $dstCg, $dstUi)
  foreach ($base in $targets) {
    $dir = if ($sub) { Join-Path $base $sub } else { $base }
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    Copy-Item $file.FullName (Join-Path $dir $slotName) -Force
  }
  Write-Host "  $slotName <- $($file.Name)"
}

# Panels / modals
CopySlot (Rel "Popups\Popup_Bg.png") "PanelFrame.png"
CopySlot (Rel "Popups\Popup_Bg.png") "ModalFrame.png"
CopySlot (Rel "Frame\Frame_Cyan.png") "ModalInner.png"
CopySlot (Rel "Popups\Popup_Title.png") "TopFog.png"

# Buttons
CopySlot (Rel "Buttons\Btn_Mint.png") "CtaButton.png"
CopySlot (Rel "Buttons\Btn_Green.png") "UpgradeButton.png"
CopySlot (Rel "Popups\Btn_X.png") "IconClose.png"

# Bars / slots
CopySlot (Rel "Components\Bar_Back.png") "BarEmpty.png"
CopySlot (Rel "Components\Bar_Front.png") "BarFill.png"
CopySlot (Rel "Frame\Frame_List_Gray.png") "InvSlot.png"
CopySlot (Rel "Frame\Frame_List_Yellow.png") "ShopCard.png"
CopySlot (Rel "Frame\Frame_List_Gray.png") "SquareFrame.png"
CopySlot (Rel "Frame\Frame_Profile.png") "CircleFrame.png"
CopySlot (Rel "Frame\Frame_Profile.png") "SkillCircle.png"
CopySlot (Rel "UI_etcs\Bg_Menu_Bar.png") "BottomBar.png"
CopySlot (Rel "UI_etcs\Bg_Menu.png") "BottomBarAlt.png"

# Currency
CopySlot (Rel "Icons\Icon_Gold.png") "IconGold.png"
CopySlot (Rel "Icons\Icon_Gem.png") "IconGem.png"
CopySlot (Rel "Icons\Icon_Boss.png") "IconCp.png"

# Field / battle
CopySlot (Rel "Images\Bg_Ingame.png") "BattleBg.png"

# Bottom nav (same art for On/Off; HUD tints selection via underline)
$nav = @(
  @{ i = 1; f = "Icons\Menu_Chr.png" },
  @{ i = 2; f = "Icons\Menu_Ally.png" },
  @{ i = 3; f = "Icons\Menu_Skill.png" },
  @{ i = 4; f = "Icons\Menu_Gear.png" },
  @{ i = 5; f = "Icons\Menu_Store.png" }
)
foreach ($n in $nav) {
  $file = Rel $n.f
  CopySlot $file ("Nav{0}On.png" -f $n.i)
  CopySlot $file ("Nav{0}Off.png" -f $n.i)
}

# Skill icons
$skills = @(
  "Icons\Skill_LightningFang.png",
  "Icons\Skill_HeavenfallBlade.png",
  "Icons\Skill_DragonsWrath.png",
  "Icons\Skill_WindsofHaste.png",
  "Icons\Skill_JudgementBreaker.png"
)
for ($i = 0; $i -lt $skills.Count; $i++) {
  CopySlot (Rel $skills[$i]) ("SkillIcon{0}.png" -f ($i + 1))
}

# ---- Deep mapping: GrowArt only (Icon subfolder + frames) ----
function CopyGa($file, [string]$slotName, [string]$sub = "") {
  if (-not $file) { Write-Host "  skip $slotName"; return }
  $dir = if ($sub) { Join-Path $dstGa $sub } else { $dstGa }
  New-Item -ItemType Directory -Force -Path $dir | Out-Null
  Copy-Item $file.FullName (Join-Path $dir $slotName) -Force
  Write-Host "  GrowArt/$sub/$slotName <- $($file.Name)"
}

# Stat icons (growth modal)
CopyGa (Rel "Icons\Growth_STR.png") "StatStr.png" "Icon"
CopyGa (Rel "Icons\Growth_DEX.png") "StatDex.png" "Icon"
CopyGa (Rel "Icons\Growth_INT.png") "StatInt.png" "Icon"
CopyGa (Rel "Icons\Growth_VIT.png") "StatVit.png" "Icon"

# Enhance icons (equipment/upgrade rows)
CopyGa (Rel "Icons\Enhance_Attack.png") "EnhanceAttack.png" "Icon"
CopyGa (Rel "Icons\Enhance_AttackSpeed.png") "EnhanceAttackSpeed.png" "Icon"
CopyGa (Rel "Icons\Enhance_HP.png") "EnhanceHp.png" "Icon"
CopyGa (Rel "Icons\Enhance_HPRegen.png") "EnhanceHpRegen.png" "Icon"
CopyGa (Rel "Icons\Enhance_Accuracy.png") "EnhanceAccuracy.png" "Icon"

# Common UI icons
CopyGa (Rel "Icons\Icon_Setting.png") "Setting.png" "Icon"
CopyGa (Rel "Icons\Icon_Quest.png") "Quest.png" "Icon"
CopyGa (Rel "Icons\Icon_Mail.png") "Mail.png" "Icon"
CopyGa (Rel "Icons\Icon_Auto.png") "Auto.png" "Icon"
CopyGa (Rel "Icons\Icon_Boss.png") "Boss.png" "Icon"
CopyGa (Rel "Icons\Icon_Check.png") "Check.png" "Icon"
CopyGa (Rel "Icons\Icon_Lock.png") "Lock.png" "Icon"
CopyGa (Rel "Icons\Icon_Plus.png") "Plus.png" "Icon"

# Frames / tabs / badges
CopyGa (Rel "Components\Tab_On.png") "TabOn.png"
CopyGa (Rel "Components\Tab_Off.png") "TabOff.png"
CopyGa (Rel "Frame\Frame_Stage.png") "StageFrame.png"
CopyGa (Rel "Popups\Popup_Title.png") "PopupTitle.png"
CopyGa (Rel "UI_etcs\Badge_New.png") "BadgeNew.png"
CopyGa (Rel "UI_etcs\Bg_Skill_Set.png") "SkillDockBg.png"

# Buttons (full set for themed widgets)
CopyGa (Rel "Buttons\Btn_Round_Gray.png") "BtnNeutral.png"
CopyGa (Rel "Buttons\Btn_Red.png") "BtnDanger.png"
CopyGa (Rel "Buttons\Btn_Violet_S.png") "BtnAlt.png"

# Cards / chips / icon frames
CopyGa (Rel "Frame\Frame_Round_Black.png") "CardDark.png"
CopyGa (Rel "Frame\Frame_Text_01.png") "ChipFrame.png"
CopyGa (Rel "Frame\Frame_Img.png") "IconFrame.png"
CopyGa (Rel "UI_etcs\Bg_Tab.png") "TabStrip.png"

# Rarity edge frames (grade 0..5)
CopyGa (Rel "Frame\Frame_Edge_Gray.png") "Rarity0.png"
CopyGa (Rel "Frame\Frame_Edge_Green.png") "Rarity1.png"
CopyGa (Rel "Frame\Frame_Edge_Blue.png") "Rarity2.png"
CopyGa (Rel "Frame\Frame_Edge_Violet.png") "Rarity3.png"
CopyGa (Rel "Frame\Frame_Edge_Yellow.png") "Rarity4.png"
CopyGa (Rel "Frame\Frame_Edge_Red.png") "Rarity5.png"

Write-Host "Done UI mapping. Hero PNG is baked via Unity menu: IdleMvp > Apply Paid Character Maker Hero"
