using System;
using IdleMvp.Core;
using IdleMvp.Economy;
using IdleMvp.Progression;
using UnityEngine;

namespace IdleMvp.Adapters
{
    public enum SkillNodeAction
    {
        LockedLevel,
        LockedPrereq,
        Learn,
        Enhance,
        Max
    }

    /// <summary>
    /// Skill tree (8 nodes) + combat cooldown mirror for 4 actives.
    /// Learn (0→1) and Enhance (1→Max) are separate paths.
    /// </summary>
    public class SkillAdapter : MonoBehaviour
    {
        public static SkillAdapter Instance { get; private set; }

        static readonly float[] BaseCooldown = { 8f, 12f, 10f, 15f };
        static readonly float[] BaseDamage = { 0.35f, 0.55f, 0.4f, 0.7f };

        public float[] MaxCooldown = { 8f, 12f, 10f, 15f };
        int _pendingSkillFx = -1;
        public float[] DamageBonus = { 0.35f, 0.55f, 0.4f, 0.7f };
        public string[] SkillNames = { "슬래시 블래스트", "브랜드니시", "레이징 블로우", "인레이지" };
        public readonly int[] NodeLevel = new int[8];
        string _loadedTreeId = "hero";

        public float[] CurrentCd { get; private set; }
        public int UnlockedMask { get; private set; } = 0x1;
        public int BonusUnlockMask { get; private set; }
        public int TalentPointsSpent { get; private set; }
        public float PassiveMasteryPct { get; private set; }

        public float PassiveDmgPct { get; private set; }
        public float PassiveGoldPct { get; private set; }
        public float PassiveIdlePct { get; private set; }

        public event Action OnChanged;

        const string PrefKey = "IdleGrow.Maple.Skills";

        public int[] SkillEnhanceLv => NodeLevel;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            CurrentCd = new float[4];
            LoadForTree(JobProgress.TreeId);
            RefreshUnlocks();
            RecalcPassives();
        }

        public void RefreshSkillNamesFromTree()
        {
            var nodes = SkillTreeDef.Nodes;
            for (int i = 0; i < 4 && i < nodes.Length; i++)
                SkillNames[i] = nodes[i].Name;
            OnChanged?.Invoke();
        }

        /// <summary>Persist current tree, then load target tree progress.</summary>
        public void ReloadForTree(string treeId)
        {
            if (string.IsNullOrEmpty(treeId)) treeId = "hero";
            Save();
            LoadForTree(treeId);
            RefreshUnlocks();
            RecalcPassives();
            OnChanged?.Invoke();
        }

        void Update()
        {
            var host = TemplateFeatureHost.Instance;
            if (host != null && host.HasTemplateSkills)
            {
                try
                {
                    var field = host.SkillsManager.GetType().GetField("CurrentTimerCountdownValueCollection");
                    if (field?.GetValue(host.SkillsManager) is float[] cds && cds.Length >= 4)
                    {
                        for (int i = 0; i < 4; i++) CurrentCd[i] = cds[i];
                        return;
                    }
                }
                catch { /* mirror */ }
            }

            for (int i = 0; i < CurrentCd.Length; i++)
            {
                if (CurrentCd[i] > 0f)
                    CurrentCd[i] -= Time.deltaTime * 1.3f;
            }
        }

        public float SkillCp =>
            20f + TalentPointsSpent * 8f + PassiveMasteryPct * 30f + CountUnlocked() * 15f
            + PassiveDmgPct * 4f + NodeLevel[0] * 3f + NodeLevel[1] * 4f;

        public float GetDamageMultiplier()
        {
            float mul = PassiveOutgoingMul;
            mul *= TryTriggerAutoSkillMul();
            return mul;
        }

        /// <summary>Passive skill + growth final dmg — does not trigger skill CD.</summary>
        public float PassiveOutgoingMul
        {
            get
            {
                float mul = 1f + PassiveMasteryPct * 0.01f + PassiveDmgPct * 0.01f;
                if (PlayerGrowth != null)
                    mul += PlayerGrowth.SpecFinalDmgPct * 0.01f;
                return mul;
            }
        }

        /// <summary>If an unlocked skill is off CD, fire it and return (1 + bonus); else 1.</summary>
        public float TryTriggerAutoSkillMul()
        {
            for (int i = 0; i < 4; i++)
            {
                if (((UnlockedMask >> i) & 1) == 0) continue;
                if (NodeLevel[i] <= 0)
                {
                    NodeLevel[i] = 1;
                    ApplyActiveStats();
                    Save();
                }
                if (CurrentCd[i] > 0f) continue;
                CurrentCd[i] = MaxCooldown[i];
                _pendingSkillFx = i;
                TryActivateTemplateSkill(i);
                return 1f + DamageBonus[i];
            }
            return 1f;
        }

        /// <summary>Consume one skill-cast FX id fired by GetDamageMultiplier this frame.</summary>
        public bool TryConsumeSkillPulse(out int skillId)
        {
            skillId = _pendingSkillFx;
            _pendingSkillFx = -1;
            return skillId >= 0;
        }

        /// <summary>Player tapped a HUD skill. Requires unlocked + learned (or unlock-by-level).</summary>
        public bool TryBeginManualCast(int skillId, out string error)
        {
            error = "";
            if (skillId < 0 || skillId >= 4)
            {
                error = "잘못된 스킬";
                return false;
            }
            RefreshUnlocks();
            if (((UnlockedMask >> skillId) & 1) == 0)
            {
                error = "아직 해금되지 않음";
                return false;
            }
            if (NodeLevel[skillId] <= 0)
            {
                // Soft-learn so dock skills are usable after unlock without extra menu trip.
                NodeLevel[skillId] = 1;
                ApplyActiveStats();
                Save();
                OnChanged?.Invoke();
            }
            if (CurrentCd[skillId] > 0.05f)
            {
                error = $"쿨다운 {CurrentCd[skillId]:0.0}초";
                return false;
            }
            CurrentCd[skillId] = MaxCooldown[skillId];
            _pendingSkillFx = skillId;
            TryActivateTemplateSkill(skillId);
            return true;
        }

        public void RefundManualCast(int skillId)
        {
            if (skillId < 0 || skillId >= CurrentCd.Length) return;
            CurrentCd[skillId] = 0f;
            if (_pendingSkillFx == skillId) _pendingSkillFx = -1;
        }

        void TryActivateTemplateSkill(int id)
        {
            var host = TemplateFeatureHost.Instance;
            if (host == null || !host.HasTemplateSkills) return;
            try
            {
                var m = host.SkillsManager.GetType().GetMethod("SkillActivate", new[] { typeof(int) });
                m?.Invoke(host.SkillsManager, new object[] { id });
            }
            catch { /* optional VFX */ }
        }

        public void RefreshUnlocks()
        {
            int lv = PlayerGrowth != null ? PlayerGrowth.Level : 1;
            int mask = 0;
            for (int i = 0; i < 4; i++)
            {
                if (lv >= SkillTreeDef.Nodes[i].ReqLevel || NodeLevel[i] > 0)
                    mask |= 1 << i;
            }
            if (lv >= 1) mask |= 0x1;
            UnlockedMask = mask | BonusUnlockMask;
            ApplyActiveStats();
        }

        PlayerGrowth PlayerGrowth => PlayerGrowth.Instance;

        public bool MeetsLevel(int nodeId)
        {
            var n = SkillTreeDef.Nodes[nodeId];
            int lv = PlayerGrowth != null ? PlayerGrowth.Level : 1;
            return lv >= n.ReqLevel || NodeLevel[nodeId] > 0;
        }

        public bool MeetsPrereq(int nodeId)
        {
            var n = SkillTreeDef.Nodes[nodeId];
            if (n.IsPassive) return true;
            if (nodeId <= 0) return true;
            return NodeLevel[nodeId - 1] >= 1;
        }

        public bool IsNodeUnlocked(int nodeId) => MeetsLevel(nodeId) && MeetsPrereq(nodeId);

        public SkillNodeAction GetNodeAction(int nodeId)
        {
            nodeId = Mathf.Clamp(nodeId, 0, SkillTreeDef.Nodes.Length - 1);
            int cur = NodeLevel[nodeId];
            if (cur >= SkillTreeDef.Nodes[nodeId].MaxLevel) return SkillNodeAction.Max;
            if (cur >= 1) return SkillNodeAction.Enhance;
            if (!MeetsLevel(nodeId)) return SkillNodeAction.LockedLevel;
            if (!MeetsPrereq(nodeId)) return SkillNodeAction.LockedPrereq;
            return SkillNodeAction.Learn;
        }

        public void GetActionCosts(int nodeId, out double gold, out double stone, out int rd)
        {
            nodeId = Mathf.Clamp(nodeId, 0, SkillTreeDef.Nodes.Length - 1);
            var n = SkillTreeDef.Nodes[nodeId];
            int cur = NodeLevel[nodeId];
            if (cur <= 0)
                SkillTreeDef.GetLearnCosts(n, out gold, out stone, out rd);
            else
                SkillTreeDef.GetEnhanceCosts(n, cur, out gold, out stone, out rd);
        }

        public bool CanAfford(int nodeId)
        {
            GetActionCosts(nodeId, out double gold, out double stone, out int rd);
            double haveGold = WalletAdapter.Instance != null ? WalletAdapter.Instance.Gold : 0;
            double haveStone = EnhanceStoneBalance();
            double haveRd = WalletAdapter.Instance != null ? WalletAdapter.Instance.RedDiamond : 0;
            return haveGold >= gold && haveStone >= stone && haveRd >= rd;
        }

        public bool CanLearn(int nodeId)
        {
            var a = GetNodeAction(nodeId);
            return a == SkillNodeAction.Learn && CanAfford(nodeId);
        }

        public bool CanEnhance(int nodeId)
        {
            var a = GetNodeAction(nodeId);
            return a == SkillNodeAction.Enhance && CanAfford(nodeId);
        }

        public string NodeStatusLine(int nodeId)
        {
            var n = SkillTreeDef.Nodes[nodeId];
            int cur = NodeLevel[nodeId];
            var action = GetNodeAction(nodeId);
            switch (action)
            {
                case SkillNodeAction.LockedLevel:
                    return $"Lv.{n.ReqLevel} 해금";
                case SkillNodeAction.LockedPrereq:
                    return $"{SkillTreeDef.Nodes[nodeId - 1].Name} 습득 필요";
                case SkillNodeAction.Max:
                    return "MAX";
                case SkillNodeAction.Learn:
                    GetActionCosts(nodeId, out double lg, out double ls, out _);
                    return $"습득 · 골드 {lg:0} · 강화석 {ls:0.#}";
                default:
                    GetActionCosts(nodeId, out double eg, out double es, out int er);
                    return $"강화 · 강화석 {es:0.#} · 골드 {eg:0} · RD {er}";
            }
        }

        public string ActionReason(int nodeId)
        {
            var n = SkillTreeDef.Nodes[nodeId];
            switch (GetNodeAction(nodeId))
            {
                case SkillNodeAction.LockedLevel:
                    return $"캐릭터 Lv.{n.ReqLevel} 필요";
                case SkillNodeAction.LockedPrereq:
                    return $"{SkillTreeDef.Nodes[nodeId - 1].Name}을(를) 먼저 습득하세요";
                case SkillNodeAction.Max:
                    return "최대 레벨";
                case SkillNodeAction.Learn:
                    if (!CanAfford(nodeId))
                    {
                        GetActionCosts(nodeId, out double g, out double s, out _);
                        return AffordFailMessage(g, s, 0);
                    }
                    return "습득 가능";
                case SkillNodeAction.Enhance:
                    if (!CanAfford(nodeId))
                    {
                        GetActionCosts(nodeId, out double g, out double s, out int r);
                        return AffordFailMessage(g, s, r);
                    }
                    return "강화 가능";
                default:
                    return "";
            }
        }

        public string ActionButtonLabel(int nodeId)
        {
            switch (GetNodeAction(nodeId))
            {
                case SkillNodeAction.Learn: return "습득";
                case SkillNodeAction.Enhance: return "강화";
                case SkillNodeAction.Max: return "MAX";
                case SkillNodeAction.LockedLevel: return "Lv 부족";
                case SkillNodeAction.LockedPrereq: return "선행 필요";
                default: return "—";
            }
        }

        public bool CanPerformAction(int nodeId)
        {
            var a = GetNodeAction(nodeId);
            if (a == SkillNodeAction.Learn) return CanLearn(nodeId);
            if (a == SkillNodeAction.Enhance) return CanEnhance(nodeId);
            return false;
        }

        public string PerformNodeAction(int nodeId)
        {
            switch (GetNodeAction(nodeId))
            {
                case SkillNodeAction.Learn: return LearnNode(nodeId);
                case SkillNodeAction.Enhance: return EnhanceNode(nodeId);
                case SkillNodeAction.Max: return SkillTreeDef.Nodes[nodeId].Name + " MAX";
                case SkillNodeAction.LockedLevel:
                    return $"{SkillTreeDef.Nodes[nodeId].Name} · 캐릭터 Lv.{SkillTreeDef.Nodes[nodeId].ReqLevel} 필요";
                case SkillNodeAction.LockedPrereq:
                    return $"{SkillTreeDef.Nodes[nodeId - 1].Name} 습득 필요";
                default: return "불가";
            }
        }

        public string InvestTalent()
        {
            RefreshUnlocks();
            if (WalletAdapter.Instance == null || !WalletAdapter.Instance.TrySpendRedDiamond(10))
                return "레드다이아 10 필요 (스킬/특성)";
            TalentPointsSpent++;
            PassiveMasteryPct += 1.5f;
            RecalcPassives();
            Save();
            OnChanged?.Invoke();
            return $"특성 투자 {TalentPointsSpent} · 패시브 {PassiveMasteryPct:0.#}%";
        }

        public bool CanInvestTalent() =>
            WalletAdapter.Instance != null && WalletAdapter.Instance.RedDiamond >= 10;

        public bool HasLearnableSkill
        {
            get
            {
                var nodes = SkillTreeDef.Nodes;
                for (int i = 0; i < nodes.Length; i++)
                    if (CanPerformAction(i)) return true;
                return CanInvestTalent();
            }
        }

        public string EnhanceSkill(int skillIndex) => PerformNodeAction(Mathf.Clamp(skillIndex, 0, 3));

        public string LearnNode(int nodeId)
        {
            RefreshUnlocks();
            nodeId = Mathf.Clamp(nodeId, 0, SkillTreeDef.Nodes.Length - 1);
            var n = SkillTreeDef.Nodes[nodeId];
            if (NodeLevel[nodeId] > 0)
                return n.Name + " 이미 습득함";
            if (!MeetsLevel(nodeId))
                return $"{n.Name} · 캐릭터 Lv.{n.ReqLevel} 필요";
            if (!MeetsPrereq(nodeId))
                return $"{SkillTreeDef.Nodes[nodeId - 1].Name} 습득 필요";

            SkillTreeDef.GetLearnCosts(n, out double gold, out double stone, out int rd);
            if (!TrySpendMaterials(gold, stone, rd, out string fail))
                return fail;

            NodeLevel[nodeId] = 1;
            if (!n.IsPassive)
                BonusUnlockMask |= 1 << nodeId;
            RefreshUnlocks();
            RecalcPassives();
            Save();
            OnChanged?.Invoke();
            return $"{n.Name} 습득 완료 (Lv.1/{n.MaxLevel})";
        }

        public string EnhanceNode(int nodeId)
        {
            RefreshUnlocks();
            nodeId = Mathf.Clamp(nodeId, 0, SkillTreeDef.Nodes.Length - 1);
            var n = SkillTreeDef.Nodes[nodeId];
            if (NodeLevel[nodeId] <= 0)
                return $"{n.Name} · 먼저 습득하세요";
            if (NodeLevel[nodeId] >= n.MaxLevel)
                return n.Name + " MAX";

            SkillTreeDef.GetEnhanceCosts(n, NodeLevel[nodeId], out double gold, out double stone, out int rd);
            if (!TrySpendMaterials(gold, stone, rd, out string fail))
                return fail;

            NodeLevel[nodeId]++;
            RefreshUnlocks();
            RecalcPassives();
            Save();
            OnChanged?.Invoke();
            return $"{n.Name} 강화 Lv.{NodeLevel[nodeId]} / {n.MaxLevel}";
        }

        /// <summary>Shop boost: force-learn next unlearned active (or enhance first).</summary>
        public string GrantSummonBoost()
        {
            RefreshUnlocks();
            for (int i = 0; i < 8; i++)
            {
                if (NodeLevel[i] > 0) continue;
                var n = SkillTreeDef.Nodes[i];
                NodeLevel[i] = 1;
                if (!n.IsPassive)
                    BonusUnlockMask |= 1 << i;
                RefreshUnlocks();
                RecalcPassives();
                Save();
                OnChanged?.Invoke();
                return n.Name + " 즉시 습득!";
            }
            return EnhanceNode(0);
        }

        public string EffectPreview(int nodeId)
        {
            nodeId = Mathf.Clamp(nodeId, 0, 7);
            var n = SkillTreeDef.Nodes[nodeId];
            int lv = Mathf.Max(1, NodeLevel[nodeId]);
            if (!n.IsPassive && nodeId < 4)
            {
                float dmg = Mathf.Min(BaseDamage[nodeId] + lv * 0.08f, 2.5f);
                float cd = Mathf.Max(4f, BaseCooldown[nodeId] - lv * 0.35f);
                return $"피해 +{dmg * 100f:0}% · CD {cd:0.#}초";
            }
            if (nodeId == 4) return $"최종뎀 +{lv * 2f:0.#}%";
            if (nodeId == 5) return $"골드 +{lv * 3f:0.#}%";
            if (nodeId == 6) return $"방치 +{lv * 4f:0.#}%";
            if (nodeId == 7) return $"최종뎀 +{lv * 3f:0.#}%";
            return n.EffectHint ?? "";
        }

        bool TrySpendMaterials(double gold, double stone, int rd, out string fail)
        {
            fail = null;
            var wallet = WalletAdapter.Instance;
            var cw = CurrencyWallet.Instance;
            var eq = EquipmentService.Instance;
            if (wallet == null || !wallet.TrySpendGold(gold))
            {
                fail = $"골드 {gold:0} 필요";
                return false;
            }
            bool stoneOk = false;
            if (stone <= 0.0001)
                stoneOk = true;
            else if (eq != null && eq.TrySpendEnhanceStones(stone))
                stoneOk = true;
            else if (cw != null && cw.TrySpend(CurrencyId.WeaponEnhanceStone, stone))
                stoneOk = true;
            if (!stoneOk)
            {
                wallet.AddGold(gold);
                fail = $"강화석 {stone:0.#} 필요";
                return false;
            }
            if (rd > 0 && wallet.TrySpendRedDiamond(rd) == false)
            {
                wallet.AddGold(gold);
                if (stone > 0.0001) eq?.AddEnhanceStones(stone);
                fail = $"레드다이아 {rd} 필요";
                return false;
            }
            return true;
        }

        string AffordFailMessage(double gold, double stone, int rd)
        {
            double haveGold = WalletAdapter.Instance != null ? WalletAdapter.Instance.Gold : 0;
            double haveStone = EnhanceStoneBalance();
            double haveRd = WalletAdapter.Instance != null ? WalletAdapter.Instance.RedDiamond : 0;
            if (haveGold < gold) return $"골드 부족 ({haveGold:0}/{gold:0})";
            if (haveStone < stone) return $"강화석 부족 ({haveStone:0.#}/{stone:0.#})";
            if (haveRd < rd) return $"RD 부족 ({haveRd:0}/{rd})";
            return "재화 부족";
        }

        double EnhanceStoneBalance()
        {
            double s = 0;
            if (EquipmentService.Instance != null)
                s += EquipmentService.Instance.EnhanceStones;
            if (CurrencyWallet.Instance != null)
                s += CurrencyWallet.Instance.Get(CurrencyId.WeaponEnhanceStone);
            return s;
        }

        void ApplyActiveStats()
        {
            for (int i = 0; i < 4; i++)
            {
                int lv = Mathf.Max(0, NodeLevel[i]);
                DamageBonus[i] = Mathf.Min(BaseDamage[i] + lv * 0.08f, 2.5f);
                MaxCooldown[i] = Mathf.Max(4f, BaseCooldown[i] - lv * 0.35f);
            }
        }

        void RecalcPassives()
        {
            PassiveDmgPct = NodeLevel[4] * 2f + NodeLevel[7] * 3f + PassiveMasteryPct * 0.2f;
            PassiveGoldPct = NodeLevel[5] * 3f + TalentPointsSpent * 0.5f;
            PassiveIdlePct = NodeLevel[6] * 4f + PassiveMasteryPct * 0.15f;
        }

        int CountUnlocked()
        {
            int n = 0;
            for (int i = 0; i < 4; i++) if (NodeLevel[i] > 0) n++;
            return n;
        }

        string TreePrefRoot(string treeId) => PrefKey + "." + treeId;

        void Save()
        {
            string root = TreePrefRoot(_loadedTreeId);
            PlayerPrefs.SetInt(root + ".tp", TalentPointsSpent);
            PlayerPrefs.SetFloat(root + ".pas", PassiveMasteryPct);
            PlayerPrefs.SetInt(root + ".bonus", BonusUnlockMask);
            for (int i = 0; i < 8; i++)
                PlayerPrefs.SetInt(root + ".n" + i, NodeLevel[i]);
            // Keep legacy keys for hero as shared fallback
            if (_loadedTreeId == "hero")
            {
                PlayerPrefs.SetInt(PrefKey + ".tp", TalentPointsSpent);
                PlayerPrefs.SetFloat(PrefKey + ".pas", PassiveMasteryPct);
                PlayerPrefs.SetInt(PrefKey + ".bonus", BonusUnlockMask);
                for (int i = 0; i < 8; i++)
                    PlayerPrefs.SetInt(PrefKey + ".n" + i, NodeLevel[i]);
            }
            PlayerPrefs.Save();
        }

        void LoadForTree(string treeId)
        {
            _loadedTreeId = string.IsNullOrEmpty(treeId) ? "hero" : treeId;
            string root = TreePrefRoot(_loadedTreeId);
            bool hasTree = PlayerPrefs.HasKey(root + ".n0") || PlayerPrefs.HasKey(root + ".tp");
            string src = hasTree ? root : PrefKey;
            TalentPointsSpent = PlayerPrefs.GetInt(src + ".tp", 0);
            PassiveMasteryPct = PlayerPrefs.GetFloat(src + ".pas", 0f);
            BonusUnlockMask = PlayerPrefs.GetInt(src + ".bonus", 0);
            for (int i = 0; i < 8; i++)
            {
                int v = PlayerPrefs.GetInt(src + ".n" + i, -1);
                if (v < 0 && i < 4)
                    v = PlayerPrefs.GetInt(src + ".enh" + i, 0);
                NodeLevel[i] = Mathf.Max(0, v);
            }
            RefreshSkillNamesFromTree();
            ApplyActiveStats();
            RecalcPassives();
        }
    }
}
