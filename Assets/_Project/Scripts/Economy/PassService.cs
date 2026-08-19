using System;
using IdleMvp.Adapters;
using UnityEngine;

namespace IdleMvp.Economy
{
    /// <summary>
    /// Maple summon pass milestones (weapon/companion). Free track + paid track (BlueDiamond).
    /// </summary>
    public class PassService : MonoBehaviour
    {
        public static PassService Instance { get; private set; }

        public int WeaponSummons { get; private set; }
        public int CompanionSummons { get; private set; }
        public bool WeaponPassPaid { get; private set; }
        public bool CompanionPassPaid { get; private set; }
        public int WeaponFreeClaimed { get; private set; }
        public int CompanionFreeClaimed { get; private set; }

        public event Action OnChanged;

        static readonly int[] WeaponMilestones = { 10, 30, 60, 100, 200 };
        static readonly int[] CompanionMilestones = { 5, 20, 50 };

        const string PrefKey = "IdleGrow.Maple.Pass";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Load();
        }

        public void NotifyWeaponSummon()
        {
            WeaponSummons++;
            Save();
            OnChanged?.Invoke();
        }

        public void NotifyCompanionSummon()
        {
            CompanionSummons++;
            Save();
            OnChanged?.Invoke();
        }

        public string BuyWeaponPass()
        {
            if (WeaponPassPaid) return "무기 패스 이미 구매";
            if (CurrencyWallet.Instance == null ||
                !CurrencyWallet.Instance.TrySpend(CurrencyId.BlueDiamond, 100))
                return "블루다이아 100 필요";
            WeaponPassPaid = true;
            CurrencyWallet.Instance.Add(CurrencyId.WeaponTicket, 20);
            WalletAdapter.Instance?.AddRedDiamond(50);
            Save();
            OnChanged?.Invoke();
            return "무기 패스 구매! 티켓+20 RD+50";
        }

        public string BuyCompanionPass()
        {
            if (CompanionPassPaid) return "동료 패스 이미 구매";
            if (CurrencyWallet.Instance == null ||
                !CurrencyWallet.Instance.TrySpend(CurrencyId.BlueDiamond, 150))
                return "블루다이아 150 필요";
            CompanionPassPaid = true;
            CurrencyWallet.Instance.Add(CurrencyId.CompanionTicket, 10);
            WalletAdapter.Instance?.AddRedDiamond(80);
            Save();
            OnChanged?.Invoke();
            return "동료 패스 구매! 티켓+10 RD+80";
        }

        public string ClaimWeaponFree()
        {
            if (WeaponFreeClaimed >= WeaponMilestones.Length) return "무기 패스 무료트랙 완료";
            int need = WeaponMilestones[WeaponFreeClaimed];
            if (WeaponSummons < need) return $"누적 소환 {WeaponSummons}/{need}";
            WeaponFreeClaimed++;
            CurrencyWallet.Instance?.Add(CurrencyId.WeaponTicket, 5 + WeaponFreeClaimed);
            Save();
            OnChanged?.Invoke();
            return $"무기 패스 보상 수령 ({WeaponFreeClaimed}/{WeaponMilestones.Length})";
        }

        public string ClaimCompanionFree()
        {
            if (CompanionFreeClaimed >= CompanionMilestones.Length) return "동료 패스 무료트랙 완료";
            int need = CompanionMilestones[CompanionFreeClaimed];
            if (CompanionSummons < need) return $"누적 소환 {CompanionSummons}/{need}";
            CompanionFreeClaimed++;
            CurrencyWallet.Instance?.Add(CurrencyId.CompanionTicket, 3 + CompanionFreeClaimed);
            Save();
            OnChanged?.Invoke();
            return $"동료 패스 보상 수령 ({CompanionFreeClaimed}/{CompanionMilestones.Length})";
        }

        public string StatusText()
        {
            return $"무기패스 소환 {WeaponSummons} 수령 {WeaponFreeClaimed}/{WeaponMilestones.Length} 유료:{(WeaponPassPaid ? "Y" : "N")}\n" +
                   $"동료패스 소환 {CompanionSummons} 수령 {CompanionFreeClaimed}/{CompanionMilestones.Length} 유료:{(CompanionPassPaid ? "Y" : "N")}";
        }

        public bool HasClaimable
        {
            get
            {
                if (WeaponFreeClaimed < WeaponMilestones.Length &&
                    WeaponSummons >= WeaponMilestones[WeaponFreeClaimed])
                    return true;
                if (CompanionFreeClaimed < CompanionMilestones.Length &&
                    CompanionSummons >= CompanionMilestones[CompanionFreeClaimed])
                    return true;
                return false;
            }
        }

        void Save()
        {
            PlayerPrefs.SetInt(PrefKey + ".ws", WeaponSummons);
            PlayerPrefs.SetInt(PrefKey + ".cs", CompanionSummons);
            PlayerPrefs.SetInt(PrefKey + ".wp", WeaponPassPaid ? 1 : 0);
            PlayerPrefs.SetInt(PrefKey + ".cp", CompanionPassPaid ? 1 : 0);
            PlayerPrefs.SetInt(PrefKey + ".wfc", WeaponFreeClaimed);
            PlayerPrefs.SetInt(PrefKey + ".cfc", CompanionFreeClaimed);
            PlayerPrefs.Save();
        }

        void Load()
        {
            WeaponSummons = PlayerPrefs.GetInt(PrefKey + ".ws", 0);
            CompanionSummons = PlayerPrefs.GetInt(PrefKey + ".cs", 0);
            WeaponPassPaid = PlayerPrefs.GetInt(PrefKey + ".wp", 0) == 1;
            CompanionPassPaid = PlayerPrefs.GetInt(PrefKey + ".cp", 0) == 1;
            WeaponFreeClaimed = PlayerPrefs.GetInt(PrefKey + ".wfc", 0);
            CompanionFreeClaimed = PlayerPrefs.GetInt(PrefKey + ".cfc", 0);
        }
    }
}
