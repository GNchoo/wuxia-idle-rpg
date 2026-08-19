using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.UI
{
    /// <summary>
    /// Shared layout helpers + legacy palette. Maple screens use MapleUiTheme colors.
    /// </summary>
    public static class GrowTheme
    {
        public static readonly Color Sky = new Color(0.45f, 0.62f, 0.78f, 1f);
        public static readonly Color Cream = new Color(0.97f, 0.98f, 0.99f, 1f);
        public static readonly Color Panel = new Color(0.97f, 0.98f, 0.99f, 0.97f);
        public static readonly Color Accent = new Color(1f, 0.55f, 0.18f, 1f);
        public static readonly Color AccentDark = new Color(0.20f, 0.65f, 0.72f, 1f);
        public static readonly Color TextDark = new Color(0.15f, 0.18f, 0.22f, 1f);
        public static readonly Color TabIdle = new Color(0.22f, 0.24f, 0.28f, 0.9f);
        public static readonly Color TabActive = new Color(0.25f, 0.72f, 0.85f, 1f);
        public static readonly Color Hero = new Color(0.25f, 0.72f, 0.85f, 1f);
        public static readonly Color Enemy = new Color(0.85f, 0.35f, 0.35f, 1f);
        public static readonly Color HpBar = new Color(0.95f, 0.28f, 0.38f, 1f);
        public static readonly Color Good = new Color(0.72f, 0.82f, 0.15f, 1f);

        public static Text MakeText(Transform parent, string name, int size, FontStyle style, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            KoreanUiFont.Apply(t);
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Image MakeImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        public static Button MakeButton(Transform parent, string name, string label, Color bg)
        {
            var img = MakeImage(parent, name, bg);
            var btn = img.gameObject.AddComponent<Button>();
            var text = MakeText(img.transform, "Label", 22, FontStyle.Bold, Cream);
            text.text = label;
            Stretch(text.rectTransform);
            text.raycastTarget = false;
            return btn;
        }

        public static void Stretch(RectTransform rt, float pad = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(pad, pad);
            rt.offsetMax = new Vector2(-pad, -pad);
        }

        public static void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }
    }
}
