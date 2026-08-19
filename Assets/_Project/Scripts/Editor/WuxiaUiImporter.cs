using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IdleMvp.EditorTools
{
    /// <summary>
    /// Resources/WuxiaUi/*.png 를 9-slice 스프라이트로 임포트한다.
    /// 보더는 생성 아트의 테두리 두께 실측값 — 새 에셋을 추가하면 표에 한 줄 추가.
    /// </summary>
    public static class WuxiaUiImporter
    {
        const string Dir = "Assets/_Project/Resources/WuxiaUi";

        // name → border (left, bottom, right, top)
        static readonly Dictionary<string, Vector4> Borders = new Dictionary<string, Vector4>
        {
            { "panel_hanji", new Vector4(52, 52, 52, 52) },
            { "panel_dark", new Vector4(44, 44, 44, 44) },
            { "btn_primary", new Vector4(30, 24, 30, 24) },
            { "btn_secondary", new Vector4(30, 24, 30, 24) },
            { "header_cloud", new Vector4(165, 4, 165, 4) },
            { "frame_gold", new Vector4(80, 80, 80, 80) },
            { "tab_on", new Vector4(22, 18, 22, 18) },
            { "tab_off", new Vector4(22, 18, 22, 18) },
            { "bar_bg", new Vector4(16, 14, 16, 14) },
            { "bar_fill", new Vector4(14, 12, 14, 12) },
            { "row_dark", new Vector4(22, 22, 22, 22) },
            { "chip_gold", new Vector4(18, 18, 18, 18) },
            { "slot_frame", new Vector4(28, 28, 28, 28) },
            { "slot_empty", new Vector4(24, 24, 24, 24) },
            { "wood_board", new Vector4(96, 96, 96, 96) },
            { "btn_dark", new Vector4(36, 30, 36, 30) },
            { "row_dim", new Vector4(24, 16, 24, 16) },
            { "slot_dark", new Vector4(26, 26, 26, 26) },
            { "panel_dg", new Vector4(48, 48, 48, 48) },
            { "paper_sheet", new Vector4(48, 48, 48, 48) },
            { "btn_paper", new Vector4(46, 40, 46, 40) },
            // 구매 킷 버튼(세로형)을 가로로 늘려 쓴다 — 원본 보더 그대로
            { "kit_btn_upgrade", new Vector4(32, 28, 32, 146) },
            { "kit_btn_off", new Vector4(32, 28, 32, 146) },
            // 종이 한 장을 9-slice로 늘려 모든 쪽지를 만든다 (색·질감이 항상 같다)
            { "note_paper", new Vector4(42, 46, 62, 42) },
            // 창 조립 키트 — 한지 한 장 + 먹선 부품. 어떤 크기의 창이든 같은 결로 늘어난다.
            // 종이는 찢긴 가장자리 두께만큼, 먹칸은 모서리 장식 크기만큼 보더를 준다.
            { "kit_paper_sheet", new Vector4(70, 70, 70, 70) },
            { "kit_ink_panel", new Vector4(46, 46, 46, 46) },
        };

        [MenuItem("IdleMvp/아트/무협 UI 임포트 설정", priority = 123)]
        public static void Apply()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { Dir });
            int n = 0;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti == null) continue;
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                var ts = new TextureImporterSettings();
                ti.ReadTextureSettings(ts);
                ts.textureType = TextureImporterType.Sprite;
                ts.spriteMode = (int)SpriteImportMode.Single;
                ts.spritePixelsPerUnit = 100f;
                ts.alphaIsTransparency = true;
                if (Borders.TryGetValue(name, out var b)) ts.spriteBorder = b;
                ti.SetTextureSettings(ts);
                ti.SaveAndReimport();
                n++;
            }
            Debug.Log("[WuxiaUi] 임포트 설정 " + n + "장 완료");
        }
    }
}
