using System;
using System.Collections.Generic;
using IdleMvp.Economy;
using IdleMvp.Progression;
using UnityEngine;

namespace IdleMvp.Adapters
{
    /// <summary>
    /// Wraps template InventoryManager when present; otherwise mirrors 6 equip slots (asset layout).
    /// EquipmentService facades through this.
    /// </summary>
    public class InventoryAdapter : MonoBehaviour
    {
        public static InventoryAdapter Instance { get; private set; }

        public static readonly string[] SlotNames =
        {
            "무기", "투구", "갑옷", "장신구", "하의", "신발"
        };

        [Serializable]
        public class SlotState
        {
            public int level = 1;
            public int rarity;
            public int fragment;
            public bool owned = true;
        }

        public SlotState[] Slots { get; private set; }
        public event Action OnChanged;

        const string PrefKey = "IdleGrow.Maple.InventoryMirror";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Slots = new SlotState[6];
            for (int i = 0; i < 6; i++)
                Slots[i] = new SlotState { level = 1, rarity = 0, fragment = 0, owned = true };
            Load();
        }

        public float EquipmentCp
        {
            get
            {
                // Armor slots only (0 = weapon mirror; ATK/CP come from WeaponSummon).
                float cp = 0f;
                for (int i = 1; i < Slots.Length; i++)
                {
                    var s = Slots[i];
                    cp += (8f + s.level * 6f) * (1f + s.rarity * 0.35f);
                }
                var host = TemplateFeatureHost.Instance;
                if (host != null && host.HasTemplateInventory)
                {
                    try
                    {
                        var list = host.InventoryManager.GetType().GetField("ItemPercentageIncrease");
                        if (list?.GetValue(host.InventoryManager) is List<float> perc)
                        {
                            foreach (var p in perc) cp += p * 2f;
                        }
                    }
                    catch { /* ignore */ }
                }
                return cp;
            }
        }

        /// <summary>Armor ATK only — slot 0 excluded to avoid double-count with WeaponSummon.</summary>
        public float BonusAtk
        {
            get
            {
                float atk = 0f;
                for (int i = 1; i < Slots.Length; i++)
                    atk += Slots[i].level * (1f + Slots[i].rarity * 0.2f) * 0.4f;
                return atk;
            }
        }

        public float BonusHp
        {
            get
            {
                float hp = 0f;
                for (int i = 1; i < Slots.Length; i++)
                    hp += Slots[i].level * (4f + Slots[i].rarity * 2f);
                return hp;
            }
        }

        public float BonusDef
        {
            get
            {
                float def = 0f;
                for (int i = 1; i < Slots.Length; i++)
                    def += Slots[i].level * (0.35f + Slots[i].rarity * 0.15f);
                return def;
            }
        }

        public string SlotLabel(int i) => SlotNames[Mathf.Clamp(i, 0, SlotNames.Length - 1)];

        public bool TryUpgradeSlot(int index)
        {
            if (index < 0 || index >= Slots.Length) return false;
            float gold = 30f + Slots[index].level * 25f;
            float stone = 1f + Slots[index].level * 0.5f;
            if (WalletAdapter.Instance == null || !WalletAdapter.Instance.TrySpendGold(gold)) return false;
            if (CurrencyWallet.Instance == null || !CurrencyWallet.Instance.TrySpend(CurrencyId.ArmorStone, stone))
            {
                // allow enhance stone fallback from EquipmentService path
                if (EquipmentService.Instance == null ||
                    !EquipmentService.Instance.TrySpendEnhanceStones(stone))
                {
                    WalletAdapter.Instance.AddGold(gold);
                    return false;
                }
            }
            Slots[index].level++;
            Slots[index].fragment++;
            if (Slots[index].fragment >= 5)
            {
                Slots[index].fragment = 0;
                if (Slots[index].rarity < 3) Slots[index].rarity++;
            }
            Save();
            OnChanged?.Invoke();
            return true;
        }

        public void GrantDrop(GachaRarity rarity)
        {
            int slot = UnityEngine.Random.Range(0, Slots.Length);
            Slots[slot].owned = true;
            Slots[slot].fragment += 1 + (int)rarity;
            if (Slots[slot].rarity < (int)rarity)
                Slots[slot].rarity = (int)rarity;
            CurrencyWallet.Instance?.Add(CurrencyId.ArmorStone, 0.5f + (int)rarity * 0.5f);
            Save();
            OnChanged?.Invoke();
        }

        /// <summary>
        /// Trim low-rarity armor slot progress into ArmorStone (slots themselves remain).
        /// maxRarityExclusive: rarity below this is trimmed (default 1 = Common only).
        /// </summary>
        public string DisassembleJunk(int maxRarityExclusive = 1)
        {
            float stones = 0f;
            int trimmed = 0;
            // Skip slot 0 (weapon mirror)
            for (int i = 1; i < Slots.Length; i++)
            {
                var s = Slots[i];
                if (s == null) continue;
                if (s.rarity >= maxRarityExclusive) continue;
                if (s.fragment <= 0 && s.level <= 1) continue;

                stones += s.fragment * 0.6f + Mathf.Max(0, s.level - 1) * 0.4f + 0.5f;
                s.fragment = 0;
                if (s.level > 1) s.level = Mathf.Max(1, s.level - 1);
                // Keep rarity floor so slot isn't empty, but common stays common
                trimmed++;
            }

            if (trimmed == 0)
            {
                // Soft fallback: still grant a little stone so button isn't dead early-game
                stones = 1.5f;
                CurrencyWallet.Instance?.Add(CurrencyId.ArmorStone, stones);
                OnChanged?.Invoke();
                return $"분해 여유분 없음 · 기본 아머스톤 +{stones:0.#}";
            }

            CurrencyWallet.Instance?.Add(CurrencyId.ArmorStone, stones);
            Save();
            OnChanged?.Invoke();
            return $"방어구 정리 {trimmed}슬롯 · 아머스톤 +{stones:0.#}";
        }

        public void NotifyChanged()
        {
            Save();
            OnChanged?.Invoke();
        }

        void Save()
        {
            PlayerPrefs.SetString(PrefKey, JsonUtility.ToJson(new SaveWrap { slots = Slots }));
            PlayerPrefs.Save();
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(PrefKey)) return;
            var w = JsonUtility.FromJson<SaveWrap>(PlayerPrefs.GetString(PrefKey));
            if (w?.slots != null && w.slots.Length == 6) Slots = w.slots;
        }

        [Serializable]
        class SaveWrap
        {
            public SlotState[] slots;
        }
    }
}
