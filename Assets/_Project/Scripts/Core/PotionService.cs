using UnityEngine;

namespace IdleMvp.Core
{
    /// <summary>
    /// 체력 물약 — 소모품 + 쿨타임.
    ///
    /// 예전엔 버튼을 누를 때마다 레드다이아 5를 즉시 차감하는 '즉석 결제 회복'이라
    /// 방치 중에는 죽는 걸 지켜볼 수밖에 없었다. 키우기류 표준대로 바꾼다:
    /// 물약을 아이템으로 보유하고, HP가 임계 아래로 내려가면 자동으로 마시며,
    /// 쿨타임이 있어 연타로 밸런스가 깨지지 않는다.
    /// </summary>
    public static class PotionService
    {
        const string PrefCount = "IdleGrow.Potion.Count";
        const string PrefAuto = "IdleGrow.Potion.Auto";
        const string PrefLevel = "IdleGrow.Potion.Level";

        /// <summary>이 비율 아래로 내려가면 자동으로 마신다.</summary>
        public const float AutoThreshold = 0.4f;
        /// <summary>물약이 없을 때 구매: 레드다이아 5 → 5개.</summary>
        public const int PackCostRd = 5;
        public const int PackSize = 5;

        // ── 물약 강화 (키우기류 표준: 골드 성장 축) ─────────────────────────
        // 레벨당 회복량 +2%p, 쿨타임 -0.5초. L1이 기존 상수(60%/30초)와 같아
        // 기존 세이브의 체감이 떨어지지 않는다.
        public const int MaxLevel = 30;

        public static int Level
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(PrefLevel, 1), 1, MaxLevel);
            private set { PlayerPrefs.SetInt(PrefLevel, Mathf.Clamp(value, 1, MaxLevel)); PlayerPrefs.Save(); }
        }

        public static float HealPctAt(int lv) => Mathf.Min(1f, 0.58f + 0.02f * lv);
        public static float CooldownSecAt(int lv) => Mathf.Max(15f, 30.5f - 0.5f * lv);

        public static float HealPct => HealPctAt(Level);          // L1=60% → L21=100%
        public static float CooldownSec => CooldownSecAt(Level);  // L1=30초 → L30=15.5초

        public static bool IsMaxLevel => Level >= MaxLevel;
        public static double UpgradeCostGold => 60 + Level * 40 + Level * Level * 6;

        /// <summary>골드 차감은 호출자(UI, 지갑 소유권 분리) — 여기선 레벨만 올린다.</summary>
        public static void UpgradeOne()
        {
            if (!IsMaxLevel) Level = Level + 1;
        }

        static float _readyAt;   // Time.time 기준. 세이브 안 함 — 재접속하면 바로 사용 가능(관대한 쪽)

        public static int Count
        {
            get => PlayerPrefs.GetInt(PrefCount, 5);   // 신규 유저 기본 5개
            private set { PlayerPrefs.SetInt(PrefCount, Mathf.Max(0, value)); PlayerPrefs.Save(); }
        }

        public static bool AutoUse
        {
            get => PlayerPrefs.GetInt(PrefAuto, 1) == 1;
            set { PlayerPrefs.SetInt(PrefAuto, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static float CooldownLeft => Mathf.Max(0f, _readyAt - Time.time);
        public static bool Ready => Count > 0 && CooldownLeft <= 0f;

        /// <summary>마신다. 성공 시 true — 회복 자체는 호출자가 한다(전투 소유권 분리).</summary>
        public static bool TryUse()
        {
            if (!Ready) return false;
            Count = Count - 1;
            _readyAt = Time.time + CooldownSec;
            return true;
        }

        public static void Grant(int n) => Count = Count + n;
    }
}
