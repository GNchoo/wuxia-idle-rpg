using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAMPLETEXT.Wallet.Manager;
using UnityEngine.UI;
using TMPro;
using SAMPLETEXT.SubHeroUI.Manager;
using SAMPLETEXT.Artifact.Manager;
using SAMPLETEXT.ItemPurchase.Manager.Boost;
using SAMPLETEXT.Market.Manager;
using SAMPLETEXT.Achievement.Manager;
using System;
using SAMPLETEXT.ItemPurchase.Manager.Item;

namespace SAMPLETEXT.ItemPurchase.Manager.DailyBundle
{
    public class DailyBundlePurchaseManagerScript : MonoBehaviour
    {
        [SerializeField]
        WalletManagerScript MainPlayerWallet;
        [SerializeField]
        MarketManagerScript MainVillageMarket;

        [Header("Daily Bundle Settings")]
        [SerializeField]
        Button DailyBundlePurchaseButton;
        [SerializeField]
        float DailyBundlePurchaseCostValue;
        [SerializeField]
        TextMeshProUGUI DailyBundlePurchaseCostValueText;
        [SerializeField]
        int BundleCountID;

        [Header("Hero Selection Settings")]
        [SerializeField]
        ItemsPurchaseHeroManagerScript SubHeroItemManager;
        [SerializeField]
        SubHeroesManagerScript MainSubHero;

        [Header("Daily Bundle Collection Settings")]
        [SerializeField]
        List<BundleCollectionType> FirstBundleCollectionList = new List<BundleCollectionType>();
        [SerializeField]
        List<BundleCollectionType> SecondBundleCollectionList = new List<BundleCollectionType>();
        [SerializeField]
        List<BundleCollectionType> ThirdBundleCollectionList = new List<BundleCollectionType>();
        [SerializeField]
        List<int> BundleCollectionPurchaseCost = new List<int>();
        public enum BundleCollectionType
        {
            Rare_Hero,
            Epic_Hero,
            Legendary_Hero,
            Artifact_Chest,
            Boost_Chest,
            Normal_Chest,
            Legendary_Chest
        };
        [SerializeField]
        TextMeshProUGUI FirstBundleCollectionText;
        [SerializeField]
        TextMeshProUGUI SecondBundleCollectionText;
        [SerializeField]
        TextMeshProUGUI ThirdBundleCollectionText;
        [SerializeField]
        Image FirstBundleCollectionImage;
        [SerializeField]
        Image SecondBundleCollectionImage;
        [SerializeField]
        Image ThirdBundleCollectionImage;
        [SerializeField]
        int BundleID;
        int SetCount;
        bool LegendaryChestCheck;
        public int FirstBundleID;
        public int SecondBundleID;
        public int ThirdBundleID;
        [SerializeField]
        Sprite NormalChestImage;
        [SerializeField]
        Sprite LegendaryChestImage;
        public bool FirstBundleFix;
        public bool SecondBundleFix;
        public bool ThirdBundleFix;

        [Header("Artifact Selection Settings")]
        [SerializeField]
        ArtifactManagerScript MainArtifact;
        [SerializeField]
        ItemsPurchaseArtifactManagerScript MainPurchaseArtifact;

        [Header("Boost Selection Settings")]
        [SerializeField]
        ItemsPurchaseBoostManagerScript MainPurchaseBoost;

        [Header("Purchase Pop-Up Window")]
        [SerializeField]
        GameObject PopUpWindowObj;
        [SerializeField]
        Image PopUpWindowHeroImage;
        [SerializeField]
        TextMeshProUGUI PopUpWindowHeroNameText;
        int CollectControl;
        [SerializeField]
        Animator SubHeroCurrentAnimator;
        [SerializeField]
        Image PopUpCircleBGImage;
        [SerializeField]
        TextMeshProUGUI ReceiveItemText;

		[Header("Achievement Settings")]
		[SerializeField]
		AchievementManagerScript MainAchievement;

        private const string LastPurchaseTimeKey = "LastPurchaseTime";

        private const string FirstBundleFixKey = "FirstBundleFix";
        private const string SecondBundleFixKey = "SecondBundleFix";
        private const string ThirdBundleFixKey = "ThirdBundleFix";


        public int rareHeroSaveBundle;
        public int epicHeroSaveBundle;
        public int legendaryHeroSaveBundle;
        public int artifactSaveBundle;
        public int chestSaveBundle;



        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(DelayLoad());


            CheckButtonState();
            ResetBundleFixesIfNecessary();

            FirstBundleFix = PlayerPrefs.GetInt("FirstBundleFix") == 1 ? true : false;
            SecondBundleFix = PlayerPrefs.GetInt("SecondBundleFix") == 1 ? true : false;
            ThirdBundleFix = PlayerPrefs.GetInt("ThirdBundleFix") == 1 ? true : false;
        }

        IEnumerator DelayLoad()
        {
            yield return new WaitForSeconds(.3f);
            FirstCheck();
        }

        void FirstCheck()
        {
            DailyBundlePurchaseCostValueText.text = BundleCollectionPurchaseCost[BundleID].ToString();
            DailyBundleButtonCheckManualUpdate();

            BundleCheckManualUpdate();
            
        }

        public void SaveBundleFixes()
        {
            PlayerPrefs.SetInt(FirstBundleFixKey, FirstBundleFix ? 1 : 0);
            PlayerPrefs.SetInt(SecondBundleFixKey, SecondBundleFix ? 1 : 0);
            PlayerPrefs.SetInt(ThirdBundleFixKey, ThirdBundleFix ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void ResetBundleFixesIfNecessary()
        {
            if (PlayerPrefs.HasKey(LastPurchaseTimeKey))
            {
                string lastPurchaseTimeStr = PlayerPrefs.GetString(LastPurchaseTimeKey);
                DateTime nextInteractableTime = DateTime.Parse(lastPurchaseTimeStr);
                DateTime currentDateTime = DateTime.Now;

                if (currentDateTime >= nextInteractableTime)
                {
                    // Resetting the boolean flags
                    SaveBundleFixes();
                    PlayerPrefs.DeleteKey(LastPurchaseTimeKey); // Optional: Clear the time key if needed
                    PlayerPrefs.Save();
                }
            }
        }





        void BundleCheckManualUpdate()
        {
            if (FirstBundleFix == false)
            {
                if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Rare_Hero)
                {
                    FirstBundleCollectionText.text = "RARE HERO";
                    int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroRareID.Count);
                    FirstBundleID = a; // a;
                    FirstBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroRareID[a]];

                }

                if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Epic_Hero)
                {
                    FirstBundleCollectionText.text = "EPIC HERO";
                    int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroEpicID.Count);
                    FirstBundleID = a; // a;
                    FirstBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroEpicID[a]];
                }

                if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Hero)
                {
                    FirstBundleCollectionText.text = "LEGENDARY HERO";
                    int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroLegendaryID.Count);
                    FirstBundleID = a;// a;
                    FirstBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroLegendaryID[a]];
                }

                if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Artifact_Chest)
                {
                    FirstBundleCollectionText.text = "ARTIFACT";
                    int ArtifactCountID = UnityEngine.Random.Range(0, 9);
                    FirstBundleID = ArtifactCountID; // ArtifactCountID;
                    FirstBundleCollectionImage.sprite = MainPurchaseArtifact.ArtifactCollectionImage[ArtifactCountID];
                }

                if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Boost_Chest)
                {
                    FirstBundleCollectionText.text = "BOOST CHEST";
                    int TempBoost = UnityEngine.Random.Range(0, 4);
                    FirstBundleID = TempBoost; //tempBoost
                    FirstBundleCollectionImage.sprite = MainPurchaseBoost.BoostImageCollection[TempBoost];
                }

                if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Normal_Chest)
                {
                    FirstBundleCollectionText.text = "NORMAL CHEST";
                    FirstBundleCollectionImage.sprite = NormalChestImage;
                }

                if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Chest)
                {
                    FirstBundleCollectionText.text = "LEGENDARY CHEST";
                    FirstBundleCollectionImage.sprite = LegendaryChestImage;
                }

                PlayerPrefs.SetInt("SavedFirstBundle", FirstBundleID);
                PlayerPrefs.Save();
               // FirstBundleFix = true;
            }
            else
            {
                if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Rare_Hero)
                {
                    FirstBundleCollectionText.text = "RARE HERO";
                    //int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroRareID.Count);
                    FirstBundleID = PlayerPrefs.GetInt("SavedFirstBundle");
                    FirstBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroRareID[FirstBundleID]];

                }

                if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Epic_Hero)
                {
                    FirstBundleCollectionText.text = "EPIC HERO";
                    //int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroEpicID.Count);
                    FirstBundleID = PlayerPrefs.GetInt("SavedFirstBundle");
                    FirstBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroEpicID[FirstBundleID]];
                }

                if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Hero)
                {
                    FirstBundleCollectionText.text = "LEGENDARY HERO";
                    //int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroLegendaryID.Count);
                    FirstBundleID = PlayerPrefs.GetInt("SavedFirstBundle");
                    FirstBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroLegendaryID[FirstBundleID]];
                }

                if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Artifact_Chest)
                {
                    FirstBundleCollectionText.text = "ARTIFACT";
                    //int ArtifactCountID = UnityEngine.Random.Range(0, 9);
                    FirstBundleID = PlayerPrefs.GetInt("SavedFirstBundle");
                    FirstBundleCollectionImage.sprite = MainPurchaseArtifact.ArtifactCollectionImage[FirstBundleID];
                }

                if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Boost_Chest)
                {
                    FirstBundleCollectionText.text = "BOOST CHEST";
                    //int TempBoost = UnityEngine.Random.Range(0, 4);
                    FirstBundleID = PlayerPrefs.GetInt("SavedFirstBundle");
                    FirstBundleCollectionImage.sprite = MainPurchaseBoost.BoostImageCollection[FirstBundleID];
                }

                if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Normal_Chest)
                {
                    FirstBundleCollectionText.text = "NORMAL CHEST";
                    FirstBundleCollectionImage.sprite = NormalChestImage;
                }

                if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Chest)
                {
                    FirstBundleCollectionText.text = "LEGENDARY CHEST";
                    FirstBundleCollectionImage.sprite = LegendaryChestImage;
                }

            }

            if (SecondBundleFix == false)
            {
                //Second Bundle
                if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Rare_Hero)
                {
                    SecondBundleCollectionText.text = "RARE HERO";
                    int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroRareID.Count);
                    SecondBundleID = a;
                    SecondBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroRareID[a]];
                }

                if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Epic_Hero)
                {
                    SecondBundleCollectionText.text = "EPIC HERO";
                    int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroEpicID.Count);
                    SecondBundleID = a;
                    SecondBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroEpicID[a]];
                }

                if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Hero)
                {
                    SecondBundleCollectionText.text = "LEGENDARY HERO";
                    int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroLegendaryID.Count);
                    SecondBundleID = a;
                    SecondBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroLegendaryID[a]];
                }

                if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Artifact_Chest)
                {
                    SecondBundleCollectionText.text = "ARTIFACT";
                    int ArtifactCountID = UnityEngine.Random.Range(0, 9);
                    SecondBundleID = ArtifactCountID;
                    SecondBundleCollectionImage.sprite = MainPurchaseArtifact.ArtifactCollectionImage[ArtifactCountID];
                }

                if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Boost_Chest)
                {
                    SecondBundleCollectionText.text = "BOOST CHEST";
                    int TempBoost = UnityEngine.Random.Range(0, 4);
                    SecondBundleID = TempBoost;
                    SecondBundleCollectionImage.sprite = MainPurchaseBoost.BoostImageCollection[TempBoost];
                }

                if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Normal_Chest)
                {
                    SecondBundleCollectionText.text = "NORMAL CHEST";
                    SecondBundleCollectionImage.sprite = NormalChestImage;
                }

                if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Chest)
                {
                    SecondBundleCollectionText.text = "LEGENDARY CHEST";
                    SecondBundleCollectionImage.sprite = LegendaryChestImage;
                }

                PlayerPrefs.SetInt("SavedSecondBundle", SecondBundleID);
                PlayerPrefs.Save();
                //SecondBundleFix = true;
            }
            else
            {
                //Second Bundle
                if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Rare_Hero)
                {
                    SecondBundleCollectionText.text = "RARE HERO";
                    //int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroRareID.Count);
                    SecondBundleID = PlayerPrefs.GetInt("SavedSecondBundle");
                    SecondBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroRareID[SecondBundleID]];
                }

                if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Epic_Hero)
                {
                    SecondBundleCollectionText.text = "EPIC HERO";
                    //int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroEpicID.Count);
                    SecondBundleID = PlayerPrefs.GetInt("SavedSecondBundle");
                    SecondBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroEpicID[SecondBundleID]];
                }

                if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Hero)
                {
                    SecondBundleCollectionText.text = "LEGENDARY HERO";
                    //int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroLegendaryID.Count);
                    SecondBundleID = PlayerPrefs.GetInt("SavedSecondBundle");
                    SecondBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroLegendaryID[SecondBundleID]];
                }

                if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Artifact_Chest)
                {
                    SecondBundleCollectionText.text = "ARTIFACT";
                    //int ArtifactCountID = UnityEngine.Random.Range(0, 9);
                    SecondBundleID = PlayerPrefs.GetInt("SavedSecondBundle");
                    SecondBundleCollectionImage.sprite = MainPurchaseArtifact.ArtifactCollectionImage[SecondBundleID];
                }

                if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Boost_Chest)
                {
                    SecondBundleCollectionText.text = "BOOST CHEST";
                    //int TempBoost = UnityEngine.Random.Range(0, 4);
                    SecondBundleID = PlayerPrefs.GetInt("SavedSecondBundle");
                    SecondBundleCollectionImage.sprite = MainPurchaseBoost.BoostImageCollection[SecondBundleID];
                }

                if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Normal_Chest)
                {
                    SecondBundleCollectionText.text = "NORMAL CHEST";
                    SecondBundleCollectionImage.sprite = NormalChestImage;
                }

                if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Chest)
                {
                    SecondBundleCollectionText.text = "LEGENDARY CHEST";
                    SecondBundleCollectionImage.sprite = LegendaryChestImage;
                }
                
                
                
            }
            

            if (ThirdBundleFix == false)
            {
                //Third Bundle
                if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Rare_Hero)
                {
                    ThirdBundleCollectionText.text = "RARE HERO";
                    int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroRareID.Count);
                    ThirdBundleID = a;
                    ThirdBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroRareID[a]];
                }

                if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Epic_Hero)
                {
                    ThirdBundleCollectionText.text = "EPIC HERO";
                    int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroEpicID.Count);
                    ThirdBundleID = a;
                    ThirdBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroEpicID[a]];
                }

                if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Hero)
                {
                    ThirdBundleCollectionText.text = "LEGENDARY HERO";
                    int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroLegendaryID.Count);
                    ThirdBundleID = a;
                    ThirdBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroLegendaryID[a]];
                }

                if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Artifact_Chest)
                {
                    ThirdBundleCollectionText.text = "ARTIFACT";
                    int ArtifactCountID = UnityEngine.Random.Range(0, 9);
                    ThirdBundleID = ArtifactCountID;
                    ThirdBundleCollectionImage.sprite = MainPurchaseArtifact.ArtifactCollectionImage[ArtifactCountID];
                }

                if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Boost_Chest)
                {
                    ThirdBundleCollectionText.text = "BOOST CHEST";
                    int TempBoost = UnityEngine.Random.Range(0, 4);
                    ThirdBundleID = TempBoost;
                    ThirdBundleCollectionImage.sprite = MainPurchaseBoost.BoostImageCollection[TempBoost];
                }

                if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Normal_Chest)
                {
                    ThirdBundleCollectionText.text = "NORMAL CHEST";
                    ThirdBundleCollectionImage.sprite = NormalChestImage;
                }

                if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Chest)
                {
                    ThirdBundleCollectionText.text = "LEGENDARY CHEST";
                    ThirdBundleCollectionImage.sprite = LegendaryChestImage;
                }

                PlayerPrefs.SetInt("SavedThirdBundle", ThirdBundleID);
                PlayerPrefs.Save();
                //ThirdBundleFix = true;
            }
            else
            {
                //Third Bundle
                if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Rare_Hero)
                {
                    ThirdBundleCollectionText.text = "RARE HERO";
                    // int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroRareID.Count);
                    ThirdBundleID = PlayerPrefs.GetInt("SavedThirdBundle");
                    ThirdBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroRareID[ThirdBundleID]];
                }

                if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Epic_Hero)
                {
                    ThirdBundleCollectionText.text = "EPIC HERO";
                    // int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroEpicID.Count);
                    ThirdBundleID = PlayerPrefs.GetInt("SavedThirdBundle");
                    ThirdBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroEpicID[ThirdBundleID]];
                }

                if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Hero)
                {
                    ThirdBundleCollectionText.text = "LEGENDARY HERO";
                    // int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroLegendaryID.Count);
                    ThirdBundleID = PlayerPrefs.GetInt("SavedThirdBundle");
                    ThirdBundleCollectionImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroLegendaryID[ThirdBundleID]];
                }

                if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Artifact_Chest)
                {
                    ThirdBundleCollectionText.text = "ARTIFACT";
                    //int ArtifactCountID = UnityEngine.Random.Range(0, 9);
                    ThirdBundleID = PlayerPrefs.GetInt("SavedThirdBundle");
                    ThirdBundleCollectionImage.sprite = MainPurchaseArtifact.ArtifactCollectionImage[ThirdBundleID];
                }

                if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Boost_Chest)
                {
                    ThirdBundleCollectionText.text = "BOOST CHEST";
                    //int TempBoost = UnityEngine.Random.Range(0, 4);
                    ThirdBundleID = PlayerPrefs.GetInt("SavedThirdBundle");
                    ThirdBundleCollectionImage.sprite = MainPurchaseBoost.BoostImageCollection[ThirdBundleID];
                }

                if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Normal_Chest)
                {
                    ThirdBundleCollectionText.text = "NORMAL CHEST";
                    ThirdBundleCollectionImage.sprite = NormalChestImage;
                }

                if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Chest)
                {
                    ThirdBundleCollectionText.text = "LEGENDARY CHEST";
                    ThirdBundleCollectionImage.sprite = LegendaryChestImage;
                }
                
                
               
            }

            Invoke("LateBundleFix", 1f);
        }

        public void LateBundleFix()
        {
            FirstBundleFix = true;
            SecondBundleFix = true;
            ThirdBundleFix = true;
            SaveBundleFixes();
        }

        public void DailyBundleButtonCheckManualUpdate()
        {
            if (MainPlayerWallet.GemWalletValue >= BundleCollectionPurchaseCost[BundleID])
            {
                DailyBundlePurchaseButton.interactable = true;
            }

            if (MainPlayerWallet.GemWalletValue < BundleCollectionPurchaseCost[BundleID])
            {
                DailyBundlePurchaseButton.interactable = false;
            }
        }

        public void DailyBundlePurchase()
        {
            // MainPlayerWallet.GemWalletValue -= DailyBundlePurchaseCostValue;
            MainAchievement.FindFirstHeroCheckManualUpdate();
            MainPlayerWallet.GemWalletValue -= BundleCollectionPurchaseCost[BundleID];
            CollectControl = 1;
            FirstCheck();
            HeroCollectAcquiredBundle();

            DailyBundlePurchaseButton.interactable = false;

            // Set the next interactable time to 23:50 on the next day
            DateTime nextInteractableTime = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            PlayerPrefs.SetString(LastPurchaseTimeKey, nextInteractableTime.ToString());
            PlayerPrefs.Save();
        }

        void CheckButtonState()
        {
            if (PlayerPrefs.HasKey(LastPurchaseTimeKey))
            {
                string lastPurchaseTimeStr = PlayerPrefs.GetString(LastPurchaseTimeKey);
                DateTime nextInteractableTime = DateTime.Parse(lastPurchaseTimeStr);
                DateTime currentDateTime = DateTime.Now;

                if (currentDateTime >= nextInteractableTime)
                {
                    DailyBundleButtonCheckManualUpdate();
                }
                else if (currentDateTime <= nextInteractableTime)
                {
                    DailyBundlePurchaseButton.interactable = false;
                }
            }
            else
            {
                DailyBundleButtonCheckManualUpdate();
            }
        }



        void CommonSubHero()
        {
            foreach (string SHAC in SubHeroItemManager.SubHeroAnimationCollection)
            {
                SubHeroCurrentAnimator.SetBool(SHAC, false);
            }

            ReceiveItemText.text = CollectControl.ToString() + "/" + "3";
            //int a = Random.Range(0, SubHeroItemManager.SubHeroCommonID.Count);

            int a = 0;

            if (CollectControl == 1)
            {
                a = FirstBundleID;
            }
            else if (CollectControl == 2)
            {
                a = SecondBundleID;
            }
            else if (CollectControl == 3)
            {
                a = ThirdBundleID;
            }

            PopUpWindowHeroImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroCommonID[a]];

            PopUpWindowHeroNameText.text = SubHeroItemManager.HeroNameCollection[SubHeroItemManager.SubHeroCommonID[a]];

            PopUpWindowObj.gameObject.SetActive(true);
            //PurchaseHeroValueCountText.text = "Remaining " + SubHeroPurchaseValueCount[HeroCount].ToString();

            //SubHeroPurchaseValueCount[HeroCount] -= 1;

            SubHeroCurrentAnimator.SetBool(SubHeroItemManager.SubHeroAnimationCollection[SubHeroItemManager.SubHeroCommonID[a]], true);

            //int x = Random.Range(3, 9);
            MainSubHero.SubHeroItemCount[SubHeroItemManager.SubHeroCommonID[a]] += 1;

            //PurchaseHeroItemValueCountText.text = "X" + x;

            if (MainSubHero.SubHeroActive[SubHeroItemManager.SubHeroCommonID[a]] == false)
            {
                MainSubHero.SubHeroActive[SubHeroItemManager.SubHeroCommonID[a]] = true;
            }

            PopUpCircleBGImage.color = Color.green;

            CollectControl += 1;
            MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroCommonID[a]].transform.parent = MainSubHero.HeroActiveParentObj.transform;
            //MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroRareID[a]].transform.SetAsLastSibling();
            MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroCommonID[a]].transform.SetSiblingIndex(0);
            //MainSubHero.FirstCheck();
        }

        void RareSubHero()
        {
			MainAchievement.CollectedHeroesCheckManualUpdate();

			foreach (string SHAC in SubHeroItemManager.SubHeroAnimationCollection)
            {
                SubHeroCurrentAnimator.SetBool(SHAC, false);
            }

            ReceiveItemText.text = CollectControl.ToString() + "/" + "3";
            //int a = Random.Range(0, SubHeroItemManager.SubHeroRareID.Count);

            int a = 0;

            if (CollectControl == 1)
            {
                a = FirstBundleID;
            }
            else if (CollectControl == 2)
            {
                a = SecondBundleID;
            }
            else if (CollectControl == 3)
            {
                a = ThirdBundleID;
            }

            PopUpWindowHeroImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroRareID[a]];

            PopUpWindowHeroNameText.text = SubHeroItemManager.HeroNameCollection[SubHeroItemManager.SubHeroRareID[a]];

            PopUpWindowObj.gameObject.SetActive(true);
            //PurchaseHeroValueCountText.text = "Remaining " + SubHeroPurchaseValueCount[HeroCount].ToString();

            //SubHeroPurchaseValueCount[HeroCount] -= 1;

            SubHeroCurrentAnimator.SetBool(SubHeroItemManager.SubHeroAnimationCollection[SubHeroItemManager.SubHeroRareID[a]], true);

            //int x = Random.Range(3, 9);
            MainSubHero.SubHeroItemCount[SubHeroItemManager.SubHeroRareID[a]] += 1;

            //PurchaseHeroItemValueCountText.text = "X" + x;

            if (MainSubHero.SubHeroActive[SubHeroItemManager.SubHeroRareID[a]] == false)
            {
                MainSubHero.SubHeroActive[SubHeroItemManager.SubHeroRareID[a]] = true;
            }

            PopUpCircleBGImage.color = Color.blue;

            CollectControl += 1;
            MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroRareID[a]].transform.parent = MainSubHero.HeroActiveParentObj.transform;
            //MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroRareID[a]].transform.SetAsLastSibling();
            MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroRareID[a]].transform.SetSiblingIndex(0);
            //MainSubHero.FirstCheck();
        }

        void EpicSubHero()
        {
			MainAchievement.CollectedHeroesCheckManualUpdate();

			foreach (string SHAC in SubHeroItemManager.SubHeroAnimationCollection)
            {
                SubHeroCurrentAnimator.SetBool(SHAC, false);
            }

            ReceiveItemText.text = CollectControl.ToString() + "/" + "3";
            //int a = Random.Range(0, SubHeroItemManager.SubHeroEpicID.Count);

            int a = 0;

            if (CollectControl == 1)
            {
                a = FirstBundleID;
            }
            else if (CollectControl == 2)
            {
                a = SecondBundleID;
            }
            else if (CollectControl == 3)
            {
                a = ThirdBundleID;
            }

            PopUpWindowHeroImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroEpicID[a]];

            PopUpWindowHeroNameText.text = SubHeroItemManager.HeroNameCollection[SubHeroItemManager.SubHeroEpicID[a]];

            PopUpWindowObj.gameObject.SetActive(true);
            //PurchaseHeroValueCountText.text = "Remaining " + SubHeroPurchaseValueCount[HeroCount].ToString();

            //SubHeroPurchaseValueCount[HeroCount] -= 1;

            SubHeroCurrentAnimator.SetBool(SubHeroItemManager.SubHeroAnimationCollection[SubHeroItemManager.SubHeroEpicID[a]], true);

            //int x = Random.Range(2, 2);
            MainSubHero.SubHeroItemCount[SubHeroItemManager.SubHeroEpicID[a]] += 1;

            //PurchaseHeroItemValueCountText.text = "X" + x;

            if (MainSubHero.SubHeroActive[SubHeroItemManager.SubHeroEpicID[a]] == false)
            {
                MainSubHero.SubHeroActive[SubHeroItemManager.SubHeroEpicID[a]] = true;

            }

            PopUpCircleBGImage.color = new Color(28, 0, 128, 255);

            CollectControl += 1;
            MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroEpicID[a]].transform.parent = MainSubHero.HeroActiveParentObj.transform;
            //MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroEpicID[a]].transform.SetAsLastSibling();
            MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroEpicID[a]].transform.SetSiblingIndex(0);
            //MainSubHero.FirstCheck();
        }

        void LegendarySubHero()
        {
			MainAchievement.CollectedHeroesCheckManualUpdate();
			MainAchievement.LegendaryHeroCheckManualUpdate();

			foreach (string SHAC in SubHeroItemManager.SubHeroAnimationCollection)
            {
                SubHeroCurrentAnimator.SetBool(SHAC, false);
            }

            ReceiveItemText.text = CollectControl.ToString() + "/" + "3"; ;
            //int a = Random.Range(0, SubHeroItemManager.SubHeroLegendaryID.Count);

            int a = 0;

            if (CollectControl == 1)
            {
                a = FirstBundleID;
            }
            else if (CollectControl == 2)
            {
                a = SecondBundleID;
            }
            else if (CollectControl == 3)
            {
                a = ThirdBundleID;
            }

            PopUpWindowHeroImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroLegendaryID[a]];

            PopUpWindowHeroNameText.text = SubHeroItemManager.HeroNameCollection[SubHeroItemManager.SubHeroLegendaryID[a]];

            PopUpWindowObj.gameObject.SetActive(true);
            //PurchaseHeroValueCountText.text = "Remaining " + SubHeroPurchaseValueCount[HeroCount].ToString();

            //SubHeroPurchaseValueCount[HeroCount] -= 1;

            SubHeroCurrentAnimator.SetBool(SubHeroItemManager.SubHeroAnimationCollection[SubHeroItemManager.SubHeroLegendaryID[a]], true);

            //int x = Random.Range(1, 4);
            MainSubHero.SubHeroItemCount[SubHeroItemManager.SubHeroLegendaryID[a]] += 1;

            //PurchaseHeroItemValueCountText.text = "X" + x;

            if (MainSubHero.SubHeroActive[SubHeroItemManager.SubHeroLegendaryID[a]] == false)
            {
                MainSubHero.SubHeroActive[SubHeroItemManager.SubHeroLegendaryID[a]] = true;
            }

            PopUpCircleBGImage.color = new Color(28, 0, 128, 255);

            CollectControl += 1;

            MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroLegendaryID[a]].transform.parent = MainSubHero.HeroActiveParentObj.transform;
            //MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroLegendaryID[a]].transform.SetAsLastSibling();
            MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroLegendaryID[a]].transform.SetSiblingIndex(0);
            //MainSubHero.FirstCheck();
        }

        void ArtifactChest()
        {
            //int ArtifactCountID = Random.Range(0, 9);

            int ArtifactCountID = 0;

            if (CollectControl == 1)
            {
                ArtifactCountID = FirstBundleID;
            }
            else if (CollectControl == 2)
            {
                ArtifactCountID = SecondBundleID;
            }
            else if (CollectControl == 3)
            {
                ArtifactCountID = ThirdBundleID;
            }

            foreach (string SHAC in SubHeroItemManager.SubHeroAnimationCollection)
            {
                SubHeroCurrentAnimator.SetBool(SHAC, false);
            }

            ReceiveItemText.text = CollectControl.ToString() + "/" + "3";

            if (MainArtifact.ArtifactActiveCollection[ArtifactCountID] == false)
            {
                MainArtifact.ArtifactActiveCollection[ArtifactCountID] = true;
            }

            else if (MainArtifact.ArtifactActiveCollection[ArtifactCountID] == true)
            {
                MainArtifact.ArtifactEvolveCountCollection[ArtifactCountID] += 1;
            }


            MainArtifact.FirstCheck();

            //CollectCountButtonReference = ArtifactCountID;
            PopUpWindowHeroImage.sprite = null;
            PopUpWindowHeroNameText.text = string.Empty;

            PopUpWindowHeroImage.sprite = MainPurchaseArtifact.ArtifactCollectionImage[ArtifactCountID];

            PopUpWindowObj.gameObject.SetActive(true);
            PopUpWindowHeroNameText.text = MainPurchaseArtifact.ArtifactCollectionName[ArtifactCountID];

            CollectControl += 1;
        }

        void BoostChest()
        {
            //int TempBoost = Random.Range(0, 4);

            int TempBoost = 0;

            if (CollectControl == 1)
            {
                TempBoost = FirstBundleID;
            }
            else if (CollectControl == 2)
            {
                TempBoost = SecondBundleID;
            }
            else if (CollectControl == 3)
            {
                TempBoost = ThirdBundleID;
            }

            PopUpWindowHeroImage.sprite = null;
            PopUpWindowHeroNameText.text = string.Empty;

            PopUpWindowHeroImage.sprite = MainPurchaseBoost.BoostImageCollection[TempBoost];
            PopUpWindowHeroNameText.text = MainPurchaseBoost.BoostNameCollection[TempBoost];

            ReceiveItemText.text = CollectControl.ToString() + "/" + "3"; ;

            PopUpWindowObj.gameObject.SetActive(true);

            if (TempBoost == 0)
            {
                MainPurchaseBoost.DoubleGoldIncomeCountDownTimerValue += MainPurchaseBoost.DoubleGoldIncomeTimerCountdownReference;
                MainPurchaseBoost.ActivateDoubleGoldIncomeCondition = true;
                MainPurchaseBoost.FirstCheck();
            }

            else if (TempBoost == 1)
            {
                MainPurchaseBoost.DoubleVillageIncomeCountdownTimerValue += MainPurchaseBoost.DoubleVillageIncomeCountdownReference;
                MainPurchaseBoost.ActivateDoubleVillageIncomeCondition = true;
                MainVillageMarket.DoubleVillageGoldIncomeManualCheckAllSet();
                MainPurchaseBoost.FirstCheck();
            }

            else if (TempBoost == 2)
            {
                MainPurchaseBoost.ActivateOfflineEarningsCondition = true;
                MainVillageMarket.TotalOfflineEarningHoursValue += MainPurchaseBoost.OfflineEarningsCountdownReference;
                MainVillageMarket.OfflineEarningFirstCheck();
            }

            else if (TempBoost == 3)
            {
                MainVillageMarket.TimeTravelActivate();
                MainPurchaseBoost.DailyBundleEnemyBossTimeTravelIncome();
            }

            CollectControl += 1;
        }

        void NormalChest()
        {
            int i = UnityEngine.Random.Range(0, 50);

            if (i == 49)
            {
                LegendarySubHero();
            }

            if (i >= 45 && i <= 48)
            {
                EpicSubHero();
            }

            if (i >= 25 && i <= 44)
            {
                RareSubHero();
            }

            if (i >= 0 && i <= 24)
            {
                CommonSubHero();
            }
        }

        void LegendaryChest()
        {
            int i = UnityEngine.Random.Range(0, 50);

            if (i == 49)
            {
                SetLegendarySubHero();
            }

            if (i >= 45 && i <= 48)
            {
                SetEpicSubHero();
            }

            if (i >= 25 && i <= 44)
            {
                SetRareSubHero();
            }

            if (i >= 0 && i <= 24)
            {
                SetCommonSubHero();
            }
        }

        void SetLegendarySubHero()
        {
			MainAchievement.CollectedHeroesCheckManualUpdate();
			MainAchievement.LegendaryHeroCheckManualUpdate();

			foreach (string SHAC in SubHeroItemManager.SubHeroAnimationCollection)
            {
                SubHeroCurrentAnimator.SetBool(SHAC, false);
            }
            SetCount += 1;
            ReceiveItemText.text = CollectControl.ToString() + "/" + "3"; 
            int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroLegendaryID.Count);


            PopUpWindowHeroImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroLegendaryID[a]];

            PopUpWindowHeroNameText.text = SubHeroItemManager.HeroNameCollection[SubHeroItemManager.SubHeroLegendaryID[a]];

            PopUpWindowObj.gameObject.SetActive(true);
            //PurchaseHeroValueCountText.text = "Remaining " + SubHeroPurchaseValueCount[HeroCount].ToString();

            //SubHeroPurchaseValueCount[HeroCount] -= 1;

            SubHeroCurrentAnimator.SetBool(SubHeroItemManager.SubHeroAnimationCollection[SubHeroItemManager.SubHeroLegendaryID[a]], true);

            //int x = Random.Range(1, 4);
            MainSubHero.SubHeroItemCount[SubHeroItemManager.SubHeroLegendaryID[a]] += 1;

            //PurchaseHeroItemValueCountText.text = "X" + x;

            if (MainSubHero.SubHeroActive[SubHeroItemManager.SubHeroLegendaryID[a]] == false)
            {
                MainSubHero.SubHeroActive[SubHeroItemManager.SubHeroLegendaryID[a]] = true;
            }

            PopUpCircleBGImage.color = new Color(28, 0, 128, 255);

            

            MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroLegendaryID[a]].transform.parent = MainSubHero.HeroActiveParentObj.transform;
            //MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroLegendaryID[a]].transform.SetAsLastSibling();
            MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroLegendaryID[a]].transform.SetSiblingIndex(0);
            //MainSubHero.FirstCheck();
        }

        void SetEpicSubHero()
        {
			MainAchievement.CollectedHeroesCheckManualUpdate();
			foreach (string SHAC in SubHeroItemManager.SubHeroAnimationCollection)
            {
                SubHeroCurrentAnimator.SetBool(SHAC, false);
            }

            SetCount += 1;
            ReceiveItemText.text = CollectControl.ToString() + "/" + "3";
            int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroEpicID.Count);

            PopUpWindowHeroImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroEpicID[a]];

            PopUpWindowHeroNameText.text = SubHeroItemManager.HeroNameCollection[SubHeroItemManager.SubHeroEpicID[a]];

            PopUpWindowObj.gameObject.SetActive(true);
            //PurchaseHeroValueCountText.text = "Remaining " + SubHeroPurchaseValueCount[HeroCount].ToString();

            //SubHeroPurchaseValueCount[HeroCount] -= 1;

            SubHeroCurrentAnimator.SetBool(SubHeroItemManager.SubHeroAnimationCollection[SubHeroItemManager.SubHeroEpicID[a]], true);

            //int x = Random.Range(2, 2);
            MainSubHero.SubHeroItemCount[SubHeroItemManager.SubHeroEpicID[a]] += 1;

            //PurchaseHeroItemValueCountText.text = "X" + x;

            if (MainSubHero.SubHeroActive[SubHeroItemManager.SubHeroEpicID[a]] == false)
            {
                MainSubHero.SubHeroActive[SubHeroItemManager.SubHeroEpicID[a]] = true;

            }

            PopUpCircleBGImage.color = new Color(28, 0, 128, 255);

            
            MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroEpicID[a]].transform.parent = MainSubHero.HeroActiveParentObj.transform;
            //MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroEpicID[a]].transform.SetAsLastSibling();
            MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroEpicID[a]].transform.SetSiblingIndex(0);
            //MainSubHero.FirstCheck();
        }

        void SetRareSubHero()
        {
			MainAchievement.CollectedHeroesCheckManualUpdate();
			foreach (string SHAC in SubHeroItemManager.SubHeroAnimationCollection)
            {
                SubHeroCurrentAnimator.SetBool(SHAC, false);
            }

            SetCount += 1;
            ReceiveItemText.text = CollectControl.ToString() + "/" + "3";
            int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroRareID.Count);

            PopUpWindowHeroImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroRareID[a]];

            PopUpWindowHeroNameText.text = SubHeroItemManager.HeroNameCollection[SubHeroItemManager.SubHeroRareID[a]];

            PopUpWindowObj.gameObject.SetActive(true);
            //PurchaseHeroValueCountText.text = "Remaining " + SubHeroPurchaseValueCount[HeroCount].ToString();

            //SubHeroPurchaseValueCount[HeroCount] -= 1;

            SubHeroCurrentAnimator.SetBool(SubHeroItemManager.SubHeroAnimationCollection[SubHeroItemManager.SubHeroRareID[a]], true);

            //int x = Random.Range(3, 9);
            MainSubHero.SubHeroItemCount[SubHeroItemManager.SubHeroRareID[a]] += 1;

            //PurchaseHeroItemValueCountText.text = "X" + x;

            if (MainSubHero.SubHeroActive[SubHeroItemManager.SubHeroRareID[a]] == false)
            {
                MainSubHero.SubHeroActive[SubHeroItemManager.SubHeroRareID[a]] = true;
            }

            PopUpCircleBGImage.color = Color.blue;

            
            MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroRareID[a]].transform.parent = MainSubHero.HeroActiveParentObj.transform;
            //MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroRareID[a]].transform.SetAsLastSibling();
            MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroRareID[a]].transform.SetSiblingIndex(0);
            //MainSubHero.FirstCheck();
        }

        void SetCommonSubHero()
        {
			MainAchievement.CollectedHeroesCheckManualUpdate();
			foreach (string SHAC in SubHeroItemManager.SubHeroAnimationCollection)
            {
                SubHeroCurrentAnimator.SetBool(SHAC, false);
            }

            SetCount += 1;
            ReceiveItemText.text = CollectControl.ToString() + "/" + "3";
            int a = UnityEngine.Random.Range(0, SubHeroItemManager.SubHeroCommonID.Count);

            PopUpWindowHeroImage.sprite = SubHeroItemManager.SubHeroImageCollection[SubHeroItemManager.SubHeroCommonID[a]];

            PopUpWindowHeroNameText.text = SubHeroItemManager.HeroNameCollection[SubHeroItemManager.SubHeroCommonID[a]];

            PopUpWindowObj.gameObject.SetActive(true);
            //PurchaseHeroValueCountText.text = "Remaining " + SubHeroPurchaseValueCount[HeroCount].ToString();

            //SubHeroPurchaseValueCount[HeroCount] -= 1;

            SubHeroCurrentAnimator.SetBool(SubHeroItemManager.SubHeroAnimationCollection[SubHeroItemManager.SubHeroCommonID[a]], true);

            //int x = Random.Range(3, 9);
            MainSubHero.SubHeroItemCount[SubHeroItemManager.SubHeroCommonID[a]] += 1;

            //PurchaseHeroItemValueCountText.text = "X" + x;

            if (MainSubHero.SubHeroActive[SubHeroItemManager.SubHeroCommonID[a]] == false)
            {
                MainSubHero.SubHeroActive[SubHeroItemManager.SubHeroCommonID[a]] = true;
            }

            PopUpCircleBGImage.color = Color.green;

            
            MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroCommonID[a]].transform.parent = MainSubHero.HeroActiveParentObj.transform;
            //MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroRareID[a]].transform.SetAsLastSibling();
            MainSubHero.SubHeroUIObj[SubHeroItemManager.SubHeroCommonID[a]].transform.SetSiblingIndex(0);
            //MainSubHero.FirstCheck();
        }

        public void AcquireBundlePurchaseDeduction()
        {
            MainPlayerWallet.GemWalletValue -= BundleCollectionPurchaseCost[BundleID];
        }

        public void HeroCollectAcquiredBundle()
        {
            if (LegendaryChestCheck == false)
            {
                if (CollectControl == 1)
                {
                    if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Rare_Hero)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(true);
                        SubHeroCurrentAnimator.enabled = true;
                        RareSubHero();
                    }

                    if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Epic_Hero)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(true);
                        SubHeroCurrentAnimator.enabled = true;
                        EpicSubHero();
                    }

                    if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Hero)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(true);
                        SubHeroCurrentAnimator.enabled = true;
                        LegendarySubHero();
                    }

                    if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Artifact_Chest)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(false);
                        SubHeroCurrentAnimator.enabled = false;
                        ArtifactChest();
                    }

                    if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Boost_Chest)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(false);
                        SubHeroCurrentAnimator.enabled = false;
                        BoostChest();
                    }

                    if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Normal_Chest)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(true);
                        SubHeroCurrentAnimator.enabled = true;
                        NormalChest();
                    }

                    if (FirstBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Chest)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(true);
                        SubHeroCurrentAnimator.enabled = true;
                        SetCount = 0;
                        LegendaryChestCheck = true;
                        LegendaryChest();
                    }
                }

                else if (CollectControl == 2)
                {
                    if(SecondBundleCollectionList[BundleID] == BundleCollectionType.Rare_Hero)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(true);
                        SubHeroCurrentAnimator.enabled = true;
                        RareSubHero();
                    }

                    if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Epic_Hero)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(true);
                        SubHeroCurrentAnimator.enabled = true;
                        EpicSubHero();
                    }

                    if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Hero)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(true);
                        SubHeroCurrentAnimator.enabled = true;
                        LegendarySubHero();
                    }

                    if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Artifact_Chest)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(false);
                        SubHeroCurrentAnimator.enabled = false;
                        ArtifactChest();
                    }

                    if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Boost_Chest)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(false);
                        SubHeroCurrentAnimator.enabled = false;
                        BoostChest();
                    }

                    if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Normal_Chest)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(true);
                        SubHeroCurrentAnimator.enabled = true;
                        NormalChest();
                    }

                    if (SecondBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Chest)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(true);
                        SubHeroCurrentAnimator.enabled = true;
                        SetCount = 0;
                        LegendaryChestCheck = true;
                        LegendaryChest();
                    }
                }

                else if (CollectControl == 3)
                {
                    if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Rare_Hero)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(true);
                        SubHeroCurrentAnimator.enabled = true;
                        RareSubHero();
                    }

                    if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Epic_Hero)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(true);
                        SubHeroCurrentAnimator.enabled = true;
                        EpicSubHero();
                    }

                    if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Hero)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(true);
                        SubHeroCurrentAnimator.enabled = true;
                        LegendarySubHero();
                    }

                    if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Artifact_Chest)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(false);
                        SubHeroCurrentAnimator.enabled = false;
                        ArtifactChest();
                    }

                    if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Boost_Chest)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(false);
                        SubHeroCurrentAnimator.enabled = false;
                        BoostChest();
                    }

                    if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Normal_Chest)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(true);
                        SubHeroCurrentAnimator.enabled = true;
                        NormalChest();
                    }

                    if (ThirdBundleCollectionList[BundleID] == BundleCollectionType.Legendary_Chest)
                    {
                        PopUpCircleBGImage.gameObject.SetActive(true);
                        SubHeroCurrentAnimator.enabled = true;
                        SetCount = 0;
                        LegendaryChestCheck = true;
                        LegendaryChest();
                    }

                }

                else if (CollectControl >= 4)
                {
                    foreach (string SHAC in SubHeroItemManager.SubHeroAnimationCollection)
                    {
                        SubHeroCurrentAnimator.SetBool(SHAC, false);

                    }
                    CollectControl = 0;
                    ReceiveItemText.text = string.Empty;
                    MainSubHero.FirstCheck();
                    PopUpWindowObj.gameObject.SetActive(false);
                }
            }

            if (LegendaryChestCheck == true)
            {
                if (SetCount <= 10)
                {
                    LegendaryChest();
                }
                
                if (SetCount > 10)
                {
                    LegendaryChestCheck = false;
                    CollectControl += 1;
                    HeroCollectAcquiredBundle();
                }
            }
        }
        // Update is called once per frame
        void Update()
        {
            CheckButtonState();
        }
    }
}

