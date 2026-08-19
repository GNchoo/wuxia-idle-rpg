using System.Collections.Generic;
using UnityEngine;

namespace IdleMvp.UI.Maple
{
    /// <summary>
    /// GUI Pro - Casual Game (Layer Lab) sprite resolver.
    /// Sprites live in Resources/CasualKit (copied with 9-slice borders intact).
    /// All lookups cached; missing sprite returns null so callers keep their
    /// RoundedSprite / procedural fallback.
    /// </summary>
    public static class CasualArt
    {
        static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        // 킷 이름 → 무협 에셋 전역 별칭. C()를 지나는 모든 코드 경로가 자동 스킨된다.
        static readonly Dictionary<string, string> WuxAlias = new Dictionary<string, string>
        {
            // 2층 구조: 창 몸체=나무 판자(wood_board), 콘텐츠 섬=양피지(paper_sheet)
            { "Popoup01-03_White_Bg", "wood_board" },
            { "Popup01_Single_Navy", "header_cloud" },      // 모달 헤더 = 진홍 나무 판자
            { "Popup_FullWidth03_Single_Navy", "wood_board" },
            { "Menu_TopBtn_Focus", "tab_on" },
            { "Menu_TopBtn", "tab_off" },
            { "Slider_Basic03_Bg", "bar_bg" },
            { "Slider_Basic03_Fill_Yellow", "bar_fill" },
            { "Slider_Basic03_Fill_White", "bar_fill" },
            { "ResourceBar_Bg", "row_dark" },
            { "wux_window_large", "window_large" },     // 통짜 창 일러스트 (코드 전용 별칭)
            { "wux_window_popup", "window_popup" },
            { "Button_Hexagon199_Blue", "slot_empty" },
            { "Button_Hexagon199_Red", "slot_empty" },
            { "Button_Hexagon199_White_Bg", "slot_empty" },
        };

        /// <summary>무협 행 판 (종이 띠) — FrameList 등이 틴트 없이 쓴다.</summary>
        public static Sprite RowDark => Wux("row_dark");

        /// <summary>창 몸체(나무 판자) / 콘텐츠 양피지 — 2층 구조의 두 재질.</summary>
        public static Sprite WoodBoard => Wux("wood_board");
        public static Sprite PaperSheet => Wux("paper_sheet");

        public static Sprite C(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (_cache.TryGetValue(name, out var s)) return s;
            if (WuxAlias.TryGetValue(name, out var wn))
            {
                s = Wux(wn);
                if (s != null) { _cache[name] = s; return s; }
            }
            s = Resources.Load<Sprite>("CasualKit/" + name);
            _cache[name] = s;
            return s;
        }

        // W 시리즈: 무협풍 AI 스킨(Resources/WuxiaUi) — 있으면 킷보다 우선.
        // 호출부는 킷 규약대로 흰색 틴트를 쓰므로 프리컬러 아트가 그대로 나온다.
        static Sprite Wux(string name)
        {
            string key = "wux:" + name;
            if (_cache.TryGetValue(key, out var s)) return s;
            s = Resources.Load<Sprite>("WuxiaUi/" + name);
            _cache[key] = s;
            return s;
        }

        // Core surfaces
        public static Sprite PopupWhite => Wux("panel_hanji") ?? C("Popoup01-03_White_Bg");
        public static Sprite PopupNavy => Wux("panel_dark") ?? C("Popup01_Single_Navy");
        public static Sprite CardRound => C("BasicFrame_Round12");
        public static Sprite CardSquare => C("BasicFrame_Square");
        public static Sprite CardRoundGradient => C("BasicFrame_Round12_Gradient");
        public static Sprite CardBordered => C("BorderFrame_Round01_White1");
        public static Sprite TitleLine => C("Common_Popup_TitleLIne");

        // Buttons: Button01_195_{color} — 무협 스킨은 옻칠 2종(주홍/먹빛)으로 수렴
        public static Sprite Button(string color)
        {
            var w = WuxButton(color);
            return w != null ? w : C("Button01_195_" + color);
        }

        static Sprite WuxButton(string key)
        {
            switch (key)
            {
                case "Green": case "Yellow": case "Red": case "Orange":
                    return Wux("btn_primary");    // 주홍 옻칠 (CTA·강조)
                case "Blue": case "Sky": case "Purple": case "Gray": case "BlueGray":
                    return Wux("btn_secondary");  // 먹빛 옻칠
                default:
                    return null;
            }
        }

        // Bars
        public static Sprite BarBg => Wux("bar_bg") ?? C("Slider_Basic03_Bg");
        public static Sprite BarFillYellow => Wux("bar_fill") ?? C("Slider_Basic03_Fill_Yellow");
        public static Sprite BarFillWhite => Wux("bar_fill") ?? C("Slider_Basic03_Fill_White");

        // HUD / chips
        public static Sprite ResourceBar => C("ResourceBar_Bg");
        public static Sprite LabelRound => C("Label_Round01_White");

        // Tabs
        public static Sprite TabOn => Wux("tab_on") ?? C("Menu_TopBtn_Focus");
        public static Sprite TabOff => Wux("tab_off") ?? C("Menu_TopBtn");

        // Notification badge (빨간 네모 대신 키트 알림 점)
        public static Sprite AlertDot => C("Alert_Dot_Bg");
        public static Sprite AlertDotBorder => C("Alert_Dot_Border");

        // Hexagon (스킬 슬롯 — 강조도 같은 육각형이어야 모양이 맞는다)
        public static Sprite HexBlue => C("Button_Hexagon199_Blue");
        public static Sprite HexRed => C("Button_Hexagon199_Red");
        public static Sprite HexWhite => C("Button_Hexagon199_White_Bg");
        public static Sprite HexShadow => C("Button_Hexagon199_White_Shadow");

        // Labels / badges (직업명 '무사' 같은 임시 배지 대체)
        public static Sprite LabelTrapezoid(string color) => C("Label_Trapezoid_Single_" + color);
        public static Sprite LabelRound2 => C("Label_Round02_White");
        public static Sprite LabelTapered => C("Label_Tapered_White");
        public static Sprite TitleFlag(string color) => C("Title_Flag01_" + color);
        public static Sprite TitleRibbon(string color) => C("Title_Ribbon_Bg_" + color);

        // Cards / frames
        public static Sprite CardSingle(string color) => C("CardFrame03_Single_" + color);
        public static Sprite CardGlow => C("CardFrame03_Glow");
        public static Sprite BorderBlue => C("BorderFrame_Round01_Blue");
        public static Sprite BorderSky => C("BorderFrame_Round01_Sky");
        public static Sprite BorderNavy => C("BorderFrame_Round05_Navy");

        /// <summary>Nearest kit button color key for an arbitrary tint (RGB distance).</summary>
        public static string ButtonKeyForTint(Color tint)
        {
            string best = null;
            float bestD = float.MaxValue;
            foreach (var kv in ButtonPalette)
            {
                var c = kv.Value;
                float d = (c.r - tint.r) * (c.r - tint.r) + (c.g - tint.g) * (c.g - tint.g) + (c.b - tint.b) * (c.b - tint.b);
                if (d < bestD) { bestD = d; best = kv.Key; }
            }
            return best;
        }

        public static Sprite ButtonForTint(Color tint)
        {
            var key = ButtonKeyForTint(tint);
            return key != null ? Button(key) : null;
        }

        /// <summary>Bright candy buttons need dark labels (kit demo: dark text on yellow Select).</summary>
        public static bool ButtonIsLight(string key)
        {
            if (WuxButton(key) != null) return true;    // 무협 목판 버튼은 중앙이 양피지 — 먹 글씨
            return key == "Green" || key == "Yellow" || key == "White" || key == "Gray";
        }

        static readonly Dictionary<string, Color> ButtonPalette = new Dictionary<string, Color>
        {
            { "Green", new Color(0.35f, 0.80f, 0.15f) },
            { "Sky", new Color(0.25f, 0.65f, 0.95f) },
            { "Blue", new Color(0.15f, 0.40f, 0.90f) },
            { "Orange", new Color(0.97f, 0.55f, 0.15f) },
            { "Red", new Color(0.92f, 0.25f, 0.20f) },
            { "Purple", new Color(0.65f, 0.35f, 0.90f) },
            { "Yellow", new Color(0.98f, 0.80f, 0.15f) },
            { "Gray", new Color(0.55f, 0.55f, 0.60f) },
            { "BlueGray", new Color(0.45f, 0.50f, 0.62f) },
        };
    }
}
