using System;
using IdleMvp.Adapters;
using IdleMvp.Core;
using IdleMvp.Economy;
using IdleMvp.Progression;
using UnityEngine;

namespace IdleMvp.Adapters
{
    /// <summary>
    /// Shop facade over ShopCatalog + IapBridge/AdBridge + wallets.
    /// </summary>
    public class ShopAdapter : MonoBehaviour
    {
        public static ShopAdapter Instance { get; private set; }

        public string LastMessage { get; private set; } = "";
        public event Action OnChanged;

        const string PrefMonthlyExpire = "IdleGrow.Shop.MonthlyExpireTicks";
        const string PrefMonthlyClaimDay = "IdleGrow.Shop.MonthlyClaimDay";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool MonthlyActive
        {
            get
            {
                long ticks = long.Parse(PlayerPrefs.GetString(PrefMonthlyExpire, "0"));
                return ticks > DateTime.UtcNow.Ticks;
            }
        }

        public string BuyProduct(string productId)
        {
            if (!ShopCatalog.TryGet(productId, out var def))
                return "상품 없음";

            if (def.DailyLimit > 0 && ShopCatalog.GetDailyCount(productId) >= def.DailyLimit)
                return "오늘 구매 한도 초과";

            switch (def.CostType)
            {
                case ShopCostType.FreeViaIapOrAd:
                {
                    string result = "광고 실패";
                    AdBridge.Instance?.ShowRewarded(def.AdPlacement ?? productId,
                        () =>
                        {
                            ApplyGrant(def);
                            ShopCatalog.IncrementDaily(productId);
                            result = LastMessage = def.Title + " 수령";
                            OnChanged?.Invoke();
                        },
                        err => { result = LastMessage = err; OnChanged?.Invoke(); });
                    return result;
                }

                case ShopCostType.IapOnly:
                {
                    string result = "결제 실패";
                    IapBridge.Instance?.Purchase(def.IapProductId ?? productId,
                        () =>
                        {
                            ApplyGrant(def);
                            result = LastMessage = def.Title + " 구매 완료";
                            OnChanged?.Invoke();
                        },
                        err => { result = LastMessage = err; OnChanged?.Invoke(); });
                    return result;
                }

                case ShopCostType.BlueDiamond:
                    if (productId == "membership")
                        return MembershipService.Instance?.BuyMembership() ?? "멤버십 서비스 없음";
                    if (CurrencyWallet.Instance == null ||
                        !CurrencyWallet.Instance.TrySpend(CurrencyId.BlueDiamond, def.CostAmount))
                        return $"블루다이아 {def.CostAmount:0} 필요";
                    break;

                case ShopCostType.RedDiamond:
                    if (WalletAdapter.Instance == null ||
                        !WalletAdapter.Instance.TrySpendRedDiamond(def.CostAmount))
                        return $"레드다이아 {def.CostAmount:0} 필요";
                    break;
            }

            if (productId == "monthly")
                ActivateMonthly();

            ApplyGrant(def);
            if (def.DailyLimit > 0) ShopCatalog.IncrementDaily(productId);
            LastMessage = def.Title + " 구매 완료";
            OnChanged?.Invoke();
            return LastMessage;
        }

        public string ClaimMonthlyDaily()
        {
            if (!MonthlyActive) return "월간 패키지 미활성";
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            if (PlayerPrefs.GetString(PrefMonthlyClaimDay, "") == today)
                return "오늘 이미 수령";
            PlayerPrefs.SetString(PrefMonthlyClaimDay, today);
            PlayerPrefs.Save();
            WalletAdapter.Instance?.AddRedDiamond(10);
            CurrencyWallet.Instance?.Add(CurrencyId.WeaponTicket, 1);
            LastMessage = "월간 일일 보상 · RD+10 · 무기티켓+1";
            OnChanged?.Invoke();
            return LastMessage;
        }

        void ActivateMonthly()
        {
            var until = DateTime.UtcNow.AddDays(30);
            PlayerPrefs.SetString(PrefMonthlyExpire, until.Ticks.ToString());
            PlayerPrefs.Save();
        }

        string ApplyGrant(ShopProductDef def)
        {
            if (def.BlueDiamond > 0)
                CurrencyWallet.Instance?.Add(CurrencyId.BlueDiamond, def.BlueDiamond);
            if (def.RedDiamond > 0)
                WalletAdapter.Instance?.AddRedDiamond(def.RedDiamond);
            if (def.WeaponTicket > 0)
                CurrencyWallet.Instance?.Add(CurrencyId.WeaponTicket, def.WeaponTicket);
            if (def.CompanionTicket > 0)
                CurrencyWallet.Instance?.Add(CurrencyId.CompanionTicket, def.CompanionTicket);
            if (def.MiracleCube > 0)
                CurrencyWallet.Instance?.Add(CurrencyId.MiracleCube, def.MiracleCube);
            if (def.StarForceScroll > 0)
                CurrencyWallet.Instance?.Add(CurrencyId.StarForceScroll, def.StarForceScroll);
            if (def.ScrollTrace > 0)
                CurrencyWallet.Instance?.Add(CurrencyId.ScrollTrace, def.ScrollTrace);
            if (def.AccrueSeconds > 0)
                LootBoxService.Instance?.AccrueSeconds(def.AccrueSeconds);
            if (def.GrantSkillLearn)
            {
                string sk = SkillAdapter.Instance?.GrantSummonBoost() ?? "스킬 습득";
                return sk;
            }
            if (def.Id == "gold_chest")
            {
                double gold = 5000 + (PlayerGrowth.Instance?.Level ?? 1) * 800;
                WalletAdapter.Instance?.AddGold(gold);
            }
            return def.Title;
        }

        // ---- Compat wrappers for existing UI ----

        public string BuyBlueDiamondPack(int packIndex)
        {
            string[] ids = { "bd_ad_60", "bd_iap_300", "bd_iap_980" };
            return BuyProduct(ids[Mathf.Clamp(packIndex, 0, 2)]);
        }

        public string ExchangeBlueToRed(int blueCost, int redGain) => BuyProduct("exchange_bd_rd");

        public string BuyTicketPack(bool weapon) =>
            BuyProduct(weapon ? "ticket_weapon" : "ticket_companion");

        public string BuyCubes() => BuyProduct("cubes");
        public string BuyStarterPack() => BuyProduct("starter");
        public string BuyMonthlyPack() => BuyProduct("monthly");
        public string BuyGoldChest() => BuyProduct("gold_chest");
        public string BuySkillSummonTicket() => BuyProduct("skill_learn");
        public string FastReward() => BuyProduct("fast_reward");

        public string GrantDebugStarter()
        {
            if (!BmRuntimeFlags.AllowDebugCheats)
                return "디버그 치트 비활성";
            CurrencyWallet.Instance?.Add(CurrencyId.BlueDiamond, 500);
            WalletAdapter.Instance?.AddRedDiamond(200);
            CurrencyWallet.Instance?.Add(CurrencyId.WeaponTicket, 20);
            CurrencyWallet.Instance?.Add(CurrencyId.CompanionTicket, 10);
            CurrencyWallet.Instance?.Add(CurrencyId.MonsterPoint, 50);
            CurrencyWallet.Instance?.Add(CurrencyId.ScrollTrace, 50);
            CurrencyWallet.Instance?.Add(CurrencyId.AdditionalCube, 5);
            LastMessage = "디버그 재화 지급";
            OnChanged?.Invoke();
            return LastMessage;
        }
    }
}
