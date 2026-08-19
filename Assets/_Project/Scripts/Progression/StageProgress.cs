using IdleMvp.Core;
using UnityEngine;

namespace IdleMvp.Progression
{
    /// <summary>
    /// Push cursor (breakthrough) + HuntStage (selected farm map).
    /// </summary>
    public class StageProgress : MonoBehaviour
    {
        public static StageProgress Instance { get; private set; }

        [SerializeField] int stagesPerChapter = 10;
        [SerializeField] int mvpChapters = 5;

        /// <summary>Breakthrough / clear cursor.</summary>
        public int CurrentWave { get; private set; } = 1;
        public int MaxWaveReached { get; private set; } = 1;
        /// <summary>Selected hunt farm stage (≤ MaxWaveReached).</summary>
        public int HuntStage { get; private set; } = 1;

        public int StageIndex => Mathf.Max(1, CurrentWave);
        public int PushStage => StageIndex;
        public int Chapter => Mathf.Max(1, (StageIndex - 1) / stagesPerChapter + 1);
        public int StageInChapter => ((StageIndex - 1) % stagesPerChapter) + 1;
        public bool IsBossStage => StageInChapter == stagesPerChapter;

        public int HuntChapter => Mathf.Max(1, (HuntStage - 1) / stagesPerChapter + 1);
        public int HuntStageInChapter => ((HuntStage - 1) % stagesPerChapter) + 1;

        public float RecommendedCp
        {
            get
            {
                var row = StageTable.Get(StageIndex);
                if (row != null) return row.recommendedCp;
                return 100f * Mathf.Pow(BalanceConfig.Data.stageEnemyGrowth, StageIndex - 1)
                       * (IsBossStage ? BalanceConfig.Data.bossMultiplier : 1f);
            }
        }

        public event System.Action OnChanged;

        const string PrefKey = "IdleGrow.Stage";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            var bal = BalanceConfig.Data;
            stagesPerChapter = bal.stagesPerChapter;
            mvpChapters = bal.mvpChapters;
            Load();
        }

        public void SetFromTemplateWaves(float currentWave, float maxWave)
        {
            int cur = Mathf.Max(1, Mathf.FloorToInt(currentWave));
            int max = Mathf.Max(cur, Mathf.FloorToInt(maxWave));
            bool changed = cur != CurrentWave || max != MaxWaveReached;
            CurrentWave = cur;
            MaxWaveReached = max;
            if (HuntStage > MaxWaveReached) HuntStage = MaxWaveReached;
            if (changed)
            {
                Save();
                OnChanged?.Invoke();
            }
        }

        public void SetStageIndex(int index)
        {
            index = Mathf.Clamp(index, 1, Mathf.Max(1, StageTable.Count));
            CurrentWave = index;
            if (index > MaxWaveReached) MaxWaveReached = index;
            if (HuntStage > MaxWaveReached) HuntStage = MaxWaveReached;
            Save();
            IdleMvp.Core.AchievementService.SetProgress(IdleMvp.Core.AchievementService.Category.Stage, StageIndex);
            OnChanged?.Invoke();
        }

        public bool TrySetHuntStage(int index)
        {
            index = Mathf.Clamp(index, 1, Mathf.Max(1, MaxWaveReached));
            var row = StageTable.Get(index);
            if (row != null && !CombatPowerService.CanEnterStage(row, out _))
                return false;
            HuntStage = index;
            Save();
            OnChanged?.Invoke();
            return true;
        }

        public bool TryAdvanceAfterClear()
        {
            int next = StageIndex + 1;
            if (next > StageTable.Count) return false;
            SetStageIndex(next);
            // Keep hunting previous map unless player moves hunt cursor
            return true;
        }

        /// <summary>After auto-push stops, farm the highest cleared stage the player can still clear.</summary>
        public void SettleHuntToCleared()
        {
            int cleared = Mathf.Max(1, StageIndex - 1);
            cleared = Mathf.Min(cleared, MaxWaveReached);
            int best = 1;
            for (int i = 1; i <= cleared; i++)
            {
                var row = StageTable.Get(i);
                if (row == null) { best = i; continue; }
                if (CombatPowerService.EvaluateStageGate(row) != CpGateStatus.Blocked)
                    best = i;
            }
            HuntStage = best;
            Save();
            OnChanged?.Invoke();
        }

        public string GetDisplayLabel()
        {
            return $"Ch.{Chapter}-{StageInChapter}";
        }

        public string GetHuntLabel()
        {
            var row = StageTable.Get(HuntStage);
            string tier = row != null ? StageTable.TierLabel(row.mapTier) : "Normal";
            return $"사냥 Ch.{HuntChapter}-{HuntStageInChapter} ({tier})";
        }

        /// <summary>
        /// 환생용 초기화. Destroy+AddComponent는 Awake의 싱글턴 가드가 gameObject를
        /// 통째로 파괴하므로 쓰지 않고, 제자리에서 되돌린다.
        /// </summary>
        public void ResetForRebirth()
        {
            CurrentWave = 1;
            MaxWaveReached = 1;
            HuntStage = 1;
            Save();
        }

        void Save()
        {
            PlayerPrefs.SetInt(PrefKey + ".cur", CurrentWave);
            PlayerPrefs.SetInt(PrefKey + ".max", MaxWaveReached);
            PlayerPrefs.SetInt(PrefKey + ".hunt", HuntStage);
            PlayerPrefs.Save();
        }

        void Load()
        {
            CurrentWave = Mathf.Max(1, PlayerPrefs.GetInt(PrefKey + ".cur", 1));
            MaxWaveReached = Mathf.Max(CurrentWave, PlayerPrefs.GetInt(PrefKey + ".max", 1));
            HuntStage = Mathf.Clamp(PlayerPrefs.GetInt(PrefKey + ".hunt", CurrentWave), 1, MaxWaveReached);
        }
    }
}
