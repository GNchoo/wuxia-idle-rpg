using System;
using IdleMvp.Economy;
using UnityEngine;

namespace IdleMvp.Core
{
    public static class DailyMissionService
    {
        public static event Action OnChanged;

        public struct Mission
        {
            public string Id;
            public string Title;
            public string Desc;
            public int Goal;
            public CurrencyId RewardCurrency;
            public int RewardAmount;
        }

        public static readonly Mission[] Missions =
        {
            new Mission { Id = "kill",    Title = "몬스터 사냥", Desc = "몬스터 30마리 처치",    Goal = 30,  RewardCurrency = CurrencyId.Gold,       RewardAmount = 5000  },
            new Mission { Id = "enhance", Title = "장비 강화",   Desc = "강화 3회 시도",         Goal = 3,   RewardCurrency = CurrencyId.WeaponEnhanceStone, RewardAmount = 50 },
            new Mission { Id = "dungeon", Title = "던전 도전",   Desc = "성장 던전 1회 클리어",  Goal = 1,   RewardCurrency = CurrencyId.RedDiamond, RewardAmount = 5     },
            new Mission { Id = "arena",   Title = "아레나 도전", Desc = "아레나 1회 도전",       Goal = 1,   RewardCurrency = CurrencyId.Gold,       RewardAmount = 3000  },
            new Mission { Id = "summon",  Title = "소환 수행",   Desc = "무기 또는 동료 소환 1회", Goal = 1, RewardCurrency = CurrencyId.WeaponEnhanceStone, RewardAmount = 30 },
        };

        const string PrefDate = "IdleGrow.Daily.Date";
        const string PrefProg = "IdleGrow.Daily.Prog.";
        const string PrefClaimed = "IdleGrow.Daily.Claimed.";
        const string PrefAllClaimed = "IdleGrow.Daily.AllClaimed";

        static string Today => DateTime.UtcNow.ToString("yyyyMMdd");

        static void EnsureReset()
        {
            if (PlayerPrefs.GetString(PrefDate, "") == Today) return;
            PlayerPrefs.SetString(PrefDate, Today);
            foreach (var m in Missions)
            {
                PlayerPrefs.SetInt(PrefProg + m.Id, 0);
                PlayerPrefs.SetInt(PrefClaimed + m.Id, 0);
            }
            PlayerPrefs.SetInt(PrefAllClaimed, 0);
            PlayerPrefs.Save();
        }

        public static int Progress(string missionId)
        {
            EnsureReset();
            return PlayerPrefs.GetInt(PrefProg + missionId, 0);
        }

        public static bool IsClaimed(string missionId)
        {
            EnsureReset();
            return PlayerPrefs.GetInt(PrefClaimed + missionId, 0) == 1;
        }

        public static bool AllClaimed
        {
            get { EnsureReset(); return PlayerPrefs.GetInt(PrefAllClaimed, 0) == 1; }
        }

        public static int CompletedCount
        {
            get
            {
                EnsureReset();
                int c = 0;
                foreach (var m in Missions)
                    if (Progress(m.Id) >= m.Goal) c++;
                return c;
            }
        }

        public static void Increment(string missionId, int amount = 1)
        {
            EnsureReset();
            int cur = PlayerPrefs.GetInt(PrefProg + missionId, 0);
            Mission? mi = null;
            foreach (var m in Missions)
                if (m.Id == missionId) { mi = m; break; }
            if (mi == null) return;
            int next = Mathf.Min(cur + amount, mi.Value.Goal);
            if (next == cur) return;
            PlayerPrefs.SetInt(PrefProg + missionId, next);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }

        public static bool TryClaim(string missionId)
        {
            EnsureReset();
            if (IsClaimed(missionId)) return false;
            Mission? mi = null;
            foreach (var m in Missions)
                if (m.Id == missionId) { mi = m; break; }
            if (mi == null) return false;
            if (Progress(missionId) < mi.Value.Goal) return false;
            PlayerPrefs.SetInt(PrefClaimed + missionId, 1);
            PlayerPrefs.Save();
            Economy.CurrencyWallet.Instance?.Add(mi.Value.RewardCurrency, mi.Value.RewardAmount);
            OnChanged?.Invoke();
            return true;
        }

        public static bool TryClaimAll()
        {
            EnsureReset();
            if (AllClaimed) return false;
            if (CompletedCount < Missions.Length) return false;
            foreach (var m in Missions)
                if (!IsClaimed(m.Id)) return false;
            PlayerPrefs.SetInt(PrefAllClaimed, 1);
            PlayerPrefs.Save();
            Economy.CurrencyWallet.Instance?.Add(CurrencyId.RedDiamond, 20);
            OnChanged?.Invoke();
            return true;
        }

        public static bool HasUnclaimed
        {
            get
            {
                EnsureReset();
                foreach (var m in Missions)
                    if (Progress(m.Id) >= m.Goal && !IsClaimed(m.Id)) return true;
                if (!AllClaimed && CompletedCount >= Missions.Length)
                {
                    bool allIndiv = true;
                    foreach (var m in Missions) if (!IsClaimed(m.Id)) { allIndiv = false; break; }
                    if (allIndiv) return true;
                }
                return false;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoHook()
        {
            EnsureReset();
        }
    }
}
