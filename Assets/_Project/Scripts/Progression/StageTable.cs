using IdleMvp.Core;
using UnityEngine;

namespace IdleMvp.Progression
{
    [System.Serializable]
    public class StageRow
    {
        public int index;
        public int chapter;
        public int stage;
        public bool boss;
        public float enemyHp;
        public float enemyAtk;
        /// <summary>적 방어력. 데미지 감쇠에 쓰이고, 방어 관통이 이 값을 깎는다.</summary>
        public float enemyDef;
        public float recommendedCp;
        public float clearGold;
        public int clearXp;
        /// <summary>0=normal, 1=hard, 2=hell (display tier).</summary>
        public int mapTier;
        public float minCp;
        public float softCp;
        public float xpPerKill;
        public float mobHpMul = 1f;
        /// <summary>동시 최대 마릿수 (0이면 코드 기본값 사용).</summary>
        public int spawnCount;
        /// <summary>젠 간격 초 (0이면 코드 기본값 사용).</summary>
        public float spawnDelay;
        /// <summary>적 조합 변형 (0~2). 2~3스테이지마다 바뀌어 같은 챕터도 지겹지 않게 한다.</summary>
        public int mobPreset;
        public float bossTimeLimit = 60f;
    }

    [System.Serializable]
    class StageTableFile
    {
        public StageRow[] stages;
    }

    public static class StageTable
    {
        static StageRow[] _rows;

        public static void Load()
        {
            var asset = Resources.Load<TextAsset>("stage-table");
            if (asset != null)
            {
                var file = JsonUtility.FromJson<StageTableFile>(asset.text);
                if (file != null && file.stages != null && file.stages.Length > 0)
                {
                    _rows = file.stages;
                    for (int i = 0; i < _rows.Length; i++)
                        EnsureDerived(ref _rows[i]);
                    return;
                }
            }

            int n = BalanceConfig.Data.mvpChapters * BalanceConfig.Data.stagesPerChapter;
            _rows = new StageRow[n];
            for (int i = 0; i < n; i++)
            {
                int idx = i + 1;
                int ch = i / BalanceConfig.Data.stagesPerChapter + 1;
                int st = i % BalanceConfig.Data.stagesPerChapter + 1;
                bool boss = st == BalanceConfig.Data.stagesPerChapter;
                float growth = Mathf.Pow(BalanceConfig.Data.stageEnemyGrowth, idx - 1);
                float bossMul = boss ? BalanceConfig.Data.bossMultiplier : 1f;
                int tier = ch <= 2 ? 0 : ch <= 4 ? 1 : 2;
                float rec = 100f * growth * bossMul;
                _rows[i] = new StageRow
                {
                    index = idx,
                    chapter = ch,
                    stage = st,
                    boss = boss,
                    enemyHp = 200f * growth * bossMul,
                    enemyAtk = 8f * growth * bossMul,
                    enemyDef = 100f * growth * 0.6f * 0.35f,
                    recommendedCp = rec,
                    clearGold = 20f + idx * 5f,
                    clearXp = 8 + idx * 2,
                    mapTier = tier,
                    minCp = rec * BalanceConfig.Data.clearCpRatioMin,
                    softCp = rec * BalanceConfig.Data.clearCpRatioMax,
                    xpPerKill = Mathf.Max(1f, (8 + idx * 2) * 0.12f),
                    mobHpMul = 1f + tier * 0.15f,
                    spawnCount = 8,
                    spawnDelay = 1.5f,
                    mobPreset = ((i - 1) / 3) % 3,
                    bossTimeLimit = boss ? 60f + tier * 15f : 60f
                };
            }
        }

        static void EnsureDerived(ref StageRow r)
        {
            if (r.minCp <= 0f) r.minCp = r.recommendedCp * BalanceConfig.Data.clearCpRatioMin;
            if (r.softCp <= 0f) r.softCp = r.recommendedCp * BalanceConfig.Data.clearCpRatioMax;
            if (r.xpPerKill <= 0f) r.xpPerKill = Mathf.Max(1f, r.clearXp * 0.12f);
            if (r.mobHpMul <= 0f) r.mobHpMul = 1f;
            if (r.bossTimeLimit <= 0f) r.bossTimeLimit = 60f;
        }

        public static StageRow Get(int stageIndex)
        {
            if (_rows == null) Load();
            if (_rows == null || _rows.Length == 0) return null;
            int i = Mathf.Clamp(stageIndex - 1, 0, _rows.Length - 1);
            return _rows[i];
        }

        public static int Count
        {
            get
            {
                if (_rows == null) Load();
                return _rows != null ? _rows.Length : 0;
            }
        }

        public static string TierLabel(int tier)
        {
            switch (tier)
            {
                case 1: return "Hard";
                case 2: return "Hell";
                default: return "Normal";
            }
        }
    }
}
