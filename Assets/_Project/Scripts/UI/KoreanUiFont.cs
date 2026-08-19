using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.UI
{
    public static class KoreanUiFont
    {
        static Font _cached;

        static readonly string[] OsCandidates =
        {
            "Malgun Gothic",
            "맑은 고딕",
            "MalgunGothic",
            "Apple SD Gothic Neo",
            "Noto Sans CJK KR",
            "NanumGothic",
            "Arial Unicode MS"
        };

        public static Font Get()
        {
            if (_cached != null) return _cached;

            // Bundled project font (Resources/Fonts/UIHangul) — preferred for builds.
            _cached = Resources.Load<Font>("Fonts/UIHangul");
            if (_cached != null)
            {
                Debug.Log($"[IdleMvp] UI Hangul font (bundled): {_cached.name}");
                return _cached;
            }

            _cached = Font.CreateDynamicFontFromOSFont(OsCandidates, 32);
            if (_cached == null)
                Debug.LogWarning("[IdleMvp] OS Hangul font not found.");
            else
                Debug.Log($"[IdleMvp] UI Hangul font (OS): {_cached.name}");
            return _cached;
        }

        public static void Apply(Text text)
        {
            if (text == null) return;
            var font = Get();
            if (font != null)
                text.font = font;
        }
    }
}
