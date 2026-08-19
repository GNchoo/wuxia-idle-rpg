using System;
using UnityEngine;

namespace IdleMvp.Economy
{
    public class LevelPackageService : MonoBehaviour
    {
        public static LevelPackageService Instance { get; private set; }
        public event Action OnChanged;

        static readonly int[] Milestones = { 5, 10, 20, 30, 50 };
        const string PrefKey = "IdleGrow.LevelPkg.";

        public struct Package
        {
            public int Level;
            public int RedDiamond;
            public int BlueDiamond;
            public int WeaponTicket;
            public int CompanionTicket;
            public int CostBlue;
        }

        static readonly Package[] Packages =
        {
            new Package { Level = 5,  RedDiamond = 30,  BlueDiamond = 0,  WeaponTicket = 5,  CompanionTicket = 2, CostBlue = 0 },
            new Package { Level = 10, RedDiamond = 80,  BlueDiamond = 50, WeaponTicket = 10, CompanionTicket = 5, CostBlue = 50 },
            new Package { Level = 20, RedDiamond = 150, BlueDiamond = 100, WeaponTicket = 15, CompanionTicket = 8, CostBlue = 100 },
            new Package { Level = 30, RedDiamond = 250, BlueDiamond = 150, WeaponTicket = 20, CompanionTicket = 10, CostBlue = 150 },
            new Package { Level = 50, RedDiamond = 500, BlueDiamond = 300, WeaponTicket = 30, CompanionTicket = 15, CostBlue = 250 },
        };

        int _pendingIndex = -1;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        void Start()
        {
            var pg = Progression.PlayerGrowth.Instance;
            if (pg != null) pg.OnChanged += CheckMilestone;
            CheckMilestone();
        }

        void CheckMilestone()
        {
            var pg = Progression.PlayerGrowth.Instance;
            if (pg == null) return;

            // PlayerGrowth.OnChanged는 사냥 중 경험치가 오를 때마다(=몹 하나 잡을 때마다) 불린다.
            // 대기 중인 패키지가 '바뀐' 순간에만 알린다. 매번 쏘면 토스트가 화면을 계속 가린다.
            int next = -1;
            for (int i = Packages.Length - 1; i >= 0; i--)
            {
                if (pg.Level >= Packages[i].Level && !IsClaimed(i)) { next = i; break; }
            }
            if (next == _pendingIndex) return;
            _pendingIndex = next;
            OnChanged?.Invoke();
        }

        public bool HasPending => _pendingIndex >= 0;
        public Package? PendingPackage => _pendingIndex >= 0 ? Packages[_pendingIndex] : (Package?)null;

        public bool IsClaimed(int index) => PlayerPrefs.GetInt(PrefKey + index, 0) == 1;

        public string TryClaim(int index)
        {
            if (index < 0 || index >= Packages.Length) return "잘못된 패키지";
            if (IsClaimed(index)) return "이미 수령함";
            var pkg = Packages[index];
            var cw = CurrencyWallet.Instance;
            if (pkg.CostBlue > 0)
            {
                if (cw == null || cw.Get(CurrencyId.BlueDiamond) < pkg.CostBlue)
                    return $"블루다이아 {pkg.CostBlue} 필요";
                cw.TrySpend(CurrencyId.BlueDiamond, pkg.CostBlue);
            }

            var wallet = Adapters.WalletAdapter.Instance;
            if (wallet != null && pkg.RedDiamond > 0) wallet.AddRedDiamond(pkg.RedDiamond);
            if (cw != null)
            {
                if (pkg.BlueDiamond > 0) cw.Add(CurrencyId.BlueDiamond, pkg.BlueDiamond);
                if (pkg.WeaponTicket > 0) cw.Add(CurrencyId.WeaponTicket, pkg.WeaponTicket);
                if (pkg.CompanionTicket > 0) cw.Add(CurrencyId.CompanionTicket, pkg.CompanionTicket);
            }

            PlayerPrefs.SetInt(PrefKey + index, 1);
            PlayerPrefs.Save();
            _pendingIndex = -1;
            CheckMilestone();
            OnChanged?.Invoke();
            return null;
        }

        public string ClaimFree()
        {
            if (!HasPending) return "수령 가능한 패키지 없음";
            var pkg = PendingPackage.Value;
            if (pkg.CostBlue > 0) return TryClaim(_pendingIndex);
            return TryClaim(_pendingIndex);
        }

        public Package[] AllPackages => Packages;
    }
}
