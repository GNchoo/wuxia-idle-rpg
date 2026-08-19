using System;
using IdleMvp.Economy;
using UnityEngine;

namespace IdleMvp.Economy
{
    /// <summary>
    /// Timed VIP membership: offline cap/rate + RemoveAds.
    /// </summary>
    public class MembershipService : MonoBehaviour
    {
        public static MembershipService Instance { get; private set; }

        public event Action OnChanged;

        const string PrefExpire = "IdleGrow.Maple.Membership.ExpireTicks";
        const string PrefLegacy = "IdleGrow.Maple.Membership";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            MigrateLegacy();
        }

        void MigrateLegacy()
        {
            if (PlayerPrefs.GetInt(PrefLegacy, 0) == 1 && !HasExpireKey())
            {
                // Legacy permanent → convert to 30 days from now
                SetExpire(DateTime.UtcNow.AddDays(30));
                PlayerPrefs.DeleteKey(PrefLegacy);
                PlayerPrefs.Save();
            }
        }

        bool HasExpireKey() => !string.IsNullOrEmpty(PlayerPrefs.GetString(PrefExpire, ""));

        public bool Active
        {
            get
            {
                long ticks = 0;
                long.TryParse(PlayerPrefs.GetString(PrefExpire, "0"), out ticks);
                return ticks > DateTime.UtcNow.Ticks;
            }
        }

        public bool RemoveAds => Active;

        public string BuyMembership()
        {
            if (Active) return "멤버십 이미 활성";
            if (CurrencyWallet.Instance == null ||
                !CurrencyWallet.Instance.TrySpend(CurrencyId.BlueDiamond, 80))
                return "블루다이아 80 필요";
            SetExpire(DateTime.UtcNow.AddDays(30));
            OnChanged?.Invoke();
            return "멤버십 30일 — 오프라인 캡 12시간 · 광고제거 · 상자 효율↑";
        }

        void SetExpire(DateTime utc)
        {
            PlayerPrefs.SetString(PrefExpire, utc.Ticks.ToString());
            PlayerPrefs.Save();
        }

        // 오프라인 캡의 정의는 balance-seed(offlineCapHours) 한 곳 — 멤버십은 ×1.5
        public float CapHours => IdleMvp.Core.BalanceConfig.Data.offlineCapHours * (Active ? 1.5f : 1f);
        public float AccrueMul => Active ? 1.5f : 1f;

        public string StatusText()
        {
            if (!Active) return "멤버십 OFF";
            long.TryParse(PlayerPrefs.GetString(PrefExpire, "0"), out long ticks);
            var left = new DateTime(ticks, DateTimeKind.Utc) - DateTime.UtcNow;
            return $"멤버십 · 남은 {Mathf.Max(0, (int)left.TotalDays)}일";
        }
    }
}
