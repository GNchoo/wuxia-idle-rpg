using System;
using UnityEngine;

namespace IdleMvp.Core
{
    public class RebirthService : MonoBehaviour
    {
        public static RebirthService Instance { get; private set; }
        public static event Action OnReborn;

        const string PrefKey = "IdleGrow.Rebirth";
        /// <summary>스테이지 200개 기준 — 1회차는 후반부(챕터 12)에 닿아야 열린다.</summary>
        const int MinStage = 115;

        public int Count { get; private set; }
        public int Stones { get; private set; }
        public int StoneAtk { get; private set; }
        public int StoneHp { get; private set; }
        public int StoneCrit { get; private set; }

        // 레벨 100 이후 성장의 주축 — 회차 배수를 곱연산으로 키운다
        public float AtkMul => Mathf.Pow(1.12f, Count) + StoneAtk * 0.02f;
        public float HpMul => Mathf.Pow(1.12f, Count) + StoneHp * 0.02f;
        public float GoldMul => Mathf.Pow(1.08f, Count);
        public float XpMul => Mathf.Pow(1.10f, Count);
        public float CritBonus => StoneCrit * 0.5f;

        public bool CanRebirth
        {
            get
            {
                var sp = Progression.StageProgress.Instance;
                return sp != null && sp.MaxWaveReached >= MinStage;
            }
        }

        public int StonesOnRebirth
        {
            get
            {
                var sp = Progression.StageProgress.Instance;
                if (sp == null) return 0;
                // 더 깊이 밀고 환생할수록 보상이 커진다 (200스테이지 기준 최대 30개)
                return Mathf.Max(1, Mathf.RoundToInt(sp.MaxWaveReached / 10f
                    + Mathf.Max(0, sp.MaxWaveReached - 150) / 5f));
            }
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            Load();
        }

        public bool TryRebirth()
        {
            if (!CanRebirth) return false;

            Stones += StonesOnRebirth;
            Count++;

            ResetGrowth();
            ResetStage();
            ResetGold();

            Save();
            OnReborn?.Invoke();
            return true;
        }

        static void ResetGrowth()
        {
            var g = Progression.PlayerGrowth.Instance;
            if (g != null) g.ResetForRebirth();
            else PlayerPrefs.DeleteKey("IdleMvp.PlayerGrowth");
        }

        static void ResetStage()
        {
            var sp = Progression.StageProgress.Instance;
            if (sp != null) sp.ResetForRebirth();
            else
            {
                PlayerPrefs.DeleteKey("IdleGrow.Stage.cur");
                PlayerPrefs.DeleteKey("IdleGrow.Stage.max");
                PlayerPrefs.DeleteKey("IdleGrow.Stage.hunt");
            }
        }

        static void ResetGold()
        {
            var w = Economy.CurrencyWallet.Instance;
            if (w != null)
            {
                w.Set(Economy.CurrencyId.Gold, 100, false);
            }
        }

        public struct ShopItem
        {
            public string Id, Name;
            public int Cost;
        }

        public static readonly ShopItem[] Shop =
        {
            new ShopItem { Id = "atk",  Name = "공격력 +2%",  Cost = 3 },
            new ShopItem { Id = "hp",   Name = "체력 +2%",    Cost = 3 },
            new ShopItem { Id = "crit", Name = "치명타 +0.5%", Cost = 5 },
        };

        public string TryBuyShop(string id)
        {
            int cost = 0;
            foreach (var item in Shop) if (item.Id == id) { cost = item.Cost; break; }
            if (cost <= 0) return "잘못된 항목";
            if (Stones < cost) return $"환생석 {cost}개 필요 (보유: {Stones})";
            Stones -= cost;
            switch (id)
            {
                case "atk":  StoneAtk++;  break;
                case "hp":   StoneHp++;   break;
                case "crit": StoneCrit++; break;
            }
            Save();
            return null;
        }

        void Save()
        {
            PlayerPrefs.SetInt(PrefKey + ".count", Count);
            PlayerPrefs.SetInt(PrefKey + ".stones", Stones);
            PlayerPrefs.SetInt(PrefKey + ".sAtk", StoneAtk);
            PlayerPrefs.SetInt(PrefKey + ".sHp", StoneHp);
            PlayerPrefs.SetInt(PrefKey + ".sCrit", StoneCrit);
            PlayerPrefs.Save();
        }

        void Load()
        {
            Count = PlayerPrefs.GetInt(PrefKey + ".count", 0);
            Stones = PlayerPrefs.GetInt(PrefKey + ".stones", 0);
            StoneAtk = PlayerPrefs.GetInt(PrefKey + ".sAtk", 0);
            StoneHp = PlayerPrefs.GetInt(PrefKey + ".sHp", 0);
            StoneCrit = PlayerPrefs.GetInt(PrefKey + ".sCrit", 0);
        }
    }
}
