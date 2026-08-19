using UnityEngine;

namespace IdleMvp.Core
{
    /// <summary>
    /// First-run guided tour. Step index persists in PlayerPrefs; the HUD renders
    /// the current step card and advances via user taps.
    /// </summary>
    public static class TutorialService
    {
        const string PrefKey = "IdleGrow.Tutorial.Step";

        public struct Step
        {
            public string Title;
            public string Desc;
            /// <summary>DebugOpen screen id opened by the [이동] button (null = no target).</summary>
            public string Screen;
        }

        public static readonly Step[] Steps =
        {
            new Step { Title = "모험의 시작", Desc = "사냥은 자동으로 진행됩니다. 영웅이 몬스터를 처치하며 골드와 경험치를 모아요.", Screen = null },
            new Step { Title = "능력치 강화", Desc = "레벨업으로 얻은 스탯 포인트로 공격력·방어력·HP를 강화해보세요.", Screen = "char" },
            new Step { Title = "스킬 습득", Desc = "스킬을 배우면 전투 중 자동으로 발동됩니다. 첫 스킬을 습득해보세요.", Screen = "skill" },
            new Step { Title = "무기 소환", Desc = "무기를 소환하고 장착하면 영웅이 실제로 손에 들어요. 등급이 높을수록 강력합니다.", Screen = "weapon" },
            new Step { Title = "동료 소환", Desc = "동료를 소환해 메인·서브로 배치하면 함께 싸웁니다.", Screen = "comp" },
            new Step { Title = "던전 도전", Desc = "성장 던전에서 골드·강화석을 빠르게 모을 수 있어요. 티켓은 매일 충전됩니다.", Screen = "dungeon" },
            // 무협 차별화 시스템 — 여기부터가 이 게임을 사는 이유다. 구세대 6스텝만
            // 가르치고 세력·경지·전향·기연을 안 알려주던 걸 보강.
            new Step { Title = "세력 입문", Desc = "레벨 6이 되면 정파·사파·마도 중 한 세력에 입문합니다. 세력마다 스킬과 무공이 완전히 다릅니다.", Screen = null },
            new Step { Title = "무력 경지", Desc = "전투력이 오르면 경지가 상승합니다 — 삼류에서 화경까지. 경지가 오르면 몸에서 기운이 뿜어져 나와요.", Screen = "realm" },
            new Step { Title = "문파 가입", Desc = "문파에 들면 문파 무공과 전용 병기를 다룰 수 있습니다.", Screen = "sect" },
            new Step { Title = "전직과 전향", Desc = "레벨 30부터 상급 직업으로 전직하고, 세력을 갈아타는 '파천'도 가능합니다. 이전 세력의 무공은 그대로 남아 시너지를 냅니다.", Screen = "job" },
            new Step { Title = "기연(奇緣)", Desc = "강호를 떠돌다 보면 아주 드물게 기연을 만납니다. 받아들이면 히든 직업이 열립니다. 방치 사냥 중에만 찾아옵니다.", Screen = null },
        };

        public static int StepIndex
        {
            get => PlayerPrefs.GetInt(PrefKey, 0);
            set { PlayerPrefs.SetInt(PrefKey, value); PlayerPrefs.Save(); }
        }

        public static bool Done => StepIndex >= Steps.Length;
        public static Step Current => Steps[Mathf.Clamp(StepIndex, 0, Steps.Length - 1)];

        /// <summary>Advance one step. Returns true when the tour just finished (reward moment).</summary>
        public static bool Advance()
        {
            if (Done) return false;
            StepIndex = StepIndex + 1;
            return Done;
        }

        public static void SkipAll()
        {
            StepIndex = Steps.Length;
        }
    }
}
