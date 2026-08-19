using IdleMvp.Core;
using UnityEngine;

namespace IdleMvp.Adapters
{
    /// <summary>How an active skill actually plays out in the field.</summary>
    public enum SkillMechanic
    {
        Single,      // 강타 1회 (기본)
        MultiHit,    // 다단히트 (Hits회 연속타)
        Projectile,  // 투사체 발사 (명중 시 피해)
        Pierce,      // 관통 — 전방 직선상의 모든 적
        Homing,      // 유도탄 — 가까운 적 Hits기에 분산
        AoE,         // 광역 — Radius 내 모든 적
        Buff,        // 자기 버프 (일정 시간 스탯 강화)
        Counter,     // 반격 (피격 시 확률 반사)
        Summon,      // 분신/소환 (임시 아군 생성)
        HPCost,      // HP 소모 후 강화 광역
        DOT,         // 지속 피해 (출혈/중독 틱)
    }

    public struct SkillTreeNode
    {
        public int Id;
        public string Name;
        public bool IsPassive;
        public int ReqLevel;
        public int MaxLevel;
        public double LearnGold;
        public double LearnStone;
        public double GoldCostBase;
        public double StoneCostBase;
        public int RdCostBase;
        public string Description;
        public string EffectHint;
        public SkillMechanic Mechanic;
        public int Hits;       // MultiHit/Homing/Projectile 발수, Summon 소환수, DOT 틱수
        public float Radius;   // AoE/HPCost 반경 (필드 px)
        public float Duration; // Buff/Counter/Summon 지속시간 (초)
        public float HpCostPct; // HPCost 시 HP 소모 비율 (0.1 = 10%)
    }

    /// <summary>Faction-keyed skill trees (hero=정파 / bowmaster=사파 / archmage=마도). Nodes property follows current job.</summary>
    public static class SkillTreeDef
    {
        static readonly SkillTreeNode[] HeroNodes =
        {
            N(0, "태극기공", true, 10, 10, 200, 0.5, 500, 1, 5, "정순한 내공을 쌓아 최대 내력과 회복 속도를 높입니다.", "패시브 · 내력+20%"),
            N(1, "매화검기", false, 20, 10, 800, 1, 800, 1.5, 8, "전방에 매화 형상의 검기를 날려 범위 피해를 입힙니다.", "액티브 · 광역 피해", SkillMechanic.AoE, 1, 180f),
            N(2, "금강불괴", false, 40, 10, 2000, 2, 1200, 2, 10, "8초간 받는 모든 피해를 절반으로 줄이고 상태이상을 면역합니다.", "액티브 · 생존 버프", SkillMechanic.Buff, duration: 8f),
            N(3, "태극유운검", false, 70, 10, 5000, 3, 2000, 3, 12, "피격 시 30% 확률로 적 공격을 반사하고 내력을 환원합니다.", "액티브 · 반격", SkillMechanic.Counter, duration: 10f),
            N(4, "만검귀종", false, 100, 10, 10000, 5, 4000, 5, 20, "수천 개의 무형검을 생성해 화면 전체 적에게 파상 타격합니다.", "궁극기 · 전체 광역", SkillMechanic.AoE, 1, 400f),
            N(5, "내공심법", true, 15, 5, 900, 1, 900, 1.5, 8, "내공 숙련으로 골드 획득량을 늘립니다.", "패시브 · 골드%"),
            N(6, "기혈순환", true, 25, 5, 1500, 1.5, 1100, 2, 10, "기혈 순환으로 방치 효율을 올립니다.", "패시브 · 방치%"),
            N(7, "검의", true, 50, 5, 3000, 2, 1500, 2.5, 12, "검의 경지로 최종 데미지를 크게 강화합니다.", "패시브 · 최종뎀%"),
        };

        static readonly SkillTreeNode[] BowNodes =
        {
            N(0, "야행심법", true, 10, 10, 200, 0.5, 500, 1, 5, "민첩성을 극대화하여 이동 속도와 치명타 확률을 높입니다.", "패시브 · 치명타+15%"),
            N(1, "비영출혈도", false, 20, 10, 800, 1, 800, 1.5, 8, "적의 후방으로 순간 이동해 단도로 타격, 5초간 출혈을 부여합니다.", "액티브 · 유도 출혈", SkillMechanic.Homing, 1),
            N(2, "만독비술", true, 40, 10, 2000, 2, 1200, 2, 10, "모든 공격에 20% 확률로 중독을 부여하고 적 공속을 낮춥니다.", "패시브 · 중독 부여"),
            N(3, "혈영분신술", false, 70, 10, 5000, 3, 2000, 3, 12, "분신 2개를 12초간 소환하여 자동 사냥을 보조합니다.", "액티브 · 분신 소환", SkillMechanic.Summon, 2, duration: 12f),
            N(4, "십살난무", false, 100, 10, 10000, 5, 4000, 5, 20, "초고속 난무를 펼쳐 극상의 치명타 피해와 30% 흡혈을 합니다.", "궁극기 · 10연타 흡혈", SkillMechanic.MultiHit, 10),
            N(5, "독공심법", true, 15, 5, 900, 1, 900, 1.5, 8, "독공 숙련으로 골드 획득량을 늘립니다.", "패시브 · 골드%"),
            N(6, "잠행술", true, 25, 5, 1500, 1.5, 1100, 2, 10, "은신 숙련으로 방치 효율을 올립니다.", "패시브 · 방치%"),
            N(7, "암살극의", true, 50, 5, 3000, 2, 1500, 2.5, 12, "암살 극의로 최종 데미지를 크게 강화합니다.", "패시브 · 최종뎀%"),
        };

        static readonly SkillTreeNode[] MageNodes =
        {
            N(0, "마화공", true, 10, 10, 200, 0.5, 500, 1, 5, "체력이 줄어들수록 공격력과 흡혈 비율이 증가합니다.", "패시브 · 광폭화"),
            N(1, "혈무폭쇄", false, 20, 10, 800, 1, 800, 1.5, 8, "자신의 체력 10%를 소모해 광범위 마기 폭발을 일으킵니다.", "액티브 · HP소모 광역", SkillMechanic.HPCost, 1, 170f, hpCostPct: 0.1f),
            N(2, "마신강림", false, 40, 10, 2000, 2, 1200, 2, 10, "15초간 마신 상태로 변신하여 공격 속도가 두 배로 증가합니다.", "액티브 · 변신 버프", SkillMechanic.Buff, duration: 15f),
            N(3, "단천마공", false, 70, 10, 5000, 3, 2000, 3, 12, "거대한 마검으로 지면을 내리쳐 광역 피해와 기절을 부여합니다.", "액티브 · 광역 기절", SkillMechanic.AoE, 1, 260f),
            N(4, "천마체", false, 100, 10, 10000, 5, 4000, 5, 20, "치명상 시 무적 상태로 전환되며 화면 전체에 마기 폭발을 일으킵니다.", "궁극기 · 불사 폭발", SkillMechanic.AoE, 1, 400f),
            N(5, "마공심법", true, 15, 5, 900, 1, 900, 1.5, 8, "마공 숙련으로 골드 획득량을 늘립니다.", "패시브 · 골드%"),
            N(6, "마기순환", true, 25, 5, 1500, 1.5, 1100, 2, 10, "마기 순환으로 방치 효율을 올립니다.", "패시브 · 방치%"),
            N(7, "마도극의", true, 50, 5, 3000, 2, 1500, 2.5, 12, "마도 극의로 최종 데미지를 크게 강화합니다.", "패시브 · 최종뎀%"),
        };

        static SkillTreeNode N(int id, string name, bool passive, int req, int max,
            double learnG, double learnS, double gBase, double sBase, int rd,
            string desc, string hint,
            SkillMechanic mech = SkillMechanic.Single, int hits = 1, float radius = 0f,
            float duration = 0f, float hpCostPct = 0f) => new SkillTreeNode
        {
            Id = id, Name = name, IsPassive = passive, ReqLevel = req, MaxLevel = max,
            LearnGold = learnG, LearnStone = learnS, GoldCostBase = gBase, StoneCostBase = sBase, RdCostBase = rd,
            Description = desc, EffectHint = hint,
            Mechanic = mech, Hits = hits, Radius = radius,
            Duration = duration, HpCostPct = hpCostPct
        };

        static readonly SkillTreeNode[] HiddenNodes =
        {
            N(0, "천지합일", true, 10, 10, 500, 1, 1000, 2, 10, "두 기운을 조화시켜 모든 스탯이 소폭 상승합니다.", "패시브 · 전스탯+8%"),
            N(1, "절세무공", false, 30, 10, 3000, 2, 2000, 3, 15, "이전 세력과 현재 세력의 기운을 융합하여 강력한 일격을 날립니다.", "액티브 · 융합 광역", SkillMechanic.AoE, 1, 300f),
            N(2, "무아지경", false, 60, 10, 8000, 4, 4000, 5, 20, "10초간 무아 상태 진입: 공속+50%, 피해-30%, 반격 활성.", "액티브 · 초월 버프", SkillMechanic.Buff, duration: 10f),
            N(3, "개벽", false, 100, 10, 20000, 8, 8000, 8, 30, "천지의 기운을 모아 화면 전체에 절대적 파괴를 일으킵니다.", "궁극기 · 개벽", SkillMechanic.AoE, 1, 500f),
        };

        public static SkillTreeNode[] GetNodes(string treeId)
        {
            if (treeId == "bowmaster") return BowNodes;
            if (treeId == "archmage") return MageNodes;
            if (treeId == "hidden") return HiddenNodes;
            return HeroNodes;
        }

        /// <summary>Current job tree (defaults to hero).</summary>
        public static SkillTreeNode[] Nodes => GetNodes(JobProgress.TreeId);

        public static void GetLearnCosts(SkillTreeNode n, out double gold, out double stone, out int rd)
        {
            gold = n.LearnGold;
            stone = n.LearnStone;
            rd = 0;
        }

        public static void GetEnhanceCosts(SkillTreeNode n, int currentLv, out double gold, out double stone, out int rd)
        {
            int next = Mathf.Max(1, currentLv);
            gold = n.GoldCostBase * (next + 1);
            stone = n.StoneCostBase * (next + 1);
            rd = n.RdCostBase + currentLv * 2;
        }

        public static void GetCosts(SkillTreeNode n, int currentLv, out double gold, out double stone, out int rd)
        {
            if (currentLv <= 0)
                GetLearnCosts(n, out gold, out stone, out rd);
            else
                GetEnhanceCosts(n, currentLv, out gold, out stone, out rd);
        }
    }
}
