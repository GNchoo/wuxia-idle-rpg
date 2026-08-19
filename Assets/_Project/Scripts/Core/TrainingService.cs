using System;
using UnityEngine;

namespace IdleMvp.Core
{
    /// <summary>
    /// 수련 — 벤치마크('용사의 힘') 구조 카피: 증표로 8종 능력치를 단계 강화하고
    /// (모든 트랙을 단계 상한까지 채워야 다음 단계), 훈장으로 어빌리티 3슬롯을 리롤한다.
    /// 공격속도·크리·보스뎀·경험치 등 "기본 스탯 밖의 성장 축"이 전부 여기 산다.
    /// </summary>
    public static class TrainingService
    {
        public enum Track
        {
            NormalDmg = 0,   // 일반 몬스터 피해 %
            BossDmg = 1,     // 보스 피해 %
            CritRate = 2,    // 치명타 확률 %p
            CritDmg = 3,     // 치명타 피해 %p
            AtkSpeed = 4,    // 공격 속도 %
            XpGain = 5,      // 경험치 획득 %
            GoldGain = 6,    // 골드 획득 % (MP가 표시 전용이라 벤치마크의 MP회복 대신 재화 트랙)
            DefPen = 7,      // 방어 관통 %
        }

        public const int TrackCount = 8;
        /// <summary>단계당 트랙 레벨 폭 — 8트랙 전부 이만큼 채우면 다음 단계.</summary>
        public const int LevelsPerStep = 10;
        public const int MaxStep = 10;

        public static readonly string[] TrackNames =
            { "파괴력", "패왕격", "회심", "회심 강타", "신속", "오성", "재물운", "파갑" };

        // 트랙별 레벨당 효과 (표시 단위 = %)
        static readonly float[] PerLevel = { 0.8f, 1.0f, 0.15f, 1.5f, 1.0f, 1.0f, 1.0f, 0.5f };

        [Serializable]
        class SaveData
        {
            public int[] lv = new int[TrackCount];
            public int rerolls;
            public int[] abTrack = { -1, -1, -1 };
            public int[] abTier = new int[3];
        }

        const string PrefKey = "IdleGrow.Training";
        static SaveData _d;
        public static event Action OnChanged;

        static SaveData D
        {
            get
            {
                if (_d == null)
                {
                    _d = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(PrefKey, "{}")) ?? new SaveData();
                    if (_d.lv == null || _d.lv.Length != TrackCount) _d.lv = new int[TrackCount];
                    if (_d.abTrack == null || _d.abTrack.Length != 3) _d.abTrack = new[] { -1, -1, -1 };
                    if (_d.abTier == null || _d.abTier.Length != 3) _d.abTier = new int[3];
                }
                return _d;
            }
        }

        static void Save()
        {
            PlayerPrefs.SetString(PrefKey, JsonUtility.ToJson(D));
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }

        public static int LevelOf(Track t) => D.lv[(int)t];

        /// <summary>현재 단계 = 가장 덜 오른 트랙 기준 — 전 트랙을 채워야 다음 단계가 열린다.</summary>
        public static int Step
        {
            get
            {
                int min = int.MaxValue;
                for (int i = 0; i < TrackCount; i++) min = Mathf.Min(min, D.lv[i]);
                return Mathf.Min(MaxStep, min / LevelsPerStep);
            }
        }

        public static int LevelCap => Mathf.Min(MaxStep * LevelsPerStep, (Step + 1) * LevelsPerStep);
        public static bool IsTrackCapped(Track t) => LevelOf(t) >= LevelCap;

        /// <summary>증표 비용 — 레벨 비례 (벤치마크: 점진 증가).</summary>
        public static int UpgradeCost(Track t) => 2 + LevelOf(t);

        public static bool TryUpgrade(Track t)
        {
            if (IsTrackCapped(t)) return false;
            var w = Economy.CurrencyWallet.Instance;
            if (w == null || !w.TrySpend(Economy.CurrencyId.TrainingToken, UpgradeCost(t))) return false;
            D.lv[(int)t]++;
            Save();
            return true;
        }

        // ── 어빌리티 (3슬롯 랜덤 옵션 + 리롤) ───────────────────────────────
        // 등급 1~4 배율 ×1/×2/×3.3/×5. 리롤을 거듭할수록 상위 등급 확률이 조금씩 오른다.
        static readonly float[] TierMul = { 1f, 2f, 3.3f, 5f };
        public static readonly string[] TierNames = { "범", "호", "룡", "선" };
        /// <summary>어빌리티 기본값 — 트랙 10레벨치에 해당.</summary>
        static float AbilityBase(Track t) => PerLevel[(int)t] * 10f;

        public static int RerollCount => D.rerolls;
        public static int RerollCost => 1 + D.rerolls / 25;

        public static bool HasAbility(int slot) => D.abTrack[slot] >= 0;
        public static Track AbilityTrack(int slot) => (Track)D.abTrack[slot];
        public static int AbilityTier(int slot) => D.abTier[slot];
        public static float AbilityValue(int slot) =>
            HasAbility(slot) ? AbilityBase((Track)D.abTrack[slot]) * TierMul[D.abTier[slot]] : 0f;

        public static bool TryReroll()
        {
            var w = Economy.CurrencyWallet.Instance;
            if (w == null || !w.TrySpend(Economy.CurrencyId.HonorMedal, RerollCost)) return false;
            D.rerolls++;
            float luck = Mathf.Min(0.20f, D.rerolls * 0.002f);   // 리롤 누적 → 상위 등장률↑
            for (int s = 0; s < 3; s++)
            {
                D.abTrack[s] = UnityEngine.Random.Range(0, TrackCount);
                float r = UnityEngine.Random.value + luck;
                D.abTier[s] = r >= 0.97f ? 3 : r >= 0.85f ? 2 : r >= 0.55f ? 1 : 0;
            }
            Save();
            return true;
        }

        // ── 합산 (트랙 레벨 + 어빌리티) ────────────────────────────────────
        public static float TotalPct(Track t)
        {
            float v = LevelOf(t) * PerLevel[(int)t];
            for (int s = 0; s < 3; s++)
                if (D.abTrack[s] == (int)t) v += AbilityValue(s);
            return v;
        }

        public static float NormalDmgPct => TotalPct(Track.NormalDmg);
        public static float BossDmgPct => TotalPct(Track.BossDmg);
        public static float CritRatePct => TotalPct(Track.CritRate);
        public static float CritDmgPct => TotalPct(Track.CritDmg);
        public static float AtkSpeedPct => TotalPct(Track.AtkSpeed);
        public static float XpGainPct => TotalPct(Track.XpGain);
        public static float GoldGainPct => TotalPct(Track.GoldGain);
        public static float DefPenPct => TotalPct(Track.DefPen);

        public static float TrainingCp
        {
            get
            {
                int total = 0;
                for (int i = 0; i < TrackCount; i++) total += D.lv[i];
                float ab = 0f;
                for (int s = 0; s < 3; s++) ab += AbilityValue(s);
                return total * 6f + Step * 40f + ab * 3f;
            }
        }
    }
}
