using IdleMvp.Adapters;
using IdleMvp.Core;
using UnityEngine;

namespace IdleMvp.Progression
{
    public enum CpGateStatus
    {
        Blocked,
        Normal,
        Overkill
    }

    public struct CombatStatBreakdown
    {
        public float Atk;
        public float MaxHp;
        public float Def;
        public float OutgoingMul;
        public float TotalCp;
        public float GrowthAtk;
        public float ArmorAtk;
        public float WeaponAtk;
        public float ArmorCp;
        public float WeaponCp;
        public float SlotCp;
            public float CompanionCp;
            public float SkillCp;
            public float CostumeCp;
            public float ArtifactCp;
            public int Level;
        }

    /// <summary>
    /// Single authority for Atk / MaxHp / Def / outgoing mul / total CP / stage gates.
    /// </summary>
    public class CombatPowerService : MonoBehaviour
    {
        public static CombatPowerService Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public static float GetAtk() => GetBreakdown().Atk;

        public static float GetMaxHp() => GetBreakdown().MaxHp;

        public static float GetDef() => GetBreakdown().Def;

        /// <summary>Slot enhance × skill passive × spec final. Does not trigger skill CD.</summary>
        public static float GetOutgoingMul() => GetBreakdown().OutgoingMul;

        public static float GetTotalCp() => GetBreakdown().TotalCp;

        public static CombatStatBreakdown GetBreakdown()
        {
            var b = new CombatStatBreakdown();

            var growth = PlayerGrowth.Instance;
            if (growth != null)
            {
                b.GrowthAtk = growth.Atk;
                b.Level = growth.Level;
                b.MaxHp = Mathf.Max(500f, growth.Hp * 10f);
                b.Def = growth.Def;
            }
            else
            {
                b.GrowthAtk = 10f;
                b.Level = 1;
                b.MaxHp = 500f;
                b.Def = 5f;
            }

            var inv = InventoryAdapter.Instance;
            if (inv != null)
            {
                b.ArmorAtk = inv.BonusAtk;
                b.ArmorCp = inv.EquipmentCp;
                b.MaxHp += inv.BonusHp;
                b.Def += inv.BonusDef;
            }

            var weap = WeaponSummonAdapter.Instance;
            if (weap != null)
            {
                b.WeaponAtk = weap.EquippedWeaponAtk;
                b.WeaponCp = weap.WeaponCp;   // 보유+장착 표시용
            }

            b.Atk = b.GrowthAtk + b.ArmorAtk + b.WeaponAtk;
            if (CompanionAdapter.Instance != null)
            {
                b.Atk *= 1f + CompanionAdapter.Instance.PassiveAtkPct * 0.01f;
                b.MaxHp *= 1f + CompanionAdapter.Instance.PassiveHpPct * 0.01f;
            }

            // Job primary multipliers
            b.Atk *= JobProgress.AtkMul;
            b.MaxHp *= JobProgress.HpMul;
            b.Def *= JobProgress.DefMul;

            if (ArtifactService.Instance != null)
            {
                b.Atk *= 1f + ArtifactService.Instance.AtkPctBonus * 0.01f;
                b.ArtifactCp = ArtifactService.Instance.ArtifactCp;
            }

            b.Atk *= 1f + IdleMvp.Core.CollectionService.BonusAtkPct * 0.01f;
            b.MaxHp *= 1f + IdleMvp.Core.CollectionService.BonusHpPct * 0.01f;
            // 무기 보유 효과 (벤치마크: 획득만으로 적용) — 도감과 별개의 누적 축
            if (weap != null)
                b.Atk *= 1f + (weap.HoldAtkPct + weap.SetAtkPct) * 0.01f;   // 보유 효과 + 세트 효과

            // 길드 스킬 (개인 연구) — 벤치마크 카피
            var guild = GuildAdapter.Instance;
            if (guild != null && guild.Joined)
            {
                b.Atk *= 1f + guild.GuildAtkPct * 0.01f;
                b.MaxHp *= 1f + guild.GuildHpPct * 0.01f;
            }

            // 아레나 랭크 보너스 — 티어당 공격 +1%
            if (ArenaAdapter.Instance != null)
                b.Atk *= 1f + ArenaAdapter.Instance.TierAtkPct * 0.01f;

            var rebirth = Core.RebirthService.Instance;
            if (rebirth != null && rebirth.Count > 0)
            {
                b.Atk *= rebirth.AtkMul;
                b.MaxHp *= rebirth.HpMul;
            }

            float outMul = 1f;
            if (SlotEnhanceService.Instance != null)
                outMul *= SlotEnhanceService.Instance.DamageMultiplier;
            if (SkillAdapter.Instance != null)
                outMul *= SkillAdapter.Instance.PassiveOutgoingMul;
            else if (growth != null)
                outMul *= 1f + growth.SpecFinalDmgPct * 0.01f;
            if (Core.FactionService.SynergyName != null)
                outMul *= 1.15f;
            // 경지는 레벨과 별개로 붙는 곱연산 — 벽을 넘을 때마다 한 단계 세진다
            outMul *= Core.RealmService.PowerMul;
            // 수련: 일반 피해 + 방어 관통(관통은 절반 계수 — 보스뎀은 타격 지점에서 별도)
            // 방어 관통은 여기서 데미지를 더하지 않는다 — 적 방어력을 깎는 쪽(MitigateByDef)으로만 작동.
            // 예전엔 관통이 곱연산 데미지에 섞여 들어가 이중 계산이었다.
            outMul *= 1f + Core.TrainingService.NormalDmgPct * 0.01f;
            b.OutgoingMul = Mathf.Max(0.1f, outMul);

            if (SlotEnhanceService.Instance != null)
                b.SlotCp = SlotEnhanceService.Instance.BonusCp;
            if (CompanionAdapter.Instance != null)
                b.CompanionCp = CompanionAdapter.Instance.CompanionCp;
            if (SkillAdapter.Instance != null)
                b.SkillCp = SkillAdapter.Instance.SkillCp;
            if (CostumeAdapter.Instance != null)
                b.CostumeCp = CostumeAdapter.Instance.CostumeCp;

            // Same live combat stats + system CPs (Growth.CombatPower / equipped weapon not double-counted).
            float weaponHoldOnly = 0f;
            if (weap != null)
                weaponHoldOnly = Mathf.Max(0f, weap.WeaponCp - weap.EquippedWeaponCp);
            b.TotalCp = b.Atk * 10f
                        + b.MaxHp * 0.05f
                        + b.Def * 8f
                        + b.Level * 15f
                        + b.SlotCp
                        + b.CompanionCp
                        + b.SkillCp
                        + b.CostumeCp
                        + b.ArtifactCp
                        + Core.TrainingService.TrainingCp
                        + weaponHoldOnly;
            b.TotalCp = Mathf.Max(1f, b.TotalCp);
            return b;
        }

        public static float GetMaxMp()
        {
            int lv = PlayerGrowth.Instance != null ? PlayerGrowth.Instance.Level : 1;
            return 80f + lv * 10f;
        }

        /// <summary>Crit chance % (display). Scales lightly with grade + accuracy-like sources.</summary>
        public static float GetCritRatePct()
        {
            float rate = 5f;
            if (PlayerGrowth.Instance != null)
                rate += PlayerGrowth.Instance.Grade * 0.35f;
            if (SkillAdapter.Instance != null)
                rate += Mathf.Max(0f, (SkillAdapter.Instance.PassiveOutgoingMul - 1f) * 4f);
            var rebirth = Core.RebirthService.Instance;
            if (rebirth != null)
                rate += rebirth.CritBonus;
            rate += Core.TrainingService.CritRatePct;
            if (PlayerGrowth.Instance != null)
                rate += PlayerGrowth.Instance.SpecCritRatePct;   // 특별 능력치 '치명타 확률'
            return Mathf.Clamp(rate, 0f, 60f);
        }

        public static float GetCritDamagePct()
        {
            float dmg = 130f;
            if (PlayerGrowth.Instance != null)
                dmg += PlayerGrowth.Instance.SpecFinalDmgPct * 0.5f;
            dmg += Core.TrainingService.CritDmgPct;
            if (PlayerGrowth.Instance != null)
                dmg += PlayerGrowth.Instance.SpecCritDmgPct;     // 특별 능력치 '치명타 피해'
            return Mathf.Clamp(dmg, 100f, 400f);
        }

        /// <summary>공속 배율 (1.0 = 기본). 수련 '신속' 트랙이 유일한 성장 축.</summary>
        public static float GetAttackSpeedMul()
        {
            float pct = Core.TrainingService.AtkSpeedPct;
            if (PlayerGrowth.Instance != null) pct += PlayerGrowth.Instance.SpecAtkSpeedPct;
            return 1f + pct * 0.01f;
        }

        /// <summary>Attack speed as % of a 1.0 APS baseline (FieldAutoHunt AttackInterval).</summary>
        public static float GetAttackSpeedPct()
        {
            const float baseInterval = 0.45f;
            float aps = GetAttackSpeedMul() / Mathf.Max(0.15f, baseInterval);
            return aps * 100f; // 기본 ~222%, 수련으로 성장
        }

        /// <summary>수련 + 특별 능력치의 방어 관통 합계 % (상한 80).</summary>
        public static float DefPenPct()
        {
            float pen = Core.TrainingService.DefPenPct;
            if (PlayerGrowth.Instance != null) pen += PlayerGrowth.Instance.SpecDefPenPct;
            return Mathf.Clamp(pen, 0f, 80f);
        }

        /// <summary>
        /// 적 방어력으로 데미지를 깎는다. 감쇠율 = def / (def + K), K는 그 스테이지의
        /// 권장 전투력에서 뽑아 쓴다(플레이어가 과성장해도 감쇠율은 적의 성질로 유지).
        /// 방어 관통은 def를 직접 깎는다.
        /// </summary>
        public static float MitigateByDef(float raw, StageRow row)
        {
            if (row == null || row.enemyDef <= 0f) return raw;
            float def = row.enemyDef * (1f - DefPenPct() * 0.01f);
            if (def <= 0f) return raw;
            float k = Mathf.Max(1f, row.recommendedCp * 0.6f);
            float mitigation = def / (def + k);
            return Mathf.Max(1f, raw * (1f - mitigation));
        }

        public static CpGateStatus GetGateStatus(float recommendedCp)
        {
            float cp = GetTotalCp();
            if (recommendedCp <= 0f) return CpGateStatus.Normal;
            float min = recommendedCp * BalanceConfig.Data.clearCpRatioMin;
            float max = recommendedCp * BalanceConfig.Data.clearCpRatioMax;
            if (cp < min) return CpGateStatus.Blocked;
            if (cp > max) return CpGateStatus.Overkill;
            return CpGateStatus.Normal;
        }

        /// <summary>Unified stage gate: absolute minCp floor + recommendedCp soft bands.</summary>
        /// <remarks>
        /// 차단 판정에만 실딜(OutgoingMul)을 반영한다 — 경지·시너지·주문서가 CP에
        /// 안 잡혀 '실딜은 충분한데 막히는' 역전 방지(진단 #2). CP 자체에 곱하면
        /// Q4에서 동결한 스테이지 밸런스가 전부 오버킬로 뒤집혀서(실측 19.7k→44.7k)
        /// 보상·오버킬 판정은 기존 CP 그대로 둔다.
        /// </remarks>
        public static CpGateStatus EvaluateStageGate(StageRow row)
        {
            if (row == null) return CpGateStatus.Normal;
            var bd = GetBreakdown();
            float cp = bd.TotalCp;
            float cpEff = cp + bd.Atk * 10f * Mathf.Max(0f, bd.OutgoingMul - 1f);
            float min = row.minCp > 0f
                ? row.minCp
                : row.recommendedCp * BalanceConfig.Data.clearCpRatioMin;
            if (min > 0f && cpEff < min) return CpGateStatus.Blocked;
            float rec = row.recommendedCp > 0f
                ? row.recommendedCp
                : min / Mathf.Max(0.01f, BalanceConfig.Data.clearCpRatioMin);
            if (rec > 0f && cp > rec * BalanceConfig.Data.clearCpRatioMax)
                return CpGateStatus.Overkill;
            return CpGateStatus.Normal;
        }

        public static bool CanEnterStage(StageRow row, out string reason)
        {
            reason = "";
            if (row == null)
            {
                reason = "스테이지 데이터 없음";
                return false;
            }
            if (EvaluateStageGate(row) == CpGateStatus.Blocked)
            {
                float min = row.minCp > 0f
                    ? row.minCp
                    : row.recommendedCp * BalanceConfig.Data.clearCpRatioMin;
                reason = $"스펙 부족 — CP {GetTotalCp():0} / 필요 {min:0}";
                return false;
            }
            return true;
        }

        public static float RewardMulForGate(CpGateStatus gate)
        {
            switch (gate)
            {
                case CpGateStatus.Blocked: return 0f;
                case CpGateStatus.Overkill: return 0.65f;
                default: return 1f;
            }
        }
    }
}
