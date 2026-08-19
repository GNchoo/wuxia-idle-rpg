using System;
using IdleMvp.Core;
using IdleMvp.Economy;
using UnityEngine;
using CurrencyWallet = IdleMvp.Economy.CurrencyWallet;

namespace IdleMvp.Progression
{
    public enum DungeonId
    {
        GoldTemple = 0,
        EnhanceTower = 1,
        EquipmentRoom = 2,
        TrainingGround = 3   // 수련장 — 수련 증표·명성 훈장 (벤치마크 '용사의 수련장' 카피)
    }

    /// <summary>난이도 3단계 — 열쇠 소모는 같고, 요구 전투력과 보상이 함께 커진다.</summary>
    public enum DungeonDifficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }

    /// <summary>
    /// Daily ticket dungeons: gold / enhance stones / equipment XP (slot boost).
    /// </summary>
    public class DungeonService : MonoBehaviour
    {
        public static DungeonService Instance { get; private set; }

        public int[] TicketsLeft { get; private set; } = { 2, 2, 2, 2 };
        public string LastDayKey { get; private set; }

        public event Action OnChanged;

        const string PrefKey = "IdleGrow.Dungeon";

        static readonly string[] Names = { "황금 비경", "연무의 탑", "장비의 밀실", "수련장" };

        public static readonly string[] DifficultyNames = { "하", "중", "상" };
        /// <summary>보상 배수. 열쇠는 똑같이 하나 쓰므로 상위 난이도가 항상 이득이다.</summary>
        public static readonly float[] DifficultyReward = { 1f, 2.5f, 6f };
        /// <summary>현재 스테이지 권장 전투력 대비 요구치.</summary>
        public static readonly float[] DifficultyCpReq = { 0.7f, 1.5f, 3.0f };

        public static string DifficultyName(DungeonDifficulty d) =>
            DifficultyNames[Mathf.Clamp((int)d, 0, DifficultyNames.Length - 1)];

        /// <summary>난이도 입장에 필요한 전투력 (현재 스테이지 권장치 기준).</summary>
        public static float RequiredCp(DungeonDifficulty d)
        {
            var row = StageTable.Get(StageProgress.Instance != null
                ? StageProgress.Instance.StageIndex : 1);
            float rec = row != null ? row.recommendedCp : 100f;
            return rec * DifficultyCpReq[Mathf.Clamp((int)d, 0, DifficultyCpReq.Length - 1)];
        }

        public static bool CanEnter(DungeonDifficulty d) =>
            CombatPowerService.GetTotalCp() >= RequiredCp(d);

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Load();
            RefreshDay();
        }

        public string NameOf(DungeonId id) => Names[(int)id];

        /// <summary>열쇠제 (벤치마크 카피): 매일 3개 충전, 최대 10개 누적 — 당일 소진 강제가 없다.</summary>
        public const int KeysPerDay = 3;
        public const int KeyCap = 10;

        int _adMask;   // 던전별 광고 열쇠 1회/일 사용 여부 (비트)

        public void RefreshDay()
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            if (LastDayKey == today) return;
            bool firstEver = string.IsNullOrEmpty(LastDayKey);
            LastDayKey = today;
            _adMask = 0;
            for (int i = 0; i < TicketsLeft.Length; i++)
                TicketsLeft[i] = firstEver
                    ? KeysPerDay
                    : Mathf.Min(KeyCap, Mathf.Max(0, TicketsLeft[i]) + KeysPerDay);
            Save();
            OnChanged?.Invoke();
        }

        public bool AdKeyAvailable(DungeonId id) => (_adMask & (1 << (int)id)) == 0;

        /// <summary>광고 시청 보상으로 열쇠 +1 (던전별 일 1회). 광고 재생은 호출자(AdBridge) 소관.</summary>
        public string GrantAdKey(DungeonId id)
        {
            RefreshDay();
            int i = (int)id;
            if (!AdKeyAvailable(id)) return "오늘은 이미 광고 열쇠를 받았습니다";
            _adMask |= 1 << i;
            TicketsLeft[i] = Mathf.Min(KeyCap, TicketsLeft[i] + 1);
            Save();
            OnChanged?.Invoke();
            return $"{NameOf(id)} 열쇠 +1 (남은 {TicketsLeft[i]}개)";
        }

        public bool TryRun(DungeonId id, PlayerGrowth growth, PlayerWallet wallet, EquipmentService equip,
            out string message, DungeonDifficulty difficulty = DungeonDifficulty.Easy)
        {
            RefreshDay();
            int i = (int)id;
            if (TicketsLeft[i] <= 0)
            {
                message = "열쇠가 없습니다 — 매일 3개 충전 (최대 10개), 광고로 +1 가능.";
                return false;
            }
            // 전투력이 모자라면 열쇠를 쓰지 않고 돌려보낸다
            if (!CanEnter(difficulty))
            {
                message = $"{DifficultyName(difficulty)} 난이도는 전투력 {UiNum(RequiredCp(difficulty))} 필요";
                return false;
            }

            TicketsLeft[i]--;
            int stage = StageProgress.Instance != null ? StageProgress.Instance.StageIndex : 1;
            float scale = (1f + stage * 0.08f)
                * DifficultyReward[Mathf.Clamp((int)difficulty, 0, DifficultyReward.Length - 1)];
            string dn = DifficultyName(difficulty);

            switch (id)
            {
                case DungeonId.GoldTemple:
                    double gold = 80 * scale;
                    wallet?.AddGold(gold);
                    message = $"{NameOf(id)} [{dn}] 클리어! 골드 +{gold:0}";
                    break;
                case DungeonId.EnhanceTower:
                    double stone = 3 * scale;
                    equip?.AddEnhanceStones(stone);
                    CurrencyWallet.Instance?.Add(CurrencyId.ScrollTrace, 5 * scale);
                    CurrencyWallet.Instance?.Add(CurrencyId.StarForceScroll, 1 * scale);
                    message = $"{NameOf(id)} [{dn}] 클리어! 강화석·주문의흔적·스타포스";
                    break;
                case DungeonId.TrainingGround:
                    double token = 6 * scale;
                    double medal = System.Math.Max(1, System.Math.Floor(1 * scale));
                    CurrencyWallet.Instance?.Add(CurrencyId.TrainingToken, token);
                    CurrencyWallet.Instance?.Add(CurrencyId.HonorMedal, medal);
                    message = $"{NameOf(id)} [{dn}] 클리어! 수련 증표 +{token:0} · 명성 훈장 +{medal:0}";
                    break;
                default:
                    equip?.AddEnhanceStones(2 * scale);
                    growth?.AddXp(Mathf.FloorToInt(15 * scale));
                    CurrencyWallet.Instance?.Add(CurrencyId.MonsterPoint, 8 * scale);
                    CurrencyWallet.Instance?.Add(CurrencyId.WeaponTicket, 1);
                    CurrencyWallet.Instance?.Add(CurrencyId.ArmorStone, 3 * scale);
                    message = $"{NameOf(id)} [{dn}] 클리어! MP·무기티켓·아머스톤";
                    break;
            }

            Save();
            IdleMvp.Core.DailyMissionService.Increment("dungeon");
            IdleMvp.Core.QuestService.Notify(IdleMvp.Core.QuestService.Kind.Dungeon);
            IdleMvp.Core.AchievementService.IncrementProgress(IdleMvp.Core.AchievementService.Category.Dungeon);
            OnChanged?.Invoke();
            return true;
        }

        static string UiNum(float v)
        {
            if (v >= 1e8f) return (v / 1e8f).ToString("0.#") + "억";
            if (v >= 1e4f) return (v / 1e4f).ToString("0.#") + "만";
            return v.ToString("0");
        }

        void Save()
        {
            var data = new SaveData
            {
                day = LastDayKey,
                t0 = TicketsLeft[0],
                t1 = TicketsLeft[1],
                t2 = TicketsLeft[2],
                t3 = TicketsLeft.Length > 3 ? TicketsLeft[3] : 2,
                adMask = _adMask
            };
            PlayerPrefs.SetString(PrefKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(PrefKey)) return;
            var data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(PrefKey));
            if (data == null) return;
            LastDayKey = data.day;
            // 구세이브(3종)엔 t3 필드가 없다 — JsonUtility는 없는 필드를 초기값(-1)으로
            // 남기므로, -1이면 신설 던전에 당일 기본 2회를 준다
            TicketsLeft = new[] { data.t0, data.t1, data.t2, data.t3 < 0 ? 2 : data.t3 };
            _adMask = data.adMask;
        }

        [Serializable]
        class SaveData
        {
            public string day;
            public int t0, t1, t2;
            public int t3 = -1;   // -1 = 구세이브 (필드 자체가 없던 저장본)
            public int adMask;
        }
    }
}
