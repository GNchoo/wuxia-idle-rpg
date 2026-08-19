using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleMvp.Core
{
    /// <summary>
    /// 문파(門派) 시스템.
    ///
    /// 고증 설계:
    ///  - **평민**(전직 전)은 아무 무기나 쓸 수 있다. 강호에 갓 나온 자는 가리지 않는다.
    ///  - **정파**는 문파에 들어가면 그 문파가 취급하는 무기만 쓴다.
    ///    무당은 검과 불진, 소림은 곤과 권, 아미는 검과 아미자… 문파의 무공 체계가
    ///    특정 병기에 묶여 있기 때문이다.
    ///  - **사파·마도**는 제한이 없다. 이기면 그만이라 가리지 않고 쓴다.
    ///
    /// 무기 종류: 0=검 1=도 2=창곤 3=기병 4=권갑
    /// </summary>
    public static class SectService
    {
        public static event Action OnChanged;

        const string PrefSect = "IdleGrow.Sect.Id";

        public class SectDef
        {
            public string Id;
            public string Name;
            public string Faction;      // hero=정파 / bowmaster=사파 / archmage=마도
            public int[] AllowedKinds;  // 정파만 의미가 있다 (빈 배열 = 제한 없음)
            public string Signature;    // 대표 무기 설명
            public string Robe;         // 전용 의상 이름
            public Color RobeMain;      // 의상 주색
            public Color RobeSub;       // 의상 보조색
            public string Desc;
        }

        /// <summary>
        /// 정파 문파는 병기가 정해져 있고, 사파·마도는 자유다.
        /// 색은 docs/wuxia-reference.md 의 세력별 시각 규칙을 따랐다
        /// (정파=저채도 고명도, 사파=저채도 저명도, 마도=고채도 강대비).
        /// </summary>
        public static readonly SectDef[] All =
        {
            // ---- 정파 (무기 제한 있음) ----
            new SectDef {
                Id = "wudang", Name = "무당파", Faction = "hero",
                AllowedKinds = new[] { 0, 2 },
                Signature = "송문고정검 · 불진", Robe = "도포",
                RobeMain = new Color(0.85f, 0.88f, 0.92f), RobeSub = new Color(0.35f, 0.50f, 0.70f),
                Desc = "태극의 이치로 검을 쓴다. 부드러움으로 강함을 제압한다.",
            },
            new SectDef {
                Id = "shaolin", Name = "소림사", Faction = "hero",
                AllowedKinds = new[] { 2, 4 },
                Signature = "백랍곤 · 권", Robe = "치의",
                RobeMain = new Color(0.42f, 0.40f, 0.38f), RobeSub = new Color(0.62f, 0.42f, 0.22f),
                Desc = "곤법과 권법의 종가. 백병지조를 다룬다.",
            },
            new SectDef {
                Id = "huashan", Name = "화산파", Faction = "hero",
                AllowedKinds = new[] { 0 },
                Signature = "매화검", Robe = "백매 무복",
                RobeMain = new Color(0.94f, 0.94f, 0.96f), RobeSub = new Color(0.80f, 0.25f, 0.35f),
                Desc = "매화가 흩날리듯 빠르고 매서운 검을 쓴다.",
            },
            new SectDef {
                Id = "emei", Name = "아미파", Faction = "hero",
                AllowedKinds = new[] { 0, 3 },
                Signature = "단검 · 아미자", Robe = "청의",
                RobeMain = new Color(0.80f, 0.86f, 0.88f), RobeSub = new Color(0.30f, 0.55f, 0.55f),
                Desc = "짧은 병기와 암기를 다룬다. 빠르고 정확하다.",
            },
            new SectDef {
                Id = "tangmen", Name = "당문", Faction = "hero",
                AllowedKinds = new[] { 3 },
                Signature = "암기 · 독", Robe = "흑청 경장",
                RobeMain = new Color(0.28f, 0.32f, 0.38f), RobeSub = new Color(0.45f, 0.65f, 0.40f),
                Desc = "암기와 독의 명가. 손에 든 것보다 던지는 것이 무섭다.",
            },

            // ---- 사파 (제한 없음) ----
            new SectDef {
                Id = "greenwood", Name = "녹림맹", Faction = "bowmaster",
                AllowedKinds = new int[0],
                Signature = "박도 · 무엇이든", Robe = "가죽 경장",
                RobeMain = new Color(0.38f, 0.30f, 0.20f), RobeSub = new Color(0.25f, 0.35f, 0.22f),
                Desc = "산을 넘는 자들. 손에 잡히는 것이 곧 병기다.",
            },
            new SectDef {
                Id = "blackwind", Name = "흑풍채", Faction = "bowmaster",
                AllowedKinds = new int[0],
                Signature = "귀두도 · 무엇이든", Robe = "흑의",
                RobeMain = new Color(0.18f, 0.18f, 0.20f), RobeSub = new Color(0.45f, 0.20f, 0.20f),
                Desc = "바람처럼 왔다 사라진다. 규칙 따위는 없다.",
            },

            // ---- 마도 (제한 없음) ----
            new SectDef {
                Id = "cheonma", Name = "천마신교", Faction = "archmage",
                AllowedKinds = new int[0],
                Signature = "연병기 · 무엇이든", Robe = "마의",
                RobeMain = new Color(0.12f, 0.10f, 0.16f), RobeSub = new Color(0.70f, 0.10f, 0.25f),
                Desc = "천마의 뜻을 따른다. 힘이 곧 도리다.",
            },
            new SectDef {
                Id = "blood", Name = "혈교", Faction = "archmage",
                AllowedKinds = new int[0],
                Signature = "혈공 · 무엇이든", Robe = "혈의",
                RobeMain = new Color(0.35f, 0.08f, 0.12f), RobeSub = new Color(0.85f, 0.75f, 0.25f),
                Desc = "피로써 힘을 얻는다. 금기를 두려워하지 않는다.",
            },
        };

        public static string SelectedId { get; private set; } = "";
        public static bool HasSect => !string.IsNullOrEmpty(SelectedId);

        public static SectDef Current
        {
            get
            {
                if (!HasSect) return null;
                for (int i = 0; i < All.Length; i++)
                    if (All[i].Id == SelectedId) return All[i];
                return null;
            }
        }

        public static string DisplayName => Current != null ? Current.Name : "무소속";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            SelectedId = PlayerPrefs.GetString(PrefSect, "");
        }

        /// <summary>현재 세력에서 고를 수 있는 문파들.</summary>
        public static List<SectDef> ForFaction(string faction)
        {
            var list = new List<SectDef>();
            if (string.IsNullOrEmpty(faction)) return list;
            for (int i = 0; i < All.Length; i++)
                if (All[i].Faction == faction) list.Add(All[i]);
            return list;
        }

        public static string Join(string sectId)
        {
            for (int i = 0; i < All.Length; i++)
            {
                if (All[i].Id != sectId) continue;
                SelectedId = sectId;
                PlayerPrefs.SetString(PrefSect, sectId);
                PlayerPrefs.Save();
                OnChanged?.Invoke();
                return All[i].Name + " 입문";
            }
            return "그런 문파가 없습니다";
        }

        public static void Leave()
        {
            SelectedId = "";
            PlayerPrefs.DeleteKey(PrefSect);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }

        /// <summary>
        /// 이 무기 종류를 쓸 수 있는가.
        ///  - 평민(세력 미선택) → 전부 허용. 강호에 갓 나온 자는 가리지 않는다
        ///  - 정파 + 문파 소속 → 그 문파의 병기만
        ///  - 사파·마도 → 전부 허용
        /// </summary>
        public static bool CanUseKind(int kind)
        {
            if (!FactionService.HasSelected) return true;         // 평민
            var s = Current;
            if (s == null) return true;                            // 세력만 정하고 문파는 아직
            if (s.AllowedKinds == null || s.AllowedKinds.Length == 0) return true;  // 사파·마도
            for (int i = 0; i < s.AllowedKinds.Length; i++)
                if (s.AllowedKinds[i] == kind) return true;
            return false;
        }

        /// <summary>못 쓰는 이유 (UI 안내용).</summary>
        public static string WhyCannotUse(int kind)
        {
            if (CanUseKind(kind)) return null;
            var s = Current;
            return s.Name + "은(는) " + s.Signature + "만 다룹니다";
        }

        static readonly string[] KindNames = { "검", "도", "창곤", "기병", "권갑" };

        public static string KindName(int kind)
            => kind >= 0 && kind < KindNames.Length ? KindNames[kind] : "무기";

        /// <summary>문파 전용 의상 색 (없으면 false).</summary>
        public static bool TryGetRobeColors(out Color main, out Color sub)
        {
            var s = Current;
            if (s == null) { main = Color.white; sub = Color.white; return false; }
            main = s.RobeMain; sub = s.RobeSub; return true;
        }
    }
}
