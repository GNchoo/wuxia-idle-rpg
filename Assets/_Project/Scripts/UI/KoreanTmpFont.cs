using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace IdleMvp.UI
{
    /// <summary>
    /// Runtime TMP Hangul from bundled Resources/Fonts/UIHangul (fallback: OS).
    /// Prefer KoreanUiFont + uGUI Text for Maple HUD until TMP materials are validated in Play.
    /// </summary>
    public static class KoreanTmpFont
    {
        static TMP_FontAsset _cached;

        static readonly string[] OsCandidates =
        {
            "Malgun Gothic",
            "맑은 고딕",
            "MalgunGothic",
            "Apple SD Gothic Neo",
            "Noto Sans CJK KR",
            "Noto Sans KR",
            "NanumGothic",
            "Arial Unicode MS"
        };

        public static TMP_FontAsset Get()
        {
            if (_cached != null) return _cached;

            Font source = Resources.Load<Font>("Fonts/UIHangul");
            if (source == null)
                source = Font.CreateDynamicFontFromOSFont(OsCandidates, 64);
            if (source == null)
            {
                Debug.LogWarning("[IdleMvp] No Hangul font for TMP.");
                return null;
            }

            _cached = TMP_FontAsset.CreateFontAsset(
                source,
                72,
                8,
                GlyphRenderMode.SDFAA,
                2048,
                2048,
                AtlasPopulationMode.Dynamic,
                true);

            if (_cached != null)
            {
                _cached.name = "IdleMvp_Hangul_Dynamic_SDF";
                _cached.isMultiAtlasTexturesEnabled = true;
                Debug.Log($"[IdleMvp] Korean TMP SDF ready from '{source.name}'.");
            }

            return _cached;
        }

        public static void Apply(TextMeshProUGUI tmp)
        {
            if (tmp == null) return;
            var font = Get();
            if (font != null)
            {
                tmp.font = font;
                tmp.fontSharedMaterial = font.material;
            }
        }
    }
}
