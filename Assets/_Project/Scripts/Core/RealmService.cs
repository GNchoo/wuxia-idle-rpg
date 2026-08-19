using System;
using UnityEngine;

namespace IdleMvp.Core
{
    /// <summary>
    /// 무력 경지(境地) 시스템.
    ///
    /// 무협에서 경지는 레벨과 별개다. 레벨은 쌓이는 것이고 경지는 **벽을 깨는 것**이라,
    /// 조건을 채웠다고 저절로 오르지 않는다. 깨달음(悟) 퀘스트를 통과해야 다음 단계로 간다.
    ///
    /// 경지 자체는 강호 공용이지만 **부르는 이름은 세력마다 다르다**.
    /// 정파는 격식대로, 사파는 실력 위주로, 마도는 마(魔)를 붙여 부른다.
    /// </summary>
    public static class RealmService
    {
        public static event Action OnChanged;

        const string PrefRealm = "IdleGrow.Realm.Index";
        const string PrefQuest = "IdleGrow.Realm.QuestActive";

        public class RealmDef
        {
            public int Index;
            public int ReqLevel;        // 이 레벨을 넘겨야 깨달음이 찾아온다
            public string Orthodox;     // 정파 명칭
            public string Unorthodox;   // 사파 명칭
            public string Demonic;      // 마도 명칭
            public string Common;       // 세력 없을 때(평민)
            public Color Aura;          // 경지 이펙트 색
            public float AuraScale;     // 이펙트 크기 배수
            public string Insight;      // 깨달음 한 줄 (퀘스트 제목)
        }

        /// <summary>
        /// 8단계. 위로 갈수록 요구 레벨 간격이 벌어진다 — 벽은 높을수록 두껍다.
        /// </summary>
        public static readonly RealmDef[] All =
        {
            new RealmDef { Index = 0, ReqLevel = 1,
                Orthodox = "삼류", Unorthodox = "하수", Demonic = "마졸", Common = "무명",
                Aura = new Color(0.75f, 0.75f, 0.78f), AuraScale = 0.0f,
                Insight = "첫 걸음" },
            new RealmDef { Index = 1, ReqLevel = 15,
                Orthodox = "이류", Unorthodox = "중수", Demonic = "마병", Common = "수련자",
                Aura = new Color(0.70f, 0.85f, 0.95f), AuraScale = 0.35f,
                Insight = "기혈이 트이다" },
            new RealmDef { Index = 2, ReqLevel = 35,
                Orthodox = "일류", Unorthodox = "고수", Demonic = "마인", Common = "무인",
                Aura = new Color(0.45f, 0.80f, 1.00f), AuraScale = 0.55f,
                Insight = "내공이 돌다" },
            new RealmDef { Index = 3, ReqLevel = 60,
                Orthodox = "절정", Unorthodox = "패자", Demonic = "마장", Common = "강자",
                Aura = new Color(0.55f, 1.00f, 0.75f), AuraScale = 0.75f,
                Insight = "검이 마음을 따르다" },
            new RealmDef { Index = 4, ReqLevel = 90,
                Orthodox = "초절정", Unorthodox = "패왕", Demonic = "마왕", Common = "절대자",
                Aura = new Color(1.00f, 0.85f, 0.35f), AuraScale = 0.95f,
                Insight = "기를 밖으로 뿜다" },
            new RealmDef { Index = 5, ReqLevel = 120,
                Orthodox = "화경", Unorthodox = "천패", Demonic = "마신", Common = "초인",
                Aura = new Color(1.00f, 0.55f, 0.25f), AuraScale = 1.20f,
                Insight = "형을 벗다" },
            new RealmDef { Index = 6, ReqLevel = 155,
                Orthodox = "현경", Unorthodox = "무적", Demonic = "천마", Common = "선인",
                Aura = new Color(0.80f, 0.45f, 1.00f), AuraScale = 1.45f,
                Insight = "천지와 하나되다" },
            new RealmDef { Index = 7, ReqLevel = 195,
                Orthodox = "생사경", Unorthodox = "생사경", Demonic = "생사경", Common = "생사경",
                Aura = new Color(1.00f, 0.30f, 0.45f), AuraScale = 1.80f,
                Insight = "생사를 넘다" },
        };

        public static int Index { get; private set; }
        public static bool QuestActive { get; private set; }

        public static RealmDef Current => All[Mathf.Clamp(Index, 0, All.Length - 1)];
        public static RealmDef Next =>
            Index + 1 < All.Length ? All[Index + 1] : null;

        public static bool IsMax => Index >= All.Length - 1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            Index = Mathf.Clamp(PlayerPrefs.GetInt(PrefRealm, 0), 0, All.Length - 1);
            QuestActive = PlayerPrefs.GetInt(PrefQuest, 0) == 1;
        }

        /// <summary>세력에 따라 부르는 이름이 다르다.</summary>
        public static string NameOf(RealmDef d)
        {
            if (d == null) return "";
            if (!FactionService.HasSelected) return d.Common;
            switch (FactionService.Selected)
            {
                case "hero":      return d.Orthodox;
                case "bowmaster": return d.Unorthodox;
                case "archmage":  return d.Demonic;
                default:          return d.Common;
            }
        }

        public static string DisplayName => NameOf(Current);
        public static string NextName => NameOf(Next);

        /// <summary>다음 경지의 벽 앞에 섰는가 (레벨 충족 + 아직 승급 안 함).</summary>
        public static bool CanAwaken
        {
            get
            {
                if (IsMax) return false;
                var g = Progression.PlayerGrowth.Instance;
                int lv = g != null ? g.Level : 1;
                return lv >= Next.ReqLevel;
            }
        }

        /// <summary>깨달음 퀘스트를 시작한다.</summary>
        public static bool BeginAwakening()
        {
            if (!CanAwaken || QuestActive) return false;
            QuestActive = true;
            PlayerPrefs.SetInt(PrefQuest, 1);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
            return true;
        }

        public static void CancelAwakening()
        {
            if (!QuestActive) return;
            QuestActive = false;
            PlayerPrefs.SetInt(PrefQuest, 0);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }

        /// <summary>퀘스트를 통과해 벽을 넘는다.</summary>
        public static string CompleteAwakening()
        {
            if (!QuestActive) return "지금은 깨달음의 때가 아닙니다";
            if (IsMax) return "더 오를 곳이 없습니다";
            Index++;
            QuestActive = false;
            PlayerPrefs.SetInt(PrefRealm, Index);
            PlayerPrefs.SetInt(PrefQuest, 0);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
            return DisplayName + " 경지에 올랐습니다";
        }

        /// <summary>
        /// 경지가 오를수록 전투력이 붙는다 (레벨과 별개인 곱연산).
        /// 레벨 100 이후에는 레벨업이 느려지는 대신 이 곱연산이 성장의 주축이 되도록
        /// 후반 경지(화경·현경·생사경)의 배수를 크게 벌려 두었다.
        /// </summary>
        static readonly float[] PowerMulTable = { 1.00f, 1.10f, 1.25f, 1.45f, 1.75f, 2.20f, 3.00f, 4.20f };

        public static float PowerMul => PowerMulTable[Mathf.Clamp(Index, 0, PowerMulTable.Length - 1)];

        // ---- 깨달음 퀘스트 (세력별 스토리) ----

        public class Trial
        {
            public string Title;
            public string Story;
            public string Objective;
            public int KillCount;      // 이만큼 잡으면 통과
        }

        /// <summary>
        /// 세력마다 벽을 넘는 방식이 다르다.
        /// 정파는 스스로를 다스리고, 사파는 힘으로 증명하며, 마도는 마기를 삼킨다.
        /// </summary>
        public static Trial CurrentTrial()
        {
            var next = Next;
            if (next == null) return null;
            string faction = FactionService.HasSelected ? FactionService.Selected : "";
            int kills = 20 + next.Index * 15;

            switch (faction)
            {
                case "hero":
                    return new Trial {
                        Title = next.Orthodox + " — " + next.Insight,
                        Story = "스승이 말했다. \"칼을 쥔 손보다 그 손을 멈출 줄 아는 마음이 어렵다.\"\n" +
                                "네가 벤 것들 앞에서 흔들리지 않을 수 있느냐. 강호로 나가 스스로를 시험하라.",
                        Objective = "적 " + kills + "명을 쓰러뜨려 마음을 다스린다",
                        KillCount = kills };
                case "bowmaster":
                    return new Trial {
                        Title = next.Unorthodox + " — " + next.Insight,
                        Story = "녹림에 규칙은 없다. 다만 살아남은 자의 말이 규칙이 된다.\n" +
                                "네가 그 자리에 설 만한지, 피로 증명해 보여라.",
                        Objective = "적 " + kills + "명을 베어 실력을 증명한다",
                        KillCount = kills };
                case "archmage":
                    return new Trial {
                        Title = next.Demonic + " — " + next.Insight,
                        Story = "마기는 삼키는 자의 것이다. 두려워하면 잡아먹힌다.\n" +
                                "더 많은 목숨을 취해 마기를 네 것으로 만들어라.",
                        Objective = "적 " + kills + "명의 기운을 흡수한다",
                        KillCount = kills };
                default:
                    return new Trial {
                        Title = next.Common + " — " + next.Insight,
                        Story = "아직 어느 문파에도 속하지 않았다. 강호는 넓고, 네 이름은 아직 없다.\n" +
                                "스스로 부딪히며 길을 찾아라.",
                        Objective = "적 " + kills + "명을 쓰러뜨린다",
                        KillCount = kills };
            }
        }

        // ---- 퀘스트 진행도 ----
        const string PrefKills = "IdleGrow.Realm.QuestKills";

        public static int QuestKills
        {
            get => PlayerPrefs.GetInt(PrefKills, 0);
            private set { PlayerPrefs.SetInt(PrefKills, value); }
        }

        /// <summary>전투에서 적을 처치할 때 호출된다.</summary>
        public static void ReportKill()
        {
            if (!QuestActive) return;
            var t = CurrentTrial();
            if (t == null) return;
            int k = QuestKills + 1;
            QuestKills = k;
            if (k >= t.KillCount) OnChanged?.Invoke();
        }

        public static bool TrialCleared
        {
            get
            {
                if (!QuestActive) return false;
                var t = CurrentTrial();
                return t != null && QuestKills >= t.KillCount;
            }
        }

        public static void ResetQuestProgress()
        {
            PlayerPrefs.SetInt(PrefKills, 0);
            PlayerPrefs.Save();
        }
    }
}
