using System;
using IdleMvp.Economy;
using UnityEngine;

namespace IdleMvp.Core
{
    /// <summary>
    /// 연쇄 서브퀘스트 — 키우기류 표준의 '항상 떠 있는 다음 할 일' 사다리.
    ///
    /// 가이드 퀘스트(튜토리얼)가 끝나면 이 카드가 그 자리를 이어받는다.
    /// 완료 → 즉시 보상 → 다음 퀘스트. 사다리는 한 바퀴 돌면 회차가 올라가며
    /// 목표·보상이 함께 커진다 (무한 반복).
    ///
    /// 진행 카운트는 DailyMissionService 와 같은 호출부(처치/강화/소환/던전)에서
    /// Notify()로 받는다. 스테이지·레벨·세력은 평가 시점에 실값을 읽는다 —
    /// 카운터로 세면 과거 달성분을 놓친다.
    /// </summary>
    public static class QuestService
    {
        public static event Action OnChanged;

        public enum Kind { Kill, Enhance, Summon, Dungeon, StageReach, LevelReach, FactionJoin }

        public struct Quest
        {
            public Kind Kind;
            public string Title;
            /// <summary>{0}=목표 수치가 들어가는 설명.</summary>
            public string DescFmt;
            public int BaseGoal;
            /// <summary>회차마다 목표에 곱해지는 증가율.</summary>
            public float GoalGrowth;
            public CurrencyId RewardCur;
            public int BaseReward;
        }

        // 사다리 한 바퀴. 순서 = 초반 유저의 자연스러운 학습 순서.
        static readonly Quest[] Ladder =
        {
            new Quest { Kind = Kind.Kill,       Title = "무뢰배 소탕",  DescFmt = "몬스터 {0}마리 처치",      BaseGoal = 50,  GoalGrowth = 1.6f, RewardCur = CurrencyId.Gold,       BaseReward = 8000 },
            new Quest { Kind = Kind.Enhance,    Title = "병기 손질",    DescFmt = "장비 강화 {0}회",          BaseGoal = 3,   GoalGrowth = 1.4f, RewardCur = CurrencyId.WeaponEnhanceStone, BaseReward = 60 },
            new Quest { Kind = Kind.StageReach, Title = "강호 행보",    DescFmt = "스테이지 {0} 도달",        BaseGoal = 5,   GoalGrowth = 1.5f, RewardCur = CurrencyId.RedDiamond, BaseReward = 10 },
            new Quest { Kind = Kind.Summon,     Title = "인연 넓히기",  DescFmt = "소환 {0}회",               BaseGoal = 2,   GoalGrowth = 1.5f, RewardCur = CurrencyId.Gold,       BaseReward = 12000 },
            new Quest { Kind = Kind.LevelReach, Title = "내공 증진",    DescFmt = "레벨 {0} 달성",            BaseGoal = 8,   GoalGrowth = 1.45f, RewardCur = CurrencyId.RedDiamond, BaseReward = 8 },
            new Quest { Kind = Kind.Kill,       Title = "흑도 토벌",    DescFmt = "몬스터 {0}마리 처치",      BaseGoal = 120, GoalGrowth = 1.6f, RewardCur = CurrencyId.WeaponEnhanceStone, BaseReward = 80 },
            new Quest { Kind = Kind.Dungeon,    Title = "비경 탐사",    DescFmt = "성장 던전 {0}회 클리어",   BaseGoal = 1,   GoalGrowth = 1.3f, RewardCur = CurrencyId.RedDiamond, BaseReward = 12 },
            new Quest { Kind = Kind.FactionJoin,Title = "세력 입문",    DescFmt = "정파·사파·마도 중 입문",   BaseGoal = 1,   GoalGrowth = 1f,   RewardCur = CurrencyId.RedDiamond, BaseReward = 30 },
            new Quest { Kind = Kind.StageReach, Title = "강호 명성",    DescFmt = "스테이지 {0} 도달",        BaseGoal = 12,  GoalGrowth = 1.5f, RewardCur = CurrencyId.Gold,       BaseReward = 30000 },
        };

        const string PrefStep = "IdleGrow.Quest.Step";     // 누적 완료 수 (사다리 인덱스 = step % 길이, 회차 = step / 길이)
        const string PrefProg = "IdleGrow.Quest.Prog";     // 카운트형 퀘스트의 현재 진행

        public static int Step
        {
            get => PlayerPrefs.GetInt(PrefStep, 0);
            private set { PlayerPrefs.SetInt(PrefStep, value); PlayerPrefs.Save(); }
        }

        static int Round => Step / Ladder.Length;
        public static Quest Current => Ladder[Step % Ladder.Length];

        /// <summary>회차 보정이 들어간 현재 목표치.</summary>
        public static int Goal
        {
            get
            {
                var q = Current;
                if (q.Kind == Kind.StageReach)
                    // 스테이지는 곱이 아니라 사다리 전체 진행에 비례해 앞으로 밀린다
                    return Mathf.Min(100, q.BaseGoal + Step * 4);
                if (q.Kind == Kind.LevelReach)
                    return Mathf.Min(60, q.BaseGoal + Step * 2);
                return Mathf.CeilToInt(q.BaseGoal * Mathf.Pow(q.GoalGrowth, Round));
            }
        }

        public static int Reward => Mathf.CeilToInt(Current.BaseReward * Mathf.Pow(1.35f, Round));

        public static int Progress
        {
            get
            {
                switch (Current.Kind)
                {
                    case Kind.StageReach:
                        var sp = IdleMvp.Progression.StageProgress.Instance;
                        return sp != null ? sp.StageIndex : 0;
                    case Kind.LevelReach:
                        var pg = IdleMvp.Progression.PlayerGrowth.Instance;
                        return pg != null ? pg.Level : 0;
                    case Kind.FactionJoin:
                        return FactionService.HasSelected ? 1 : 0;
                    default:
                        return PlayerPrefs.GetInt(PrefProg, 0);
                }
            }
        }

        public static bool IsComplete => Progress >= Goal;
        public static string DescText => string.Format(Current.DescFmt, Goal);

        /// <summary>카운트형 진행 보고 — 처치/강화/소환/던전 호출부에서 부른다.</summary>
        public static void Notify(Kind kind, int amount = 1)
        {
            var q = Current;
            if (q.Kind != kind) return;
            if (kind == Kind.StageReach || kind == Kind.LevelReach || kind == Kind.FactionJoin) return;
            int cur = PlayerPrefs.GetInt(PrefProg, 0);
            if (cur >= Goal) return;
            PlayerPrefs.SetInt(PrefProg, Mathf.Min(cur + amount, Goal));
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }

        static string CurLabel(CurrencyId id)
        {
            switch (id)
            {
                case CurrencyId.Gold: return "골드";
                case CurrencyId.RedDiamond: return "레드다이아";
                case CurrencyId.WeaponEnhanceStone: return "강화석";
                default: return id.ToString();
            }
        }

        /// <summary>완료 보상 수령 + 다음 퀘스트로. 성공 시 보상 문구를 돌려준다.</summary>
        public static string TryClaim()
        {
            if (!IsComplete) return null;
            int reward = Reward;
            var cur = Current.RewardCur;
            CurrencyWallet.Instance?.Add(cur, reward);
            Step = Step + 1;
            PlayerPrefs.SetInt(PrefProg, 0);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
            return CurLabel(cur) + " +" + reward;
        }
    }
}
