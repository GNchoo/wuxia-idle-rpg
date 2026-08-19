using System;
using IdleMvp.Economy;
using UnityEngine;

namespace IdleMvp.Core
{
    public static class AchievementService
    {
        public static event Action OnChanged;

        public enum Category { Kill, Stage, Summon, Enhance, Level, Dungeon, Arena }

        public struct Achievement
        {
            public string Id;
            public Category Cat;
            public string Title;
            public int Goal;
            public CurrencyId RewardCurrency;
            public int RewardAmount;
            public string TitleReward;
        }

        public static readonly Achievement[] List =
        {
            // Kill milestones
            new Achievement { Id="kill_100",    Cat=Category.Kill,    Title="사냥꾼",       Goal=100,    RewardCurrency=CurrencyId.Gold,              RewardAmount=2000  },
            new Achievement { Id="kill_500",    Cat=Category.Kill,    Title="숙련된 사냥꾼", Goal=500,    RewardCurrency=CurrencyId.RedDiamond,        RewardAmount=10    },
            new Achievement { Id="kill_2000",   Cat=Category.Kill,    Title="학살자",       Goal=2000,   RewardCurrency=CurrencyId.RedDiamond,        RewardAmount=30,   TitleReward="학살자" },
            new Achievement { Id="kill_10000",  Cat=Category.Kill,    Title="전장의 지배자", Goal=10000,  RewardCurrency=CurrencyId.RedDiamond,        RewardAmount=80,   TitleReward="전장의 지배자" },

            // Stage milestones
            new Achievement { Id="stage_10",    Cat=Category.Stage,   Title="모험 시작",     Goal=10,    RewardCurrency=CurrencyId.Gold,              RewardAmount=3000  },
            new Achievement { Id="stage_30",    Cat=Category.Stage,   Title="대지의 탐험가", Goal=30,    RewardCurrency=CurrencyId.RedDiamond,        RewardAmount=15    },
            new Achievement { Id="stage_50",    Cat=Category.Stage,   Title="끝없는 여정",   Goal=50,    RewardCurrency=CurrencyId.RedDiamond,        RewardAmount=50,   TitleReward="끝없는 여정" },
            new Achievement { Id="stage_100",   Cat=Category.Stage,   Title="전설의 여행자", Goal=100,   RewardCurrency=CurrencyId.RedDiamond,        RewardAmount=100,  TitleReward="전설의 여행자" },

            // Summon milestones
            new Achievement { Id="summon_10",   Cat=Category.Summon,  Title="첫 소환",       Goal=10,    RewardCurrency=CurrencyId.WeaponTicket,      RewardAmount=3     },
            new Achievement { Id="summon_50",   Cat=Category.Summon,  Title="소환의 달인",   Goal=50,    RewardCurrency=CurrencyId.RedDiamond,        RewardAmount=20    },
            new Achievement { Id="summon_200",  Cat=Category.Summon,  Title="소환왕",        Goal=200,   RewardCurrency=CurrencyId.RedDiamond,        RewardAmount=60,   TitleReward="소환왕" },

            // Enhance milestones
            new Achievement { Id="enh_10",      Cat=Category.Enhance, Title="강화 입문",     Goal=10,    RewardCurrency=CurrencyId.WeaponEnhanceStone,RewardAmount=20    },
            new Achievement { Id="enh_50",      Cat=Category.Enhance, Title="강화 장인",     Goal=50,    RewardCurrency=CurrencyId.RedDiamond,        RewardAmount=15    },
            new Achievement { Id="enh_200",     Cat=Category.Enhance, Title="대장장이",      Goal=200,   RewardCurrency=CurrencyId.RedDiamond,        RewardAmount=50,   TitleReward="대장장이" },

            // Level milestones
            new Achievement { Id="lv_10",       Cat=Category.Level,   Title="성장의 발걸음", Goal=10,    RewardCurrency=CurrencyId.Gold,              RewardAmount=5000  },
            new Achievement { Id="lv_30",       Cat=Category.Level,   Title="숙련자",        Goal=30,    RewardCurrency=CurrencyId.RedDiamond,        RewardAmount=20    },
            new Achievement { Id="lv_50",       Cat=Category.Level,   Title="마스터",        Goal=50,    RewardCurrency=CurrencyId.RedDiamond,        RewardAmount=50,   TitleReward="마스터" },

            // Dungeon milestones
            new Achievement { Id="dg_5",        Cat=Category.Dungeon, Title="던전 초보",     Goal=5,     RewardCurrency=CurrencyId.Gold,              RewardAmount=3000  },
            new Achievement { Id="dg_30",       Cat=Category.Dungeon, Title="던전 정복자",   Goal=30,    RewardCurrency=CurrencyId.RedDiamond,        RewardAmount=25,   TitleReward="던전 정복자" },

            // Arena milestones
            new Achievement { Id="arena_5",     Cat=Category.Arena,   Title="투기장 신참",   Goal=5,     RewardCurrency=CurrencyId.Gold,              RewardAmount=3000  },
            new Achievement { Id="arena_30",    Cat=Category.Arena,   Title="투기장의 왕",   Goal=30,    RewardCurrency=CurrencyId.RedDiamond,        RewardAmount=25,   TitleReward="투기장의 왕" },
        };

        const string PrefProg = "IdleGrow.Achv.Prog.";
        const string PrefClaimed = "IdleGrow.Achv.Claimed.";
        const string PrefTitle = "IdleGrow.Achv.Title";

        public static string ActiveTitle
        {
            get => PlayerPrefs.GetString(PrefTitle, "");
            set { PlayerPrefs.SetString(PrefTitle, value ?? ""); PlayerPrefs.Save(); OnChanged?.Invoke(); }
        }

        public static int Progress(string achvId) => PlayerPrefs.GetInt(PrefProg + achvId, 0);
        public static bool IsClaimed(string achvId) => PlayerPrefs.GetInt(PrefClaimed + achvId, 0) == 1;

        public static void SetProgress(Category cat, int value)
        {
            bool changed = false;
            foreach (var a in List)
            {
                if (a.Cat != cat) continue;
                int cur = PlayerPrefs.GetInt(PrefProg + a.Id, 0);
                int next = Mathf.Min(value, a.Goal);
                if (next > cur)
                {
                    PlayerPrefs.SetInt(PrefProg + a.Id, next);
                    changed = true;
                }
            }
            if (changed)
            {
                PlayerPrefs.Save();
                OnChanged?.Invoke();
            }
        }

        public static void IncrementProgress(Category cat, int amount = 1)
        {
            bool changed = false;
            foreach (var a in List)
            {
                if (a.Cat != cat) continue;
                int cur = PlayerPrefs.GetInt(PrefProg + a.Id, 0);
                if (cur >= a.Goal) continue;
                int next = Mathf.Min(cur + amount, a.Goal);
                PlayerPrefs.SetInt(PrefProg + a.Id, next);
                changed = true;
            }
            if (changed)
            {
                PlayerPrefs.Save();
                OnChanged?.Invoke();
            }
        }

        public static bool TryClaim(string achvId)
        {
            if (IsClaimed(achvId)) return false;
            Achievement? found = null;
            foreach (var a in List)
                if (a.Id == achvId) { found = a; break; }
            if (found == null) return false;
            var ach = found.Value;
            if (Progress(achvId) < ach.Goal) return false;
            PlayerPrefs.SetInt(PrefClaimed + achvId, 1);
            PlayerPrefs.Save();
            CurrencyWallet.Instance?.Add(ach.RewardCurrency, ach.RewardAmount);
            OnChanged?.Invoke();
            return true;
        }

        public static int UnclaimedCount
        {
            get
            {
                int c = 0;
                foreach (var a in List)
                    if (Progress(a.Id) >= a.Goal && !IsClaimed(a.Id)) c++;
                return c;
            }
        }

        public static string[] EarnedTitles
        {
            get
            {
                var titles = new System.Collections.Generic.List<string>();
                foreach (var a in List)
                    if (!string.IsNullOrEmpty(a.TitleReward) && IsClaimed(a.Id))
                        titles.Add(a.TitleReward);
                return titles.ToArray();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void SyncOnBoot()
        {
            SyncFromGameState();
        }

        public static void SyncFromGameState()
        {
            var sp = Progression.StageProgress.Instance;
            if (sp != null) SetProgress(Category.Stage, sp.StageIndex);

            var pg = UnityEngine.Object.FindObjectOfType<Progression.PlayerGrowth>();
            if (pg != null) SetProgress(Category.Level, pg.Level);

            var ws = Adapters.WeaponSummonAdapter.Instance;
            if (ws != null)
            {
                var cs = Adapters.CompanionAdapter.Instance;
                int totalSummons = ws.TotalSummons + (cs != null ? cs.TotalSummons : 0);
                SetProgress(Category.Summon, totalSummons);
            }
        }
    }
}
