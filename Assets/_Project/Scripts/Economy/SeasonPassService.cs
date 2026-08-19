using System;
using UnityEngine;

namespace IdleMvp.Economy
{
    public class SeasonPassService : MonoBehaviour
    {
        public static SeasonPassService Instance { get; private set; }
        public event Action OnChanged;

        const string PrefXp       = "IdleGrow.SeasonPass.Xp";
        const string PrefTier     = "IdleGrow.SeasonPass.ClaimedTier";
        const string PrefPremium  = "IdleGrow.SeasonPass.Premium";
        const string PrefSeason   = "IdleGrow.SeasonPass.Season";

        const int SeasonDays = 30;
        const int XpPerMission = 20;
        const int PremiumCostBlue = 200;

        public struct Reward
        {
            public CurrencyId Currency;
            public int Amount;
        }

        public struct Tier
        {
            public int XpRequired;
            public Reward FreeReward;
            public Reward PremiumReward;
        }

        static readonly Tier[] Tiers =
        {
            new Tier { XpRequired = 0,   FreeReward = R(CurrencyId.Gold, 5000),        PremiumReward = R(CurrencyId.RedDiamond, 20) },
            new Tier { XpRequired = 100, FreeReward = R(CurrencyId.WeaponTicket, 5),    PremiumReward = R(CurrencyId.WeaponTicket, 10) },
            new Tier { XpRequired = 250, FreeReward = R(CurrencyId.Gold, 15000),        PremiumReward = R(CurrencyId.BlueDiamond, 30) },
            new Tier { XpRequired = 450, FreeReward = R(CurrencyId.CompanionTicket, 3), PremiumReward = R(CurrencyId.CompanionTicket, 8) },
            new Tier { XpRequired = 700, FreeReward = R(CurrencyId.RedDiamond, 30),     PremiumReward = R(CurrencyId.RedDiamond, 80) },
            new Tier { XpRequired = 1000, FreeReward = R(CurrencyId.WeaponTicket, 10),  PremiumReward = R(CurrencyId.BlueDiamond, 100) },
            new Tier { XpRequired = 1400, FreeReward = R(CurrencyId.Gold, 50000),       PremiumReward = R(CurrencyId.RedDiamond, 150) },
            new Tier { XpRequired = 1900, FreeReward = R(CurrencyId.CompanionTicket, 5),PremiumReward = R(CurrencyId.BlueDiamond, 200) },
        };

        static Reward R(CurrencyId c, int a) => new Reward { Currency = c, Amount = a };

        public int Xp { get; private set; }
        public int ClaimedTier { get; private set; }
        public bool IsPremium { get; private set; }
        public int CurrentSeason { get; private set; }

        public int CurrentTierIndex
        {
            get
            {
                for (int i = Tiers.Length - 1; i >= 0; i--)
                    if (Xp >= Tiers[i].XpRequired) return i;
                return 0;
            }
        }

        public bool HasClaimable => ClaimedTier < CurrentTierIndex;
        public Tier[] AllTiers => Tiers;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        void Start()
        {
            int savedSeason = PlayerPrefs.GetInt(PrefSeason, 0);
            int now = SeasonNumber();
            if (savedSeason != now)
            {
                Xp = 0;
                ClaimedTier = -1;
                IsPremium = false;
                PlayerPrefs.SetInt(PrefSeason, now);
                Save();
            }
            else
            {
                Xp = PlayerPrefs.GetInt(PrefXp, 0);
                ClaimedTier = PlayerPrefs.GetInt(PrefTier, -1);
                IsPremium = PlayerPrefs.GetInt(PrefPremium, 0) == 1;
            }
            CurrentSeason = now;
            Core.DailyMissionService.OnChanged += OnMissionChanged;
        }

        static int SeasonNumber()
        {
            var epoch = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (int)((DateTime.UtcNow - epoch).TotalDays / SeasonDays);
        }

        void OnMissionChanged()
        {
            int completed = Core.DailyMissionService.CompletedCount;
            int newXp = completed * XpPerMission;
            if (newXp > Xp)
            {
                Xp = newXp;
                Save();
                OnChanged?.Invoke();
            }
        }

        public void AddXp(int amount)
        {
            Xp += amount;
            Save();
            OnChanged?.Invoke();
        }

        public string BuyPremium()
        {
            if (IsPremium) return "이미 프리미엄 활성";
            var cw = CurrencyWallet.Instance;
            if (cw == null || cw.Get(CurrencyId.BlueDiamond) < PremiumCostBlue)
                return $"블루다이아 {PremiumCostBlue} 필요";
            cw.TrySpend(CurrencyId.BlueDiamond, PremiumCostBlue);
            IsPremium = true;
            Save();
            OnChanged?.Invoke();
            return null;
        }

        public string ClaimNextTier()
        {
            int next = ClaimedTier + 1;
            if (next >= Tiers.Length) return "모든 보상 수령 완료";
            if (Xp < Tiers[next].XpRequired) return "XP 부족";

            var tier = Tiers[next];
            GrantReward(tier.FreeReward);
            if (IsPremium) GrantReward(tier.PremiumReward);

            ClaimedTier = next;
            Save();
            OnChanged?.Invoke();
            return null;
        }

        static void GrantReward(Reward r)
        {
            if (r.Currency == CurrencyId.RedDiamond)
            {
                Adapters.WalletAdapter.Instance?.AddRedDiamond(r.Amount);
                return;
            }
            CurrencyWallet.Instance?.Add(r.Currency, r.Amount);
        }

        void Save()
        {
            PlayerPrefs.SetInt(PrefXp, Xp);
            PlayerPrefs.SetInt(PrefTier, ClaimedTier);
            PlayerPrefs.SetInt(PrefPremium, IsPremium ? 1 : 0);
            PlayerPrefs.Save();
        }

        public int DaysRemaining
        {
            get
            {
                var epoch = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                double daysSinceEpoch = (DateTime.UtcNow - epoch).TotalDays;
                int seasonStart = CurrentSeason * SeasonDays;
                int dayInSeason = (int)(daysSinceEpoch - seasonStart);
                return Mathf.Max(0, SeasonDays - dayInSeason);
            }
        }
    }
}
