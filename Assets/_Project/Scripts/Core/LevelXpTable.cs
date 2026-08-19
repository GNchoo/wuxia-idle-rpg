using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleMvp.Core
{
    [Serializable]
    public class LevelXpEntry
    {
        public int level;
        public int requiredXP;
    }

    [Serializable]
    class LevelXpListWrapper
    {
        public List<LevelXpEntry> items;
    }

    public static class LevelXpTable
    {
        static Dictionary<int, int> _map;
        static int _maxLevel = 1;

        public static void Load()
        {
            _map = new Dictionary<int, int>();
            var asset = Resources.Load<TextAsset>("level-xp");
            if (asset == null)
            {
                Debug.LogWarning("[IdleMvp] level-xp not in Resources. Using fallback curve.");
                for (int i = 1; i <= 100; i++)
                {
                    _map[i] = Mathf.RoundToInt(50 + i * i * 10 * Mathf.Pow(1.04f, Mathf.Max(0, i - 30)));
                    _maxLevel = i;
                }
                return;
            }

            // Unity JsonUtility cannot deserialize top-level arrays; wrap if needed.
            string json = asset.text.Trim();
            if (json.StartsWith("["))
                json = "{\"items\":" + json + "}";

            var wrapper = JsonUtility.FromJson<LevelXpListWrapper>(json);
            if (wrapper?.items == null)
            {
                Debug.LogError("[IdleMvp] Failed to parse level-xp.json");
                return;
            }

            foreach (var e in wrapper.items)
            {
                _map[e.level] = e.requiredXP;
                if (e.level > _maxLevel)
                    _maxLevel = e.level;
            }
        }

        public static int GetRequiredXp(int level)
        {
            if (_map == null)
                Load();
            if (_map.TryGetValue(level, out int xp))
                return xp;
            // 표 밖(현재 60레벨 초과)은 마지막 값에서 같은 비율로 이어 붙인다.
            // 예전 식(10000 * 1.12^n)은 60레벨 33만 → 61레벨 1.1만으로 곤두박질쳤다.
            if (!_map.TryGetValue(_maxLevel, out int lastXp) || lastXp <= 0)
                lastXp = 10000;
            return Mathf.RoundToInt(lastXp * Mathf.Pow(1.12f, Mathf.Max(1, level - _maxLevel)));
        }

        public static int MaxDefinedLevel
        {
            get
            {
                if (_map == null)
                    Load();
                return _maxLevel;
            }
        }
    }
}
