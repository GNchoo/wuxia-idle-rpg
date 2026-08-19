using IdleMvp.Adapters;
using IdleMvp.Combat;
using IdleMvp.Core;
using IdleMvp.Economy;
using IdleMvp.Progression;
using UnityEngine;

namespace IdleMvp.Boot
{
    /// <summary>
    /// Spawns grow + maple adapter services. Does not load GameScene combat.
    /// </summary>
    public static class GrowGameBootstrap
    {
        public const string RootName = "IdleGrow_Root";

        public static GameObject EnsureRoot()
        {
            var existing = GameObject.Find(RootName);
            if (existing != null) return existing;

            BalanceConfig.Load();
            LevelXpTable.Load();
            StageTable.Load();
            ContentCatalog.Load();

            var root = new GameObject(RootName);
            Object.DontDestroyOnLoad(root);

            root.AddComponent<StageProgress>();
            root.AddComponent<PlayerGrowth>();
            root.AddComponent<CurrencyWallet>();
            root.AddComponent<PlayerWallet>();
            root.AddComponent<LootBoxService>();
            root.AddComponent<EquipmentService>();
            root.AddComponent<DungeonService>();

            root.AddComponent<TemplateFeatureHost>();
            root.AddComponent<WalletAdapter>();
            root.AddComponent<InventoryAdapter>();
            root.AddComponent<WeaponSummonAdapter>();
            root.AddComponent<CompanionAdapter>();
            root.AddComponent<SkillAdapter>();
            root.AddComponent<ShopAdapter>();
            root.AddComponent<CombatPowerService>();
            root.AddComponent<SlotEnhanceService>();
            root.AddComponent<EliteSummonService>();
            root.AddComponent<PassService>();
            root.AddComponent<MembershipService>();
            root.AddComponent<CompanionCombatBridge>();
            root.AddComponent<IapBridge>();
            root.AddComponent<AdBridge>();
            root.AddComponent<MailService>();
            root.AddComponent<GuildAdapter>();
            root.AddComponent<ArenaAdapter>();
            root.AddComponent<RaidService>();
            root.AddComponent<CostumeAdapter>();
            root.AddComponent<ArtifactService>();
            root.AddComponent<SaveSnapshotService>();
            root.AddComponent<LevelPackageService>();
            root.AddComponent<SeasonPassService>();
            root.AddComponent<IdleMvp.Core.FatedEventService>();
            root.AddComponent<IdleMvp.Core.RebirthService>();

            Debug.Log("[IdleGrow] Services + Template adapters ready.");
            return root;
        }
    }
}
