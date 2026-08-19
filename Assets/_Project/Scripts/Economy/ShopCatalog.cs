using IdleMvp.Economy;
using UnityEngine;

namespace IdleMvp.Economy
{
    public enum ShopCostType
    {
        FreeViaIapOrAd,
        BlueDiamond,
        RedDiamond,
        IapOnly
    }

    public struct ShopProductDef
    {
        public string Id;
        public string Title;
        public string Subtitle;
        public ShopCostType CostType;
        public double CostAmount;
        public string IapProductId;
        public string AdPlacement;
        public int BlueDiamond;
        public int RedDiamond;
        public int WeaponTicket;
        public int CompanionTicket;
        public int MiracleCube;
        public int StarForceScroll;
        public int ScrollTrace;
        public float AccrueSeconds;
        public bool GrantSkillLearn;
        public int DailyLimit;
    }

    /// <summary>Static shop catalog shared by ShopAdapter / UI.</summary>
    public static class ShopCatalog
    {
        public static readonly ShopProductDef[] Products =
        {
            new ShopProductDef
            {
                Id = "bd_ad_60", Title = "블루다이아 60", Subtitle = "광고 시청 보상",
                CostType = ShopCostType.FreeViaIapOrAd, AdPlacement = "rewarded_bd60",
                BlueDiamond = 60, DailyLimit = 5
            },
            new ShopProductDef
            {
                Id = "bd_iap_300", Title = "블루다이아 300", Subtitle = "IAP 패키지",
                CostType = ShopCostType.IapOnly, IapProductId = "bd_300", BlueDiamond = 300
            },
            new ShopProductDef
            {
                Id = "bd_iap_980", Title = "블루다이아 980", Subtitle = "IAP 패키지",
                CostType = ShopCostType.IapOnly, IapProductId = "bd_980", BlueDiamond = 980
            },
            new ShopProductDef
            {
                Id = "ticket_weapon", Title = "무기 티켓팩", Subtitle = "무기 소환권 +5",
                CostType = ShopCostType.RedDiamond, CostAmount = 30, WeaponTicket = 5
            },
            new ShopProductDef
            {
                Id = "ticket_companion", Title = "동료 티켓팩", Subtitle = "동료 소환권 +3",
                CostType = ShopCostType.RedDiamond, CostAmount = 30, CompanionTicket = 3
            },
            new ShopProductDef
            {
                Id = "skill_learn", Title = "스킬 즉시 습득", Subtitle = "미습득 스킬 1개 (RD 25)",
                CostType = ShopCostType.RedDiamond, CostAmount = 25, GrantSkillLearn = true
            },
            new ShopProductDef
            {
                Id = "starter", Title = "스타터팩", Subtitle = "성장 가속 + 티켓",
                CostType = ShopCostType.BlueDiamond, CostAmount = 100,
                RedDiamond = 80, WeaponTicket = 5, CompanionTicket = 3, ScrollTrace = 10
            },
            new ShopProductDef
            {
                Id = "cubes", Title = "큐브 패키지", Subtitle = "잠재·스타포스 재료",
                CostType = ShopCostType.RedDiamond, CostAmount = 40,
                MiracleCube = 5, StarForceScroll = 3, ScrollTrace = 20
            },
            new ShopProductDef
            {
                Id = "monthly", Title = "월간 패키지", Subtitle = "30일 · 일일 수령",
                CostType = ShopCostType.BlueDiamond, CostAmount = 200,
                RedDiamond = 150, WeaponTicket = 10, AccrueSeconds = 7200f
            },
            new ShopProductDef
            {
                Id = "gold_chest", Title = "골드 상자", Subtitle = "레벨 비례 골드",
                CostType = ShopCostType.RedDiamond, CostAmount = 20
            },
            new ShopProductDef
            {
                Id = "fast_reward", Title = "빠른 사냥", Subtitle = "방치 1시간분",
                CostType = ShopCostType.RedDiamond, CostAmount = 25, AccrueSeconds = 3600f, DailyLimit = 3   // balance-seed fastRewardPerDay와 정합
            },
            new ShopProductDef
            {
                Id = "membership", Title = "멤버십 VIP", Subtitle = "30일 · 방치↑ · 광고제거",
                CostType = ShopCostType.BlueDiamond, CostAmount = 80
            },
            new ShopProductDef
            {
                Id = "exchange_bd_rd", Title = "BD→RD 교환", Subtitle = "블루 50 → RD 30",
                CostType = ShopCostType.BlueDiamond, CostAmount = 50, RedDiamond = 30
            },
        };

        public static bool TryGet(string id, out ShopProductDef def)
        {
            for (int i = 0; i < Products.Length; i++)
            {
                if (Products[i].Id == id)
                {
                    def = Products[i];
                    return true;
                }
            }
            def = default;
            return false;
        }

        public static int GetDailyCount(string productId)
        {
            string key = DailyKey(productId);
            string day = PlayerPrefs.GetString(key + ".day", "");
            string today = System.DateTime.UtcNow.ToString("yyyyMMdd");
            if (day != today) return 0;
            return PlayerPrefs.GetInt(key + ".n", 0);
        }

        public static void IncrementDaily(string productId)
        {
            string key = DailyKey(productId);
            string today = System.DateTime.UtcNow.ToString("yyyyMMdd");
            string day = PlayerPrefs.GetString(key + ".day", "");
            int n = day == today ? PlayerPrefs.GetInt(key + ".n", 0) : 0;
            PlayerPrefs.SetString(key + ".day", today);
            PlayerPrefs.SetInt(key + ".n", n + 1);
            PlayerPrefs.Save();
        }

        static string DailyKey(string id) => "IdleGrow.ShopDaily." + id;
    }
}
