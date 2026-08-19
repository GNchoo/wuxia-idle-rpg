using System;
using IdleMvp.Adapters;
using IdleMvp.Economy;
using UnityEngine;

namespace IdleMvp.Progression
{
    /// <summary>
    /// Maple gap: spend Monster Point → elite kill → equipment drop via InventoryAdapter.
    /// </summary>
    public class EliteSummonService : MonoBehaviour
    {
        public static EliteSummonService Instance { get; private set; }

        public int SummonLevel { get; private set; } = 1;
        public event Action OnChanged;

        const string PrefKey = "IdleGrow.Maple.EliteSummon";
        public const float MonsterPointCost = 10f;
        public const float ArmorStonePerLevel = 15f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            SummonLevel = Mathf.Max(1, PlayerPrefs.GetInt(PrefKey + ".lv", 1));
        }

        public string TrySummonElite()
        {
            if (CurrencyWallet.Instance == null ||
                !CurrencyWallet.Instance.TrySpend(CurrencyId.MonsterPoint, MonsterPointCost))
                return "몬스터 포인트 부족";

            // Higher summon level → better rarity weight (mirrors elite drop intent)
            int roll = UnityEngine.Random.Range(0, 100) + SummonLevel * 2;
            GachaRarity r = GachaRarity.Common;
            if (roll >= 95) r = GachaRarity.Legendary;
            else if (roll >= 80) r = GachaRarity.Epic;
            else if (roll >= 55) r = GachaRarity.Rare;

            InventoryAdapter.Instance?.GrantDrop(r);
            OnChanged?.Invoke();
            return $"엘리트 처치! {r} 장비 파편 획득 (소환Lv {SummonLevel})";
        }

        public string TryRaiseSummonLevel()
        {
            if (CurrencyWallet.Instance == null ||
                !CurrencyWallet.Instance.TrySpend(CurrencyId.ArmorStone, ArmorStonePerLevel))
                return "아머 스톤 부족";
            SummonLevel++;
            PlayerPrefs.SetInt(PrefKey + ".lv", SummonLevel);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
            return $"엘리트 소환 레벨 → {SummonLevel}";
        }
    }
}
