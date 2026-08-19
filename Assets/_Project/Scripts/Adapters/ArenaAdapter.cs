using System;
using IdleMvp.Economy;
using IdleMvp.Progression;
using UnityEngine;

namespace IdleMvp.Adapters
{
    /// <summary>Async arena: CP-ratio bot matches, daily challenges.</summary>
    public class ArenaAdapter : MonoBehaviour
    {
        public static ArenaAdapter Instance { get; private set; }

        public int Score { get; private set; } = 1000;
        public int Tier => Mathf.Clamp(Score / 500, 0, 5);
        public int ChallengesToday { get; private set; }
        public const int MaxDaily = 5;

        public event Action OnChanged;

        static readonly string[] TierNames = { "무생", "하수", "고수", "절정", "화경", "천하제일" };
        const string PrefKey = "IdleGrow.Maple.Arena";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Load();
        }

        public string TierName => TierNames[Tier];

        /// <summary>랭크 보너스 — 티어당 공격 +1% (무생 0% ~ 천하제일 +5%).
        /// 아레나가 보상 전달만 하던 문제(흐름도 진단 #7)의 마지막 조각.
        /// 패배 시 점수가 깎여 티어가 내려가면 보너스도 같이 내려간다.</summary>
        public float TierAtkPct => Tier * 1f;

        public string StatusText() =>
            $"{TierName} · 점수 {Score} · 오늘 {ChallengesToday}/{MaxDaily} · 랭크 보너스 공격 +{TierAtkPct:0}%";

        public string[] ListOpponents()
        {
            float cp = CombatPowerService.GetTotalCp();
            return new[]
            {
                $"도전자A · CP {Mathf.RoundToInt(cp * 0.85f)}",
                $"도전자B · CP {Mathf.RoundToInt(cp * 1.0f)}",
                $"도전자C · CP {Mathf.RoundToInt(cp * 1.15f)}"
            };
        }

        public string Challenge(int opponentIndex)
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            if (PlayerPrefs.GetString(PrefKey + ".day", "") != today)
            {
                ChallengesToday = 0;
                PlayerPrefs.SetString(PrefKey + ".day", today);
            }
            if (ChallengesToday >= MaxDaily) return "오늘 도전 횟수 소진";

            float myCp = CombatPowerService.GetTotalCp();
            float[] mul = { 0.85f, 1f, 1.15f };
            float their = myCp * mul[Mathf.Clamp(opponentIndex, 0, 2)];
            float winChance = Mathf.Clamp01(0.5f + (myCp - their) / Mathf.Max(1f, their) * 0.35f);
            bool win = UnityEngine.Random.value < winChance;
            ChallengesToday++;
            IdleMvp.Core.DailyMissionService.Increment("arena");
            IdleMvp.Core.AchievementService.IncrementProgress(IdleMvp.Core.AchievementService.Category.Arena);

            if (win)
            {
                Score += 25;
                MailService.Instance?.Send("아레나 승리", $"{TierName} 도전 성공",
                    gold: 800, rd: 8, extra: CurrencyId.BlueDiamond, extraAmt: 5);
                Save();
                OnChanged?.Invoke();
                return $"승리! 점수 {Score} · 우편 보상";
            }

            Score = Mathf.Max(0, Score - 10);
            Save();
            OnChanged?.Invoke();
            return $"패배… 점수 {Score}";
        }

        void Save()
        {
            PlayerPrefs.SetInt(PrefKey + ".score", Score);
            PlayerPrefs.SetInt(PrefKey + ".ch", ChallengesToday);
            PlayerPrefs.Save();
        }

        void Load()
        {
            Score = PlayerPrefs.GetInt(PrefKey + ".score", 1000);
            ChallengesToday = PlayerPrefs.GetInt(PrefKey + ".ch", 0);
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            if (PlayerPrefs.GetString(PrefKey + ".day", "") != today) ChallengesToday = 0;
        }
    }
}
