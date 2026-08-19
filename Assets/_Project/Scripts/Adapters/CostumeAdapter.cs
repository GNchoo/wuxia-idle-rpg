using System;
using System.Collections.Generic;
using IdleMvp.Economy;
using UnityEngine;

namespace IdleMvp.Adapters
{
    /// <summary>Owned costume ids + equipped tint index (no mesh swap).</summary>
    public class CostumeAdapter : MonoBehaviour
    {
        public static CostumeAdapter Instance { get; private set; }

        public List<int> Owned { get; private set; } = new List<int>();
        public int Equipped { get; private set; } = -1;

        public event Action OnChanged;

        const string PrefKey = "IdleGrow.Maple.Costume";
        public const int CatalogCount = 6;
        public const int PriceBlue = 60;

        static readonly Color[] Tints =
        {
            Color.white,
            new Color(0.7f, 0.85f, 1f),
            new Color(1f, 0.75f, 0.7f),
            new Color(0.75f, 1f, 0.8f),
            new Color(1f, 0.9f, 0.55f),
            new Color(0.85f, 0.7f, 1f)
        };

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

        public bool Owns(int id) => Owned.Contains(id);

        public string Buy(int id)
        {
            id = Mathf.Clamp(id, 0, CatalogCount - 1);
            if (Owns(id)) return Equip(id);
            if (CurrencyWallet.Instance == null ||
                !CurrencyWallet.Instance.TrySpend(CurrencyId.BlueDiamond, PriceBlue))
                return $"블루다이아 {PriceBlue} 필요";
            Owned.Add(id);
            Equipped = id;
            Save();
            OnChanged?.Invoke();
            return $"코스튬 {id + 1} 구매 · 적용";
        }

        public string Equip(int id)
        {
            if (!Owns(id)) return "미보유 코스튬";
            Equipped = id;
            Save();
            OnChanged?.Invoke();
            return $"코스튬 {id + 1} 장착";
        }

        public Color EquippedTint =>
            Equipped >= 0 && Equipped < Tints.Length ? Tints[Equipped] : Color.white;

        public float CostumeCp => Equipped >= 0 ? 15f : 0f;

        void Save()
        {
            PlayerPrefs.SetString(PrefKey, JsonUtility.ToJson(new Wrap
            {
                owned = Owned.ToArray(),
                eq = Equipped
            }));
            PlayerPrefs.Save();
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(PrefKey)) return;
            var w = JsonUtility.FromJson<Wrap>(PlayerPrefs.GetString(PrefKey));
            if (w?.owned != null) Owned = new List<int>(w.owned);
            Equipped = w?.eq ?? -1;
        }

        [Serializable]
        class Wrap
        {
            public int[] owned;
            public int eq;
        }
    }
}
