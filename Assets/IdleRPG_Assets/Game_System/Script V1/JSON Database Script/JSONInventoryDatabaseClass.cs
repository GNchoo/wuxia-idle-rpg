using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SAMPLETEXT.Data.Class.Village
{
    public class JSONInventoryDatabaseClass
    {
        public List<bool> _JSONInventoryActiveCollectionList = new List<bool>();
        public List<int> _JSONInventoryLevelCollectionList = new List<int>();
        public List<int> _JSONInventoryFragmentCollectionList = new List<int>();
        public List<bool> _JSONItemEquipList = new List<bool>();
        public List<int> _JSONVillageUpgradeLevel = new List<int>();

        public int _JSONWeaponEquipID;
        public int _JSONHeadEquipID;
        public int _JSONChestEquipID;
        public int _JSONAccessoryEquipID;
        public int _JSONLegsEquipID;
        public int _JSONShoesEquipID;
    }
}

