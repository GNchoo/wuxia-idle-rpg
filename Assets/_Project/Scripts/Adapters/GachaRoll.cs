using UnityEngine;

namespace IdleMvp.Adapters
{
    /// <summary>
    /// Port of ItemsPurchaseHeroManagerScript rarity roll (Random.Range 0..50) + soft pity.
    /// </summary>
    public enum GachaRarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3
    }

    public static class GachaRoll
    {
        public const int SoftPityEpic = 20;
        public const int HardPityLegendary = 80;

        public static GachaRarity RollHeroChestStyle()
        {
            int i = Random.Range(0, 50);
            if (i == 49) return GachaRarity.Legendary;
            if (i >= 45) return GachaRarity.Epic;
            if (i >= 35) return GachaRarity.Rare;
            return GachaRarity.Common;
        }

        /// <summary>Applies soft/hard pity. Updates counters via ref.</summary>
        public static GachaRarity RollWithPity(ref int pityEpic, ref int pityLegendary)
        {
            pityEpic++;
            pityLegendary++;

            GachaRarity rarity;
            if (pityLegendary >= HardPityLegendary)
            {
                rarity = GachaRarity.Legendary;
            }
            else if (pityEpic >= SoftPityEpic)
            {
                rarity = GachaRarity.Epic;
            }
            else
            {
                rarity = RollHeroChestStyle();
            }

            if (rarity >= GachaRarity.Epic) pityEpic = 0;
            if (rarity == GachaRarity.Legendary) pityLegendary = 0;
            return rarity;
        }

        public static int RarityToInt(GachaRarity r) => (int)r;
    }
}
