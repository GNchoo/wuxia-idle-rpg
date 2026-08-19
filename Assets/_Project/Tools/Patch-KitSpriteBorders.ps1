# Patches Fantasy Idle UI Kit sprite .meta files with real 9-slice borders
# (kit ships with none) and FullRect sprite meshes. Mirrors FantasyKitImporterFix.cs
# for when the Unity editor is open and batchmode cannot run.
$root = "H:\Game\IdleRPG\NewRPG\Assets\FantasyIdleGameGUI\Resources\Sprites"

# border = left, bottom, right, top (meta: x, y, z, w)
$borders = @{
    'Buttons\Btn_Mint.png'          = @(35, 35, 35, 35)
    'Buttons\Btn_Green.png'         = @(35, 35, 35, 35)
    'Buttons\Btn_Red.png'           = @(35, 35, 35, 35)
    'Buttons\Btn_Round_Gray.png'    = @(35, 25, 35, 25)
    'Buttons\Btn_Violet_S.png'      = @(35, 25, 35, 25)
    'Popups\Popup_Bg.png'           = @(48, 48, 48, 48)
    'Popups\Popup_Title.png'        = @(54, 44, 54, 44)
    'Frame\Frame_List_Gray.png'     = @(30, 30, 30, 30)
    'Frame\Frame_List_Redbrown.png' = @(30, 30, 30, 30)
    'Frame\Frame_List_Yellow.png'   = @(30, 30, 30, 30)
    'Frame\Frame_Round_Black.png'   = @(40, 30, 40, 30)
    'Frame\Frame_Text_01.png'       = @(30, 20, 30, 20)
    'Frame\Frame_Green_Text.png'    = @(25, 22, 25, 22)
    'Frame\Frame_Img.png'           = @(35, 35, 35, 35)
    'Frame\Frame_Cyan.png'          = @(22, 20, 22, 20)
    'Frame\Frame_Stage.png'         = @(55, 40, 55, 40)
    'Frame\Frame_Edge_Gray.png'     = @(40, 40, 40, 40)
    'Frame\Frame_Edge_Green.png'    = @(40, 40, 40, 40)
    'Frame\Frame_Edge_Blue.png'     = @(40, 40, 40, 40)
    'Frame\Frame_Edge_Violet.png'   = @(40, 40, 40, 40)
    'Frame\Frame_Edge_Yellow.png'   = @(40, 40, 40, 40)
    'Frame\Frame_Edge_Red.png'      = @(40, 40, 40, 40)
    'Frame\Frame_Edge_None.png'     = @(40, 40, 40, 40)
    'Frame\Frame_Edge_Selete.png'   = @(40, 40, 40, 40)
    'Components\Tab_On.png'         = @(35, 30, 35, 30)
    'Components\Tab_Off.png'        = @(35, 30, 35, 30)
    'Components\Bar_Back.png'       = @(15, 15, 15, 15)
    'Components\Bar_Back_S.png'     = @(12, 12, 12, 12)
    'Components\Bar_Front.png'      = @(12, 12, 12, 12)
}

$done = 0
foreach ($key in $borders.Keys) {
    $meta = Join-Path $root ($key + '.meta')
    if (-not (Test-Path $meta)) { Write-Warning "Missing: $meta"; continue }
    $b = $borders[$key]
    $borderLine = "spriteBorder: {x: $($b[0]), y: $($b[1]), z: $($b[2]), w: $($b[3])}"
    $text = Get-Content $meta -Raw
    $text = $text -replace 'spriteBorder: \{x: [^\}]+\}', $borderLine
    $text = $text -replace 'spriteMeshType: \d', 'spriteMeshType: 0'
    Set-Content -Path $meta -Value $text -NoNewline
    Write-Output "$key -> border ($($b -join ', '))"
    $done++
}
Write-Output "Patched $done meta files."
