using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SAMPLETEXT.Gameplay.Manager.MainHero;
using SAMPLETEXT.Gameplay.Manager.Enemy;
using SAMPLETEXT.Market.Manager;
using TMPro;

namespace SAMPLETEXT.Inventory.Manager
{
    public class InventoryManagerScript : MonoBehaviour
    {
        [Header("Inventory Settings")]
        public List<bool> InventoryActiveCollectionList = new List<bool>();// New Json
        public List<int> InventoryLevelCollectionList = new List<int>();// New Json
        public List<int> InventoryFragmentCollectionList = new List<int>();// New Json
        [SerializeField]
        GameObject[] InventoryItemObjCollection;
        public string[] InventoryNameCollection; // No Saving
        public Sprite[] InventoryImageCollection; // No Saving
        [SerializeField]
        string[] InventoryDescriptionCollection;
        [SerializeField]
        int[] InventoryMaxFragmentLevelCollection;
        string TempStringID;

        [Header("Inventory Front Display Text Settings")]
        [SerializeField]
        TextMeshProUGUI[] InventoryDisplayTextCollection;

        enum ItemClassifcationType
        {
            INCREASEDPS,
            BOSSGOLDDROP,
            ATTACKSPEED,
            CRITICALDAMAGEINCREASE,
            SPELLCOOLDOWN,
            VILLAGEINCOMEINCREASE,
        }
        [SerializeField]
        List<ItemClassifcationType> ItemClassification = new List<ItemClassifcationType>();
        
        public List<float> ItemPercentageIncrease = new List<float>();
        [SerializeField]
        List<float> ItemPercentageAdditionalIncrease = new List<float>();

        [Header("PopUp Window Item Settings")]
        [SerializeField]
        GameObject ItemWindowPopUpObj;
        [SerializeField]
        TextMeshProUGUI ItemNameText;
        [SerializeField] 
        TextMeshProUGUI ItemDescriptionText;
        [SerializeField]
        TextMeshProUGUI ItemStatText;
        [SerializeField]
        TextMeshProUGUI ItemFragmentCountText;
        [SerializeField]
        Button UpgradeButton;
        [SerializeField]
        Button EquipButton;
        [SerializeField]
        Image ItemDisplayImage;

        [Header("Equip Settings")]
        [SerializeField]
        List<ItemEquipType> ItemEquipClassification = new List<ItemEquipType>();

        public List<bool> ItemEquipList = new List<bool>(); // New JSON
        enum ItemEquipType
        {
            WEAPON,
            HEAD,
            CHEST,
            ACCESSORY,
            LEGS,
            SHOES
        }
        [SerializeField]
        Image WeaponEquipDisplay;
        [SerializeField]
        GameObject WeaponDefaultIcon;
        public int WeaponEquipID; // New JSON
        [SerializeField]
        Image HeadEquipDisplay;
        [SerializeField]
        GameObject HeadDefaultIcon;
        public int HeadEquipID;// New JSON
        [SerializeField]
        Image ChestEquipDisplay;
        [SerializeField]
        GameObject ChestDefaultIcon;
        public int ChestEquipID;// New JSON
        [SerializeField]
        Image AccessoryEquipDisplay;
        [SerializeField]
        GameObject AccessoryDefaultIcon;
        public int AccessoryEquipID;// New JSON
        [SerializeField]
        Image LegsEquipDisplay;
        [SerializeField]
        GameObject LegsDefaultIcon;
        public int LegsEquipID;// New JSON
        [SerializeField]
        Image ShoesEquipDisplay;
        [SerializeField]
        GameObject ShoesDefaultIcon;
        public int ShoesEquipID;// New JSON
        int ItemGeneralID;

        [Header("UnEquip Settings")]
        [SerializeField]
        GameObject UnEquipItemWindowPopUpObj;
        [SerializeField]
        TextMeshProUGUI UnEquipItemNameText;
        [SerializeField]
        TextMeshProUGUI UnEquipItemDescriptionText;
        [SerializeField]
        TextMeshProUGUI UnEquipItemStatText;
        [SerializeField]
        TextMeshProUGUI UnEquipItemFragmentCountText;
        [SerializeField]
        Button UnEquipUpgradeButton;
        [SerializeField]
        Image UnEquipItemDisplayImage;
        [SerializeField]
        List<ItemEquipType> UnEquipItemSlotClassification = new List<ItemEquipType>();
        [SerializeField]
        Button[] UnEquipSlotButton;
        public List<bool> UnEquipButtonControlCollectionList = new List<bool>();
        int UnEquipItemSlotID;
        int UnEquipItemTypeID;

        [Header("Inventory To Main Hero Settings")]
        [SerializeField]
        GameplayMainHeroManagerScript MainHero;
        float MainIncreaseDPS;
        float MainAttackSpeed;
        float MainCriticalDamageIncrease;

        [Header("Inventory To Main Enemy Settings")]
        [SerializeField]
        GameplayEnemyManagerScript MainEnemy;
        float MainBossGoldDrop;

        [Header("Inventory To Spell Cooldown Settings")]
        [SerializeField]
        float MainSpellCoolDown;

        [Header("Inventory To Village Income Increase Settings")]
        [SerializeField]
        MarketManagerScript MainVillage;
        [SerializeField]
        float MainVillageIncomeIncrease;

        // Start is called before the first frame update
        void Start()
        {
            FirstCheck();
        }

        public void FirstCheck()
        {
            for(int i = 0; i < InventoryActiveCollectionList.Count; i++)
            {
                if (InventoryActiveCollectionList[i] == true)
                {
                    if (ItemEquipList[i] == false)
                    {
                        InventoryItemObjCollection[i].gameObject.SetActive(true);
                    }

                    if (ItemEquipList[i] == true)
                    {
                        InventoryItemObjCollection[i].gameObject.SetActive(false);
                    }
                }

                if (InventoryActiveCollectionList[i] == false)
                {
                    InventoryItemObjCollection[i].gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < InventoryDisplayTextCollection.Length; i++)
            {
                InventoryDisplayTextCollection[i].text = InventoryFragmentCollectionList[i] + "/" + InventoryMaxFragmentLevelCollection[InventoryLevelCollectionList[i]];
            }

            for (int i = 0; i < UnEquipButtonControlCollectionList.Count; i ++)
            {
                if (UnEquipButtonControlCollectionList[i] == false)
                {
                    UnEquipSlotButton[i].interactable = false;
                }

                if (UnEquipButtonControlCollectionList[i] == true)
                {
                    UnEquipSlotButton[i].interactable = true;
                }
            }

            LoadEquipItem();
        }

        public void LoadEquipItem()
        {
            for (int i = 0; i < ItemEquipList.Count; i++)
            {
                if (ItemEquipList[i] == true)
                {
                    if (ItemEquipList[i] == true)
                    {
                        if (ItemEquipClassification[i] == ItemEquipType.WEAPON)
                        {
                            WeaponEquipDisplay.gameObject.SetActive(true);
                            WeaponEquipDisplay.sprite = InventoryImageCollection[WeaponEquipID];

                            WeaponDefaultIcon.gameObject.SetActive(false);

                            UnEquipButtonControlCollectionList[0] = true;

                            if (ItemClassification[WeaponEquipID] == ItemClassifcationType.INCREASEDPS)
                            {
                                MainIncreaseDPS = ItemPercentageIncrease[WeaponEquipID];
                                MainHero.InventoryAdditionalDPS = MainIncreaseDPS;
                            }

                            if (ItemClassification[WeaponEquipID] == ItemClassifcationType.ATTACKSPEED)
                            {
                                MainAttackSpeed = ItemPercentageIncrease[WeaponEquipID];
                                MainHero.InventoryAdditionalAttackSpeed = MainAttackSpeed;
                            }

                            if (ItemClassification[WeaponEquipID] == ItemClassifcationType.CRITICALDAMAGEINCREASE)
                            {
                                MainCriticalDamageIncrease = ItemPercentageIncrease[WeaponEquipID];
                                MainHero.InventoryCriticalDamageIncrease = MainCriticalDamageIncrease;
                            }

                            if (ItemClassification[WeaponEquipID] == ItemClassifcationType.BOSSGOLDDROP)
                            {
                                MainBossGoldDrop = ItemPercentageIncrease[WeaponEquipID];
                                MainEnemy.InventoryBossGoldDrop = MainBossGoldDrop;
                            }
                            if (ItemClassification[WeaponEquipID] == ItemClassifcationType.VILLAGEINCOMEINCREASE)
                            {
                                MainVillageIncomeIncrease = ItemPercentageIncrease[WeaponEquipID];
                                MainVillage.InventoryVillageIncomeIncrease = MainVillageIncomeIncrease;
                            }
                            //break;
                        }

                        if (ItemEquipClassification[i] == ItemEquipType.CHEST)
                        {
                            ChestEquipDisplay.gameObject.SetActive(true);
                            ChestEquipDisplay.sprite = InventoryImageCollection[ChestEquipID];

                            ChestDefaultIcon.gameObject.SetActive(false);

                            UnEquipButtonControlCollectionList[1] = true;

                            if (ItemClassification[ChestEquipID] == ItemClassifcationType.INCREASEDPS)
                            {
                                MainIncreaseDPS = ItemPercentageIncrease[ChestEquipID];
                                MainHero.InventoryAdditionalDPS = MainIncreaseDPS;
                            }

                            if (ItemClassification[ChestEquipID] == ItemClassifcationType.ATTACKSPEED)
                            {
                                MainAttackSpeed = ItemPercentageIncrease[ChestEquipID];
                                MainHero.InventoryAdditionalAttackSpeed = MainAttackSpeed;
                            }

                            if (ItemClassification[ChestEquipID] == ItemClassifcationType.CRITICALDAMAGEINCREASE)
                            {
                                MainCriticalDamageIncrease = ItemPercentageIncrease[ChestEquipID];
                                MainHero.InventoryCriticalDamageIncrease = MainCriticalDamageIncrease;
                            }

                            if (ItemClassification[ChestEquipID] == ItemClassifcationType.BOSSGOLDDROP)
                            {
                                MainBossGoldDrop = ItemPercentageIncrease[ChestEquipID];
                                MainEnemy.InventoryBossGoldDrop = MainBossGoldDrop;
                            }
                            if (ItemClassification[ChestEquipID] == ItemClassifcationType.VILLAGEINCOMEINCREASE)
                            {
                                MainVillageIncomeIncrease = ItemPercentageIncrease[ChestEquipID];
                                MainVillage.InventoryVillageIncomeIncrease = MainVillageIncomeIncrease;
                            }
                            //break;
                        }

                        if (ItemEquipClassification[i] == ItemEquipType.HEAD)
                        {
                            HeadEquipDisplay.gameObject.SetActive(true);
                            HeadEquipDisplay.sprite = InventoryImageCollection[HeadEquipID];

                            HeadDefaultIcon.gameObject.SetActive(false);

                            UnEquipButtonControlCollectionList[2] = true;

                            if (ItemClassification[HeadEquipID] == ItemClassifcationType.INCREASEDPS)
                            {
                                MainIncreaseDPS = ItemPercentageIncrease[HeadEquipID];
                                MainHero.InventoryAdditionalDPS = MainIncreaseDPS;
                            }

                            if (ItemClassification[HeadEquipID] == ItemClassifcationType.ATTACKSPEED)
                            {
                                MainAttackSpeed = ItemPercentageIncrease[HeadEquipID];
                                MainHero.InventoryAdditionalAttackSpeed = MainAttackSpeed;
                            }

                            if (ItemClassification[HeadEquipID] == ItemClassifcationType.CRITICALDAMAGEINCREASE)
                            {
                                MainCriticalDamageIncrease = ItemPercentageIncrease[HeadEquipID];
                                MainHero.InventoryCriticalDamageIncrease = MainCriticalDamageIncrease;
                            }

                            if (ItemClassification[HeadEquipID] == ItemClassifcationType.BOSSGOLDDROP)
                            {
                                MainBossGoldDrop = ItemPercentageIncrease[HeadEquipID];
                                MainEnemy.InventoryBossGoldDrop = MainBossGoldDrop;
                            }
                            if (ItemClassification[HeadEquipID] == ItemClassifcationType.VILLAGEINCOMEINCREASE)
                            {
                                MainVillageIncomeIncrease = ItemPercentageIncrease[HeadEquipID];
                                MainVillage.InventoryVillageIncomeIncrease = MainVillageIncomeIncrease;
                            }

                            //break;
                        }

                        if (ItemEquipClassification[i] == ItemEquipType.ACCESSORY)
                        {
                            AccessoryEquipDisplay.gameObject.SetActive(true);
                            AccessoryEquipDisplay.sprite = InventoryImageCollection[AccessoryEquipID];

                            AccessoryDefaultIcon.gameObject.SetActive(false);

                            UnEquipButtonControlCollectionList[3] = true;

                            if (ItemClassification[AccessoryEquipID] == ItemClassifcationType.INCREASEDPS)
                            {
                                MainIncreaseDPS = ItemPercentageIncrease[AccessoryEquipID];
                                MainHero.InventoryAdditionalDPS = MainIncreaseDPS;
                            }

                            if (ItemClassification[AccessoryEquipID] == ItemClassifcationType.ATTACKSPEED)
                            {
                                MainAttackSpeed = ItemPercentageIncrease[AccessoryEquipID];
                                MainHero.InventoryAdditionalAttackSpeed = MainAttackSpeed;
                            }

                            if (ItemClassification[AccessoryEquipID] == ItemClassifcationType.CRITICALDAMAGEINCREASE)
                            {
                                MainCriticalDamageIncrease = ItemPercentageIncrease[AccessoryEquipID];
                                MainHero.InventoryCriticalDamageIncrease = MainCriticalDamageIncrease;
                            }

                            if (ItemClassification[AccessoryEquipID] == ItemClassifcationType.BOSSGOLDDROP)
                            {
                                MainBossGoldDrop = ItemPercentageIncrease[AccessoryEquipID];
                                MainEnemy.InventoryBossGoldDrop = MainBossGoldDrop;
                            }
                            if (ItemClassification[AccessoryEquipID] == ItemClassifcationType.VILLAGEINCOMEINCREASE)
                            {
                                MainVillageIncomeIncrease = ItemPercentageIncrease[AccessoryEquipID];
                                MainVillage.InventoryVillageIncomeIncrease = MainVillageIncomeIncrease;
                            }
                            //break;
                        }

                        if (ItemEquipClassification[i] == ItemEquipType.LEGS)
                        {
                            LegsEquipDisplay.gameObject.SetActive(true);
                            LegsEquipDisplay.sprite = InventoryImageCollection[LegsEquipID];

                            LegsDefaultIcon.gameObject.SetActive(false);

                            UnEquipButtonControlCollectionList[4] = true;

                            if (ItemClassification[LegsEquipID] == ItemClassifcationType.INCREASEDPS)
                            {
                                MainIncreaseDPS = ItemPercentageIncrease[LegsEquipID];
                                MainHero.InventoryAdditionalDPS = MainIncreaseDPS;
                            }

                            if (ItemClassification[LegsEquipID] == ItemClassifcationType.ATTACKSPEED)
                            {
                                MainAttackSpeed = ItemPercentageIncrease[LegsEquipID];
                                MainHero.InventoryAdditionalAttackSpeed = MainAttackSpeed;
                            }

                            if (ItemClassification[LegsEquipID] == ItemClassifcationType.CRITICALDAMAGEINCREASE)
                            {
                                MainCriticalDamageIncrease = ItemPercentageIncrease[LegsEquipID];
                                MainHero.InventoryCriticalDamageIncrease = MainCriticalDamageIncrease;
                            }

                            if (ItemClassification[LegsEquipID] == ItemClassifcationType.BOSSGOLDDROP)
                            {
                                MainBossGoldDrop = ItemPercentageIncrease[LegsEquipID];
                                MainEnemy.InventoryBossGoldDrop = MainBossGoldDrop;
                            }
                            if (ItemClassification[LegsEquipID] == ItemClassifcationType.VILLAGEINCOMEINCREASE)
                            {
                                MainVillageIncomeIncrease = ItemPercentageIncrease[LegsEquipID];
                                MainVillage.InventoryVillageIncomeIncrease = MainVillageIncomeIncrease;
                            }
                            //break;
                        }

                        if (ItemEquipClassification[i] == ItemEquipType.SHOES)
                        {
                            ShoesEquipDisplay.gameObject.SetActive(true);
                            ShoesEquipDisplay.sprite = InventoryImageCollection[ShoesEquipID];

                            ShoesDefaultIcon.gameObject.SetActive(false);

                            UnEquipButtonControlCollectionList[5] = true;

                            if (ItemClassification[ShoesEquipID] == ItemClassifcationType.INCREASEDPS)
                            {
                                MainIncreaseDPS = ItemPercentageIncrease[ShoesEquipID];
                                MainHero.InventoryAdditionalDPS = MainIncreaseDPS;
                            }

                            if (ItemClassification[ShoesEquipID] == ItemClassifcationType.ATTACKSPEED)
                            {
                                MainAttackSpeed = ItemPercentageIncrease[ShoesEquipID];
                                MainHero.InventoryAdditionalAttackSpeed = MainAttackSpeed;
                            }

                            if (ItemClassification[ShoesEquipID] == ItemClassifcationType.CRITICALDAMAGEINCREASE)
                            {
                                MainCriticalDamageIncrease = ItemPercentageIncrease[ShoesEquipID];
                                MainHero.InventoryCriticalDamageIncrease = MainCriticalDamageIncrease;
                            }

                            if (ItemClassification[ShoesEquipID] == ItemClassifcationType.BOSSGOLDDROP)
                            {
                                MainBossGoldDrop = ItemPercentageIncrease[ShoesEquipID];
                                MainEnemy.InventoryBossGoldDrop = MainBossGoldDrop;
                            }
                            if (ItemClassification[ShoesEquipID] == ItemClassifcationType.VILLAGEINCOMEINCREASE)
                            {
                                MainVillageIncomeIncrease = ItemPercentageIncrease[ShoesEquipID];
                                MainVillage.InventoryVillageIncomeIncrease = MainVillageIncomeIncrease;
                            }
                            //break;
                        }
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void DisplayItemDetails(int ItemID)
        {
            ItemWindowPopUpObj.gameObject.SetActive(true);
            if (ItemClassification[ItemID] == ItemClassifcationType.INCREASEDPS)
            {
                TempStringID = "INCREASE DPS: ";
            }

            if (ItemClassification[ItemID] == ItemClassifcationType.BOSSGOLDDROP)
            {
                TempStringID = "BOSS GOLD DROP: ";
            }

            if (ItemClassification[ItemID] == ItemClassifcationType.ATTACKSPEED)
            {
                TempStringID = "ATTACK SPEED: ";
            }

            if (ItemClassification[ItemID] == ItemClassifcationType.CRITICALDAMAGEINCREASE)
            {
                TempStringID = "CRITICAL DAMAGE INCREASE: ";
            }

            if (ItemClassification[ItemID] == ItemClassifcationType.SPELLCOOLDOWN)
            {
                TempStringID = "SPELL COOLDOWN: ";
            }

            if (ItemClassification[ItemID] == ItemClassifcationType.VILLAGEINCOMEINCREASE)
            {
                TempStringID = "VILLAGE INCOME INCREASE: ";
            }


            ItemNameText.text = InventoryNameCollection[ItemID] + " +" + InventoryLevelCollectionList[ItemID];

            ItemDisplayImage.sprite = InventoryImageCollection[ItemID];
            ItemDescriptionText.text = InventoryDescriptionCollection[ItemID];
            ItemStatText.text = TempStringID + ItemPercentageIncrease[ItemID] + "%";

            ItemFragmentCountText.text = InventoryFragmentCollectionList[ItemID] + "/" + InventoryMaxFragmentLevelCollection[InventoryLevelCollectionList[ItemID]];

            if (InventoryFragmentCollectionList[ItemID] >= InventoryMaxFragmentLevelCollection[InventoryLevelCollectionList[ItemID]])
            {
                UpgradeButton.interactable = true;
            }

            if (InventoryFragmentCollectionList[ItemID] < InventoryMaxFragmentLevelCollection[InventoryLevelCollectionList[ItemID]])
            {
                UpgradeButton.interactable = false;
            }

            ItemGeneralID = ItemID;
        }

        public void UpgradeActivate()
        {
            InventoryFragmentCollectionList[ItemGeneralID] -= InventoryMaxFragmentLevelCollection[InventoryLevelCollectionList[ItemGeneralID]];

            ItemPercentageIncrease[ItemGeneralID] += ItemPercentageAdditionalIncrease[ItemGeneralID];

            ItemFragmentCountText.text = InventoryFragmentCollectionList[ItemGeneralID] + "/" + InventoryMaxFragmentLevelCollection[InventoryLevelCollectionList[ItemGeneralID]];


            if (ItemClassification[ItemGeneralID] == ItemClassifcationType.INCREASEDPS)
            {
                MainIncreaseDPS = ItemPercentageIncrease[ItemGeneralID];
                MainHero.InventoryAdditionalDPS = MainIncreaseDPS;
            }

            if (ItemClassification[ItemGeneralID] == ItemClassifcationType.ATTACKSPEED)
            {
                MainAttackSpeed = ItemPercentageIncrease[ItemGeneralID];
                MainHero.InventoryAdditionalAttackSpeed = MainAttackSpeed;
            }

            if (ItemClassification[ItemGeneralID] == ItemClassifcationType.CRITICALDAMAGEINCREASE)
            {
                MainCriticalDamageIncrease = ItemPercentageIncrease[ItemGeneralID];
                MainHero.InventoryCriticalDamageIncrease = MainCriticalDamageIncrease;
            }

            if (ItemClassification[ItemGeneralID] == ItemClassifcationType.BOSSGOLDDROP)
            {
                MainBossGoldDrop = ItemPercentageIncrease[ItemGeneralID];
                MainEnemy.InventoryBossGoldDrop = MainBossGoldDrop;
            }
            if (ItemClassification[ItemGeneralID] == ItemClassifcationType.VILLAGEINCOMEINCREASE)
            {
                MainVillageIncomeIncrease = ItemPercentageIncrease[ItemGeneralID];
                MainVillage.InventoryVillageIncomeIncrease = MainVillageIncomeIncrease;
            }
        }

        public void EquipActivate()
        {
            ItemWindowPopUpObj.gameObject.SetActive(false);
            ItemEquipList[ItemGeneralID] = true;

            if (InventoryActiveCollectionList[ItemGeneralID] == true)
            {
                int TempID = 0;
                if (ItemEquipClassification[ItemGeneralID] == ItemEquipType.WEAPON)
                {
                    if (UnEquipButtonControlCollectionList[0] == true)
                    {
                        TempID = WeaponEquipID;

                        ItemEquipList[TempID] = false;
                        InventoryItemObjCollection[TempID].gameObject.SetActive(true);
                    }
                }

                if (ItemEquipClassification[ItemGeneralID] == ItemEquipType.CHEST)
                {
                    if (UnEquipButtonControlCollectionList[1] == true)
                    {
                        TempID = ChestEquipID;

                        ItemEquipList[TempID] = false;
                        InventoryItemObjCollection[TempID].gameObject.SetActive(true);
                    }
                }

                if (ItemEquipClassification[ItemGeneralID] == ItemEquipType.HEAD)
                {
                    if (UnEquipButtonControlCollectionList[2] == true)
                    {
                        TempID = HeadEquipID;

                        ItemEquipList[TempID] = false;
                        InventoryItemObjCollection[TempID].gameObject.SetActive(true);
                    }
                }

                if (ItemEquipClassification[ItemGeneralID] == ItemEquipType.ACCESSORY)
                {
                    if (UnEquipButtonControlCollectionList[3] == true)
                    {
                        TempID = AccessoryEquipID;

                        ItemEquipList[TempID] = false;
                        InventoryItemObjCollection[TempID].gameObject.SetActive(true);
                    }
                }

                if (ItemEquipClassification[ItemGeneralID] == ItemEquipType.LEGS)
                {
                    if (UnEquipButtonControlCollectionList[4] == true)
                    {
                        TempID = LegsEquipID;

                        ItemEquipList[TempID] = false;
                        InventoryItemObjCollection[TempID].gameObject.SetActive(true);
                    }
                }

                if (ItemEquipClassification[ItemGeneralID] == ItemEquipType.SHOES)
                {
                    if (UnEquipButtonControlCollectionList[5] == true)
                    {
                        TempID = ShoesEquipID;

                        ItemEquipList[TempID] = false;
                        InventoryItemObjCollection[TempID].gameObject.SetActive(true);
                    }
                }

                if (ItemClassification[ItemGeneralID] == ItemClassifcationType.INCREASEDPS)
                {
                    MainIncreaseDPS = ItemPercentageIncrease[ItemGeneralID];
                    MainHero.InventoryAdditionalDPS = MainIncreaseDPS;
                }

                if (ItemClassification[ItemGeneralID] == ItemClassifcationType.ATTACKSPEED)
                {
                    MainAttackSpeed = ItemPercentageIncrease[ItemGeneralID];
                    MainHero.InventoryAdditionalAttackSpeed = MainAttackSpeed;
                }

                if (ItemClassification[ItemGeneralID] == ItemClassifcationType.CRITICALDAMAGEINCREASE)
                {
                    MainCriticalDamageIncrease = ItemPercentageIncrease[ItemGeneralID];
                    MainHero.InventoryCriticalDamageIncrease = MainCriticalDamageIncrease;
                }

                if (ItemClassification[ItemGeneralID] == ItemClassifcationType.BOSSGOLDDROP)
                {
                    MainBossGoldDrop = ItemPercentageIncrease[ItemGeneralID];
                    MainEnemy.InventoryBossGoldDrop = MainBossGoldDrop;
                }
                if (ItemClassification[ItemGeneralID] == ItemClassifcationType.VILLAGEINCOMEINCREASE)
                {
                    MainVillageIncomeIncrease = ItemPercentageIncrease[ItemGeneralID];
                    MainVillage.InventoryVillageIncomeIncrease = MainVillageIncomeIncrease;
                }

            }

            CheckEquipItem();
        }

        void CheckEquipItem()
        {
            //for (int i = 0; i < ItemEquipList.Count; i++)
            //{
                //if (InventoryActiveCollectionList[i] == true)
                {
                    if (ItemEquipList[ItemGeneralID] == true)
                    {
                        if (ItemEquipClassification[ItemGeneralID] == ItemEquipType.WEAPON)
                        {
                            WeaponEquipDisplay.gameObject.SetActive(true);
                            WeaponEquipDisplay.sprite = InventoryImageCollection[ItemGeneralID];

                            WeaponDefaultIcon.gameObject.SetActive(false);

                            UnEquipButtonControlCollectionList[0] = true;

                            WeaponEquipID = ItemGeneralID;
                            //break;
                        }

                        if (ItemEquipClassification[ItemGeneralID] == ItemEquipType.CHEST)
                        {
                            ChestEquipDisplay.gameObject.SetActive(true);
                            ChestEquipDisplay.sprite = InventoryImageCollection[ItemGeneralID];

                            ChestDefaultIcon.gameObject.SetActive(false);

                            UnEquipButtonControlCollectionList[1] = true;

                            ChestEquipID = ItemGeneralID;
                            //break;
                        }

                        if (ItemEquipClassification[ItemGeneralID] == ItemEquipType.HEAD)
                        {
                            HeadEquipDisplay.gameObject.SetActive(true);
                            HeadEquipDisplay.sprite = InventoryImageCollection[ItemGeneralID];

                            HeadDefaultIcon.gameObject.SetActive(false);

                            UnEquipButtonControlCollectionList[2] = true;

                            HeadEquipID = ItemGeneralID;

                           
                            //break;
                        }

                        if (ItemEquipClassification[ItemGeneralID] == ItemEquipType.ACCESSORY)
                        {
                            AccessoryEquipDisplay.gameObject.SetActive(true);
                            AccessoryEquipDisplay.sprite = InventoryImageCollection[ItemGeneralID];

                            AccessoryDefaultIcon.gameObject.SetActive(false);

                            UnEquipButtonControlCollectionList[3] = true;

                            AccessoryEquipID = ItemGeneralID;
                            //break;
                        }

                        if (ItemEquipClassification[ItemGeneralID] == ItemEquipType.LEGS)
                        {
                            LegsEquipDisplay.gameObject.SetActive(true);
                            LegsEquipDisplay.sprite = InventoryImageCollection[ItemGeneralID];

                            LegsDefaultIcon.gameObject.SetActive(false);

                            UnEquipButtonControlCollectionList[4] = true;

                            LegsEquipID = ItemGeneralID;
                            //break;
                        }

                        if (ItemEquipClassification[ItemGeneralID] == ItemEquipType.SHOES)
                        {
                            ShoesEquipDisplay.gameObject.SetActive(true);
                            ShoesEquipDisplay.sprite = InventoryImageCollection[ItemGeneralID];

                            ShoesDefaultIcon.gameObject.SetActive(false);

                            UnEquipButtonControlCollectionList[5] = true;

                            ShoesEquipID = ItemGeneralID;
                            //break;
                        }

                        InventoryItemObjCollection[ItemGeneralID].gameObject.SetActive(false);
                    }
                }
            //}

            FirstCheck();
        }

        public void CheckUnEquipItem(int UnEquipSlotID)
        {
            UnEquipItemWindowPopUpObj.gameObject.SetActive(true);
            int TempUnEquipItemTypeID = 0;
            if (UnEquipItemSlotClassification[UnEquipSlotID] == ItemEquipType.WEAPON)
            {
                TempUnEquipItemTypeID = WeaponEquipID;
                //Debug.Log(TempUnEquipItemTypeID);
            }

            if (UnEquipItemSlotClassification[UnEquipSlotID] == ItemEquipType.CHEST)
            {
                TempUnEquipItemTypeID = ChestEquipID;
                
            }

            if (UnEquipItemSlotClassification[UnEquipSlotID] == ItemEquipType.HEAD)
            {
                TempUnEquipItemTypeID = HeadEquipID;
                //Debug.Log(TempUnEquipItemTypeID);
            }

            if (UnEquipItemSlotClassification[UnEquipSlotID] == ItemEquipType.ACCESSORY)
            {
                TempUnEquipItemTypeID = AccessoryEquipID;
            }

            if (UnEquipItemSlotClassification[UnEquipSlotID] == ItemEquipType.LEGS)
            {
                TempUnEquipItemTypeID = LegsEquipID;
            }

            if (UnEquipItemSlotClassification[UnEquipSlotID] == ItemEquipType.SHOES)
            {

                TempUnEquipItemTypeID = ShoesEquipID;
            }
            if (ItemClassification[TempUnEquipItemTypeID] == ItemClassifcationType.INCREASEDPS)
            {
                TempStringID = "INCREASE DPS: ";
            }

            if (ItemClassification[TempUnEquipItemTypeID] == ItemClassifcationType.BOSSGOLDDROP)
            {
                TempStringID = "BOSS GOLD DROP: ";
            }

            if (ItemClassification[TempUnEquipItemTypeID] == ItemClassifcationType.ATTACKSPEED)
            {
                TempStringID = "ATTACK SPEED: ";
            }

            if (ItemClassification[TempUnEquipItemTypeID] == ItemClassifcationType.CRITICALDAMAGEINCREASE)
            {
                TempStringID = "CRITICAL DAMAGE INCREASE: ";
            }

            if (ItemClassification[TempUnEquipItemTypeID] == ItemClassifcationType.SPELLCOOLDOWN)
            {
                TempStringID = "SPELL COOLDOWN: ";
            }

            if (ItemClassification[TempUnEquipItemTypeID] == ItemClassifcationType.VILLAGEINCOMEINCREASE)
            {
                TempStringID = "VILLAGE INCOME INCREASE: ";
            }


            UnEquipItemNameText.text = InventoryNameCollection[TempUnEquipItemTypeID] + " +" + InventoryLevelCollectionList[TempUnEquipItemTypeID];

            UnEquipItemDisplayImage.sprite = InventoryImageCollection[TempUnEquipItemTypeID];
            UnEquipItemDescriptionText.text = InventoryDescriptionCollection[TempUnEquipItemTypeID];
            UnEquipItemStatText.text = TempStringID + ItemPercentageIncrease[TempUnEquipItemTypeID] + "%";

            UnEquipItemFragmentCountText.text = InventoryFragmentCollectionList[TempUnEquipItemTypeID] + "/" + InventoryMaxFragmentLevelCollection[InventoryLevelCollectionList[TempUnEquipItemTypeID]];

            if (InventoryFragmentCollectionList[TempUnEquipItemTypeID] >= InventoryMaxFragmentLevelCollection[InventoryLevelCollectionList[TempUnEquipItemTypeID]])
            {
                UnEquipUpgradeButton.interactable = true;
            }

            if (InventoryFragmentCollectionList[TempUnEquipItemTypeID] < InventoryMaxFragmentLevelCollection[InventoryLevelCollectionList[TempUnEquipItemTypeID]])
            {
                UnEquipUpgradeButton.interactable = false;
            }

            UnEquipItemSlotID = UnEquipSlotID;
            UnEquipItemTypeID = TempUnEquipItemTypeID;
        }

        public void UnEquipActivate()
        {
            ItemEquipList[UnEquipItemTypeID] = false;
            UnEquipItemWindowPopUpObj.gameObject.SetActive(false);

            if (InventoryActiveCollectionList[UnEquipItemTypeID] == true)
            {
                if (ItemClassification[ItemGeneralID] == ItemClassifcationType.INCREASEDPS)
                {
                    MainIncreaseDPS = 0;
                    MainHero.InventoryAdditionalDPS = MainIncreaseDPS;
                }

                if (ItemClassification[ItemGeneralID] == ItemClassifcationType.ATTACKSPEED)
                {
                    MainAttackSpeed = 0;
                    MainHero.InventoryAdditionalAttackSpeed = MainAttackSpeed;
                }

                if (ItemClassification[ItemGeneralID] == ItemClassifcationType.CRITICALDAMAGEINCREASE)
                {
                    MainCriticalDamageIncrease = 0;
                    MainHero.InventoryCriticalDamageIncrease = MainCriticalDamageIncrease;
                }

                if (ItemClassification[ItemGeneralID] == ItemClassifcationType.BOSSGOLDDROP)
                {
                    MainBossGoldDrop = 0;
                    MainEnemy.InventoryBossGoldDrop = MainBossGoldDrop;
                }
                if (ItemClassification[ItemGeneralID] == ItemClassifcationType.VILLAGEINCOMEINCREASE)
                {
                    MainVillageIncomeIncrease = 0;
                    MainVillage.InventoryVillageIncomeIncrease = MainVillageIncomeIncrease;
                }

                InventoryItemObjCollection[UnEquipItemTypeID].gameObject.SetActive(true);
                
            }
            CheckUnEquipItem();
        }
        public void UnEquipUpgradeActivate()
        {
            InventoryFragmentCollectionList[UnEquipItemTypeID] -= InventoryMaxFragmentLevelCollection[InventoryLevelCollectionList[UnEquipItemTypeID]];

            ItemPercentageIncrease[UnEquipItemTypeID] += ItemPercentageAdditionalIncrease[UnEquipItemTypeID];

            UnEquipItemFragmentCountText.text = InventoryFragmentCollectionList[UnEquipItemTypeID] + "/" + InventoryMaxFragmentLevelCollection[InventoryLevelCollectionList[UnEquipItemTypeID]];

            if (ItemClassification[UnEquipItemTypeID] == ItemClassifcationType.INCREASEDPS)
            {
                MainIncreaseDPS = ItemPercentageIncrease[UnEquipItemTypeID];
                MainHero.InventoryAdditionalDPS = MainIncreaseDPS;
            }

            if (ItemClassification[UnEquipItemTypeID] == ItemClassifcationType.ATTACKSPEED)
            {
                MainAttackSpeed = ItemPercentageIncrease[UnEquipItemTypeID];
                MainHero.InventoryAdditionalAttackSpeed = MainAttackSpeed;
            }

            if (ItemClassification[UnEquipItemTypeID] == ItemClassifcationType.CRITICALDAMAGEINCREASE)
            {
                MainCriticalDamageIncrease = ItemPercentageIncrease[UnEquipItemTypeID];
                MainHero.InventoryCriticalDamageIncrease = MainCriticalDamageIncrease;
            }

            if (ItemClassification[UnEquipItemTypeID] == ItemClassifcationType.BOSSGOLDDROP)
            {
                MainBossGoldDrop = ItemPercentageIncrease[UnEquipItemTypeID];
                MainEnemy.InventoryBossGoldDrop = MainBossGoldDrop;
            }
            if (ItemClassification[UnEquipItemTypeID] == ItemClassifcationType.VILLAGEINCOMEINCREASE)
            {
                MainVillageIncomeIncrease = ItemPercentageIncrease[UnEquipItemTypeID];
                MainVillage.InventoryVillageIncomeIncrease = MainVillageIncomeIncrease;
            }
        }

        void CheckUnEquipItem()
        {
            //for (int i = 0; i < ItemEquipList.Count; i++)
            //{
                //if (InventoryActiveCollectionList[UnEquipItemSlotID] == true)
                //{
                    if (ItemEquipList[UnEquipItemTypeID] == false)
                    {
                        if (UnEquipItemSlotClassification[UnEquipItemSlotID] == ItemEquipType.WEAPON)
                        {
                            if (UnEquipButtonControlCollectionList[UnEquipItemSlotID] == true)
                            {
                                WeaponEquipDisplay.gameObject.SetActive(false);
                                WeaponEquipDisplay.sprite = null;

                                WeaponDefaultIcon.gameObject.SetActive(true);

                                UnEquipButtonControlCollectionList[UnEquipItemSlotID] = false;

                                WeaponEquipID = 0;
                            }
                        }

                        if (UnEquipItemSlotClassification[UnEquipItemSlotID] == ItemEquipType.CHEST)
                        {
                            if (UnEquipButtonControlCollectionList[UnEquipItemSlotID] == true)
                            {
                                ChestEquipDisplay.gameObject.SetActive(false);
                                ChestEquipDisplay.sprite = null;

                                ChestDefaultIcon.gameObject.SetActive(true);

                                UnEquipButtonControlCollectionList[UnEquipItemSlotID] = false;

                                ChestEquipID = 0;
                            }
                        }

                        if (UnEquipItemSlotClassification[UnEquipItemSlotID] == ItemEquipType.HEAD)
                        {
                           
                            if (UnEquipButtonControlCollectionList[UnEquipItemSlotID] == true)
                            {
                                HeadEquipDisplay.gameObject.SetActive(false);
                                HeadEquipDisplay.sprite = null;

                                HeadDefaultIcon.gameObject.SetActive(true);

                                UnEquipButtonControlCollectionList[UnEquipItemSlotID] = false;

                                HeadEquipID = 0;
                                

                                //break;
                            }
                        }

                        if (UnEquipItemSlotClassification[UnEquipItemSlotID] == ItemEquipType.ACCESSORY)
                        {
                            if (UnEquipButtonControlCollectionList[UnEquipItemSlotID] == true)
                            {
                                AccessoryEquipDisplay.gameObject.SetActive(false);
                                AccessoryEquipDisplay.sprite = null;

                                AccessoryDefaultIcon.gameObject.SetActive(true);

                                UnEquipButtonControlCollectionList[UnEquipItemSlotID] = false;

                                AccessoryEquipID = 0;
                                //break;
                            }
                        }

                        if (UnEquipItemSlotClassification[UnEquipItemSlotID] == ItemEquipType.LEGS)
                        {
                            if (UnEquipButtonControlCollectionList[UnEquipItemSlotID] == true)
                            {
                                LegsEquipDisplay.gameObject.SetActive(false);
                                LegsEquipDisplay.sprite = null;

                                LegsDefaultIcon.gameObject.SetActive(true);

                                UnEquipButtonControlCollectionList[UnEquipItemSlotID] = false;

                                LegsEquipID = 0;
                                //break;
                            }

                        }

                        if (UnEquipItemSlotClassification[UnEquipItemSlotID] == ItemEquipType.SHOES)
                        {
                            if (UnEquipButtonControlCollectionList[UnEquipItemSlotID] == true)
                            {
                                ShoesEquipDisplay.gameObject.SetActive(false);
                                ShoesEquipDisplay.sprite = null;

                                ShoesDefaultIcon.gameObject.SetActive(true);

                                UnEquipButtonControlCollectionList[UnEquipItemSlotID] = false;

                                ShoesEquipID = 0;
                                //break;
                            }

                        }

                        InventoryItemObjCollection[UnEquipItemTypeID].gameObject.SetActive(true);
                    }
                //}
            //}

            FirstCheck();
        }
    }
}

