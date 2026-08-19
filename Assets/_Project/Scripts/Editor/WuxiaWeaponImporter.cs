using UnityEditor;
using UnityEngine;

namespace IdleMvp.EditorTools
{
    /// <summary>
    /// Resources/WuxiaWeapons/*.png 를 Hippo 무기 규격으로 임포트한다:
    /// Sprite(Single), Custom 피벗(0.5, 0.25)=손잡이, PPU 100 (SamuraiSword1 실측 규격).
    /// spritePivot은 spriteAlignment=Custom일 때만 적용된다 — HeroTall에서 이미 겪은 함정.
    /// </summary>
    public static class WuxiaWeaponImporter
    {
        const string Dir = "Assets/_Project/Resources/WuxiaWeapons";

        [MenuItem("IdleMvp/아트/무협 무기 임포트 설정", priority = 122)]
        public static void Apply()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { Dir });
            int n = 0;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti == null) continue;
                var ts = new TextureImporterSettings();
                ti.ReadTextureSettings(ts);
                ts.textureType = TextureImporterType.Sprite;
                ts.spriteMode = (int)SpriteImportMode.Single;
                ts.spriteAlignment = (int)SpriteAlignment.Custom;
                ts.spritePivot = new Vector2(0.5f, 0.25f);
                ts.spritePixelsPerUnit = 100f;
                ts.alphaIsTransparency = true;
                ti.SetTextureSettings(ts);
                ti.SaveAndReimport();
                n++;
            }
            Debug.Log("[WuxiaWeapon] 임포트 설정 " + n + "장 완료");
        }
    }
}
