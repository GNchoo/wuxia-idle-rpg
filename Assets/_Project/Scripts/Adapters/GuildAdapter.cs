using System;
using IdleMvp.Economy;
using UnityEngine;

namespace IdleMvp.Adapters
{
    /// <summary>Local thin guild: create/join, daily donate, one quest.</summary>
    public class GuildAdapter : MonoBehaviour
    {
        public static GuildAdapter Instance { get; private set; }

        public string GuildName { get; private set; } = "";
        public int GuildLevel { get; private set; } = 1;
        public int DonateToday { get; private set; }
        public bool QuestDoneToday { get; private set; }
        public bool Joined => !string.IsNullOrEmpty(GuildName);

        public event Action OnChanged;

        const string PrefKey = "IdleGrow.Maple.Guild";
        static readonly string[] MockMembers = { "길드장", "부길드장", "모험가A" };

        public string[] Members => new[] { "길드장(연습)", "부길드장(연습)", "모험가A(연습)" };

        public bool HasDailyReward => Joined && (!QuestDoneToday || DonateToday < 1);

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

        public string CreateOrJoin(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) name = "초보 길드";
            GuildName = name.Trim();
            if (GuildLevel < 1) GuildLevel = 1;
            Save();
            OnChanged?.Invoke();
            return $"길드 가입: {GuildName}";
        }

        public string Donate()
        {
            if (!Joined) return "길드에 먼저 가입하세요";
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            if (PlayerPrefs.GetString(PrefKey + ".donateDay", "") == today && DonateToday >= 3)
                return "오늘 기부 한도";
            if (WalletAdapter.Instance == null || !WalletAdapter.Instance.TrySpendGold(500))
                return "골드 500 필요";
            if (PlayerPrefs.GetString(PrefKey + ".donateDay", "") != today)
            {
                DonateToday = 0;
                PlayerPrefs.SetString(PrefKey + ".donateDay", today);
            }
            DonateToday++;
            GuildLevel = Mathf.Min(20, GuildLevel + (DonateToday == 3 ? 1 : 0));
            MailService.Instance?.Send("길드 기부 보상", $"{GuildName} 기부 감사", gold: 200, rd: 2);
            Save();
            OnChanged?.Invoke();
            return "기부 완료 · 우편 보상";
        }

        // ── 길드 스킬 (벤치마크 '길드원 스킬 개인 연구' 카피) ──────────────────
        // 골드로 개인 연구, 길드 레벨이 스킬 상한을 연다. 전투 스탯에 실기여 —
        // 길드가 보상 전달만 하던 문제(진단 #7) 해소.
        public const int GuildSkillCount = 3;
        public static readonly string[] GuildSkillNames = { "합격진", "호신강기", "재물 분배" };
        static readonly float[] SkillPerLevel = { 1.0f, 1.0f, 0.5f };   // 공격% / 체력% / 골드%

        public int[] SkillLv { get; private set; } = new int[GuildSkillCount];

        /// <summary>스킬 상한 = 길드 레벨의 절반 + 1 (Lv20 길드 = 스킬 11).</summary>
        public int SkillCap => GuildLevel / 2 + 1;
        public double SkillCost(int i) => 2000 * (SkillLv[i] + 1);

        public float GuildAtkPct => SkillLv[0] * SkillPerLevel[0];
        public float GuildHpPct => SkillLv[1] * SkillPerLevel[1];
        public float GuildGoldPct => SkillLv[2] * SkillPerLevel[2];

        public string BuySkill(int i)
        {
            if (!Joined) return "길드에 먼저 가입하세요";
            if (i < 0 || i >= GuildSkillCount) return "잘못된 스킬";
            if (SkillLv[i] >= SkillCap) return $"연구 상한 — 길드 레벨을 올리면 열립니다 (현재 상한 {SkillCap})";
            double cost = SkillCost(i);
            if (WalletAdapter.Instance == null || !WalletAdapter.Instance.TrySpendGold(cost))
                return $"골드 {cost:0} 필요";
            SkillLv[i]++;
            Save();
            OnChanged?.Invoke();
            return $"{GuildSkillNames[i]} 연구 Lv.{SkillLv[i]} (+{SkillLv[i] * SkillPerLevel[i]:0.#}%)";
        }

        public string CompleteDailyQuest()
        {
            if (!Joined) return "길드에 먼저 가입하세요";
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            if (PlayerPrefs.GetString(PrefKey + ".questDay", "") == today && QuestDoneToday)
                return "오늘 퀘스트 완료됨";
            PlayerPrefs.SetString(PrefKey + ".questDay", today);
            QuestDoneToday = true;
            MailService.Instance?.Send("길드 퀘스트 보상", "일일 사냥 지원",
                gold: 1000, rd: 5, extra: CurrencyId.CompanionTicket, extraAmt: 1);
            Save();
            OnChanged?.Invoke();
            return "길드 퀘스트 완료 · 우편 확인";
        }

        public string StatusText()
        {
            if (!Joined) return "미가입";
            return $"{GuildName} Lv.{GuildLevel} · 기부 {DonateToday}/3 · 퀘스트 {(QuestDoneToday ? "완료" : "가능")}";
        }

        void Save()
        {
            PlayerPrefs.SetString(PrefKey + ".name", GuildName);
            PlayerPrefs.SetInt(PrefKey + ".lv", GuildLevel);
            PlayerPrefs.SetInt(PrefKey + ".donate", DonateToday);
            PlayerPrefs.SetInt(PrefKey + ".quest", QuestDoneToday ? 1 : 0);
            for (int i = 0; i < GuildSkillCount; i++)
                PlayerPrefs.SetInt(PrefKey + ".skill" + i, SkillLv[i]);
            PlayerPrefs.Save();
        }

        void Load()
        {
            GuildName = PlayerPrefs.GetString(PrefKey + ".name", "");
            GuildLevel = Mathf.Max(1, PlayerPrefs.GetInt(PrefKey + ".lv", 1));
            DonateToday = PlayerPrefs.GetInt(PrefKey + ".donate", 0);
            QuestDoneToday = PlayerPrefs.GetInt(PrefKey + ".quest", 0) == 1;
            for (int i = 0; i < GuildSkillCount; i++)
                SkillLv[i] = PlayerPrefs.GetInt(PrefKey + ".skill" + i, 0);
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            if (PlayerPrefs.GetString(PrefKey + ".donateDay", "") != today) DonateToday = 0;
            if (PlayerPrefs.GetString(PrefKey + ".questDay", "") != today) QuestDoneToday = false;
        }
    }
}
