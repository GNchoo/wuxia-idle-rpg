using System.Collections.Generic;
using IdleMvp.Core;
using UnityEngine;

namespace IdleMvp.Progression
{
    public class PlayerGrowth : MonoBehaviour
    {
        public static PlayerGrowth Instance { get; private set; }

        public int Level { get; private set; } = 1;
        public int CurrentXp { get; private set; }
        public int StatPoints { get; private set; }
        public int Grade { get; private set; }
        public int SpecialStatPoints { get; private set; }
        public int SpentStatPoints { get; private set; }

        public int Atk { get; private set; }
        public int Hp { get; private set; } = 100;
        public int Def { get; private set; }

        /// <summary>특별 능력치 한 종류. 캐릭터 레벨로 해금되고 특별 포인트로 올린다.</summary>
        public struct SpecDef
        {
            public string Id, Name, Unit;
            public int UnlockLevel, MaxLevel;
            public float PerLevel;
        }

        /// <summary>해금 레벨 순서 그대로 화면에 뿌린다.</summary>
        public static readonly SpecDef[] SpecCatalog =
        {
            new SpecDef { Id = "DMG",     Name = "최종 데미지",  Unit = "%", UnlockLevel = 1,  MaxLevel = 20, PerLevel = 1.5f },
            new SpecDef { Id = "GOLD",    Name = "금전 획득",    Unit = "%", UnlockLevel = 1,  MaxLevel = 20, PerLevel = 2f },
            new SpecDef { Id = "IDLE",    Name = "방치 보상",    Unit = "%", UnlockLevel = 1,  MaxLevel = 20, PerLevel = 2.5f },
            new SpecDef { Id = "CRIT",    Name = "치명타 확률",  Unit = "%", UnlockLevel = 15, MaxLevel = 20, PerLevel = 0.5f },
            new SpecDef { Id = "CRITDMG", Name = "치명타 피해",  Unit = "%", UnlockLevel = 30, MaxLevel = 20, PerLevel = 2f },
            new SpecDef { Id = "ASPD",    Name = "공격 속도",    Unit = "%", UnlockLevel = 45, MaxLevel = 20, PerLevel = 1f },
            new SpecDef { Id = "EXP",     Name = "경험치 획득",  Unit = "%", UnlockLevel = 60, MaxLevel = 20, PerLevel = 2f },
            new SpecDef { Id = "PIERCE",  Name = "방어 관통",    Unit = "%", UnlockLevel = 75, MaxLevel = 20, PerLevel = 1f },
        };

        /// <summary>해금 뒤 25레벨마다 상한이 +5씩 열린다 (60레벨 이후에도 계속 성장).</summary>
        public const int SpecMaxStep = 25;
        public const int SpecMaxPerStep = 5;
        public const int SpecMaxCap = 60;

        public int SpecMax(string id)
        {
            var d = SpecDefOf(id);
            if (string.IsNullOrEmpty(d.Id)) return 0;
            int over = Mathf.Max(0, Level - d.UnlockLevel);
            return Mathf.Min(SpecMaxCap, d.MaxLevel + (over / SpecMaxStep) * SpecMaxPerStep);
        }

        /// <summary>다음 상한이 열리는 레벨 (이미 최대면 0).</summary>
        public int SpecNextCapLevel(string id)
        {
            var d = SpecDefOf(id);
            if (string.IsNullOrEmpty(d.Id) || SpecMax(id) >= SpecMaxCap) return 0;
            int over = Mathf.Max(0, Level - d.UnlockLevel);
            return d.UnlockLevel + (over / SpecMaxStep + 1) * SpecMaxStep;
        }

        readonly Dictionary<string, int> _specLv = new Dictionary<string, int>();

        public static SpecDef SpecDefOf(string id)
        {
            foreach (var d in SpecCatalog) if (d.Id == id) return d;
            return default;
        }

        public int SpecLevel(string id)
        {
            return _specLv.TryGetValue(id, out int v) ? v : 0;
        }

        /// <summary>현재 수치 (레벨 × 레벨당 증가).</summary>
        public float SpecValue(string id) => SpecLevel(id) * SpecDefOf(id).PerLevel;

        public bool SpecUnlocked(string id) => Level >= SpecDefOf(id).UnlockLevel;

        public bool SpecMaxed(string id) => SpecLevel(id) >= SpecMax(id);

        /// <summary>Grade reward: final damage %.</summary>
        public float SpecFinalDmgPct => SpecValue("DMG");
        /// <summary>Grade reward: gold gain %.</summary>
        public float SpecGoldPct => SpecValue("GOLD");
        /// <summary>Grade reward: idle / offline efficiency %.</summary>
        public float SpecIdlePct => SpecValue("IDLE");
        public float SpecCritRatePct => SpecValue("CRIT");
        public float SpecCritDmgPct => SpecValue("CRITDMG");
        public float SpecAtkSpeedPct => SpecValue("ASPD");
        public float SpecXpPct => SpecValue("EXP");
        public float SpecDefPenPct => SpecValue("PIERCE");

        public event System.Action OnChanged;

        const string PrefKey = "IdleMvp.PlayerGrowth";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LevelXpTable.Load();
            Load();
        }

        public int XpToNext => LevelXpTable.GetRequiredXp(Level);

        /// <summary>레벨 상한(표의 마지막 레벨). 여기 닿으면 경험치는 더 쌓이지 않고
        /// 성장은 경지·환생·장비 쪽으로 넘어간다.</summary>
        public int MaxLevel => LevelXpTable.MaxDefinedLevel;
        public bool IsMaxLevel => Level >= MaxLevel;

        /// <summary>Legacy alias — ability points no longer cost gold (Maple Idle style).</summary>
        public double StatGoldCost => 0;

        public void AddXp(int amount)
        {
            if (amount <= 0) return;
            if (IsMaxLevel)
            {
                if (CurrentXp == 0) return;   // 상한: 경험치를 쌓지 않는다
                CurrentXp = 0;
            }
            else
            {
                CurrentXp += amount;
                int safety = 400;
                while (!IsMaxLevel && CurrentXp >= XpToNext && safety-- > 0)
                {
                    CurrentXp -= XpToNext;
                    Level++;
                    StatPoints += BalanceConfig.Data.statPointsPerLevel;
                }
                if (IsMaxLevel) CurrentXp = 0;
            }
            Save();
            IdleMvp.Core.AchievementService.SetProgress(IdleMvp.Core.AchievementService.Category.Level, Level);
            OnChanged?.Invoke();
        }

        public bool TrySpendStatPoint(string stat)
        {
            if (StatPoints <= 0) return false;
            StatPoints--;
            SpentStatPoints++;
            switch (stat)
            {
                case "ATK": Atk += 1; break;
                case "HP": Hp += 5; break;
                case "DEF": Def += 1; break;
                default: StatPoints++; SpentStatPoints--; return false;
            }

            int pointsPerGrade = BalanceConfig.Data.pointsPerGrade;
            while (SpentStatPoints >= pointsPerGrade)
            {
                SpentStatPoints -= pointsPerGrade;
                Grade++;
                SpecialStatPoints += BalanceConfig.Data.specialStatPerGrade;
            }

            Save();
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>Spend one ability point. Gold is not required (matches Maple Idle).</summary>
        public bool TryBuyStatWithGold(string stat, IdleMvp.Economy.PlayerWallet wallet)
        {
            return TrySpendStatPoint(stat);
        }

        /// <summary>특별 능력치 1레벨 강화. 해금·최대치·포인트를 모두 확인한다.</summary>
        public bool TryUpgradeSpecial(string id)
        {
            var d = SpecDefOf(id);
            if (string.IsNullOrEmpty(d.Id)) return false;
            if (SpecialStatPoints <= 0) return false;
            if (!SpecUnlocked(id) || SpecMaxed(id)) return false;
            _specLv[id] = SpecLevel(id) + 1;
            SpecialStatPoints--;
            Save();
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>구버전 호출부 호환 (MapleMainHud 등).</summary>
        public bool TrySpendSpecialStat(string kind) => TryUpgradeSpecial(kind);

        public float CombatPower =>
            Atk * 10f + Hp * 0.5f + Def * 8f + Level * 15f
            + SpecFinalDmgPct * 4f + SpecGoldPct * 1.5f + SpecIdlePct * 1.2f
            + SpecCritRatePct * 6f + SpecCritDmgPct * 2f + SpecAtkSpeedPct * 5f
            + SpecXpPct * 1f + SpecDefPenPct * 3f;

        /// <summary>
        /// 환생용 초기화. 컴포넌트를 Destroy+AddComponent로 되살리면 Awake의 싱글턴 가드가
        /// gameObject 전체를 파괴해버리므로, 제자리에서 필드만 되돌린다.
        /// </summary>
        public void ResetForRebirth()
        {
            Level = 1;
            CurrentXp = 0;
            StatPoints = 0;
            Grade = 0;
            SpecialStatPoints = 0;
            SpentStatPoints = 0;
            Atk = 10;
            Hp = 100;
            Def = 5;
            _specLv.Clear();
            Save();
            OnChanged?.Invoke();
        }

        void Save()
        {
            var json = JsonUtility.ToJson(new SaveData
            {
                level = Level,
                xp = CurrentXp,
                statPoints = StatPoints,
                grade = Grade,
                special = SpecialStatPoints,
                spent = SpentStatPoints,
                atk = Atk,
                hp = Hp,
                def = Def,
                specIds = new List<string>(_specLv.Keys),
                specLvs = new List<int>(_specLv.Values)
            });
            PlayerPrefs.SetString(PrefKey, json);
            PlayerPrefs.Save();
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(PrefKey))
            {
                Atk = 10;
                Hp = 100;
                Def = 5;
                return;
            }
            var data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(PrefKey));
            if (data == null) return;
            Level = Mathf.Max(1, data.level);
            CurrentXp = data.xp;
            StatPoints = data.statPoints;
            Grade = data.grade;
            SpecialStatPoints = data.special;
            SpentStatPoints = data.spent;
            Atk = data.atk;
            Hp = data.hp > 0 ? data.hp : 100;
            Def = data.def;
            _specLv.Clear();
            if (data.specIds != null && data.specLvs != null)
                for (int i = 0; i < data.specIds.Count && i < data.specLvs.Count; i++)
                    _specLv[data.specIds[i]] = data.specLvs[i];
            // 구버전 세이브(퍼센트 값)를 레벨로 환산해 이어받는다
            if (_specLv.Count == 0)
            {
                if (data.specDmg > 0f) _specLv["DMG"] = Mathf.RoundToInt(data.specDmg / 1.5f);
                if (data.specGold > 0f) _specLv["GOLD"] = Mathf.RoundToInt(data.specGold / 2f);
                if (data.specIdle > 0f) _specLv["IDLE"] = Mathf.RoundToInt(data.specIdle / 2.5f);
            }
        }

        [System.Serializable]
        class SaveData
        {
            public int level, xp, statPoints, grade, special, spent, atk, hp, def;
            public float specDmg, specGold, specIdle;      // 구버전 호환용
            public List<string> specIds;
            public List<int> specLvs;
        }
    }
}
