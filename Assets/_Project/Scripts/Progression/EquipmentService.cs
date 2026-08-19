using System;
using IdleMvp.Adapters;
using IdleMvp.Core;
using UnityEngine;

namespace IdleMvp.Progression
{
    public enum EquipSlot
    {
        Weapon = 0,
        Helm = 1,
        Armor = 2,
        Accessory = 3,
        Legs = 4,
        Boots = 5
    }

    [Serializable]
    public class EquipItem
    {
        public EquipSlot slot;
        public int level = 1;
        public int rarity; // 0=N .. 3=SSR
        public int atkBonus;
        public int hpBonus;
        public int defBonus;
    }

    /// <summary>
    /// Facade over InventoryAdapter 6 slots + enhance stone wallet.
    /// Legacy 4-slot saves migrate on load.
    /// </summary>
    public class EquipmentService : MonoBehaviour
    {
        public static EquipmentService Instance { get; private set; }

        public EquipItem[] Slots { get; private set; }
        public double EnhanceStones { get; private set; }

        public event Action OnChanged;

        const string PrefKey = "IdleGrow.Equipment";
        public const int SlotCount = 6;

        static readonly string[] SlotNames = { "무기", "투구", "갑옷", "장신구", "하의", "신발" };
        static readonly string[] RarityNames = { "N", "R", "SR", "SSR" };

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            EnsureSlots();
            Load();
            SyncFromInventory();
        }

        void EnsureSlots()
        {
            Slots = new EquipItem[SlotCount];
            for (int i = 0; i < SlotCount; i++)
            {
                Slots[i] = new EquipItem
                {
                    slot = (EquipSlot)i,
                    level = 1,
                    rarity = 0,
                    atkBonus = i == 0 ? 3 : 0,
                    hpBonus = (i == 2 || i == 4) ? 10 : 0,
                    defBonus = (i == 1 || i == 5) ? 1 : 0
                };
            }
        }

        public string SlotLabel(int i) => SlotNames[Mathf.Clamp(i, 0, SlotNames.Length - 1)];
        public string RarityLabel(int r) => RarityNames[Mathf.Clamp(r, 0, 3)];

        void SyncFromInventory()
        {
            var inv = InventoryAdapter.Instance;
            if (inv?.Slots == null) return;
            for (int i = 0; i < SlotCount && i < inv.Slots.Length; i++)
            {
                Slots[i].level = inv.Slots[i].level;
                Slots[i].rarity = inv.Slots[i].rarity;
            }
        }

        public float BonusAtk =>
            InventoryAdapter.Instance != null
                ? InventoryAdapter.Instance.BonusAtk
                : LegacyBonusAtk;

        float LegacyBonusAtk
        {
            get
            {
                float v = 0;
                foreach (var s in Slots) v += s.atkBonus * s.level * (1f + s.rarity * 0.25f);
                return v;
            }
        }

        public float BonusHp =>
            InventoryAdapter.Instance != null
                ? InventoryAdapter.Instance.BonusHp
                : LegacyBonusHp;

        float LegacyBonusHp
        {
            get
            {
                float v = 0;
                foreach (var s in Slots) v += s.hpBonus * s.level * (1f + s.rarity * 0.25f);
                return v;
            }
        }

        public float BonusDef =>
            InventoryAdapter.Instance != null
                ? InventoryAdapter.Instance.BonusDef
                : LegacyBonusDef;

        float LegacyBonusDef
        {
            get
            {
                float v = 0;
                foreach (var s in Slots) v += s.defBonus * s.level * (1f + s.rarity * 0.25f);
                return v;
            }
        }

        /// <summary>Legacy aggregate — combat/CP authority is CombatPowerService.</summary>
        public float EquipmentCp =>
            InventoryAdapter.Instance != null
                ? InventoryAdapter.Instance.EquipmentCp
                : BonusAtk * 10f + BonusHp * 0.5f + BonusDef * 8f;

        public void AddEnhanceStones(double amount)
        {
            if (amount == 0) return;
            EnhanceStones = System.Math.Max(0, EnhanceStones + amount);
            Save();
            OnChanged?.Invoke();
        }

        public bool TrySpendEnhanceStones(double amount)
        {
            if (amount <= 0) return true;
            if (EnhanceStones < amount) return false;
            EnhanceStones -= amount;
            Save();
            OnChanged?.Invoke();
            return true;
        }

        public float UpgradeGoldCost(int slotIndex)
        {
            if (IdleMvp.Adapters.InventoryAdapter.Instance != null)
            {
                var inv = IdleMvp.Adapters.InventoryAdapter.Instance.Slots;
                if (inv != null && slotIndex >= 0 && slotIndex < inv.Length)
                    return 30f + inv[slotIndex].level * 25f + inv[slotIndex].rarity * 40f;
            }
            if (Slots == null || slotIndex < 0 || slotIndex >= Slots.Length)
                return 30f;
            var s = Slots[slotIndex];
            return 30f + s.level * 25f + s.rarity * 40f;
        }

        public float UpgradeStoneCost(int slotIndex)
        {
            if (IdleMvp.Adapters.InventoryAdapter.Instance != null)
            {
                var inv = IdleMvp.Adapters.InventoryAdapter.Instance.Slots;
                if (inv != null && slotIndex >= 0 && slotIndex < inv.Length)
                    return 1f + inv[slotIndex].level * 0.5f;
            }
            if (Slots == null || slotIndex < 0 || slotIndex >= Slots.Length)
                return 1f;
            var s = Slots[slotIndex];
            return 1f + s.level * 0.5f;
        }

        public bool TryUpgrade(int slotIndex, IdleMvp.Economy.PlayerWallet wallet)
        {
            if (IdleMvp.Adapters.InventoryAdapter.Instance != null && slotIndex < SlotCount)
            {
                bool ok = IdleMvp.Adapters.InventoryAdapter.Instance.TryUpgradeSlot(slotIndex);
                if (ok)
                {
                    SyncFromInventory();
                    OnChanged?.Invoke();
                }
                return ok;
            }

            if (Slots == null || slotIndex < 0 || slotIndex >= Slots.Length) return false;
            float gold = UpgradeGoldCost(slotIndex);
            float stone = UpgradeStoneCost(slotIndex);
            if (wallet == null || !wallet.TrySpendGold(gold)) return false;
            if (EnhanceStones < stone)
            {
                wallet.AddGold(gold); // refund
                return false;
            }
            EnhanceStones -= stone;
            Slots[slotIndex].level++;
            if (Slots[slotIndex].level % 5 == 0 && Slots[slotIndex].rarity < 3)
                Slots[slotIndex].rarity++;
            Save();
            OnChanged?.Invoke();
            return true;
        }

        void Save()
        {
            SyncFromInventory();
            var data = new SaveData
            {
                stones = EnhanceStones,
                slots = Slots
            };
            PlayerPrefs.SetString(PrefKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(PrefKey)) return;
            var data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(PrefKey));
            if (data == null) return;
            EnhanceStones = data.stones;
            if (data.slots == null) return;
            if (data.slots.Length == SlotCount)
                Slots = data.slots;
            else
            {
                // Migrate 4 → 6
                for (int i = 0; i < data.slots.Length && i < SlotCount; i++)
                    Slots[i] = data.slots[i];
                // Old boots (3) → new boots (5)
                if (data.slots.Length == 4 && data.slots[3] != null)
                {
                    Slots[5] = data.slots[3];
                    Slots[5].slot = EquipSlot.Boots;
                }
            }
        }

        [Serializable]
        class SaveData
        {
            public double stones;
            public EquipItem[] slots;
        }
    }
}
