using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Globalization;


namespace SAMPLETEXT
{


    [CreateAssetMenu(menuName = "SO Item")]
    public class ItemSO : ScriptableObject
    {
        [Header("SO ITEM Settings")]
        public string[] LanguageReferenceOnly;
        public string[] ItemName;
        public string[] ItemDescription;
        public int ItemNameID;
        public ItemType itemType;

        [Header("SO ITEM UI Settings")]
        public Sprite Image;
        public int ItemIDSO;
        public float ItemCurrentCount;
        public int ItemMaxLevelID;
        //public int ItemMaxLevel;
        public int[] ItemNextMaxLevel;
        public float ItemStatsValue;

        public enum ItemType
        {
            Weapon,
            Head,
            Chest,
            Accessory,
            Leg,
            Shoes
        }
    }
}
