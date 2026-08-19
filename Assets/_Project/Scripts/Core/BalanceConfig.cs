using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleMvp.Core
{
    [Serializable]
    public class DungeonTicketConfig
    {
        public string id;
        public int ticketsPerDay;
        public int adTickets;
    }

    [Serializable]
    public class BalanceConfigData
    {
        public float offlineCapHours = 8f;
        public int fastRewardPerDay = 3;   // seed·ShopCatalog와 정합
        public float fastRewardHoursEquivalent = 1f;
        public float stageEnemyGrowth = 1.08f;
        public float bossMultiplier = 1.6f;
        public float clearCpRatioMin = 0.9f;
        public float clearCpRatioMax = 1.1f;
        public int statPointsPerLevel = 5;
        public int pointsPerGrade = 25;
        public int specialStatPerGrade = 1;
        public List<DungeonTicketConfig> dungeons = new List<DungeonTicketConfig>();
        public List<string> equipmentSlots = new List<string>();
        public List<string> rarities = new List<string>();
        public int mvpChapters = 5;
        public int stagesPerChapter = 10;
    }

    public static class BalanceConfig
    {
        static BalanceConfigData _data;
        public static BalanceConfigData Data
        {
            get
            {
                if (_data == null)
                    Load();
                return _data;
            }
        }

        public static void Load()
        {
            var asset = Resources.Load<TextAsset>("balance-seed");
            if (asset == null)
            {
                // Fallback: StreamingAssets-style path under _Project/Data via TextAsset in Resources
                Debug.LogWarning("[IdleMvp] balance-seed not in Resources. Using defaults.");
                _data = new BalanceConfigData();
                return;
            }

            _data = JsonUtility.FromJson<BalanceConfigData>(asset.text);
            if (_data == null)
                _data = new BalanceConfigData();
        }
    }
}
