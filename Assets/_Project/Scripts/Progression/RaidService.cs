using System;
using IdleMvp.Combat;
using IdleMvp.Core;
using IdleMvp.Economy;
using IdleMvp.Progression;
using UnityEngine;

namespace IdleMvp.Progression
{
    /// <summary>
    /// Daily world boss raid — 단계제 (벤치마크 카피: '총 20단계, 클리어할 때마다
    /// 다음 단계로 넘어가며 HP가 급격히 높아진다'). 하루 1회, 클리어 시 단계 영구 상승.
    /// </summary>
    public class RaidService : MonoBehaviour
    {
        public static RaidService Instance { get; private set; }

        public const int MaxStage = 20;

        /// <summary>난이도 3단계. 권장 전투력을 크게 벌려 장기 목표를 만든다.</summary>
        public enum RaidDifficulty
        {
            Easy = 0,
            Normal = 1,
            Hard = 2
        }

        public static readonly string[] DifficultyNames = { "약", "중", "강" };
        /// <summary>요구 전투력 배수 — 벤치마크의 난이도 간 도약(×50, ×1000)을 그대로 옮겼다.</summary>
        public static readonly float[] DifficultyCpMul = { 1f, 50f, 1000f };
        /// <summary>보상 배수.</summary>
        public static readonly float[] DifficultyReward = { 1f, 6f, 30f };
        /// <summary>
        /// 적 공격력 배수. 요구 전투력(×1/×50/×1000)을 그대로 쓰면 한 대에 죽으므로
        /// 지수를 눌러(≈^0.65) 완화했다. HP만 늘리면 난이도가 '오래 때리기'가 된다.
        /// </summary>
        public static readonly float[] DifficultyAtkMul = { 1f, 12f, 90f };

        public static float AtkMulOf(RaidDifficulty d) =>
            DifficultyAtkMul[Mathf.Clamp((int)d, 0, DifficultyAtkMul.Length - 1)];

        /// <summary>주간 입장권 — 매주 3장. 없으면 '도전 모드'로만 들어간다(보상 없음).</summary>
        public const int WeeklyTickets = 3;
        public int TicketsLeft { get; private set; } = WeeklyTickets;
        public RaidDifficulty Difficulty { get; private set; } = RaidDifficulty.Easy;
        /// <summary>이번 판이 입장권을 쓴 정식 도전인가 (아니면 보상 없는 연습).</summary>
        public bool RewardRun { get; private set; }

        public static string DifficultyName(RaidDifficulty d) =>
            DifficultyNames[Mathf.Clamp((int)d, 0, DifficultyNames.Length - 1)];

        /// <summary>난이도별 권장 전투력.</summary>
        public static float RequiredCp(RaidDifficulty d)
        {
            var row = StageTable.Get(StageProgress.Instance != null
                ? StageProgress.Instance.StageIndex : 1);
            float rec = (row != null ? row.recommendedCp : 100f) * 1.2f;
            return rec * DifficultyCpMul[Mathf.Clamp((int)d, 0, DifficultyCpMul.Length - 1)];
        }

        public static bool CanEnter(RaidDifficulty d) =>
            CombatPowerService.GetTotalCp() >= RequiredCp(d);

        /// <summary>난이도를 고른다. 전투력이 모자라면 거절.</summary>
        public string SelectDifficulty(RaidDifficulty d)
        {
            if (!CanEnter(d))
                return $"{DifficultyName(d)} 난이도는 전투력 부족";
            Difficulty = d;
            RefreshWeek();
            Save();
            OnChanged?.Invoke();
            return $"{DifficultyName(d)} 난이도 선택";
        }

        /// <summary>입장권을 써서 정식 도전으로 들어간다. 없으면 연습(보상 없음).</summary>
        public string UseTicket()
        {
            RefreshWeek();
            if (TicketsLeft <= 0)
            {
                RewardRun = false;
                Save();
                OnChanged?.Invoke();
                return "입장권 소진 — 연습 도전(보상 없음)으로 진행합니다";
            }
            TicketsLeft--;
            RewardRun = true;
            Save();
            OnChanged?.Invoke();
            return $"입장권 사용 (남은 {TicketsLeft}장)";
        }

        /// <summary>주 1회 입장권 충전 (월요일 기준 주차).</summary>
        void RefreshWeek()
        {
            var now = DateTime.UtcNow;
            var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
            string week = now.Year + "-" + cal.GetWeekOfYear(now,
                System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            if (PlayerPrefs.GetString(PrefKey + ".week", "") == week) return;
            PlayerPrefs.SetString(PrefKey + ".week", week);
            TicketsLeft = WeeklyTickets;
            PlayerPrefs.SetInt(PrefKey + ".tickets", TicketsLeft);
            PlayerPrefs.Save();
        }

        public float BossMaxHp { get; private set; } = 50000f;
        public float BossHp { get; private set; } = 50000f;
        public float MyContribution { get; private set; }
        public bool ClearedToday { get; private set; }
        public bool ParticipatedToday { get; private set; }
        /// <summary>현재 도전 단계 (1..20). 클리어할 때마다 +1, 영구 저장.</summary>
        public int Stage { get; private set; } = 1;

        public event Action OnChanged;

        const string PrefKey = "IdleGrow.Maple.Raid";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Load();
            RefreshWeek();
            RefreshDay();
        }

        void RefreshDay()
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            if (PlayerPrefs.GetString(PrefKey + ".day", "") == today) return;
            PlayerPrefs.SetString(PrefKey + ".day", today);
            // 단계가 오를수록 HP가 가파르게 — CP 비례 기본치 × 단계 배율(1.3^n)
            float stageMul = Mathf.Pow(1.3f, Stage - 1);
            float diffMul = DifficultyCpMul[Mathf.Clamp((int)Difficulty, 0, DifficultyCpMul.Length - 1)];
            BossMaxHp = (40000f + CombatPowerService.GetTotalCp() * 8f) * stageMul * diffMul;
            BossHp = BossMaxHp;
            MyContribution = 0f;
            ClearedToday = false;
            ParticipatedToday = false;
            Save();
        }

        public string StatusText()
        {
            RefreshDay();
            float pct = BossMaxHp > 0 ? BossHp / BossMaxHp : 0f;
            return $"월드보스 {Stage}단계 [{DifficultyName(Difficulty)}] · HP {pct * 100f:0.#}%"
                + $" · 기여 {MyContribution:0} · 입장권 {TicketsLeft}/{WeeklyTickets}"
                + $" · {(ClearedToday ? "클리어" : ParticipatedToday ? "참여중" : "대기")}";
        }

        /// <summary>Deal a raid strike (from UI button or WorldBoss mode).</summary>
        public string Strike()
        {
            RefreshDay();
            if (ClearedToday) return "오늘 이미 클리어";
            float coef = 2.2f;
            float dmg = Mathf.Max(50f,
                CombatPowerService.GetAtk() * coef * CombatPowerService.GetOutgoingMul());
            BossHp = Mathf.Max(0f, BossHp - dmg);
            MyContribution += dmg;
            ParticipatedToday = true;

            if (BossHp <= 0f)
            {
                ClearedToday = true;
                int tier = MyContribution >= BossMaxHp * 0.25f ? 3 : MyContribution >= BossMaxHp * 0.1f ? 2 : 1;
                // 단계 비례 보상 — 높은 단계일수록 도전할 이유가 있다
                float dmul = DifficultyReward[Mathf.Clamp((int)Difficulty, 0, DifficultyReward.Length - 1)];
                double gold = 2000 * tier * (1.0 + (Stage - 1) * 0.25) * dmul;
                double rd = (10 * tier + (Stage - 1) * 2) * dmul;
                int clearedStage = Stage;
                Stage = Mathf.Min(MaxStage, Stage + 1);
                string dn = DifficultyName(Difficulty);
                if (RewardRun)
                {
                    MailService.Instance?.Send("월드보스 클리어", $"{clearedStage}단계 [{dn}] · 기여 티어 {tier}",
                        gold: gold, rd: rd, extra: CurrencyId.MonsterPoint,
                        extraAmt: (5 * tier + clearedStage) * Mathf.RoundToInt(dmul));
                }
                Save();
                OnChanged?.Invoke();
                if (!RewardRun)
                    return $"{clearedStage}단계 [{dn}] 연습 클리어 — 보상은 입장권을 써야 받습니다";
                return clearedStage >= MaxStage
                    ? $"최종 {clearedStage}단계 [{dn}] 클리어! 티어 {tier} 보상 우편"
                    : $"{clearedStage}단계 [{dn}] 클리어! 다음 도전은 {Stage}단계 · 보상 우편";
            }

            Save();
            OnChanged?.Invoke();
            return $"타격 -{dmg:0} · 남은 HP {BossHp:0}";
        }

        public bool TryEnterWorldBoss(FieldAutoHuntController battle, out string reason)
        {
            reason = "";
            RefreshDay();
            if (ClearedToday)
            {
                reason = "오늘 클리어 완료";
                return false;
            }
            float rec = RequiredCp(Difficulty);
            var raidGate = new StageRow
            {
                recommendedCp = rec,
                minCp = rec * BalanceConfig.Data.clearCpRatioMin,
                softCp = rec * BalanceConfig.Data.clearCpRatioMax
            };
            if (!CombatPowerService.CanEnterStage(raidGate, out reason))
            {
                if (string.IsNullOrEmpty(reason)) reason = "전투력 부족";
                return false;
            }
            return true;
        }

        void Save()
        {
            PlayerPrefs.SetFloat(PrefKey + ".hp", BossHp);
            PlayerPrefs.SetFloat(PrefKey + ".max", BossMaxHp);
            PlayerPrefs.SetFloat(PrefKey + ".contrib", MyContribution);
            PlayerPrefs.SetInt(PrefKey + ".clear", ClearedToday ? 1 : 0);
            PlayerPrefs.SetInt(PrefKey + ".part", ParticipatedToday ? 1 : 0);
            PlayerPrefs.SetInt(PrefKey + ".stage", Stage);
            PlayerPrefs.SetInt(PrefKey + ".tickets", TicketsLeft);
            PlayerPrefs.SetInt(PrefKey + ".diff", (int)Difficulty);
            PlayerPrefs.SetInt(PrefKey + ".rewardRun", RewardRun ? 1 : 0);
            PlayerPrefs.Save();
        }

        void Load()
        {
            BossHp = PlayerPrefs.GetFloat(PrefKey + ".hp", 50000f);
            BossMaxHp = PlayerPrefs.GetFloat(PrefKey + ".max", 50000f);
            MyContribution = PlayerPrefs.GetFloat(PrefKey + ".contrib", 0f);
            ClearedToday = PlayerPrefs.GetInt(PrefKey + ".clear", 0) == 1;
            ParticipatedToday = PlayerPrefs.GetInt(PrefKey + ".part", 0) == 1;
            Stage = Mathf.Clamp(PlayerPrefs.GetInt(PrefKey + ".stage", 1), 1, MaxStage);
            TicketsLeft = Mathf.Clamp(PlayerPrefs.GetInt(PrefKey + ".tickets", WeeklyTickets), 0, WeeklyTickets);
            Difficulty = (RaidDifficulty)Mathf.Clamp(PlayerPrefs.GetInt(PrefKey + ".diff", 0), 0, 2);
            RewardRun = PlayerPrefs.GetInt(PrefKey + ".rewardRun", 0) == 1;
        }
    }
}
