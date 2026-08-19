using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SAMPLETEXT.Wallet.Manager;
using SAMPLETEXT.Data.Manager.Village;
using SAMPLETEXT.ItemPurchase.Manager.Boost;
using System;
using SAMPLETEXT.Talent.Manager;
using SAMPLETEXT.Artifact.Manager;
using SAMPLETEXT.Gameplay.Manager.MainHero;
using SAMPLETEXT.Market.Manager.Status;
using SAMPLETEXT.Achievement.Manager;

namespace SAMPLETEXT.Market.Manager
{
    public class MarketManagerScript : MonoBehaviour
    {
        [SerializeField] Achievment_NewSystem achievmentSystem;
        [SerializeField]
        ArtifactManagerScript MainArtifact;
        [SerializeField]
        TalentsManagerScript MainTalent;
        [SerializeField]
        ItemsPurchaseBoostManagerScript MainBoost;
        [SerializeField]
        JSONGameplayVillageManagerScript SaveData;
        [SerializeField]
        WalletManagerScript MainWallet;

        [Header("Village Manager Settings")]
        public string[] MarketNameStringCollection;
        [SerializeField]
        TextMeshProUGUI[] MarketNameStringTextCollection;
        
        [SerializeField]
        TextMeshProUGUI[] MarketIncomeValueTextCollection;
        public List<float> MarketIncomeValueCollection = new List<float>();
        //[SerializeField]
        //TextMeshProUGUI[] MarketTimerCDValueTextCollection;
        public List<float>MarketTimerCDValueCollection = new List<float>();
        [SerializeField]
        Button[] BuyButtonCollectionCollection;
        [SerializeField]
        GameObject[] VillageInFieldObj;
        [Header("Village Manager Cost Settings")]
        [SerializeField]
        TextMeshProUGUI[] MarketPurchaseCountValueTextCollection;
        public List<float>MarketPurchaseCountValueCollection = new List<float>();

        [SerializeField]
        TextMeshProUGUI[] MarketPurchaseCostCountValueCollectionText;

        [SerializeField] TextMeshProUGUI[] MarketPurchaseCostCountValueCollectionTextLevels;
        public List<float> MarketPurchaseCostCountValueCollection = new List<float>();
        public List<float> MarketPurchaseCostCountValueCollectionTemp = new List<float>();

        [Header("Village Count Down Settings")]
        [SerializeField]
        bool[] TimerControl;
        [SerializeField]
        float[] TimerCountDownTemp;
        [SerializeField]
        TextMeshProUGUI[] TimerCountDownTempText;
        [SerializeField]
        Image[] TimerBarImage;

        [Header("Village Offline Earnings")]
        public float TotalOfflineEarningHoursValue;
        [SerializeField]
        float OfflineTimeValue; // Total Offline Time in Seconds
        [SerializeField]
        float[] OfflineCountValue;
        public float[] OfflineCoinEarningsValue;
        float TempOfflineTotalCoinEarningsValue;
        [SerializeField]
        TextMeshProUGUI OfflineTotalCoinEarningsValueText;
        public float OfflineTotalCoinEarningsValue;
        [SerializeField]
        GameObject OfflineCollectButtonObj;
        //[SerializeField]
        //float[] TotalTimeOfflineValueCollection;
        //[SerializeField]
        //float[] TotalOfflineIncomeValue;
        //[SerializeField]
        //float OverAllTotalOfflineIncomeValue;

        [Header("Village Date Offline Settings")]
        DateTime dateQuit;
        DateTime dateNow;
        bool DateAcquired;


        [Header("Village Time Travel Settings")]
        [HideInInspector]
        public float TotalTimeTravelEarningsCoinValue;
        [SerializeField]
        float[] TimeTravelEarningsCoinValueCollection;
        [SerializeField]
        float[] TimeTravelEarningsCountValueCollection;

        [Header("Total Settings")]
        public float TotalIncomeValue;
        public float TotalCostValue;

        [Header("Purchase Collection Settings")]
        int PurchaseID;
        [SerializeField]
        GameObject[] PurchaseButtonCollection;

        [Header("Village Upgrade Settings")]
        public List<int> VillageUpgradeLevel = new List<int>(); //New JSON
        enum VillageType
        {
            MARKET,
           TOWNHALL,
           MAGESGUILD,
           BUTCHER,
           ALCHEMIST,
           FISHERMANSHUT,
           HARBOR,
           COWFARM,
           VOLCANO,
           OURSTATUE,
           HOLEINGROUND,
           COURT,
           CENTERSQUARE,
           TRAININGGROUND,
           PLAINCOTTAGE,
           CHURCH,
           ALTAR,
           CHARIOTPARKING,
           SHIP,
           ARMORY,
           CLINIC,
           IRONWORKS,
           ARTIFACTSHOP,
           CEMETERY,
           POWERSTATION,
           THIEVESGUILD,
           GREENHOUSE,
           ROBOTFACTORY,
           MONSTERZOO


        };

        [SerializeField]
        List<VillageType> VillageClassification = new List<VillageType>();

        [Header("Village Path Settings")]
        [SerializeField]
        GameObject[] PathObjCollection;

        [Header("Village Market Image Settings")]
        public int VillageMarketUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageMarketUpgradeNextLevel;
        [SerializeField]
        Sprite[] MarketSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer MarketFieldSprite;
        [SerializeField]
        Image MarketUIImage;

        [Header("Village Town Hall Image Settings")]
        public int VillageTownHallUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageTownHallUpgradeNextLevel;
        [SerializeField]
        Sprite[] TownHallSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer TownHallFieldSprite;
        [SerializeField]
        Image TownHallUIImage;

        [Header("Village Mages Guild Image Settings")]
        public int VillageMagesGuildUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageMagesGuildUpgradeNextLevel;
        [SerializeField]
        Sprite[] MagesGuildSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer MagesGuildFieldSprite;
        [SerializeField]
        Image MagesGuildUIImage;

        [Header("Village Butcher Image Settings")]
        public int VillageButcherUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageButcherUpgradeNextLevel;
        [SerializeField]
        Sprite[] ButcherSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer ButcherFieldSprite;
        [SerializeField]
        Image ButcherUIImage;

        [Header("Village Alchemist Image Settings")]
        public int VillageAlchemistUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageAlchemistUpgradeNextLevel;
        [SerializeField]
        Sprite[] AlchemistSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer AlchemistFieldSprite;
        [SerializeField]
        Image AlchemistUIImage;

        [Header("Village Fisherhmans Hut Image Settings")]
        public int VillageFisherMansHutUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageFisherMansHutUpgradeNextLevel;
        [SerializeField]
        Sprite[] FisherMansHutSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer FisherMansHutFieldSprite;
        [SerializeField]
        Image FisherMansHutUIImage;

        [Header("Village Harbor Image Settings")]
        public int VillageHarborUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageHarborUpgradeNextLevel;
        [SerializeField]
        Sprite[] HarborSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer HarborFieldSprite;
        [SerializeField]
        Image HarborUIImage;

        [Header("Village Cow Farm Image Settings")]
        public int VillageCowFarmUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageCowFarmUpgradeNextLevel;
        [SerializeField]
        Sprite[] CowFarmSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer CowFarmFieldSprite;
        [SerializeField]
        Image CowFarmUIImage;

        [Header("Village Volcano Image Settings")]
        public int VillageVolcanoUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageVolcanoUpgradeNextLevel;
        [SerializeField]
        Sprite[] VolcanoSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer VolcanoFieldSprite;
        [SerializeField]
        Image VolcanoUIImage;

        [Header("Village Our Statue Image Settings")]
        public int VillageOurStatueUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageOurStatueUpgradeNextLevel;
        [SerializeField]
        Sprite[] OurStatueSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer OurStatueFieldSprite;
        [SerializeField]
        Image OurStatueUIImage;

        [Header("Village Hole In Ground Image Settings")]
        public int VillageHoleInGroundUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageHoleInGroundUpgradeNextLevel;
        [SerializeField]
        Sprite[] HoleInGroundSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer HoleInGroundFieldSprite;
        [SerializeField]
        Image HoleInGroundUIImage;

        [Header("Village Court Image Settings")]
        public int VillageCourtUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageCourtUpgradeNextLevel;
        [SerializeField]
        Sprite[] CourtSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer CourtFieldSprite;
        [SerializeField]
        Image CourtUIImage;

        [Header("Village Center Square Settings")]
        public int VillageCenterSquareUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageCenterSquareUpgradeNextLevel;
        [SerializeField]
        Sprite[] CenterSquareSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer CenterSquareFieldSprite;
        [SerializeField]
        Image CenterSquareUIImage;

        [Header("Village Training Ground Settings")]
        public int VillageTrainingGroundUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageTrainingGroundUpgradeNextLevel;
        [SerializeField]
        Sprite[] TrainingGroundSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer TrainingGroundFieldSprite;
        [SerializeField]
        Image TrainingGroundUIImage;

        [Header("Village Plain Cottage Settings")]
        public int VillagePlainCottageUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillagePlainCottageUpgradeNextLevel;
        [SerializeField]
        Sprite[] PlainCottageSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer PlainCottageFieldSprite;
        [SerializeField]
        Image PlainCottageUIImage;

        [Header("Village Church Settings")]
        public int VillageChurchUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageChurchUpgradeNextLevel;
        [SerializeField]
        Sprite[] ChurchSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer ChurchFieldSprite;
        [SerializeField]
        Image ChurchUIImage;

        [Header("Village Altar Settings")]
        public int VillageAltarUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageAltarUpgradeNextLevel;
        [SerializeField]
        Sprite[] AltarSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer AltarFieldSprite;
        [SerializeField]
        Image AltarUIImage;

        [Header("Village Chariot Parking Settings")]
        public int VillageChariotParkingUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageChariotParkingUpgradeNextLevel;
        [SerializeField]
        Sprite[] ChariotParkingSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer ChariotParkingFieldSprite;
        [SerializeField]
        Image ChariotParkingUIImage;

        [Header("Village Ship Settings")]
        public int VillageShipUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageShipUpgradeNextLevel;
        [SerializeField]
        Sprite[] ShipSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer ShipFieldSprite;
        [SerializeField]
        Image ShipUIImage;

        [Header("Village Armory Settings")]
        public int VillageArmoryUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageArmoryUpgradeNextLevel;
        [SerializeField]
        Sprite[] ArmorySpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer ArmoryFieldSprite;
        [SerializeField]
        Image ArmoryUIImage;

        [Header("Village Clinic Settings")]
        public int VillageClinicUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageClinicUpgradeNextLevel;
        [SerializeField]
        Sprite[] ClinicSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer ClinicFieldSprite;
        [SerializeField]
        Image ClinicUIImage;

        [Header("Village Iron Works Settings")]
        public int VillageIronWorksUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageIronWorksUpgradeNextLevel;
        [SerializeField]
        Sprite[] IronWorksSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer IronWorksFieldSprite;
        [SerializeField]
        Image IronWorksUIImage;

        [Header("Village Artifact Shop Settings")]
        public int VillageArtifactShopUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageArtifactShopUpgradeNextLevel;
        [SerializeField]
        Sprite[] ArtifactShopSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer ArtifactShopFieldSprite;
        [SerializeField]
        Image ArtifactShopUIImage;

        [Header("Village Cemetery Settings")]
        public int VillageCemeteryUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageCemeteryUpgradeNextLevel;
        [SerializeField]
        Sprite[] CemeterySpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer CemeteryFieldSprite;
        [SerializeField]
        Image CemeteryUIImage;

        [Header("Village Power Station Settings")]
        public int VillagePowerStationUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillagePowerStationUpgradeNextLevel;
        [SerializeField]
        Sprite[] PowerStationSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer PowerStationFieldSprite;
        [SerializeField]
        Image PowerStationUIImage;

        [Header("Village Thieves Guild Settings")]
        public int VillageThievesGuildUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageThievesGuildUpgradeNextLevel;
        [SerializeField]
        Sprite[] ThievesGuildSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer ThievesGuildFieldSprite;
        [SerializeField]
        Image ThievesGuildUIImage;

        [Header("Village Green House Settings")]
        public int VillageGreenHouseUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageGreenHouseUpgradeNextLevel;
        [SerializeField]
        Sprite[] GreenHouseSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer GreenHouseFieldSprite;
        [SerializeField]
        Image GreenHouseUIImage;

        [Header("Village Robot Factory Settings")]
        public int VillageRobotFactoryUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageRobotFactoryUpgradeNextLevel;
        [SerializeField]
        Sprite[] RobotFactorySpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer RobotFactoryFieldSprite;
        [SerializeField]
        Image RobotFactoryUIImage;

        [Header("Village Monster Zoo Settings")]
        public int VillageMonsterZooUpgradeCurrentLevel; //New JSON
        [SerializeField]
        int[] VillageMonsterZooUpgradeNextLevel;
        [SerializeField]
        Sprite[] MonsterZooSpriteUpgradeCollection;
        [SerializeField]
        SpriteRenderer MonsterZooFieldSprite;
        [SerializeField]
        Image MonsterZooUIImage;

        [Header("Prestige Reset Settings")]
        [SerializeField]
        GameplayMainHeroManagerScript MainHero;
        [SerializeField]
        float[] MarketIncomeValueCollectionReset;
        [SerializeField]
        float[] MarketPurchaseCostCountValueCollectionReset;

        [Header("Market Status Settings")]
        [SerializeField]
        MarketStatusManagerScript MainMarketStatus;
        public int MarketStatusID;
        public List<float> MarketNextMileStoneStatus = new List<float>();
        public List<float> MarketNextIncomeStatus = new List<float>();
        public List<Sprite> MarketSpriteStatus = new List<Sprite>();

        [Header("Market Manual Collect Settings")]
        [SerializeField]
        GameObject[] ManualCollectButton;

        [Header("Inventory Settings")]
        public float InventoryVillageIncomeIncrease;

        [Header("Group Purchase Settings")]
        [SerializeField]
        float[] TempMaxPurchaseGroupValue;
        [SerializeField]
        int[] FinalTempMaxPurchaseGroupValue;
        [SerializeField]
        float[] TempMaxMarketPurchaseCostGroupValue;

		[Header ("Achievement Settings")]
		[SerializeField]
		AchievementManagerScript MainAchievement;

        // Start is called before the first frame update
        void Start()
        {
            //SaveData = GameObject.FindObjectOfType<JSONGameplayVillageManagerScript>();
            //MainWallet = GameObject.FindObjectOfType<WalletManagerScript>();  


            string dateQuitString = PlayerPrefs.GetString("Date Quit", "");

            if (dateQuitString.Equals("") == false)
            {
                dateQuit = DateTime.Parse(dateQuitString);
                dateNow = DateTime.Now;

                if (dateNow > dateQuit)
                {
                    TimeSpan timeSpan = dateNow - dateQuit;
                    //Debug.Log("Quit For" + timeSpan.TotalSeconds + " Seconds");

                    float Tempfloat = (float)timeSpan.TotalSeconds;

                    OfflineTimeValue = Tempfloat;
                    Debug.Log(OfflineTimeValue);
                    //DateAcquired = false;
                }

            }

            for (int i = 0; i < MarketPurchaseCostCountValueCollection.Count; i++)
            {
                MarketPurchaseCostCountValueCollectionTemp[i] = (MarketPurchaseCostCountValueCollection[i] * (1 - MainArtifact.DeflationTotalValue) - MainTalent.TotalReduceUpgradeCost);
        }

            ManualMarketValueCheckUpdate();
        }
       
      

        public void ManualMarketValueCheckUpdate()
        {
            //TotalIncomeValue = 0;
            for (int i = 0; i < MarketPurchaseCountValueCollection.Count; i++)
            {
                MarketNameStringTextCollection[i].text = MarketNameStringCollection[i].ToString();
                GoldIncomeCheckManualUpdate(i);
                MarketPurchaseCountValueTextCollection[i].text = "LV" + (MarketPurchaseCountValueCollection[i] * (1 - MainArtifact.DeflationTotalValue) - MainTalent.TotalReduceUpgradeCost).ToString();
                GoldCostCheckManualUpdate(i);

                //FIX DRYMARTI TIMER SET TO 0 AT THE END
                if(TimerCountDownTemp[i] >= 0)
                {
                    TimerCountDownTempText[i].text = "" + Mathf.Round(TimerCountDownTemp[i]);
                    TimerBarImage[i].fillAmount = TimerCountDownTemp[i] / MarketTimerCDValueCollection[i];
                }
                else if (TimerCountDownTemp[i] < 0)
                {
                    TimerCountDownTempText[i].text = "COLLECT";
                    TimerBarImage[i].fillAmount = 1;
                }
               

                if (MarketPurchaseCountValueCollection[i] >= 1)
                {
                    VillageInFieldObj[i].gameObject.SetActive(true);
                }

                if (MarketPurchaseCountValueCollection[i] <= 0)
                {
                    VillageInFieldObj[i].gameObject.SetActive(false);
                }

                // Offline System Earnings
                


            }

            //for (int i = 0; i < MarketPurchaseCostCountValueCollection.Count; i++)
            //{
            //    MarketPurchaseCostCountValueCollectionTemp[i] = MarketPurchaseCostCountValueCollection[i];
            //}

            CheckTotalValueManualUpdate();
            CheckTotalCostValueManualUpdate();
            CheckVillageStructureManualUpdate();
			CheckPurchaseCountValueTextManualUpdate();

		}

        void CheckVillageStructureManualUpdate()
        {
           for (int i = 0; i < VillageClassification.Count; i++)
            {
                for (int x = 0; x < VillageUpgradeLevel.Count; x++)
                {
                    if (x == i)
                    {
                        if (VillageClassification[i] == VillageType.MARKET)
                        {
                            if (VillageUpgradeLevel[i] <= VillageMarketUpgradeNextLevel[VillageMarketUpgradeCurrentLevel])
                            {
                                if(VillageMarketUpgradeCurrentLevel == 0 )
                                {
                                    MarketFieldSprite.sprite = MarketSpriteUpgradeCollection[VillageMarketUpgradeCurrentLevel];
                                    MarketUIImage.sprite = MarketSpriteUpgradeCollection[5];
                                }
                                else if (VillageMarketUpgradeCurrentLevel == 1)
                                {
                                    MarketFieldSprite.sprite = MarketSpriteUpgradeCollection[VillageMarketUpgradeCurrentLevel];
                                    MarketUIImage.sprite = MarketSpriteUpgradeCollection[6];
                                }
                                else if (VillageMarketUpgradeCurrentLevel == 2)
                                {
                                    MarketFieldSprite.sprite = MarketSpriteUpgradeCollection[VillageMarketUpgradeCurrentLevel];
                                    MarketUIImage.sprite = MarketSpriteUpgradeCollection[7];
                                }
                                else if (VillageMarketUpgradeCurrentLevel == 3)
                                {
                                    MarketFieldSprite.sprite = MarketSpriteUpgradeCollection[VillageMarketUpgradeCurrentLevel];
                                    MarketUIImage.sprite = MarketSpriteUpgradeCollection[8];
                                }
                                else if (VillageMarketUpgradeCurrentLevel == 4)
                                {
                                    MarketFieldSprite.sprite = MarketSpriteUpgradeCollection[VillageMarketUpgradeCurrentLevel];
                                    MarketUIImage.sprite = MarketSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    MarketFieldSprite.sprite = MarketSpriteUpgradeCollection[VillageMarketUpgradeCurrentLevel];
                                    MarketUIImage.sprite = MarketSpriteUpgradeCollection[VillageMarketUpgradeCurrentLevel];
                                }

                                MarketFieldSprite.sprite = MarketSpriteUpgradeCollection[VillageMarketUpgradeCurrentLevel];


                            }

                            if (VillageUpgradeLevel[i] > VillageMarketUpgradeNextLevel[VillageMarketUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageMarketUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageMarketUpgradeCurrentLevel < TempCount )
                                {
                                    VillageMarketUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageMarketUpgradeCurrentLevel >= TempCount)
                                {
                                    MarketFieldSprite.sprite = MarketSpriteUpgradeCollection[VillageMarketUpgradeCurrentLevel];
                                    MarketUIImage.sprite = MarketSpriteUpgradeCollection[VillageMarketUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageMarketUpgradeNextLevel[VillageMarketUpgradeCurrentLevel];
                            
                            MarketSpriteStatus[i] = MarketSpriteUpgradeCollection[VillageMarketUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.TOWNHALL)
                        {
                            if (VillageUpgradeLevel[i] <= VillageTownHallUpgradeNextLevel[VillageTownHallUpgradeCurrentLevel])
                            {
                                if (VillageTownHallUpgradeCurrentLevel == 0)
                                {
                                    TownHallFieldSprite.sprite = TownHallSpriteUpgradeCollection[VillageTownHallUpgradeCurrentLevel];
                                    TownHallUIImage.sprite = TownHallSpriteUpgradeCollection[5];
                                }
                                else if (VillageTownHallUpgradeCurrentLevel == 1)
                                {
                                    TownHallFieldSprite.sprite = TownHallSpriteUpgradeCollection[VillageTownHallUpgradeCurrentLevel];
                                    TownHallUIImage.sprite = TownHallSpriteUpgradeCollection[6];
                                }
                                else if (VillageTownHallUpgradeCurrentLevel == 2)
                                {
                                    TownHallFieldSprite.sprite = TownHallSpriteUpgradeCollection[VillageTownHallUpgradeCurrentLevel];
                                    TownHallUIImage.sprite = TownHallSpriteUpgradeCollection[7];
                                }
                                else if (VillageTownHallUpgradeCurrentLevel == 3)
                                {
                                    TownHallFieldSprite.sprite = TownHallSpriteUpgradeCollection[VillageTownHallUpgradeCurrentLevel];
                                    TownHallUIImage.sprite = TownHallSpriteUpgradeCollection[8];
                                }
                                else if (VillageTownHallUpgradeCurrentLevel == 4)
                                {
                                    TownHallFieldSprite.sprite = TownHallSpriteUpgradeCollection[VillageTownHallUpgradeCurrentLevel];
                                    TownHallUIImage.sprite = TownHallSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    TownHallUIImage.sprite = TownHallSpriteUpgradeCollection[VillageTownHallUpgradeCurrentLevel];
                                }
                                TownHallFieldSprite.sprite = TownHallSpriteUpgradeCollection[VillageTownHallUpgradeCurrentLevel];
                               // TownHallUIImage.sprite = TownHallSpriteUpgradeCollection[VillageTownHallUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageTownHallUpgradeNextLevel[VillageTownHallUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageTownHallUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageTownHallUpgradeCurrentLevel < TempCount)
                                {
                                    VillageTownHallUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageTownHallUpgradeCurrentLevel >= TempCount)
                                {
                                    TownHallFieldSprite.sprite = TownHallSpriteUpgradeCollection[VillageTownHallUpgradeCurrentLevel];
                                    TownHallUIImage.sprite = TownHallSpriteUpgradeCollection[VillageTownHallUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageTownHallUpgradeNextLevel[VillageTownHallUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = TownHallSpriteUpgradeCollection[VillageTownHallUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.MAGESGUILD)
                        {
                            if (VillageUpgradeLevel[i] <= VillageMagesGuildUpgradeNextLevel[VillageMagesGuildUpgradeCurrentLevel])
                            {
                                if (VillageMagesGuildUpgradeCurrentLevel == 0)
                                {
                                    MagesGuildFieldSprite.sprite = MagesGuildSpriteUpgradeCollection[VillageMagesGuildUpgradeCurrentLevel];
                                    MagesGuildUIImage.sprite = MagesGuildSpriteUpgradeCollection[5];
                                }
                                else if (VillageMagesGuildUpgradeCurrentLevel == 1)
                                {
                                    MagesGuildFieldSprite.sprite = MagesGuildSpriteUpgradeCollection[VillageMagesGuildUpgradeCurrentLevel];
                                    MagesGuildUIImage.sprite = MagesGuildSpriteUpgradeCollection[6];
                                }
                                else if (VillageMagesGuildUpgradeCurrentLevel == 2)
                                {
                                    MagesGuildFieldSprite.sprite = MagesGuildSpriteUpgradeCollection[VillageMagesGuildUpgradeCurrentLevel];
                                    MagesGuildUIImage.sprite = MagesGuildSpriteUpgradeCollection[7];
                                }
                                else if (VillageMagesGuildUpgradeCurrentLevel == 3)
                                {
                                    MagesGuildFieldSprite.sprite = MagesGuildSpriteUpgradeCollection[VillageMagesGuildUpgradeCurrentLevel];
                                    MagesGuildUIImage.sprite = MagesGuildSpriteUpgradeCollection[8];
                                }
                                else if (VillageMagesGuildUpgradeCurrentLevel == 4)
                                {
                                    MagesGuildFieldSprite.sprite = MagesGuildSpriteUpgradeCollection[VillageMagesGuildUpgradeCurrentLevel];
                                    MagesGuildUIImage.sprite = MagesGuildSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    MagesGuildUIImage.sprite = MagesGuildSpriteUpgradeCollection[VillageMagesGuildUpgradeCurrentLevel];
                                }
                              
                                MagesGuildFieldSprite.sprite = MagesGuildSpriteUpgradeCollection[VillageMagesGuildUpgradeCurrentLevel];
                               // MagesGuildUIImage.sprite = MagesGuildSpriteUpgradeCollection[VillageMagesGuildUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageMagesGuildUpgradeNextLevel[VillageMagesGuildUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageMagesGuildUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageMagesGuildUpgradeCurrentLevel < TempCount)
                                {
                                    VillageMagesGuildUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageMagesGuildUpgradeCurrentLevel >= TempCount)
                                {
                                    MagesGuildFieldSprite.sprite = MagesGuildSpriteUpgradeCollection[VillageMagesGuildUpgradeCurrentLevel];
                                    MagesGuildUIImage.sprite = MagesGuildSpriteUpgradeCollection[VillageMagesGuildUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageMagesGuildUpgradeNextLevel[VillageMagesGuildUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = MagesGuildSpriteUpgradeCollection[VillageMagesGuildUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.BUTCHER)
                        {
                            if (VillageUpgradeLevel[i] <= VillageButcherUpgradeNextLevel[VillageButcherUpgradeCurrentLevel])
                            {
                                if (VillageButcherUpgradeCurrentLevel == 0)
                                {
                                    ButcherFieldSprite.sprite = ButcherSpriteUpgradeCollection[VillageButcherUpgradeCurrentLevel];
                                    ButcherUIImage.sprite = ButcherSpriteUpgradeCollection[5];
                                }
                                else if (VillageButcherUpgradeCurrentLevel == 1)
                                {
                                    ButcherFieldSprite.sprite = ButcherSpriteUpgradeCollection[VillageButcherUpgradeCurrentLevel];
                                    ButcherUIImage.sprite = ButcherSpriteUpgradeCollection[6];
                                }
                                else if (VillageButcherUpgradeCurrentLevel == 2)
                                {
                                    ButcherFieldSprite.sprite = ButcherSpriteUpgradeCollection[VillageButcherUpgradeCurrentLevel];
                                    ButcherUIImage.sprite = ButcherSpriteUpgradeCollection[7];
                                }
                                else if (VillageButcherUpgradeCurrentLevel == 3)
                                {
                                    ButcherFieldSprite.sprite = ButcherSpriteUpgradeCollection[VillageButcherUpgradeCurrentLevel];
                                    ButcherUIImage.sprite = ButcherSpriteUpgradeCollection[8];
                                }
                                else if (VillageButcherUpgradeCurrentLevel == 4)
                                {
                                    ButcherFieldSprite.sprite = ButcherSpriteUpgradeCollection[VillageButcherUpgradeCurrentLevel];
                                    ButcherUIImage.sprite = ButcherSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    ButcherUIImage.sprite = ButcherSpriteUpgradeCollection[VillageButcherUpgradeCurrentLevel];
                                }

                                ButcherFieldSprite.sprite = ButcherSpriteUpgradeCollection[VillageButcherUpgradeCurrentLevel];
                                // MagesGuildUIImage.sprite = MagesGuildSpriteUpgradeCollection[VillageMagesGuildUpgradeCurrentLevel];

                            }

                            if (VillageUpgradeLevel[i] > VillageButcherUpgradeNextLevel[VillageButcherUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageButcherUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageButcherUpgradeCurrentLevel < TempCount)
                                {
                                    VillageButcherUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageButcherUpgradeCurrentLevel >= TempCount)
                                {
                                    ButcherFieldSprite.sprite = ButcherSpriteUpgradeCollection[VillageButcherUpgradeCurrentLevel];
                                    ButcherUIImage.sprite = ButcherSpriteUpgradeCollection[VillageButcherUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageButcherUpgradeNextLevel[VillageButcherUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = ButcherSpriteUpgradeCollection[VillageButcherUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.ALCHEMIST)
                        {
                            if (VillageUpgradeLevel[i] <= VillageAlchemistUpgradeNextLevel[VillageAlchemistUpgradeCurrentLevel])
                            {
                                if (VillageAlchemistUpgradeCurrentLevel == 0)
                                {
                                    AlchemistFieldSprite.sprite = AlchemistSpriteUpgradeCollection[VillageAlchemistUpgradeCurrentLevel];
                                    AlchemistUIImage.sprite = AlchemistSpriteUpgradeCollection[5];
                                }
                                else if (VillageAlchemistUpgradeCurrentLevel == 1)
                                {
                                    AlchemistFieldSprite.sprite = AlchemistSpriteUpgradeCollection[VillageAlchemistUpgradeCurrentLevel];
                                    AlchemistUIImage.sprite = AlchemistSpriteUpgradeCollection[6];
                                }
                                else if (VillageAlchemistUpgradeCurrentLevel == 2)
                                {
                                    AlchemistFieldSprite.sprite = AlchemistSpriteUpgradeCollection[VillageAlchemistUpgradeCurrentLevel];
                                    AlchemistUIImage.sprite = AlchemistSpriteUpgradeCollection[7];
                                }
                                else if (VillageAlchemistUpgradeCurrentLevel == 3)
                                {
                                    AlchemistFieldSprite.sprite = AlchemistSpriteUpgradeCollection[VillageAlchemistUpgradeCurrentLevel];
                                    AlchemistUIImage.sprite = AlchemistSpriteUpgradeCollection[8];
                                }
                                else if (VillageAlchemistUpgradeCurrentLevel == 4)
                                {
                                    AlchemistFieldSprite.sprite = AlchemistSpriteUpgradeCollection[VillageAlchemistUpgradeCurrentLevel];
                                    AlchemistUIImage.sprite = AlchemistSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    AlchemistUIImage.sprite = AlchemistSpriteUpgradeCollection[VillageAlchemistUpgradeCurrentLevel];
                                }

                                AlchemistFieldSprite.sprite = AlchemistSpriteUpgradeCollection[VillageAlchemistUpgradeCurrentLevel];
                                //AlchemistUIImage.sprite = AlchemistSpriteUpgradeCollection[VillageAlchemistUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageAlchemistUpgradeNextLevel[VillageAlchemistUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageAlchemistUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageAlchemistUpgradeCurrentLevel < TempCount)
                                {
                                    VillageAlchemistUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageAlchemistUpgradeCurrentLevel >= TempCount)
                                {
                                    AlchemistFieldSprite.sprite = AlchemistSpriteUpgradeCollection[VillageAlchemistUpgradeCurrentLevel];
                                    AlchemistUIImage.sprite = AlchemistSpriteUpgradeCollection[VillageAlchemistUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageAlchemistUpgradeNextLevel[VillageAlchemistUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = AlchemistSpriteUpgradeCollection[VillageAlchemistUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.FISHERMANSHUT)
                        {
                            if (VillageUpgradeLevel[i] <= VillageFisherMansHutUpgradeNextLevel[VillageFisherMansHutUpgradeCurrentLevel])
                            {
                                if (VillageFisherMansHutUpgradeCurrentLevel == 0)
                                {
                                    FisherMansHutFieldSprite.sprite = FisherMansHutSpriteUpgradeCollection[VillageFisherMansHutUpgradeCurrentLevel];
                                    FisherMansHutUIImage.sprite = FisherMansHutSpriteUpgradeCollection[5];
                                }
                                else if (VillageFisherMansHutUpgradeCurrentLevel == 1)
                                {
                                    FisherMansHutFieldSprite.sprite = FisherMansHutSpriteUpgradeCollection[VillageFisherMansHutUpgradeCurrentLevel];
                                    FisherMansHutUIImage.sprite = FisherMansHutSpriteUpgradeCollection[6];
                                }
                                else if (VillageFisherMansHutUpgradeCurrentLevel == 2)
                                {
                                    FisherMansHutFieldSprite.sprite = FisherMansHutSpriteUpgradeCollection[VillageFisherMansHutUpgradeCurrentLevel];
                                    FisherMansHutUIImage.sprite = FisherMansHutSpriteUpgradeCollection[7];
                                }
                                else if (VillageFisherMansHutUpgradeCurrentLevel == 3)
                                {
                                    FisherMansHutFieldSprite.sprite = FisherMansHutSpriteUpgradeCollection[VillageFisherMansHutUpgradeCurrentLevel];
                                    FisherMansHutUIImage.sprite = FisherMansHutSpriteUpgradeCollection[8];
                                }
                                else if (VillageFisherMansHutUpgradeCurrentLevel == 4)
                                {
                                    FisherMansHutFieldSprite.sprite = FisherMansHutSpriteUpgradeCollection[VillageFisherMansHutUpgradeCurrentLevel];
                                    FisherMansHutUIImage.sprite = FisherMansHutSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    FisherMansHutUIImage.sprite = FisherMansHutSpriteUpgradeCollection[VillageFisherMansHutUpgradeCurrentLevel];
                                }
                                FisherMansHutFieldSprite.sprite = FisherMansHutSpriteUpgradeCollection[VillageFisherMansHutUpgradeCurrentLevel];
                               // FisherMansHutUIImage.sprite = FisherMansHutSpriteUpgradeCollection[VillageFisherMansHutUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageFisherMansHutUpgradeNextLevel[VillageFisherMansHutUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageFisherMansHutUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageFisherMansHutUpgradeCurrentLevel < TempCount)
                                {
                                    VillageFisherMansHutUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageFisherMansHutUpgradeCurrentLevel >= TempCount)
                                {
                                    FisherMansHutFieldSprite.sprite = FisherMansHutSpriteUpgradeCollection[VillageFisherMansHutUpgradeCurrentLevel];
                                    FisherMansHutUIImage.sprite = FisherMansHutSpriteUpgradeCollection[VillageFisherMansHutUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageFisherMansHutUpgradeNextLevel[VillageFisherMansHutUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = FisherMansHutSpriteUpgradeCollection[VillageFisherMansHutUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.HARBOR)
                        {
                            if (VillageUpgradeLevel[i] <= VillageHarborUpgradeNextLevel[VillageHarborUpgradeCurrentLevel])
                            {
                                if (VillageHarborUpgradeCurrentLevel == 0)
                                {
                                    HarborFieldSprite.sprite = HarborSpriteUpgradeCollection[VillageHarborUpgradeCurrentLevel];
                                    HarborUIImage.sprite = HarborSpriteUpgradeCollection[5];
                                }
                                else if (VillageHarborUpgradeCurrentLevel == 1)
                                {
                                    HarborFieldSprite.sprite = HarborSpriteUpgradeCollection[VillageHarborUpgradeCurrentLevel];
                                    HarborUIImage.sprite = HarborSpriteUpgradeCollection[6];
                                }
                                else if (VillageHarborUpgradeCurrentLevel == 2)
                                {
                                    HarborFieldSprite.sprite = HarborSpriteUpgradeCollection[VillageHarborUpgradeCurrentLevel];
                                    HarborUIImage.sprite = HarborSpriteUpgradeCollection[7];
                                }
                                else if (VillageHarborUpgradeCurrentLevel == 3)
                                {
                                    HarborFieldSprite.sprite = HarborSpriteUpgradeCollection[VillageHarborUpgradeCurrentLevel];
                                    HarborUIImage.sprite = HarborSpriteUpgradeCollection[8];
                                }
                                else if (VillageHarborUpgradeCurrentLevel == 4)
                                {
                                    HarborFieldSprite.sprite = HarborSpriteUpgradeCollection[VillageHarborUpgradeCurrentLevel];
                                    HarborUIImage.sprite = HarborSpriteUpgradeCollection[9];
                                }
                                else
                                    {
                                        HarborUIImage.sprite = HarborSpriteUpgradeCollection[VillageHarborUpgradeCurrentLevel];
                                    }
                                HarborFieldSprite.sprite = HarborSpriteUpgradeCollection[VillageHarborUpgradeCurrentLevel];
                                //HarborUIImage.sprite = HarborSpriteUpgradeCollection[VillageHarborUpgradeCurrentLevel];
                            }


                            if (VillageUpgradeLevel[i] > VillageHarborUpgradeNextLevel[VillageHarborUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageHarborUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageHarborUpgradeCurrentLevel < TempCount)
                                {
                                    VillageHarborUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageHarborUpgradeCurrentLevel >= TempCount)
                                {
                                    HarborFieldSprite.sprite = HarborSpriteUpgradeCollection[VillageHarborUpgradeCurrentLevel];
                                    HarborUIImage.sprite = HarborSpriteUpgradeCollection[VillageHarborUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageHarborUpgradeNextLevel[VillageHarborUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = HarborSpriteUpgradeCollection[VillageHarborUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.COWFARM)
                        {
                            if (VillageUpgradeLevel[i] <= VillageCowFarmUpgradeNextLevel[VillageCowFarmUpgradeCurrentLevel])
                            {
                                if (VillageCowFarmUpgradeCurrentLevel == 0)
                                {
                                    CowFarmFieldSprite.sprite = CowFarmSpriteUpgradeCollection[VillageCowFarmUpgradeCurrentLevel];
                                    CowFarmUIImage.sprite = CowFarmSpriteUpgradeCollection[5];
                                }
                                else if (VillageCowFarmUpgradeCurrentLevel == 1)
                                {
                                    CowFarmFieldSprite.sprite = CowFarmSpriteUpgradeCollection[VillageCowFarmUpgradeCurrentLevel];
                                    CowFarmUIImage.sprite = CowFarmSpriteUpgradeCollection[6];
                                }
                                else if (VillageCowFarmUpgradeCurrentLevel == 2)
                                {
                                    CowFarmFieldSprite.sprite = CowFarmSpriteUpgradeCollection[VillageCowFarmUpgradeCurrentLevel];
                                    CowFarmUIImage.sprite = CowFarmSpriteUpgradeCollection[7];
                                }
                                else if (VillageCowFarmUpgradeCurrentLevel == 3)
                                {
                                    CowFarmFieldSprite.sprite = CowFarmSpriteUpgradeCollection[VillageCowFarmUpgradeCurrentLevel];
                                    CowFarmUIImage.sprite = CowFarmSpriteUpgradeCollection[8];
                                }
                                else if (VillageCowFarmUpgradeCurrentLevel == 4)
                                {
                                    CowFarmFieldSprite.sprite = CowFarmSpriteUpgradeCollection[VillageCowFarmUpgradeCurrentLevel];
                                    CowFarmUIImage.sprite = CowFarmSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    CowFarmUIImage.sprite = CowFarmSpriteUpgradeCollection[VillageCowFarmUpgradeCurrentLevel];
                                }



                                CowFarmFieldSprite.sprite = CowFarmSpriteUpgradeCollection[VillageCowFarmUpgradeCurrentLevel];
                                //CowFarmUIImage.sprite = CowFarmSpriteUpgradeCollection[VillageCowFarmUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageCowFarmUpgradeNextLevel[VillageCowFarmUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageCowFarmUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageCowFarmUpgradeCurrentLevel < TempCount)
                                {
                                    VillageCowFarmUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageCowFarmUpgradeCurrentLevel >= TempCount)
                                {
                                    CowFarmFieldSprite.sprite = CowFarmSpriteUpgradeCollection[VillageCowFarmUpgradeCurrentLevel];
                                    CowFarmUIImage.sprite = CowFarmSpriteUpgradeCollection[VillageCowFarmUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageCowFarmUpgradeNextLevel[VillageCowFarmUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = CowFarmSpriteUpgradeCollection[VillageCowFarmUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.VOLCANO)
                        {
                            if (VillageUpgradeLevel[i] <= VillageVolcanoUpgradeNextLevel[VillageVolcanoUpgradeCurrentLevel])
                            {
                                if (VillageVolcanoUpgradeCurrentLevel == 0)
                                {
                                    VolcanoFieldSprite.sprite = VolcanoSpriteUpgradeCollection[VillageVolcanoUpgradeCurrentLevel];
                                    VolcanoUIImage.sprite = VolcanoSpriteUpgradeCollection[5];
                                }
                                else if (VillageVolcanoUpgradeCurrentLevel == 1)
                                {
                                    VolcanoFieldSprite.sprite = VolcanoSpriteUpgradeCollection[VillageVolcanoUpgradeCurrentLevel];
                                    VolcanoUIImage.sprite = VolcanoSpriteUpgradeCollection[6];
                                }
                                else if (VillageVolcanoUpgradeCurrentLevel == 2)
                                {
                                    VolcanoFieldSprite.sprite = VolcanoSpriteUpgradeCollection[VillageVolcanoUpgradeCurrentLevel];
                                    VolcanoUIImage.sprite = VolcanoSpriteUpgradeCollection[7];
                                }
                                else if (VillageVolcanoUpgradeCurrentLevel == 3)
                                {
                                    VolcanoFieldSprite.sprite = VolcanoSpriteUpgradeCollection[VillageVolcanoUpgradeCurrentLevel];
                                    VolcanoUIImage.sprite = VolcanoSpriteUpgradeCollection[8];
                                }
                                else if (VillageVolcanoUpgradeCurrentLevel == 4)
                                {
                                    VolcanoFieldSprite.sprite = VolcanoSpriteUpgradeCollection[VillageVolcanoUpgradeCurrentLevel];
                                    VolcanoUIImage.sprite = VolcanoSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    VolcanoUIImage.sprite = VolcanoSpriteUpgradeCollection[VillageVolcanoUpgradeCurrentLevel];
                                }

                                VolcanoFieldSprite.sprite = VolcanoSpriteUpgradeCollection[VillageVolcanoUpgradeCurrentLevel];
                                //VolcanoUIImage.sprite = VolcanoSpriteUpgradeCollection[VillageVolcanoUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageVolcanoUpgradeNextLevel[VillageVolcanoUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageVolcanoUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageVolcanoUpgradeCurrentLevel < TempCount)
                                {
                                    VillageVolcanoUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageVolcanoUpgradeCurrentLevel >= TempCount)
                                {
                                    VolcanoFieldSprite.sprite = VolcanoSpriteUpgradeCollection[VillageVolcanoUpgradeCurrentLevel];
                                    VolcanoUIImage.sprite = VolcanoSpriteUpgradeCollection[VillageVolcanoUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageVolcanoUpgradeNextLevel[VillageVolcanoUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = VolcanoSpriteUpgradeCollection[VillageVolcanoUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.OURSTATUE)
                        {
                            if (VillageUpgradeLevel[i] <= VillageOurStatueUpgradeNextLevel[VillageOurStatueUpgradeCurrentLevel])
                            {
                                if (VillageOurStatueUpgradeCurrentLevel == 0)
                                {
                                    OurStatueFieldSprite.sprite = OurStatueSpriteUpgradeCollection[VillageOurStatueUpgradeCurrentLevel];
                                    OurStatueUIImage.sprite = OurStatueSpriteUpgradeCollection[5];
                                }
                                else if (VillageOurStatueUpgradeCurrentLevel == 1)
                                {
                                    OurStatueFieldSprite.sprite = OurStatueSpriteUpgradeCollection[VillageOurStatueUpgradeCurrentLevel];
                                    OurStatueUIImage.sprite = OurStatueSpriteUpgradeCollection[6];
                                }
                                else if (VillageOurStatueUpgradeCurrentLevel == 2)
                                {
                                    OurStatueFieldSprite.sprite = OurStatueSpriteUpgradeCollection[VillageOurStatueUpgradeCurrentLevel];
                                    OurStatueUIImage.sprite = OurStatueSpriteUpgradeCollection[7];
                                }
                                else if (VillageOurStatueUpgradeCurrentLevel == 3)
                                {
                                    OurStatueFieldSprite.sprite = OurStatueSpriteUpgradeCollection[VillageOurStatueUpgradeCurrentLevel];
                                    OurStatueUIImage.sprite = OurStatueSpriteUpgradeCollection[8];
                                }
                                else if (VillageOurStatueUpgradeCurrentLevel == 4)
                                {
                                    OurStatueFieldSprite.sprite = OurStatueSpriteUpgradeCollection[VillageOurStatueUpgradeCurrentLevel];
                                    OurStatueUIImage.sprite = OurStatueSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    OurStatueUIImage.sprite = OurStatueSpriteUpgradeCollection[VillageOurStatueUpgradeCurrentLevel];
                                }

                                OurStatueFieldSprite.sprite = OurStatueSpriteUpgradeCollection[VillageOurStatueUpgradeCurrentLevel];
                              //  OurStatueUIImage.sprite = OurStatueSpriteUpgradeCollection[VillageOurStatueUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageOurStatueUpgradeNextLevel[VillageOurStatueUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageOurStatueUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageOurStatueUpgradeCurrentLevel < TempCount)
                                {
                                    VillageOurStatueUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageOurStatueUpgradeCurrentLevel >= TempCount)
                                {
                                    OurStatueFieldSprite.sprite = OurStatueSpriteUpgradeCollection[VillageOurStatueUpgradeCurrentLevel];
                                    OurStatueUIImage.sprite = OurStatueSpriteUpgradeCollection[VillageOurStatueUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageOurStatueUpgradeNextLevel[VillageOurStatueUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = OurStatueSpriteUpgradeCollection[VillageOurStatueUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.HOLEINGROUND)
                        {
                            if (VillageUpgradeLevel[i] <= VillageHoleInGroundUpgradeNextLevel[VillageHoleInGroundUpgradeCurrentLevel])
                            {
                                if (VillageHoleInGroundUpgradeCurrentLevel == 0)
                                {
                                    HoleInGroundFieldSprite.sprite = HoleInGroundSpriteUpgradeCollection[VillageHoleInGroundUpgradeCurrentLevel];
                                    HoleInGroundUIImage.sprite = HoleInGroundSpriteUpgradeCollection[5];
                                }
                                else if (VillageHoleInGroundUpgradeCurrentLevel == 1)
                                {
                                    HoleInGroundFieldSprite.sprite = HoleInGroundSpriteUpgradeCollection[VillageHoleInGroundUpgradeCurrentLevel];
                                    HoleInGroundUIImage.sprite = HoleInGroundSpriteUpgradeCollection[6];
                                }
                                else if (VillageHoleInGroundUpgradeCurrentLevel == 2)
                                {
                                    HoleInGroundFieldSprite.sprite = HoleInGroundSpriteUpgradeCollection[VillageHoleInGroundUpgradeCurrentLevel];
                                    HoleInGroundUIImage.sprite = HoleInGroundSpriteUpgradeCollection[7];
                                }
                                else if (VillageHoleInGroundUpgradeCurrentLevel == 3)
                                {
                                    HoleInGroundFieldSprite.sprite = HoleInGroundSpriteUpgradeCollection[VillageHoleInGroundUpgradeCurrentLevel];
                                    HoleInGroundUIImage.sprite = HoleInGroundSpriteUpgradeCollection[8];
                                }
                                else if (VillageHoleInGroundUpgradeCurrentLevel == 4)
                                {
                                    HoleInGroundFieldSprite.sprite = HoleInGroundSpriteUpgradeCollection[VillageHoleInGroundUpgradeCurrentLevel];
                                    HoleInGroundUIImage.sprite = HoleInGroundSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    HoleInGroundUIImage.sprite = HoleInGroundSpriteUpgradeCollection[VillageHoleInGroundUpgradeCurrentLevel];
                                }

                                HoleInGroundFieldSprite.sprite = HoleInGroundSpriteUpgradeCollection[VillageHoleInGroundUpgradeCurrentLevel];
                               // HoleInGroundUIImage.sprite = HoleInGroundSpriteUpgradeCollection[VillageHoleInGroundUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageHoleInGroundUpgradeNextLevel[VillageHoleInGroundUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageHoleInGroundUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageHoleInGroundUpgradeCurrentLevel < TempCount)
                                {
                                    VillageHoleInGroundUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageHoleInGroundUpgradeCurrentLevel >= TempCount)
                                {
                                    HoleInGroundFieldSprite.sprite = HoleInGroundSpriteUpgradeCollection[VillageHoleInGroundUpgradeCurrentLevel];
                                    HoleInGroundUIImage.sprite = HoleInGroundSpriteUpgradeCollection[VillageHoleInGroundUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageHoleInGroundUpgradeNextLevel[VillageHoleInGroundUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = HoleInGroundSpriteUpgradeCollection[VillageHoleInGroundUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.COURT)
                        {
                            if (VillageUpgradeLevel[i] <= VillageCourtUpgradeNextLevel[VillageCourtUpgradeCurrentLevel])
                            {
                                if (VillageCourtUpgradeCurrentLevel == 0)
                                {
                                    CourtFieldSprite.sprite = CourtSpriteUpgradeCollection[VillageCourtUpgradeCurrentLevel];
                                    CourtUIImage.sprite = CourtSpriteUpgradeCollection[5];
                                }
                                else if (VillageCourtUpgradeCurrentLevel == 1)
                                {
                                    CourtFieldSprite.sprite = CourtSpriteUpgradeCollection[VillageCourtUpgradeCurrentLevel];
                                    CourtUIImage.sprite = CourtSpriteUpgradeCollection[6];
                                }
                                else if (VillageCourtUpgradeCurrentLevel == 2)
                                {
                                    CourtFieldSprite.sprite = CourtSpriteUpgradeCollection[VillageCourtUpgradeCurrentLevel];
                                    CourtUIImage.sprite = CourtSpriteUpgradeCollection[7];
                                }
                                else if (VillageCourtUpgradeCurrentLevel == 3)
                                {
                                    CourtFieldSprite.sprite = CourtSpriteUpgradeCollection[VillageCourtUpgradeCurrentLevel];
                                    CourtUIImage.sprite = CourtSpriteUpgradeCollection[8];
                                }
                                else if (VillageCourtUpgradeCurrentLevel == 4)
                                {
                                    CourtFieldSprite.sprite = CourtSpriteUpgradeCollection[VillageCourtUpgradeCurrentLevel];
                                    CourtUIImage.sprite = CourtSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    CourtUIImage.sprite = CourtSpriteUpgradeCollection[VillageCourtUpgradeCurrentLevel];
                                }

                                CourtFieldSprite.sprite = CourtSpriteUpgradeCollection[VillageCourtUpgradeCurrentLevel];
                              //  CourtUIImage.sprite = CourtSpriteUpgradeCollection[VillageCourtUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageCourtUpgradeNextLevel[VillageCourtUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageCourtUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageCourtUpgradeCurrentLevel < TempCount)
                                {
                                    VillageCourtUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageCourtUpgradeCurrentLevel >= TempCount)
                                {
                                    CourtFieldSprite.sprite = CourtSpriteUpgradeCollection[VillageCourtUpgradeCurrentLevel];
                                    CourtUIImage.sprite = CourtSpriteUpgradeCollection[VillageCourtUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageCourtUpgradeNextLevel[VillageCourtUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = CourtSpriteUpgradeCollection[VillageCourtUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.CENTERSQUARE)
                        {
                            if (VillageUpgradeLevel[i] <= VillageCenterSquareUpgradeNextLevel[VillageCenterSquareUpgradeCurrentLevel])
                            {
                                if (VillageCenterSquareUpgradeCurrentLevel == 0)
                                {
                                    CenterSquareFieldSprite.sprite = CenterSquareSpriteUpgradeCollection[VillageCenterSquareUpgradeCurrentLevel];
                                    CenterSquareUIImage.sprite = CenterSquareSpriteUpgradeCollection[5];
                                }
                                else if (VillageCenterSquareUpgradeCurrentLevel == 1)
                                {
                                    CenterSquareFieldSprite.sprite = CenterSquareSpriteUpgradeCollection[VillageCenterSquareUpgradeCurrentLevel];
                                    CenterSquareUIImage.sprite = CenterSquareSpriteUpgradeCollection[6];
                                }
                                else if (VillageCenterSquareUpgradeCurrentLevel == 2)
                                {
                                    CenterSquareFieldSprite.sprite = CenterSquareSpriteUpgradeCollection[VillageCenterSquareUpgradeCurrentLevel];
                                    CenterSquareUIImage.sprite = CenterSquareSpriteUpgradeCollection[7];
                                }
                                else if (VillageCenterSquareUpgradeCurrentLevel == 3)
                                {
                                    CenterSquareFieldSprite.sprite = CenterSquareSpriteUpgradeCollection[VillageCenterSquareUpgradeCurrentLevel];
                                    CenterSquareUIImage.sprite = CenterSquareSpriteUpgradeCollection[8];
                                }
                                else if (VillageCenterSquareUpgradeCurrentLevel == 4)
                                {
                                    CenterSquareFieldSprite.sprite = CenterSquareSpriteUpgradeCollection[VillageCenterSquareUpgradeCurrentLevel];
                                    CenterSquareUIImage.sprite = CenterSquareSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    CenterSquareUIImage.sprite = CenterSquareSpriteUpgradeCollection[VillageCenterSquareUpgradeCurrentLevel];
                                }

                                CenterSquareFieldSprite.sprite = CenterSquareSpriteUpgradeCollection[VillageCenterSquareUpgradeCurrentLevel];
                               // CenterSquareUIImage.sprite = CenterSquareSpriteUpgradeCollection[VillageCenterSquareUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageCenterSquareUpgradeNextLevel[VillageCenterSquareUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageCenterSquareUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageCenterSquareUpgradeCurrentLevel < TempCount)
                                {
                                    VillageCenterSquareUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageCenterSquareUpgradeCurrentLevel >= TempCount)
                                {
                                    CenterSquareFieldSprite.sprite = CenterSquareSpriteUpgradeCollection[VillageCenterSquareUpgradeCurrentLevel];
                                    CenterSquareUIImage.sprite = CenterSquareSpriteUpgradeCollection[VillageCenterSquareUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageCenterSquareUpgradeNextLevel[VillageCenterSquareUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = CenterSquareSpriteUpgradeCollection[VillageCenterSquareUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.TRAININGGROUND)
                        {
                            if (VillageUpgradeLevel[i] <= VillageTrainingGroundUpgradeNextLevel[VillageTrainingGroundUpgradeCurrentLevel])
                            {
                                if (VillageTrainingGroundUpgradeCurrentLevel == 0)
                                {
                                    TrainingGroundFieldSprite.sprite = TrainingGroundSpriteUpgradeCollection[VillageTrainingGroundUpgradeCurrentLevel];
                                    TrainingGroundUIImage.sprite = TrainingGroundSpriteUpgradeCollection[5];
                                }
                                else if (VillageTrainingGroundUpgradeCurrentLevel == 1)
                                {
                                    TrainingGroundFieldSprite.sprite = TrainingGroundSpriteUpgradeCollection[VillageTrainingGroundUpgradeCurrentLevel];
                                    TrainingGroundUIImage.sprite = TrainingGroundSpriteUpgradeCollection[6];
                                }
                                else if (VillageTrainingGroundUpgradeCurrentLevel == 2)
                                {
                                    TrainingGroundFieldSprite.sprite = TrainingGroundSpriteUpgradeCollection[VillageTrainingGroundUpgradeCurrentLevel];
                                    TrainingGroundUIImage.sprite = TrainingGroundSpriteUpgradeCollection[7];
                                }
                                else if (VillageTrainingGroundUpgradeCurrentLevel == 3)
                                {
                                    TrainingGroundFieldSprite.sprite = TrainingGroundSpriteUpgradeCollection[VillageTrainingGroundUpgradeCurrentLevel];
                                    TrainingGroundUIImage.sprite = TrainingGroundSpriteUpgradeCollection[8];
                                }
                                else if (VillageTrainingGroundUpgradeCurrentLevel == 4)
                                {
                                    TrainingGroundFieldSprite.sprite = TrainingGroundSpriteUpgradeCollection[VillageTrainingGroundUpgradeCurrentLevel];
                                    TrainingGroundUIImage.sprite = TrainingGroundSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    TrainingGroundUIImage.sprite = TrainingGroundSpriteUpgradeCollection[VillageTrainingGroundUpgradeCurrentLevel];
                                }
                                TrainingGroundFieldSprite.sprite = TrainingGroundSpriteUpgradeCollection[VillageTrainingGroundUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageTrainingGroundUpgradeNextLevel[VillageTrainingGroundUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageTrainingGroundUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageTrainingGroundUpgradeCurrentLevel < TempCount)
                                {
                                    VillageTrainingGroundUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageTrainingGroundUpgradeCurrentLevel >= TempCount)
                                {
                                    TrainingGroundFieldSprite.sprite = TrainingGroundSpriteUpgradeCollection[VillageTrainingGroundUpgradeCurrentLevel];
                                    TrainingGroundUIImage.sprite = TrainingGroundSpriteUpgradeCollection[VillageTrainingGroundUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageTrainingGroundUpgradeNextLevel[VillageTrainingGroundUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = TrainingGroundSpriteUpgradeCollection[VillageTrainingGroundUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.PLAINCOTTAGE)
                        {
                            if (VillageUpgradeLevel[i] <= VillagePlainCottageUpgradeNextLevel[VillagePlainCottageUpgradeCurrentLevel])
                            {
                                if (VillagePlainCottageUpgradeCurrentLevel < 1)
                                {
                                    PlainCottageFieldSprite.sprite = PlainCottageSpriteUpgradeCollection[VillagePlainCottageUpgradeCurrentLevel];
                                    PlainCottageUIImage.sprite = PlainCottageSpriteUpgradeCollection[5];
                                }
                                else if (VillagePlainCottageUpgradeCurrentLevel < 2)
                                {
                                    PlainCottageFieldSprite.sprite = PlainCottageSpriteUpgradeCollection[VillagePlainCottageUpgradeCurrentLevel];
                                    PlainCottageUIImage.sprite = PlainCottageSpriteUpgradeCollection[6];
                                }
                                else if (VillagePlainCottageUpgradeCurrentLevel < 3)
                                {
                                    PlainCottageFieldSprite.sprite = PlainCottageSpriteUpgradeCollection[VillagePlainCottageUpgradeCurrentLevel];
                                    PlainCottageUIImage.sprite = PlainCottageSpriteUpgradeCollection[7];
                                }
                                else if (VillagePlainCottageUpgradeCurrentLevel < 4)
                                {
                                    PlainCottageFieldSprite.sprite = PlainCottageSpriteUpgradeCollection[VillagePlainCottageUpgradeCurrentLevel];
                                    PlainCottageUIImage.sprite = PlainCottageSpriteUpgradeCollection[8];
                                }
                                else if (VillagePlainCottageUpgradeCurrentLevel < 5)
                                {
                                    PlainCottageFieldSprite.sprite = PlainCottageSpriteUpgradeCollection[VillagePlainCottageUpgradeCurrentLevel];
                                    PlainCottageUIImage.sprite = PlainCottageSpriteUpgradeCollection[9];
                                }
                                PlainCottageFieldSprite.sprite = PlainCottageSpriteUpgradeCollection[VillagePlainCottageUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillagePlainCottageUpgradeNextLevel[VillagePlainCottageUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillagePlainCottageUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillagePlainCottageUpgradeCurrentLevel < TempCount)
                                {
                                    VillagePlainCottageUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillagePlainCottageUpgradeCurrentLevel >= TempCount)
                                {
                                    PlainCottageFieldSprite.sprite = PlainCottageSpriteUpgradeCollection[VillagePlainCottageUpgradeCurrentLevel];
                                    PlainCottageUIImage.sprite = PlainCottageSpriteUpgradeCollection[VillagePlainCottageUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillagePlainCottageUpgradeNextLevel[VillagePlainCottageUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = PlainCottageSpriteUpgradeCollection[VillagePlainCottageUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.CHURCH)
                        {
                            if (VillageUpgradeLevel[i] <= VillageChurchUpgradeNextLevel[VillageChurchUpgradeCurrentLevel])
                            {
                                if (VillageChurchUpgradeCurrentLevel == 0)
                                {
                                    ChurchFieldSprite.sprite = ChurchSpriteUpgradeCollection[VillageChurchUpgradeCurrentLevel];
                                    ChurchUIImage.sprite = ChurchSpriteUpgradeCollection[5];
                                }
                                else if (VillageChurchUpgradeCurrentLevel == 1)
                                {
                                    ChurchFieldSprite.sprite = ChurchSpriteUpgradeCollection[VillageChurchUpgradeCurrentLevel];
                                    ChurchUIImage.sprite = ChurchSpriteUpgradeCollection[6];
                                }
                                else if (VillageChurchUpgradeCurrentLevel == 2)
                                {
                                    ChurchFieldSprite.sprite = ChurchSpriteUpgradeCollection[VillageChurchUpgradeCurrentLevel];
                                    ChurchUIImage.sprite = ChurchSpriteUpgradeCollection[7];
                                }
                                else if (VillageChurchUpgradeCurrentLevel == 3)
                                {
                                    ChurchFieldSprite.sprite = ChurchSpriteUpgradeCollection[VillageChurchUpgradeCurrentLevel];
                                    ChurchUIImage.sprite = ChurchSpriteUpgradeCollection[8];
                                }
                                else if (VillageChurchUpgradeCurrentLevel == 4)
                                {
                                    ChurchFieldSprite.sprite = ChurchSpriteUpgradeCollection[VillageChurchUpgradeCurrentLevel];
                                    ChurchUIImage.sprite = ChurchSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    ChurchUIImage.sprite = ChurchSpriteUpgradeCollection[VillageChurchUpgradeCurrentLevel];
                                }
                                ChurchFieldSprite.sprite = ChurchSpriteUpgradeCollection[VillageChurchUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageChurchUpgradeNextLevel[VillageChurchUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageChurchUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageChurchUpgradeCurrentLevel < TempCount)
                                {
                                    VillageChurchUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageChurchUpgradeCurrentLevel >= TempCount)
                                {
                                    ChurchFieldSprite.sprite = ChurchSpriteUpgradeCollection[VillageChurchUpgradeCurrentLevel];
                                    ChurchUIImage.sprite = ChurchSpriteUpgradeCollection[VillageChurchUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageChurchUpgradeNextLevel[VillageChurchUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = ChurchSpriteUpgradeCollection[VillageChurchUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.ALTAR)
                        {
                            if (VillageUpgradeLevel[i] <= VillageAltarUpgradeNextLevel[VillageAltarUpgradeCurrentLevel])
                            {
                                if (VillageAltarUpgradeCurrentLevel == 0)
                                {
                                    AltarFieldSprite.sprite = AltarSpriteUpgradeCollection[VillageAltarUpgradeCurrentLevel];
                                    AltarUIImage.sprite = AltarSpriteUpgradeCollection[5];
                                }
                                else if (VillageAltarUpgradeCurrentLevel == 1)
                                {
                                    AltarFieldSprite.sprite = AltarSpriteUpgradeCollection[VillageAltarUpgradeCurrentLevel];
                                    AltarUIImage.sprite = AltarSpriteUpgradeCollection[6];
                                }
                                else if (VillageAltarUpgradeCurrentLevel == 2)
                                {
                                    AltarFieldSprite.sprite = AltarSpriteUpgradeCollection[VillageAltarUpgradeCurrentLevel];
                                    AltarUIImage.sprite = AltarSpriteUpgradeCollection[7];
                                }
                                else if (VillageAltarUpgradeCurrentLevel == 3)
                                {
                                    AltarFieldSprite.sprite = AltarSpriteUpgradeCollection[VillageAltarUpgradeCurrentLevel];
                                    AltarUIImage.sprite = AltarSpriteUpgradeCollection[8];
                                }
                                else if (VillageAltarUpgradeCurrentLevel == 4)
                                {
                                    AltarFieldSprite.sprite = AltarSpriteUpgradeCollection[VillageAltarUpgradeCurrentLevel];
                                    AltarUIImage.sprite = AltarSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    AltarUIImage.sprite = AltarSpriteUpgradeCollection[VillageAltarUpgradeCurrentLevel];
                                }
                                AltarFieldSprite.sprite = AltarSpriteUpgradeCollection[VillageAltarUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageAltarUpgradeNextLevel[VillageAltarUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageAltarUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageAltarUpgradeCurrentLevel < TempCount)
                                {
                                    VillageAltarUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageAltarUpgradeCurrentLevel >= TempCount)
                                {
                                    AltarFieldSprite.sprite = AltarSpriteUpgradeCollection[VillageAltarUpgradeCurrentLevel];
                                    AltarUIImage.sprite = AltarSpriteUpgradeCollection[VillageAltarUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageAltarUpgradeNextLevel[VillageAltarUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = ChurchSpriteUpgradeCollection[VillageChurchUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.CHARIOTPARKING)
                        {
                            if (VillageUpgradeLevel[i] <= VillageChariotParkingUpgradeNextLevel[VillageChariotParkingUpgradeCurrentLevel])
                            {
                                if (VillageChariotParkingUpgradeCurrentLevel == 0)
                                {
                                    ChariotParkingFieldSprite.sprite = ChariotParkingSpriteUpgradeCollection[VillageChariotParkingUpgradeCurrentLevel];
                                    ChariotParkingUIImage.sprite = ChariotParkingSpriteUpgradeCollection[5];
                                }
                                else if (VillageChariotParkingUpgradeCurrentLevel == 1)
                                {
                                    ChariotParkingFieldSprite.sprite = ChariotParkingSpriteUpgradeCollection[VillageChariotParkingUpgradeCurrentLevel];
                                    ChariotParkingUIImage.sprite = ChariotParkingSpriteUpgradeCollection[6];
                                }
                                else if (VillageChariotParkingUpgradeCurrentLevel == 2)
                                {
                                    ChariotParkingFieldSprite.sprite = ChariotParkingSpriteUpgradeCollection[VillageChariotParkingUpgradeCurrentLevel];
                                    ChariotParkingUIImage.sprite = ChariotParkingSpriteUpgradeCollection[7];
                                }
                                else if (VillageChariotParkingUpgradeCurrentLevel == 3)
                                {
                                    ChariotParkingFieldSprite.sprite = ChariotParkingSpriteUpgradeCollection[VillageChariotParkingUpgradeCurrentLevel];
                                    ChariotParkingUIImage.sprite = ChariotParkingSpriteUpgradeCollection[8];
                                }
                                else if (VillageChariotParkingUpgradeCurrentLevel == 4)
                                {
                                    ChariotParkingFieldSprite.sprite = ChariotParkingSpriteUpgradeCollection[VillageChariotParkingUpgradeCurrentLevel];
                                    ChariotParkingUIImage.sprite = ChariotParkingSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    ChariotParkingUIImage.sprite = ChariotParkingSpriteUpgradeCollection[VillageChariotParkingUpgradeCurrentLevel];
                                }
                                ChariotParkingFieldSprite.sprite = ChariotParkingSpriteUpgradeCollection[VillageChariotParkingUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageChariotParkingUpgradeNextLevel[VillageChariotParkingUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageChariotParkingUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageChariotParkingUpgradeCurrentLevel < TempCount)
                                {
                                    VillageChariotParkingUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageChariotParkingUpgradeCurrentLevel >= TempCount)
                                {
                                    ChariotParkingFieldSprite.sprite = ChariotParkingSpriteUpgradeCollection[VillageChariotParkingUpgradeCurrentLevel];
                                    ChariotParkingUIImage.sprite = ChariotParkingSpriteUpgradeCollection[VillageChariotParkingUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageChariotParkingUpgradeNextLevel[VillageChariotParkingUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = ChariotParkingSpriteUpgradeCollection[VillageChariotParkingUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.SHIP)
                        {
                            if (VillageUpgradeLevel[i] <= VillageShipUpgradeNextLevel[VillageShipUpgradeCurrentLevel])
                            {
                                if (VillageShipUpgradeCurrentLevel == 0)
                                {
                                    ShipFieldSprite.sprite = ShipSpriteUpgradeCollection[VillageShipUpgradeCurrentLevel];
                                    ShipUIImage.sprite = ShipSpriteUpgradeCollection[5];
                                }
                                else if (VillageShipUpgradeCurrentLevel == 1)
                                {
                                    ShipFieldSprite.sprite = ShipSpriteUpgradeCollection[VillageShipUpgradeCurrentLevel];
                                    ShipUIImage.sprite = ShipSpriteUpgradeCollection[6];
                                }
                                else if (VillageShipUpgradeCurrentLevel == 2)
                                {
                                    ShipFieldSprite.sprite = ShipSpriteUpgradeCollection[VillageShipUpgradeCurrentLevel];
                                    ShipUIImage.sprite = ShipSpriteUpgradeCollection[7];
                                }
                                else if (VillageShipUpgradeCurrentLevel == 3)
                                {
                                    ShipFieldSprite.sprite = ShipSpriteUpgradeCollection[VillageShipUpgradeCurrentLevel];
                                    ShipUIImage.sprite = ShipSpriteUpgradeCollection[8];
                                }
                                else if (VillageShipUpgradeCurrentLevel == 4)
                                {
                                    ShipFieldSprite.sprite = ShipSpriteUpgradeCollection[VillageShipUpgradeCurrentLevel];
                                    ShipUIImage.sprite = ShipSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    ShipUIImage.sprite = ShipSpriteUpgradeCollection[VillageShipUpgradeCurrentLevel];
                                }
                                ShipFieldSprite.sprite = ShipSpriteUpgradeCollection[VillageShipUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageShipUpgradeNextLevel[VillageShipUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageShipUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageShipUpgradeCurrentLevel < TempCount)
                                {
                                    VillageShipUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageShipUpgradeCurrentLevel >= TempCount)
                                {
                                    ShipFieldSprite.sprite = ShipSpriteUpgradeCollection[VillageShipUpgradeCurrentLevel];
                                    ShipUIImage.sprite = ShipSpriteUpgradeCollection[VillageShipUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageShipUpgradeNextLevel[VillageShipUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = ShipSpriteUpgradeCollection[VillageShipUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.ARMORY)
                        {
                            if (VillageUpgradeLevel[i] <= VillageArmoryUpgradeNextLevel[VillageArmoryUpgradeCurrentLevel])
                            {
                                if (VillageArmoryUpgradeCurrentLevel == 0)
                                {
                                    ArmoryFieldSprite.sprite = ArmorySpriteUpgradeCollection[VillageArmoryUpgradeCurrentLevel];
                                    ArmoryUIImage.sprite = ArmorySpriteUpgradeCollection[5];
                                }
                                else if (VillageArmoryUpgradeCurrentLevel == 1)
                                {
                                    ArmoryFieldSprite.sprite = ArmorySpriteUpgradeCollection[VillageArmoryUpgradeCurrentLevel];
                                    ArmoryUIImage.sprite = ArmorySpriteUpgradeCollection[6];
                                }
                                else if (VillageArmoryUpgradeCurrentLevel == 2)
                                {
                                    ArmoryFieldSprite.sprite = ArmorySpriteUpgradeCollection[VillageArmoryUpgradeCurrentLevel];
                                    ArmoryUIImage.sprite = ArmorySpriteUpgradeCollection[7];
                                }
                                else if (VillageArmoryUpgradeCurrentLevel == 3)
                                {
                                    ArmoryFieldSprite.sprite = ArmorySpriteUpgradeCollection[VillageArmoryUpgradeCurrentLevel];
                                    ArmoryUIImage.sprite = ArmorySpriteUpgradeCollection[8];
                                }
                                else if (VillageArmoryUpgradeCurrentLevel == 4)
                                {
                                    ArmoryFieldSprite.sprite = ArmorySpriteUpgradeCollection[VillageArmoryUpgradeCurrentLevel];
                                    ArmoryUIImage.sprite = ArmorySpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    ArmoryUIImage.sprite = ArmorySpriteUpgradeCollection[VillageArmoryUpgradeCurrentLevel];
                                }
                                ArmoryFieldSprite.sprite = ArmorySpriteUpgradeCollection[VillageArmoryUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageArmoryUpgradeNextLevel[VillageArmoryUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageArmoryUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageArmoryUpgradeCurrentLevel < TempCount)
                                {
                                    VillageArmoryUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageArmoryUpgradeCurrentLevel >= TempCount)
                                {
                                    ArmoryFieldSprite.sprite = ArmorySpriteUpgradeCollection[VillageArmoryUpgradeCurrentLevel];
                                    ArmoryUIImage.sprite = ArmorySpriteUpgradeCollection[VillageArmoryUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageArmoryUpgradeNextLevel[VillageArmoryUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = ArmorySpriteUpgradeCollection[VillageArmoryUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.CLINIC)
                        {
                            if (VillageUpgradeLevel[i] <= VillageClinicUpgradeNextLevel[VillageClinicUpgradeCurrentLevel])
                            {
                                if (VillageClinicUpgradeCurrentLevel == 0)
                                {
                                    ClinicFieldSprite.sprite = ClinicSpriteUpgradeCollection[VillageClinicUpgradeCurrentLevel];
                                    ClinicUIImage.sprite = ClinicSpriteUpgradeCollection[5];
                                }
                                else if (VillageClinicUpgradeCurrentLevel == 1)
                                {
                                    ClinicFieldSprite.sprite = ClinicSpriteUpgradeCollection[VillageClinicUpgradeCurrentLevel];
                                    ClinicUIImage.sprite = ClinicSpriteUpgradeCollection[6];
                                }
                                else if (VillageClinicUpgradeCurrentLevel == 2)
                                {
                                    ClinicFieldSprite.sprite = ClinicSpriteUpgradeCollection[VillageClinicUpgradeCurrentLevel];
                                    ClinicUIImage.sprite = ClinicSpriteUpgradeCollection[7];
                                }
                                else if (VillageClinicUpgradeCurrentLevel == 3)
                                {
                                    ClinicFieldSprite.sprite = ClinicSpriteUpgradeCollection[VillageClinicUpgradeCurrentLevel];
                                    ClinicUIImage.sprite = ClinicSpriteUpgradeCollection[8];
                                }
                                else if (VillageClinicUpgradeCurrentLevel == 4)
                                {
                                    ClinicFieldSprite.sprite = ClinicSpriteUpgradeCollection[VillageClinicUpgradeCurrentLevel];
                                    ClinicUIImage.sprite = ClinicSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    ClinicUIImage.sprite = ClinicSpriteUpgradeCollection[VillageClinicUpgradeCurrentLevel];
                                }
                                ClinicFieldSprite.sprite = ClinicSpriteUpgradeCollection[VillageClinicUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageClinicUpgradeNextLevel[VillageClinicUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageClinicUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageClinicUpgradeCurrentLevel < TempCount)
                                {
                                    VillageClinicUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageClinicUpgradeCurrentLevel >= TempCount)
                                {
                                    ClinicFieldSprite.sprite = ClinicSpriteUpgradeCollection[VillageClinicUpgradeCurrentLevel];
                                    ClinicUIImage.sprite = ClinicSpriteUpgradeCollection[VillageClinicUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageClinicUpgradeNextLevel[VillageClinicUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = ClinicSpriteUpgradeCollection[VillageClinicUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.IRONWORKS)
                        {
                            if (VillageUpgradeLevel[i] <= VillageIronWorksUpgradeNextLevel[VillageIronWorksUpgradeCurrentLevel])
                            {
                                if (VillageIronWorksUpgradeCurrentLevel == 0)
                                {
                                    IronWorksFieldSprite.sprite = IronWorksSpriteUpgradeCollection[VillageIronWorksUpgradeCurrentLevel];
                                    IronWorksUIImage.sprite = IronWorksSpriteUpgradeCollection[5];
                                }
                                else if (VillageIronWorksUpgradeCurrentLevel == 1)
                                {
                                    IronWorksFieldSprite.sprite = IronWorksSpriteUpgradeCollection[VillageIronWorksUpgradeCurrentLevel];
                                    IronWorksUIImage.sprite = IronWorksSpriteUpgradeCollection[6];
                                }
                                else if (VillageIronWorksUpgradeCurrentLevel == 2)
                                {
                                    IronWorksFieldSprite.sprite = IronWorksSpriteUpgradeCollection[VillageIronWorksUpgradeCurrentLevel];
                                    IronWorksUIImage.sprite = IronWorksSpriteUpgradeCollection[7];
                                }
                                else if (VillageIronWorksUpgradeCurrentLevel == 3)
                                {
                                    IronWorksFieldSprite.sprite = IronWorksSpriteUpgradeCollection[VillageIronWorksUpgradeCurrentLevel];
                                    IronWorksUIImage.sprite = IronWorksSpriteUpgradeCollection[8];
                                }
                                else if (VillageIronWorksUpgradeCurrentLevel == 4)
                                {
                                    IronWorksFieldSprite.sprite = IronWorksSpriteUpgradeCollection[VillageIronWorksUpgradeCurrentLevel];
                                    IronWorksUIImage.sprite = IronWorksSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    IronWorksUIImage.sprite = IronWorksSpriteUpgradeCollection[VillageIronWorksUpgradeCurrentLevel];
                                }
                                IronWorksFieldSprite.sprite = IronWorksSpriteUpgradeCollection[VillageIronWorksUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageIronWorksUpgradeNextLevel[VillageIronWorksUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageIronWorksUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageIronWorksUpgradeCurrentLevel < TempCount)
                                {
                                    VillageIronWorksUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageIronWorksUpgradeCurrentLevel >= TempCount)
                                {
                                    IronWorksFieldSprite.sprite = IronWorksSpriteUpgradeCollection[VillageIronWorksUpgradeCurrentLevel];
                                    IronWorksUIImage.sprite = IronWorksSpriteUpgradeCollection[VillageIronWorksUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageIronWorksUpgradeNextLevel[VillageIronWorksUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = IronWorksSpriteUpgradeCollection[VillageIronWorksUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.ARTIFACTSHOP)
                        {
                            if (VillageUpgradeLevel[i] <= VillageArtifactShopUpgradeNextLevel[VillageArtifactShopUpgradeCurrentLevel])
                            {
                                if (VillageArtifactShopUpgradeCurrentLevel == 0)
                                {
                                    ArtifactShopFieldSprite.sprite = ArtifactShopSpriteUpgradeCollection[VillageArtifactShopUpgradeCurrentLevel];
                                    ArtifactShopUIImage.sprite = ArtifactShopSpriteUpgradeCollection[5];
                                }
                                else if (VillageArtifactShopUpgradeCurrentLevel == 1)
                                {
                                    ArtifactShopFieldSprite.sprite = ArtifactShopSpriteUpgradeCollection[VillageArtifactShopUpgradeCurrentLevel];
                                    ArtifactShopUIImage.sprite = ArtifactShopSpriteUpgradeCollection[6];
                                }
                                else if (VillageArtifactShopUpgradeCurrentLevel == 2)
                                {
                                    ArtifactShopFieldSprite.sprite = ArtifactShopSpriteUpgradeCollection[VillageArtifactShopUpgradeCurrentLevel];
                                    ArtifactShopUIImage.sprite = ArtifactShopSpriteUpgradeCollection[7];
                                }
                                else if (VillageArtifactShopUpgradeCurrentLevel == 3)
                                {
                                    ArtifactShopFieldSprite.sprite = ArtifactShopSpriteUpgradeCollection[VillageArtifactShopUpgradeCurrentLevel];
                                    ArtifactShopUIImage.sprite = ArtifactShopSpriteUpgradeCollection[8];
                                }
                                else if (VillageArtifactShopUpgradeCurrentLevel == 4)
                                {
                                    ArtifactShopFieldSprite.sprite = ArtifactShopSpriteUpgradeCollection[VillageArtifactShopUpgradeCurrentLevel];
                                    ArtifactShopUIImage.sprite = ArtifactShopSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    ArtifactShopUIImage.sprite = ArtifactShopSpriteUpgradeCollection[VillageArtifactShopUpgradeCurrentLevel];
                                }
                                ArtifactShopFieldSprite.sprite = ArtifactShopSpriteUpgradeCollection[VillageArtifactShopUpgradeCurrentLevel];
                            }
                        

                            if (VillageUpgradeLevel[i] > VillageArtifactShopUpgradeNextLevel[VillageArtifactShopUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageArtifactShopUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageArtifactShopUpgradeCurrentLevel < TempCount)
                                {
                                    VillageArtifactShopUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageArtifactShopUpgradeCurrentLevel >= TempCount)
                                {
                                    ArtifactShopFieldSprite.sprite = ArtifactShopSpriteUpgradeCollection[VillageArtifactShopUpgradeCurrentLevel];
                                    ArtifactShopUIImage.sprite = ArtifactShopSpriteUpgradeCollection[VillageArtifactShopUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageArtifactShopUpgradeNextLevel[VillageArtifactShopUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = ArtifactShopSpriteUpgradeCollection[VillageArtifactShopUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.CEMETERY)
                        {
                            if (VillageUpgradeLevel[i] <= VillageCemeteryUpgradeNextLevel[VillageCemeteryUpgradeCurrentLevel])
                            {
                                if (VillageCemeteryUpgradeCurrentLevel == 0)
                                {
                                    CemeteryFieldSprite.sprite = CemeterySpriteUpgradeCollection[VillageCemeteryUpgradeCurrentLevel];
                                    CemeteryUIImage.sprite = CemeterySpriteUpgradeCollection[5];
                                }
                                else if (VillageCemeteryUpgradeCurrentLevel == 1)
                                {
                                    CemeteryFieldSprite.sprite = CemeterySpriteUpgradeCollection[VillageCemeteryUpgradeCurrentLevel];
                                    CemeteryUIImage.sprite = CemeterySpriteUpgradeCollection[6];
                                }
                                else if (VillageCemeteryUpgradeCurrentLevel == 2)
                                {
                                    CemeteryFieldSprite.sprite = CemeterySpriteUpgradeCollection[VillageCemeteryUpgradeCurrentLevel];
                                    CemeteryUIImage.sprite = CemeterySpriteUpgradeCollection[7];
                                }
                                else if (VillageCemeteryUpgradeCurrentLevel == 3)
                                {
                                    CemeteryFieldSprite.sprite = CemeterySpriteUpgradeCollection[VillageCemeteryUpgradeCurrentLevel];
                                    CemeteryUIImage.sprite = CemeterySpriteUpgradeCollection[8];
                                }
                                else if (VillageCemeteryUpgradeCurrentLevel == 4)
                                {
                                    CemeteryFieldSprite.sprite = CemeterySpriteUpgradeCollection[VillageCemeteryUpgradeCurrentLevel];
                                    CemeteryUIImage.sprite = CemeterySpriteUpgradeCollection[9];
                                }
                                else
                                {   
                                    CemeteryUIImage.sprite = CemeterySpriteUpgradeCollection[VillageCemeteryUpgradeCurrentLevel];
                                }
                                CemeteryFieldSprite.sprite = CemeterySpriteUpgradeCollection[VillageCemeteryUpgradeCurrentLevel];
                            }
                    

                            if (VillageUpgradeLevel[i] > VillageCemeteryUpgradeNextLevel[VillageCemeteryUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageCemeteryUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageCemeteryUpgradeCurrentLevel < TempCount)
                                {
                                    VillageCemeteryUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageCemeteryUpgradeCurrentLevel >= TempCount)
                                {
                                    CemeteryFieldSprite.sprite = CemeterySpriteUpgradeCollection[VillageCemeteryUpgradeCurrentLevel];
                                    CemeteryUIImage.sprite = CemeterySpriteUpgradeCollection[VillageCemeteryUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageCemeteryUpgradeNextLevel[VillageCemeteryUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = CemeterySpriteUpgradeCollection[VillageCemeteryUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.POWERSTATION)
                        {
                            if (VillageUpgradeLevel[i] <= VillagePowerStationUpgradeNextLevel[VillagePowerStationUpgradeCurrentLevel])
                            {
                                if (VillagePowerStationUpgradeCurrentLevel == 0)
                                {
                                    PowerStationFieldSprite.sprite = PowerStationSpriteUpgradeCollection[VillagePowerStationUpgradeCurrentLevel];
                                    PowerStationUIImage.sprite = PowerStationSpriteUpgradeCollection[5];
                                }
                                else if (VillagePowerStationUpgradeCurrentLevel == 1)
                                {
                                    PowerStationFieldSprite.sprite = PowerStationSpriteUpgradeCollection[VillagePowerStationUpgradeCurrentLevel];
                                    PowerStationUIImage.sprite = PowerStationSpriteUpgradeCollection[6];
                                }
                                else if (VillagePowerStationUpgradeCurrentLevel == 2)
                                {
                                    PowerStationFieldSprite.sprite = PowerStationSpriteUpgradeCollection[VillagePowerStationUpgradeCurrentLevel];
                                    PowerStationUIImage.sprite = PowerStationSpriteUpgradeCollection[7];
                                }
                                else if (VillagePowerStationUpgradeCurrentLevel == 3)
                                {
                                    PowerStationFieldSprite.sprite = PowerStationSpriteUpgradeCollection[VillagePowerStationUpgradeCurrentLevel];
                                    PowerStationUIImage.sprite = PowerStationSpriteUpgradeCollection[8];
                                }
                                else if (VillagePowerStationUpgradeCurrentLevel == 4)
                                {
                                    PowerStationFieldSprite.sprite = PowerStationSpriteUpgradeCollection[VillagePowerStationUpgradeCurrentLevel];
                                    PowerStationUIImage.sprite = PowerStationSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    PowerStationUIImage.sprite = PowerStationSpriteUpgradeCollection[VillagePowerStationUpgradeCurrentLevel];
                                }
                                PowerStationFieldSprite.sprite = PowerStationSpriteUpgradeCollection[VillagePowerStationUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillagePowerStationUpgradeNextLevel[VillagePowerStationUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillagePowerStationUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillagePowerStationUpgradeCurrentLevel < TempCount)
                                {
                                    VillagePowerStationUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillagePowerStationUpgradeCurrentLevel >= TempCount)
                                {
                                    PowerStationFieldSprite.sprite = PowerStationSpriteUpgradeCollection[VillagePowerStationUpgradeCurrentLevel];
                                    PowerStationUIImage.sprite = PowerStationSpriteUpgradeCollection[VillagePowerStationUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillagePowerStationUpgradeNextLevel[VillagePowerStationUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = PowerStationSpriteUpgradeCollection[VillagePowerStationUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.THIEVESGUILD)
                        {
                            if (VillageUpgradeLevel[i] <= VillageThievesGuildUpgradeNextLevel[VillageThievesGuildUpgradeCurrentLevel])
                            {
                                if (VillageThievesGuildUpgradeCurrentLevel == 0)
                                {
                                    ThievesGuildFieldSprite.sprite = ThievesGuildSpriteUpgradeCollection[VillageThievesGuildUpgradeCurrentLevel];
                                    ThievesGuildUIImage.sprite = ThievesGuildSpriteUpgradeCollection[5];
                                }
                                else if (VillageThievesGuildUpgradeCurrentLevel == 1)
                                {
                                    ThievesGuildFieldSprite.sprite = ThievesGuildSpriteUpgradeCollection[VillageThievesGuildUpgradeCurrentLevel];
                                    ThievesGuildUIImage.sprite = ThievesGuildSpriteUpgradeCollection[6];
                                }
                                else if (VillageThievesGuildUpgradeCurrentLevel == 2)
                                {
                                    ThievesGuildFieldSprite.sprite = ThievesGuildSpriteUpgradeCollection[VillageThievesGuildUpgradeCurrentLevel];
                                    ThievesGuildUIImage.sprite = ThievesGuildSpriteUpgradeCollection[7];
                                }
                                else if (VillageThievesGuildUpgradeCurrentLevel == 3)
                                {
                                    ThievesGuildFieldSprite.sprite = ThievesGuildSpriteUpgradeCollection[VillageThievesGuildUpgradeCurrentLevel];
                                    ThievesGuildUIImage.sprite = ThievesGuildSpriteUpgradeCollection[8];
                                }
                                else if (VillageThievesGuildUpgradeCurrentLevel == 4)
                                {
                                    ThievesGuildFieldSprite.sprite = ThievesGuildSpriteUpgradeCollection[VillageThievesGuildUpgradeCurrentLevel];
                                    ThievesGuildUIImage.sprite = ThievesGuildSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    ThievesGuildUIImage.sprite = ThievesGuildSpriteUpgradeCollection[VillageThievesGuildUpgradeCurrentLevel];
                                }
                                ThievesGuildFieldSprite.sprite = ThievesGuildSpriteUpgradeCollection[VillageThievesGuildUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageThievesGuildUpgradeNextLevel[VillageThievesGuildUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageThievesGuildUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageThievesGuildUpgradeCurrentLevel < TempCount)
                                {
                                    VillageThievesGuildUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageThievesGuildUpgradeCurrentLevel >= TempCount)
                                {
                                    ThievesGuildFieldSprite.sprite = ThievesGuildSpriteUpgradeCollection[VillageThievesGuildUpgradeCurrentLevel];
                                    ThievesGuildUIImage.sprite = ThievesGuildSpriteUpgradeCollection[VillageThievesGuildUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageThievesGuildUpgradeNextLevel[VillageThievesGuildUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = ThievesGuildSpriteUpgradeCollection[VillageThievesGuildUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.GREENHOUSE)
                        {
                            if (VillageUpgradeLevel[i] <= VillageGreenHouseUpgradeNextLevel[VillageGreenHouseUpgradeCurrentLevel])
                            {
                                if (VillageGreenHouseUpgradeCurrentLevel == 0)
                                {
                                    GreenHouseFieldSprite.sprite = GreenHouseSpriteUpgradeCollection[VillageGreenHouseUpgradeCurrentLevel];
                                    GreenHouseUIImage.sprite = GreenHouseSpriteUpgradeCollection[5];
                                }
                                else if (VillageGreenHouseUpgradeCurrentLevel == 1)
                                {
                                    GreenHouseFieldSprite.sprite = GreenHouseSpriteUpgradeCollection[VillageGreenHouseUpgradeCurrentLevel];
                                    GreenHouseUIImage.sprite = GreenHouseSpriteUpgradeCollection[6];
                                }
                                else if (VillageGreenHouseUpgradeCurrentLevel == 2)
                                {
                                    GreenHouseFieldSprite.sprite = GreenHouseSpriteUpgradeCollection[VillageGreenHouseUpgradeCurrentLevel];
                                    GreenHouseUIImage.sprite = GreenHouseSpriteUpgradeCollection[7];
                                }
                                else if (VillageGreenHouseUpgradeCurrentLevel == 3)
                                {
                                    GreenHouseFieldSprite.sprite = GreenHouseSpriteUpgradeCollection[VillageGreenHouseUpgradeCurrentLevel];
                                    GreenHouseUIImage.sprite = GreenHouseSpriteUpgradeCollection[8];
                                }
                                else if (VillageGreenHouseUpgradeCurrentLevel == 4)
                                {
                                    GreenHouseFieldSprite.sprite = GreenHouseSpriteUpgradeCollection[VillageGreenHouseUpgradeCurrentLevel];
                                    GreenHouseUIImage.sprite = GreenHouseSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    GreenHouseUIImage.sprite = GreenHouseSpriteUpgradeCollection[VillageGreenHouseUpgradeCurrentLevel];
                                }
                                GreenHouseFieldSprite.sprite = GreenHouseSpriteUpgradeCollection[VillageGreenHouseUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageGreenHouseUpgradeNextLevel[VillageGreenHouseUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageGreenHouseUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageGreenHouseUpgradeCurrentLevel < TempCount)
                                {
                                    VillageGreenHouseUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageGreenHouseUpgradeCurrentLevel >= TempCount)
                                {
                                    GreenHouseFieldSprite.sprite = GreenHouseSpriteUpgradeCollection[VillageGreenHouseUpgradeCurrentLevel];
                                    GreenHouseUIImage.sprite = GreenHouseSpriteUpgradeCollection[VillageGreenHouseUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageGreenHouseUpgradeNextLevel[VillageGreenHouseUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = GreenHouseSpriteUpgradeCollection[VillageGreenHouseUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.ROBOTFACTORY)
                        {
                            if (VillageUpgradeLevel[i] <= VillageRobotFactoryUpgradeNextLevel[VillageRobotFactoryUpgradeCurrentLevel])
                            {
                                if (VillageRobotFactoryUpgradeCurrentLevel == 0)
                                {
                                    RobotFactoryFieldSprite.sprite = RobotFactorySpriteUpgradeCollection[VillageRobotFactoryUpgradeCurrentLevel];
                                    RobotFactoryUIImage.sprite = RobotFactorySpriteUpgradeCollection[5];
                                }
                                else if (VillageRobotFactoryUpgradeCurrentLevel == 1)
                                {
                                    RobotFactoryFieldSprite.sprite = RobotFactorySpriteUpgradeCollection[VillageRobotFactoryUpgradeCurrentLevel];
                                    RobotFactoryUIImage.sprite = RobotFactorySpriteUpgradeCollection[6];
                                }
                                else if (VillageRobotFactoryUpgradeCurrentLevel == 2)
                                {
                                    RobotFactoryFieldSprite.sprite = RobotFactorySpriteUpgradeCollection[VillageRobotFactoryUpgradeCurrentLevel];
                                    RobotFactoryUIImage.sprite = RobotFactorySpriteUpgradeCollection[7];
                                }
                                else if (VillageRobotFactoryUpgradeCurrentLevel == 3)
                                {
                                    RobotFactoryFieldSprite.sprite = RobotFactorySpriteUpgradeCollection[VillageRobotFactoryUpgradeCurrentLevel];
                                    RobotFactoryUIImage.sprite = RobotFactorySpriteUpgradeCollection[8];
                                }
                                else if (VillageRobotFactoryUpgradeCurrentLevel == 4)
                                {
                                    RobotFactoryFieldSprite.sprite = RobotFactorySpriteUpgradeCollection[VillageRobotFactoryUpgradeCurrentLevel];
                                    RobotFactoryUIImage.sprite = RobotFactorySpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    RobotFactoryUIImage.sprite = RobotFactorySpriteUpgradeCollection[VillageRobotFactoryUpgradeCurrentLevel];
                                }
                                RobotFactoryFieldSprite.sprite = RobotFactorySpriteUpgradeCollection[VillageRobotFactoryUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageRobotFactoryUpgradeNextLevel[VillageRobotFactoryUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageRobotFactoryUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageRobotFactoryUpgradeCurrentLevel < TempCount)
                                {
                                    VillageRobotFactoryUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageRobotFactoryUpgradeCurrentLevel >= TempCount)
                                {
                                    RobotFactoryFieldSprite.sprite = RobotFactorySpriteUpgradeCollection[VillageRobotFactoryUpgradeCurrentLevel];
                                    RobotFactoryUIImage.sprite = RobotFactorySpriteUpgradeCollection[VillageRobotFactoryUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageRobotFactoryUpgradeNextLevel[VillageRobotFactoryUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = RobotFactorySpriteUpgradeCollection[VillageRobotFactoryUpgradeCurrentLevel];
                        }

                        if (VillageClassification[i] == VillageType.MONSTERZOO)
                        {
                            if (VillageUpgradeLevel[i] <= VillageMonsterZooUpgradeNextLevel[VillageMonsterZooUpgradeCurrentLevel])
                            {
                                if (VillageMonsterZooUpgradeCurrentLevel == 0)
                                {
                                    MonsterZooFieldSprite.sprite = MonsterZooSpriteUpgradeCollection[VillageMonsterZooUpgradeCurrentLevel];
                                    MonsterZooUIImage.sprite = MonsterZooSpriteUpgradeCollection[5];
                                }
                                else if (VillageMonsterZooUpgradeCurrentLevel == 1)
                                {
                                    MonsterZooFieldSprite.sprite = MonsterZooSpriteUpgradeCollection[VillageMonsterZooUpgradeCurrentLevel];
                                    MonsterZooUIImage.sprite = MonsterZooSpriteUpgradeCollection[6];
                                }
                                else if (VillageMonsterZooUpgradeCurrentLevel == 2)
                                {
                                    MonsterZooFieldSprite.sprite = MonsterZooSpriteUpgradeCollection[VillageMonsterZooUpgradeCurrentLevel];
                                    MonsterZooUIImage.sprite = MonsterZooSpriteUpgradeCollection[7];
                                }
                                else if (VillageMonsterZooUpgradeCurrentLevel == 3)
                                {
                                    MonsterZooFieldSprite.sprite = MonsterZooSpriteUpgradeCollection[VillageMonsterZooUpgradeCurrentLevel];
                                    MonsterZooUIImage.sprite = MonsterZooSpriteUpgradeCollection[8];
                                }
                                else if (VillageMonsterZooUpgradeCurrentLevel == 4)
                                {
                                    MonsterZooFieldSprite.sprite = MonsterZooSpriteUpgradeCollection[VillageMonsterZooUpgradeCurrentLevel];
                                    MonsterZooUIImage.sprite = MonsterZooSpriteUpgradeCollection[9];
                                }
                                else
                                {
                                    MonsterZooUIImage.sprite = MonsterZooSpriteUpgradeCollection[VillageMonsterZooUpgradeCurrentLevel];
                                }
                                MonsterZooFieldSprite.sprite = MonsterZooSpriteUpgradeCollection[VillageMonsterZooUpgradeCurrentLevel];
                            }

                            if (VillageUpgradeLevel[i] > VillageMonsterZooUpgradeNextLevel[VillageMonsterZooUpgradeCurrentLevel])
                            {
                                int TempCount = 0;
                                for (int a = 0; a < VillageMonsterZooUpgradeNextLevel.Length; a++)
                                {
                                    TempCount = a;
                                }

                                if (VillageMonsterZooUpgradeCurrentLevel < TempCount)
                                {
                                    VillageMonsterZooUpgradeCurrentLevel += 1;
                                    CheckVillageStructureManualUpdate();
                                }

                                if (VillageMonsterZooUpgradeCurrentLevel >= TempCount)
                                {
                                    MonsterZooFieldSprite.sprite = MonsterZooSpriteUpgradeCollection[VillageMonsterZooUpgradeCurrentLevel];
                                    MonsterZooUIImage.sprite = MonsterZooSpriteUpgradeCollection[VillageMonsterZooUpgradeCurrentLevel];
                                }

                            }

                            MarketNextMileStoneStatus[i] = VillageMonsterZooUpgradeNextLevel[VillageMonsterZooUpgradeCurrentLevel];

                            MarketSpriteStatus[i] = MonsterZooSpriteUpgradeCollection[VillageMonsterZooUpgradeCurrentLevel];
                        }

                    }
                }
              
            }

           //ACTIVATE PATHS BASED ON SELECTED BUILDINGS (If we have x and y and z buildings then show the path [0] etc.

           if (MarketPurchaseCountValueCollection[0] >= 1 || MarketPurchaseCountValueCollection[7] >= 1 || MarketPurchaseCountValueCollection[17] >= 1 || MarketPurchaseCountValueCollection[26] >= 1 || MarketPurchaseCountValueCollection[27] >= 1)
            {
                PathObjCollection[0].gameObject.SetActive(true);
            }

            if (MarketPurchaseCountValueCollection[10] >= 1 || MarketPurchaseCountValueCollection[11] >= 1)
            {
                PathObjCollection[1].gameObject.SetActive(true);
            }

            if (MarketPurchaseCountValueCollection[1] >= 1 || MarketPurchaseCountValueCollection[3] >= 1 || MarketPurchaseCountValueCollection[9] >= 1 || MarketPurchaseCountValueCollection[12] >= 1 || MarketPurchaseCountValueCollection[19] >= 1)
            {
                PathObjCollection[2].gameObject.SetActive(true);
            }

            if (MarketPurchaseCountValueCollection[28] >= 1)
            {
                PathObjCollection[3].gameObject.SetActive(true);
            }

            if (MarketPurchaseCountValueCollection[4] >= 1 || MarketPurchaseCountValueCollection[2] >= 1)
            {
                PathObjCollection[4].gameObject.SetActive(true);
            }

            if (MarketPurchaseCountValueCollection[5] >= 1 || MarketPurchaseCountValueCollection[15] >= 1 || MarketPurchaseCountValueCollection[16] >= 1 || MarketPurchaseCountValueCollection[20] >= 1 || MarketPurchaseCountValueCollection[22] >= 1 || MarketPurchaseCountValueCollection[23] >= 1)
            {
                PathObjCollection[5].gameObject.SetActive(true);
            }

            if (MarketPurchaseCountValueCollection[21] >= 1 || MarketPurchaseCountValueCollection[24] >= 1)
            {
                PathObjCollection[6].gameObject.SetActive(true);
            }

            MarketNextIncomeStatusManualUpdate();
        }

        void MarketNextIncomeStatusManualUpdate()
        {
            for (int i = 0; i < MarketNextIncomeStatus.Count; i++)
            {
                float TempIncome = MarketIncomeValueCollection[i] * 1.02f;

				if (TempIncome >= 999999999999999)
				{
					TempIncome = 999999999999999;
				}

                MarketNextIncomeStatus[i] = TempIncome;
            }
        }

        public void OfflineEarningFirstCheck()
        {

            if (MainBoost.ActivateOfflineEarningsCondition == false)
            {
                OfflineTimeValue = 0;
            }
            if (MainBoost.ActivateOfflineEarningsCondition == true)
            {
                float TempTotalOfflineEarningHoursValue = TotalOfflineEarningHoursValue;
                TotalOfflineEarningHoursValue -= OfflineTimeValue;

                for (int i = 0; i < MarketPurchaseCountValueCollection.Count; i++)
                {
                        //MainBoost.DoubleVillageIncomeCountdownTimerValue = TotalOfflineEarningHoursValue;
                       

                        //MainBoost.DoubleVillageIncomeCountdownTimerValue -= OfflineTimeValue;

                        if (TotalOfflineEarningHoursValue <= 0)
                        {
                            if (TempTotalOfflineEarningHoursValue >= MarketTimerCDValueCollection[i] && MarketPurchaseCountValueCollection[i] >= 1)
                            {
                                OfflineCountValue[i] = TempTotalOfflineEarningHoursValue / MarketTimerCDValueCollection[i];

                                OfflineCoinEarningsValue[i] = OfflineCountValue[i] * MarketIncomeValueCollection[i];
								
								if (OfflineCoinEarningsValue[i] >= 999999999999999)
								{
									OfflineCoinEarningsValue[i] = 999999999999999;
								}

							

								//TotalTimeOfflineValueCollection[i] = TotalOfflineEarningHoursValue / MarketTimerCDValueCollection[i];
								//TotalOfflineIncomeValue[i] = MarketIncomeValueCollection[i] * TotalTimeOfflineValueCollection[i];

							}
                            else
                            {
                                DisableOfflineEarnings();
                            }

                        }

                        if (TotalOfflineEarningHoursValue >= 1)
                        {
                            if (OfflineTimeValue >= MarketTimerCDValueCollection[i] && MarketPurchaseCountValueCollection[i] >= 1)
                            {
                                OfflineCountValue[i] = OfflineTimeValue / MarketTimerCDValueCollection[i];

                                OfflineCoinEarningsValue[i] = OfflineCountValue[i] * MarketIncomeValueCollection[i];

								if (OfflineCoinEarningsValue[i] >= 999999999999999)
								{
									OfflineCoinEarningsValue[i] = 999999999999999;
								}

							//TotalTimeOfflineValueCollection[i] = TotalOfflineEarningHoursValue / MarketTimerCDValueCollection[i];
							//TotalOfflineIncomeValue[i] = MarketIncomeValueCollection[i] * TotalTimeOfflineValueCollection[i];

							}
                        }
                    //foreach (float OCEV in OfflineCoinEarningsValue)
                    //{
                    //    TempOfflineTotalCoinEarningsValue += OCEV;
                    //}
                    OfflineTotalCoinEarningsValue += OfflineCoinEarningsValue[i];

					if (OfflineTotalCoinEarningsValue >= 99999999999999)
					{
						OfflineTotalCoinEarningsValue = 99999999999999;
					}


                }
            }
            if (OfflineTotalCoinEarningsValue >= 1)
            {
                OfflineCollectButtonObj.gameObject.SetActive(true);
                OfflineEarningsCheckUpdate();
                MainBoost.FirstCheck();
            }

            if (OfflineTotalCoinEarningsValue <= 0)
            {
                OfflineCollectButtonObj.gameObject.SetActive(false);
                MainBoost.FirstCheck();
            }

        }

        void DisableOfflineEarnings()
        {

            if (TotalOfflineEarningHoursValue <= 0)
            {
                TotalOfflineEarningHoursValue = 0;
                MainBoost.ActivateOfflineEarningsCondition = false;
            }
        }

        void OfflineEarningsCheckUpdate()
        {
			if (OfflineTotalCoinEarningsValue <= 999999999999999)
			{
				OfflineTotalCoinEarningsValue = 999999999999999;
			}
            if (OfflineTotalCoinEarningsValue <= 999)
            {
                //OfflineTotalCoinEarningsValueText.text = "" + Mathf.Round(OfflineTotalCoinEarningsValue).ToString();
                OfflineTotalCoinEarningsValueText.text = OfflineTotalCoinEarningsValue.ToString("F0");
            }

            if (OfflineTotalCoinEarningsValue >= 1000 && OfflineTotalCoinEarningsValue <= 999999)
            {
                //OfflineTotalCoinEarningsValueText.text = "" + Mathf.Round(OfflineTotalCoinEarningsValue / 1000).ToString() + "K";
                OfflineTotalCoinEarningsValueText.text = (OfflineTotalCoinEarningsValue / 1000).ToString("F2") + "K";
            }

            if (OfflineTotalCoinEarningsValue >= 1000000 && OfflineTotalCoinEarningsValue <= 999999999)
            {
                //OfflineTotalCoinEarningsValueText.text = "" + Mathf.Round(OfflineTotalCoinEarningsValue / 1000000).ToString() + "M";
                OfflineTotalCoinEarningsValueText.text = (OfflineTotalCoinEarningsValue / 1000000).ToString("F2") + "M";
            }

            if (OfflineTotalCoinEarningsValue >= 1000000000 && OfflineTotalCoinEarningsValue <= 999999999999)
            {
                //OfflineTotalCoinEarningsValueText.text = "" + Mathf.Round(OfflineTotalCoinEarningsValue / 1000000000).ToString() + "B";
                OfflineTotalCoinEarningsValueText.text = (OfflineTotalCoinEarningsValue / 1000000000).ToString("F2") + "B";
            }

			if (OfflineTotalCoinEarningsValue >= 1000000000000 && OfflineTotalCoinEarningsValue <= 999999999999999)
			{
				//OfflineTotalCoinEarningsValueText.text = "" + Mathf.Round(OfflineTotalCoinEarningsValue / 1000000000).ToString() + "B";
				OfflineTotalCoinEarningsValueText.text = (OfflineTotalCoinEarningsValue / 1000000000000).ToString("F2") + "T";
			}
		}

        public void OfflineCollectButton()
        {
            MainWallet.GoldWalletValue += OfflineTotalCoinEarningsValue;
            OfflineTotalCoinEarningsValue = 0;
            OfflineCollectButtonObj.gameObject.SetActive(false);
			MainWallet.AchievementEarnedGoldQuantityActivate();
			MainWallet.WalletValueManualUpdate();
        }

        void MarketCheckUpdate()
        {
            for (int i = 0; i < MarketPurchaseCountValueCollection.Count; i++)
            {
                if (MarketPurchaseCountValueCollection[i] >= 1)
                {
                    if (MarketPurchaseCountValueCollection[i] >= 10)
                    {
						if (MarketPurchaseCountValueCollection[i] < 999999999999999)
						{
                            //FIX DRYMARTI TIMER SET TO 0 AT THE END
                            if (TimerCountDownTemp[i] >= 0)
                            {
                                TimerCountDownTempText[i].text = "" + Mathf.Round(TimerCountDownTemp[i]);
                                TimerBarImage[i].fillAmount = TimerCountDownTemp[i] / MarketTimerCDValueCollection[i];
                            }
                            else if (TimerCountDownTemp[i] < 0)
                            {
                                TimerCountDownTempText[i].text = "COLLECT";
                                TimerBarImage[i].fillAmount = 1;
                            }

                           // TimerCountDownTempText[i].text = "" + Mathf.Round(TimerCountDownTemp[i]);
							//TimerBarImage[i].fillAmount = TimerCountDownTemp[i] / MarketTimerCDValueCollection[i];
							ManualCollectButton[i].gameObject.SetActive(false);


							if (TimerControl[i] == false)
							{
								TimerCountDownTemp[i] = MarketTimerCDValueCollection[i];
								TimerControl[i] = true;
							}

							TimerCountDownTemp[i] -= Time.deltaTime;

							if (TimerCountDownTemp[i] <= 0)
							{
								if (TimerControl[i] == true)
								{
									TimerCountDownTemp[i] = 0;
									if (MainBoost.ActivateDoubleVillageIncomeCondition == true)
									{
										float TempInventoryIncomeIncrease = MarketIncomeValueCollection[i] * InventoryVillageIncomeIncrease;

										float TempMarketIncomeValueCollection = (MarketIncomeValueCollection[i] + TempInventoryIncomeIncrease) * 2;
										float TempBoost = TempMarketIncomeValueCollection;
										MainWallet.GoldWalletValue += TempBoost;

									}
									if (MainBoost.ActivateDoubleVillageIncomeCondition == false)
									{
										float TempInventoryIncomeIncrease = MarketIncomeValueCollection[i] * InventoryVillageIncomeIncrease;
										float TempMarketIncomeValueCollection = MarketIncomeValueCollection[i] + TempInventoryIncomeIncrease;
										MainWallet.GoldWalletValue += TempMarketIncomeValueCollection + MainTalent.TotalAdditionalGoldValue + MainArtifact.GoldenSeedsTotalValue;
									}

									MainWallet.AchievementEarnedGoldQuantityActivate();
									MainWallet.WalletValueManualUpdate();

									TimerControl[i] = false;
								}
							}
						}

						else if (MarketPurchaseCountValueCollection[i] >= 999999999999999)
						{
							MarketPurchaseCountValueCollection[i] = 999999999999999;
							MarketIncomeValueCollection[i] = 999999999999999;
							MarketPurchaseCostCountValueCollection[i] = 999999999999999;
							TimerCountDownTemp[i] = 999;

							MarketPurchaseCostCountValueCollectionText[i].text = "MAX!!!";
							MarketIncomeValueTextCollection[i].text = "MAX!!!";
							TimerCountDownTempText[i].text = "MAX!!!";
							MarketPurchaseCostCountValueCollectionText[i].text = "MAX!!!";
							TimerBarImage[i].fillAmount = MarketTimerCDValueCollection[i] / MarketTimerCDValueCollection[i];
							ManualCollectButton[i].gameObject.SetActive(false);

							BuyButtonCollectionCollection[i].interactable = false;
						}

					}

					

					if (MarketPurchaseCountValueCollection[i] < 10)
                    {
                        //FIX DRYMARTI TIMER SET TO 0 AT THE END
                        if (TimerCountDownTemp[i] >= 0)
                        {
                            TimerCountDownTempText[i].text = "" + Mathf.Round(TimerCountDownTemp[i]);
                            TimerBarImage[i].fillAmount = TimerCountDownTemp[i] / MarketTimerCDValueCollection[i];
                        }
                        else if (TimerCountDownTemp[i] < 0)
                        {
                            TimerCountDownTempText[i].text = "COLLECT";
                            TimerBarImage[i].fillAmount = 1;
                        }
                       // TimerCountDownTempText[i].text = "" + Mathf.Round(TimerCountDownTemp[i]);
                       // TimerBarImage[i].fillAmount = TimerCountDownTemp[i] / MarketTimerCDValueCollection[i];

                        if (TimerControl[i] == false)
                        {
                            TimerCountDownTemp[i] = MarketTimerCDValueCollection[i];
                            TimerControl[i] = true;
                        }

                        TimerCountDownTemp[i] -= Time.deltaTime;

                        if (TimerCountDownTemp[i] <= 0)
                        {
                            if (TimerControl[i] == true)
                            {
                                TimerCountDownTempText[i].text = "COLLECT";
                                TimerBarImage[i].fillAmount = 1;
                                ManualCollectButton[i].gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
        }

        public void ManualCollectCoin(int ManualCollectCoinID)
        {
            TimerCountDownTemp[ManualCollectCoinID] = 0;
            if (MainBoost.ActivateDoubleVillageIncomeCondition == true)
            {
                float TempBoost = MarketIncomeValueCollection[ManualCollectCoinID] * 2;

				if (TempBoost >= 999999999999999)
				{
					TempBoost = 999999999999999;
				}

                MainWallet.GoldWalletValue += TempBoost;
            }
            if (MainBoost.ActivateDoubleVillageIncomeCondition == false)
            {
                MainWallet.GoldWalletValue += MarketIncomeValueCollection[ManualCollectCoinID] + MainTalent.TotalAdditionalGoldValue + MainArtifact.GoldenSeedsTotalValue;
            }

			MainWallet.AchievementEarnedGoldQuantityActivate();
			MainWallet.WalletValueManualUpdate();
            TimerControl[ManualCollectCoinID] = false;
            ManualCollectButton[ManualCollectCoinID].gameObject.SetActive(false);
        }

        public void ButtonControlUpdate()
        {
            for (int i = 0; i < BuyButtonCollectionCollection.Length; i++)
            {
                if (MainWallet.GoldWalletValue >= MarketPurchaseCostCountValueCollectionTemp[i] * (1 - MainArtifact.DeflationTotalValue) - MainTalent.TotalReduceUpgradeCost)
                {
                    BuyButtonCollectionCollection[i].interactable = true;
                }

                if (MainWallet.GoldWalletValue < MarketPurchaseCostCountValueCollectionTemp[i] * (1 - MainArtifact.DeflationTotalValue) - MainTalent.TotalReduceUpgradeCost)
                {
                    BuyButtonCollectionCollection[i].interactable = false;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

            ButtonControlUpdate();
            MarketCheckUpdate();
            PurchaseCollection(PurchaseID); //update text money 
        }

        private void RefreshPrices(int ButtonID)
        {
            // This should recalculate the temporary costs considering new levels
            PurchaseCollection(PurchaseID); // Assumes PurchaseID is set and PurchaseCollection is able to recalculate based on current levels
        }

        public void PurchaseButton(int ButtonID)
        {

            float TempValue = (MarketPurchaseCostCountValueCollectionTemp[ButtonID] * (1 - MainArtifact.DeflationTotalValue) - MainTalent.TotalReduceUpgradeCost); 

            if (TempValue <= 0)
            {
                TempValue = 1;
            }

            MainWallet.GoldWalletValue -= TempValue;
            //MainWallet.WalletDataSave();
            MainWallet.WalletValueManualUpdate();

            int levelsPurchased = 0;
            float increaseFactor = 1.03f;  // 2% increase of income

            switch (PurchaseID)
            {
                case 0: // x1
                    levelsPurchased = 1;
                    break;
                case 1: // x10
                    levelsPurchased = 10;
                    break;
                case 2: // x100
                    levelsPurchased = 100;
                    break;
                case 3: // MAX
                    levelsPurchased = FinalTempMaxPurchaseGroupValue[ButtonID];
                    break;
            }

            float cumulativeCost = (MarketPurchaseCostCountValueCollection[ButtonID] * (1 - MainArtifact.DeflationTotalValue) - MainTalent.TotalReduceUpgradeCost);
            float cumulativeIncome = MarketIncomeValueCollection[ButtonID];
            for (int j = 0; j < levelsPurchased; j++)
            {
                cumulativeCost *= increaseFactor;
                cumulativeIncome *= increaseFactor;
            }

            MarketPurchaseCostCountValueCollection[ButtonID] = cumulativeCost > 999999999999999 ? 999999999999999 : cumulativeCost;
            MarketIncomeValueCollection[ButtonID] = cumulativeIncome > 999999999999999 ? 999999999999999 : cumulativeIncome;

            MarketPurchaseCountValueCollection[ButtonID] += levelsPurchased;
            VillageUpgradeLevel[ButtonID] += levelsPurchased;

            if (VillageUpgradeLevel[ButtonID] >= 2000000000)
            {
                VillageUpgradeLevel[ButtonID] = 2000000000;
            }

            // Refresh prices after purchase
            RefreshPrices(ButtonID);

            // Update all relevant UI elements and achievements
            GoldCostCheckManualUpdate(ButtonID);
            GoldIncomeCheckManualUpdate(ButtonID);
            CheckPurchaseCountValueTextManualUpdate();
            CheckVillageStructureManualUpdate();
            ManualMarketValueCheckUpdate();
            CheckTotalValueManualUpdate();
            MainAchievement.AchievementBuildingUpgradeAdditional += levelsPurchased;
            MainAchievement.BuildingUpgradesCheckManualUpdate();
        

        /*
        if (PurchaseID == 0)
        {
            MainAchievement.AchievementBuildingUpgradeAdditional += 1;
            MainAchievement.BuildingUpgradesCheckManualUpdate();

            float a = MarketPurchaseCostCountValueCollection[ButtonID] * 1.02f;
            MarketPurchaseCostCountValueCollection[ButtonID] = a;

            if (MarketPurchaseCostCountValueCollection[ButtonID] >= 999999999999999)
            {
                MarketPurchaseCostCountValueCollection[ButtonID] = 999999999999999;
            }

            MarketPurchaseCostCountValueCollectionTemp[ButtonID] = MarketPurchaseCostCountValueCollection[ButtonID];
            GoldCostCheckManualUpdate(ButtonID);
            MarketPurchaseCountValueCollection[ButtonID] += 1;

            if (MarketPurchaseCountValueCollection[ButtonID] >= 999999999999999)
            {
                MarketPurchaseCountValueCollection[ButtonID] = 999999999999999;
            }

            float i = MarketIncomeValueCollection[ButtonID] * 1.02f;
            MarketIncomeValueCollection[ButtonID] = i;

            if (MarketIncomeValueCollection[ButtonID] >= 999999999999999)
            {
                MarketIncomeValueCollection[ButtonID] = 999999999999999;
            }

            GoldIncomeCheckManualUpdate(ButtonID);

            //MarketTimerCDValueCollection[ButtonID] += 5;
            //SaveData.SaveFile();

            VillageUpgradeLevel[ButtonID] += 1;

            if (VillageUpgradeLevel[ButtonID] >= 2000000000)
            {
                VillageUpgradeLevel[ButtonID] = 2000000000;
            }

            CheckVillageStructureManualUpdate();

            ManualMarketValueCheckUpdate();

            CheckTotalValueManualUpdate();
            CheckPurchaseCountValueTextManualUpdate();

        }
        if (PurchaseID == 1)
        {
            MainAchievement.AchievementBuildingUpgradeAdditional += 10;
            MainAchievement.BuildingUpgradesCheckManualUpdate();

            float a = (MarketPurchaseCostCountValueCollection[ButtonID] * 1.02f) * 10;
            float TempA = a;
            MarketPurchaseCostCountValueCollection[ButtonID] = TempA;

            if (MarketPurchaseCostCountValueCollection[ButtonID] >= 999999999999999)
            {
                MarketPurchaseCostCountValueCollection[ButtonID] = 999999999999999;
            }

            float TempCost = MarketPurchaseCostCountValueCollection[ButtonID] * 10;
            MarketPurchaseCostCountValueCollectionTemp[ButtonID] = TempCost;
            GoldCostCheckManualUpdate(ButtonID);
            MarketPurchaseCountValueCollection[ButtonID] += 1 * 10;

            if (MarketPurchaseCountValueCollection[ButtonID] >= 999999999999999)
            {
                MarketPurchaseCountValueCollection[ButtonID] = 999999999999999;
            }

            float i = (MarketIncomeValueCollection[ButtonID] * 1.02f) * 10;
            float TempI = i;
            MarketIncomeValueCollection[ButtonID] = TempI;

            if (MarketIncomeValueCollection[ButtonID] >= 999999999999999)
            {
                MarketIncomeValueCollection[ButtonID] = 999999999999999;
            }

            GoldIncomeCheckManualUpdate(ButtonID);

            //MarketTimerCDValueCollection[ButtonID] += 5 * 10;
            //SaveData.SaveFile();
            VillageUpgradeLevel[ButtonID] += 1 * 10;

            if (VillageUpgradeLevel[ButtonID] >= 2000000000)
            {
                VillageUpgradeLevel[ButtonID] = 2000000000;
            }

            CheckVillageStructureManualUpdate();
            ManualMarketValueCheckUpdate();

            CheckTotalValueManualUpdate();
            CheckPurchaseCountValueTextManualUpdate();
        }
        if (PurchaseID == 2)
        {
            MainAchievement.AchievementBuildingUpgradeAdditional += 100;
            MainAchievement.BuildingUpgradesCheckManualUpdate();

            float a = (MarketPurchaseCostCountValueCollection[ButtonID] * 1.02f) * 100;
            float TempA = a;
            MarketPurchaseCostCountValueCollection[ButtonID] = TempA;

            if (MarketPurchaseCostCountValueCollection[ButtonID] >= 999999999999999)
            {
                MarketPurchaseCostCountValueCollection[ButtonID] = 999999999999999;

            }
            float TempCost = MarketPurchaseCostCountValueCollection[ButtonID] * 100;
            MarketPurchaseCostCountValueCollectionTemp[ButtonID] = TempCost;
            GoldCostCheckManualUpdate(ButtonID);
            MarketPurchaseCountValueCollection[ButtonID] += 1 * 100;

            if (MarketPurchaseCountValueCollection[ButtonID] >= 999999999999999)
            {
                MarketPurchaseCountValueCollection[ButtonID] = 999999999999999;
            }

            float i = (MarketIncomeValueCollection[ButtonID] * 1.02f) * 100;
            float TempI = i;
            MarketIncomeValueCollection[ButtonID] = TempI;

            if (MarketIncomeValueCollection[ButtonID] >= 999999999999999)
            {
                MarketIncomeValueCollection[ButtonID] = 999999999999999;
            }

            GoldIncomeCheckManualUpdate(ButtonID);

            //MarketTimerCDValueCollection[ButtonID] += 5 * 100;
            //SaveData.SaveFile();
            VillageUpgradeLevel[ButtonID] += 1 * 100;

            if (VillageUpgradeLevel[ButtonID] >= 2000000000)
            {
                VillageUpgradeLevel[ButtonID] = 2000000000;
            }


            CheckVillageStructureManualUpdate();
            ManualMarketValueCheckUpdate();

            CheckTotalValueManualUpdate();
            CheckPurchaseCountValueTextManualUpdate();
        }
        if (PurchaseID == 3)
        {
            //float TempMaxPurchase = MainWallet.GoldWalletValue / MarketPurchaseCostCountValueCollection[ButtonID];
            //float TempFinalMaxPurchase = Mathf.RoundToInt(TempMaxPurchase);
            float a = (MarketPurchaseCostCountValueCollection[ButtonID] * 1.02f) * FinalTempMaxPurchaseGroupValue[ButtonID];
            float TempA = a;
            MainAchievement.AchievementBuildingUpgradeAdditional += TempA;
            MainAchievement.BuildingUpgradesCheckManualUpdate();

            MarketPurchaseCostCountValueCollection[ButtonID] = TempA;

            if (MarketPurchaseCostCountValueCollection[ButtonID] >= 999999999999999)
            {
                MarketPurchaseCostCountValueCollection[ButtonID] = 999999999999999;
            }

            float TempCost = MarketPurchaseCostCountValueCollection[ButtonID] * FinalTempMaxPurchaseGroupValue[ButtonID];
            MarketPurchaseCostCountValueCollectionTemp[ButtonID] = TempCost;
            GoldCostCheckManualUpdate(ButtonID);
            MarketPurchaseCountValueCollection[ButtonID] += 1 * FinalTempMaxPurchaseGroupValue[ButtonID];

            float i = (MarketIncomeValueCollection[ButtonID] * 1.02f) * FinalTempMaxPurchaseGroupValue[ButtonID];
            float TempI = i;
            MarketIncomeValueCollection[ButtonID] = TempI;

            if (MarketIncomeValueCollection[ButtonID] >= 999999999999999)
            {
                MarketIncomeValueCollection[ButtonID] = 999999999999999;
            }

            GoldIncomeCheckManualUpdate(ButtonID);

            //MarketTimerCDValueCollection[ButtonID] += 5 * 1000;
            //SaveData.SaveFile();
            //float TempVillageUpgradeLevel = 1 * FinalTempMaxPurchaseGroupValue[ButtonID];
            int FinalTempVillageUpgradeLevel = FinalTempMaxPurchaseGroupValue[ButtonID];
            VillageUpgradeLevel[ButtonID] += FinalTempVillageUpgradeLevel;

            if (VillageUpgradeLevel[ButtonID] >= 2000000000)
            {
                VillageUpgradeLevel[ButtonID] = 2000000000;
            }

            CheckVillageStructureManualUpdate();
            ManualMarketValueCheckUpdate();

            CheckTotalValueManualUpdate();
            CheckPurchaseCountValueTextManualUpdate();
        }*/
    }

        void CheckTotalValueManualUpdate()
        {
            TotalIncomeValue = 0;
            //for (int i = 0; i < MarketIncomeValueCollection.Count;i++)
            for (int i = 0; i < MarketPurchaseCountValueCollection.Count; i++)
            {
                if (MarketPurchaseCountValueCollection[i] >= 1)
                {
                    TotalIncomeValue += MarketIncomeValueCollection[i];

					if (TotalIncomeValue >= 999999999999999)
					{
						TotalIncomeValue = 999999999999999;
					}

                }

            }
        }

        void CheckTotalCostValueManualUpdate()
        {
            float TempValue = 0;
            //float TempTotalCountCost = 0;
            for (int i = 0; i < MarketPurchaseCostCountValueCollection.Count; i++)
            {
                if (MarketPurchaseCountValueCollection[i] >= 1)
                {
                    TempValue += (MarketPurchaseCostCountValueCollection[i] * (1 - MainArtifact.DeflationTotalValue) - MainTalent.TotalReduceUpgradeCost);
                    //TempTotalCountCost += i;
                    TotalCostValue = TempValue / 29;
                }
                    
            }
            //TotalCostValue = TempValue / TempTotalCountCost;
        }

        public void DoubleVillageGoldIncomeManualCheckAllSet()
        {
            for (int i = 0; i < MarketIncomeValueCollection.Count; i++)
            {
                if (MainBoost.ActivateDoubleVillageIncomeCondition)
                {
                    if (MarketIncomeValueCollection[i] > 999999999999999f)
                    {
                        MarketIncomeValueCollection[i] = 999999999999999f;
                    }

                    if (VillageUpgradeLevel[i] == 0)
                    {
                        if (MarketIncomeValueCollection[i] <= 999)
                        {
                            MarketIncomeValueTextCollection[i].text = " LV1: " + (MarketIncomeValueCollection[i] * 1.03f).ToString("F0") + " X2";
                        }
                        else if (MarketIncomeValueCollection[i] <= 999999)
                        {
                            MarketIncomeValueTextCollection[i].text = " LV1: " + ((MarketIncomeValueCollection[i] * 1.03f) / 1000f).ToString("F2") + "K X2";
                        }
                        else if (MarketIncomeValueCollection[i] <= 999999999f)
                        {
                            MarketIncomeValueTextCollection[i].text = " LV1: " + ((MarketIncomeValueCollection[i] * 1.03f) / 1000000f).ToString("F2") + "M X2";
                        }
                        else if (MarketIncomeValueCollection[i] <= 999999999999f)
                        {
                            MarketIncomeValueTextCollection[i].text = " LV1: " + ((MarketIncomeValueCollection[i] * 1.03f) / 1000000000f).ToString("F2") + "B X2";
                        }
                        else
                        {
                            MarketIncomeValueTextCollection[i].text = " LV1: " + ((MarketIncomeValueCollection[i] * 1.03f) / 1000000000000f).ToString("F2") + "T X2";
                        }
                    }
                    else

                        if (MarketIncomeValueCollection[i] <= 999)
                    {
                        MarketIncomeValueTextCollection[i].text = ": " + MarketIncomeValueCollection[i].ToString("F0") + " X2";
                    }
                    else if (MarketIncomeValueCollection[i] <= 999999)
                    {
                        MarketIncomeValueTextCollection[i].text = ": " + (MarketIncomeValueCollection[i] / 1000f).ToString("F2") + "K X2";
                    }
                    else if (MarketIncomeValueCollection[i] <= 999999999f)
                    {
                        MarketIncomeValueTextCollection[i].text = ": " + (MarketIncomeValueCollection[i] / 1000000f).ToString("F2") + "M X2";
                    }
                    else if (MarketIncomeValueCollection[i] <= 999999999999f)
                    {
                        MarketIncomeValueTextCollection[i].text = ": " + (MarketIncomeValueCollection[i] / 1000000000f).ToString("F2") + "B X2";
                    }
                    else
                    {
                        MarketIncomeValueTextCollection[i].text = ": " + (MarketIncomeValueCollection[i] / 1000000000000f).ToString("F2") + "T X2";
                    }
                }
                else
                {
                    if (MarketIncomeValueCollection[i] > 999999999999999f)
                    {
                        MarketIncomeValueCollection[i] = 999999999999999f;
                    }

                    if (MarketIncomeValueCollection[i] <= 999)
                    {
                        MarketIncomeValueTextCollection[i].text = ": " + MarketIncomeValueCollection[i].ToString();
                    }
                    else if (MarketIncomeValueCollection[i] <= 999999)
                    {
                        MarketIncomeValueTextCollection[i].text = ": " + (MarketIncomeValueCollection[i] / 1000f).ToString("F2") + "K";
                    }
                    else if (MarketIncomeValueCollection[i] <= 999999999f)
                    {
                        MarketIncomeValueTextCollection[i].text = ": " + (MarketIncomeValueCollection[i] / 1000000f).ToString("F2") + "M";
                    }
                    else if (MarketIncomeValueCollection[i] <= 999999999999f)
                    {
                        MarketIncomeValueTextCollection[i].text = ": " + (MarketIncomeValueCollection[i] / 1000000000f).ToString("F2") + "B";
                    }
                    else
                    {
                        MarketIncomeValueTextCollection[i].text = ": " + (MarketIncomeValueCollection[i] / 1000000000000f).ToString("F2") + "T";
                    }
                }
            }
        }

        void GoldIncomeCheckManualUpdate(int ButtonID)
        {
            if (MainBoost.ActivateDoubleVillageIncomeCondition)
            {
                if (MarketIncomeValueCollection[ButtonID] > 999999999999999f)
                {
                    MarketIncomeValueCollection[ButtonID] = 999999999999999f;
                }

                if (VillageUpgradeLevel[ButtonID] == 0)
                {
                    if (MarketIncomeValueCollection[ButtonID] <= 999)
                    {
                        MarketIncomeValueTextCollection[ButtonID].text = " LV1: " + (MarketIncomeValueCollection[ButtonID] * 1.03f).ToString("F0") + " X2";
                    }
                    else if (MarketIncomeValueCollection[ButtonID] <= 999999)
                    {
                        MarketIncomeValueTextCollection[ButtonID].text = " LV1: " + ((MarketIncomeValueCollection[ButtonID] * 1.03f) / 1000f).ToString("F2") + "K X2";
                    }
                    else if (MarketIncomeValueCollection[ButtonID] <= 999999999f)
                    {
                        MarketIncomeValueTextCollection[ButtonID].text = " LV1: " + ((MarketIncomeValueCollection[ButtonID] * 1.03f) / 1000000f).ToString("F2") + "M X2";
                    }
                    else if (MarketIncomeValueCollection[ButtonID] <= 999999999999f)
                    {
                        MarketIncomeValueTextCollection[ButtonID].text = " LV1: " + ((MarketIncomeValueCollection[ButtonID] * 1.03f) / 1000000000f).ToString("F2") + "B X2";
                    }
                    else
                    {
                        MarketIncomeValueTextCollection[ButtonID].text = " LV1: " + ((MarketIncomeValueCollection[ButtonID] * 1.03f) / 1000000000000f).ToString("F2") + "T X2";
                    }
                }
                else

                if (MarketIncomeValueCollection[ButtonID] <= 999)
                {
                    MarketIncomeValueTextCollection[ButtonID].text = ": " + MarketIncomeValueCollection[ButtonID].ToString("F0") + " X2";
                }
                else if (MarketIncomeValueCollection[ButtonID] <= 999999)
                {
                    MarketIncomeValueTextCollection[ButtonID].text = ": " + (MarketIncomeValueCollection[ButtonID] / 1000f).ToString("F2") + "K X2";
                }
                else if (MarketIncomeValueCollection[ButtonID] <= 999999999f)
                {
                    MarketIncomeValueTextCollection[ButtonID].text = ": " + (MarketIncomeValueCollection[ButtonID] / 1000000f).ToString("F2") + "M X2";
                }
                else if (MarketIncomeValueCollection[ButtonID] <= 999999999999f)
                {
                    MarketIncomeValueTextCollection[ButtonID].text = ": " + (MarketIncomeValueCollection[ButtonID] / 1000000000f).ToString("F2") + "B X2";
                }
                else
                {
                    MarketIncomeValueTextCollection[ButtonID].text = ": " + (MarketIncomeValueCollection[ButtonID] / 1000000000000f).ToString("F2") + "T X2";
                }
            }
            else
            {
                if (MarketIncomeValueCollection[ButtonID] > 999999999999999f)
                {
                    MarketIncomeValueCollection[ButtonID] = 999999999999999f;
                }

                if(VillageUpgradeLevel[ButtonID] == 0)
                {
                    if (MarketIncomeValueCollection[ButtonID] <= 999)
                    {
                        MarketIncomeValueTextCollection[ButtonID].text = " LV1: " + (MarketIncomeValueCollection[ButtonID] * 1.03f).ToString("F0");
                    }
                    else if (MarketIncomeValueCollection[ButtonID] <= 999999)
                    {
                        MarketIncomeValueTextCollection[ButtonID].text = " LV1: " + ((MarketIncomeValueCollection[ButtonID] * 1.03f) / 1000f).ToString("F2") + "K";
                    }
                    else if (MarketIncomeValueCollection[ButtonID] <= 999999999f)
                    {
                        MarketIncomeValueTextCollection[ButtonID].text = " LV1: " + ((MarketIncomeValueCollection[ButtonID] * 1.03f) / 1000000f).ToString("F2") + "M";
                    }
                    else if (MarketIncomeValueCollection[ButtonID] <= 999999999999f)
                    {
                        MarketIncomeValueTextCollection[ButtonID].text = " LV1: " + ((MarketIncomeValueCollection[ButtonID] * 1.03f) / 1000000000f).ToString("F2") + "B";
                    }
                    else
                    {
                        MarketIncomeValueTextCollection[ButtonID].text = " LV1: " + ((MarketIncomeValueCollection[ButtonID] * 1.03f) / 1000000000000f).ToString("F2") + "T";
                    }
                }

                else if (MarketIncomeValueCollection[ButtonID] <= 999)
                {
                    MarketIncomeValueTextCollection[ButtonID].text = ": " + MarketIncomeValueCollection[ButtonID].ToString("F0");
                }
                else if (MarketIncomeValueCollection[ButtonID] <= 999999)
                {
                    MarketIncomeValueTextCollection[ButtonID].text = ": " + (MarketIncomeValueCollection[ButtonID] / 1000f).ToString("F2") + "K";
                }
                else if (MarketIncomeValueCollection[ButtonID] <= 999999999f)
                {
                    MarketIncomeValueTextCollection[ButtonID].text = ": " + (MarketIncomeValueCollection[ButtonID] / 1000000f).ToString("F2") + "M";
                }
                else if (MarketIncomeValueCollection[ButtonID] <= 999999999999f)
                {
                    MarketIncomeValueTextCollection[ButtonID].text = ": " + (MarketIncomeValueCollection[ButtonID] / 1000000000f).ToString("F2") + "B";
                }
                else
                {
                    MarketIncomeValueTextCollection[ButtonID].text = ": " + (MarketIncomeValueCollection[ButtonID] / 1000000000000f).ToString("F2") + "T";
                }
            }
        }

        public bool maxIsSelected = false;
        public void GoldCostCheckManualUpdate(int ButtonID)
        {
            float TempValue = MarketPurchaseCostCountValueCollectionTemp[ButtonID] * (1 - MainArtifact.DeflationTotalValue) - MainTalent.TotalReduceUpgradeCost;

            //Drymarti fix
           // if (TempValue <= 0)
           // {
           //     TempValue = 1;
           // }

			if (TempValue >= 999999999999999)
			{
				TempValue = 999999999999999;
			}

            if(maxIsSelected == false)
            {
                if (TempValue <= 999)
                {
                    MarketPurchaseCostCountValueCollectionTextLevels[ButtonID].text = "BUY";
                    MarketPurchaseCostCountValueCollectionText[ButtonID].text = TempValue.ToString("F0");
                }
                else if (TempValue >= 1000 && TempValue <= 999999)
                {
                    MarketPurchaseCostCountValueCollectionTextLevels[ButtonID].text = "BUY";
                    MarketPurchaseCostCountValueCollectionText[ButtonID].text = (TempValue / 1000f).ToString("F2") + "K";
                }
                else if (TempValue >= 1000000 && TempValue <= 999999999)
                {
                    MarketPurchaseCostCountValueCollectionTextLevels[ButtonID].text = "BUY";
                    MarketPurchaseCostCountValueCollectionText[ButtonID].text = (TempValue / 1000000f).ToString("F2") + "M";
                }
                else if (TempValue >= 1000000000 && TempValue <= 999999999999)
                {
                    MarketPurchaseCostCountValueCollectionTextLevels[ButtonID].text = "BUY";
                    MarketPurchaseCostCountValueCollectionText[ButtonID].text = (TempValue / 1000000000f).ToString("F2") + "B";
                }
                else if (TempValue >= 1000000000000 && TempValue <= 999999999999999)
                {
                    MarketPurchaseCostCountValueCollectionTextLevels[ButtonID].text = "BUY";
                    MarketPurchaseCostCountValueCollectionText[ButtonID].text = (TempValue / 1000000000000f).ToString("F2") + "T";
                }
            }
            else if (maxIsSelected == true)
            {
                if (TempValue <= 999)
                {
                    MarketPurchaseCostCountValueCollectionTextLevels[ButtonID].text = "Count : " + FinalTempMaxPurchaseGroupValue[ButtonID].ToString("F0");
                    MarketPurchaseCostCountValueCollectionText[ButtonID].text = TempValue.ToString("F0");
                }
                else if (TempValue >= 1000 && TempValue <= 999999)
                {
                    MarketPurchaseCostCountValueCollectionTextLevels[ButtonID].text = "Count : " + FinalTempMaxPurchaseGroupValue[ButtonID].ToString("F0");
                    MarketPurchaseCostCountValueCollectionText[ButtonID].text = (TempValue / 1000f).ToString("F2") + "K";
                }
                else if (TempValue >= 1000000 && TempValue <= 999999999)
                {
                    MarketPurchaseCostCountValueCollectionTextLevels[ButtonID].text = "Count : " + FinalTempMaxPurchaseGroupValue[ButtonID].ToString("F0");
                    MarketPurchaseCostCountValueCollectionText[ButtonID].text = (TempValue / 1000000f).ToString("F2") + "M";
                }
                else if (TempValue >= 1000000000 && TempValue <= 999999999999)
                {
                    MarketPurchaseCostCountValueCollectionTextLevels[ButtonID].text = "Count : " + FinalTempMaxPurchaseGroupValue[ButtonID].ToString("F0");
                    MarketPurchaseCostCountValueCollectionText[ButtonID].text = (TempValue / 1000000000f).ToString("F2") + "B";
                }
                else if (TempValue >= 1000000000000 && TempValue <= 999999999999999)
                {
                    MarketPurchaseCostCountValueCollectionTextLevels[ButtonID].text = "Count : " + FinalTempMaxPurchaseGroupValue[ButtonID].ToString("F0");
                    MarketPurchaseCostCountValueCollectionText[ButtonID].text = (TempValue / 1000000000000f).ToString("F2") + "T";
                }
            }
            

            //MarketPurchaseCostCountValueCollectionText[i].text = "LV: " + (FinalTempMaxPurchaseGroupValue[i]) + "Cost: " + (TempMaxMarketPurchaseCostGroupValue[i]);
        }

        public void PurchaseCollection(int PurchaseCollectionID)
        {
            foreach (GameObject PBC in PurchaseButtonCollection)
            {
                PBC.gameObject.SetActive(true);
            }
            PurchaseID = PurchaseCollectionID;
            if (PurchaseCollectionID == 0)
            {
                for (int i = 0; i < MarketPurchaseCostCountValueCollectionTemp.Count; i++)
                {
                    PurchaseButtonCollection[0].gameObject.SetActive(false);
                    MarketPurchaseCostCountValueCollectionTemp[i] = (MarketPurchaseCostCountValueCollection[i] * (1 - MainArtifact.DeflationTotalValue) - MainTalent.TotalReduceUpgradeCost);
                    MarketPurchaseCostCountValueCollectionTemp[i] *= 1;

					if (MarketPurchaseCostCountValueCollectionTemp[i] >= 999999999999999)
					{
						MarketPurchaseCostCountValueCollectionTemp[i] = 999999999999999;
					}
                    maxIsSelected = false;
                    GoldCostCheckManualUpdate(i);
                }
            }
            /*
            if (PurchaseCollectionID == 1)
            {
                for (int i = 0; i < MarketPurchaseCostCountValueCollectionTemp.Count; i++)
                {
                    PurchaseButtonCollection[1].gameObject.SetActive(false);
                    MarketPurchaseCostCountValueCollectionTemp[i] = MarketPurchaseCostCountValueCollection[i];
                    // MarketPurchaseCostCountValueCollectionTemp[i] *= 10;
                    MarketPurchaseCostCountValueCollectionTemp[i] = MarketPurchaseCostCountValueCollectionTemp[i] + (MarketPurchaseCostCountValueCollectionTemp[i] + 1.03f) + ((MarketPurchaseCostCountValueCollectionTemp[i] * 1.03f) * 1.03f);


                    if (MarketPurchaseCostCountValueCollectionTemp[i] >= 999999999999999)
					{
						MarketPurchaseCostCountValueCollectionTemp[i] = 999999999999999;
					}

					GoldCostCheckManualUpdate(i);
                }
            }*/
            if (PurchaseCollectionID == 1)
            {
                for (int i = 0; i < MarketPurchaseCostCountValueCollectionTemp.Count; i++)
                {
                    PurchaseButtonCollection[1].gameObject.SetActive(false);
                    float currentCost = (MarketPurchaseCostCountValueCollection[i] * (1 - MainArtifact.DeflationTotalValue) - MainTalent.TotalReduceUpgradeCost);
                    float totalCost = currentCost; // Start with the initial cost
                    float increaseFactor = 1.03f; // 3% increase

                    for (int j = 1; j < 10; j++) // Run the loop 9 more times (total of 10 increments)
                    {
                        currentCost *= increaseFactor; // Apply the 3% increase
                        totalCost += currentCost; // Add the updated cost to the total
                    }

                    // Check if the total cost exceeds the max limit, if so, cap it
                    MarketPurchaseCostCountValueCollectionTemp[i] = totalCost >= 999999999999999 ? 999999999999999 : totalCost;

                    maxIsSelected = false;
                    GoldCostCheckManualUpdate(i); // Update the UI or other relevant fields
                }
            }

            if (PurchaseCollectionID == 2)
            {
                for (int i = 0; i < MarketPurchaseCostCountValueCollectionTemp.Count; i++)
                {
                    PurchaseButtonCollection[2].gameObject.SetActive(false);
                    float currentCost = (MarketPurchaseCostCountValueCollection[i] * (1 - MainArtifact.DeflationTotalValue) - MainTalent.TotalReduceUpgradeCost);
                    float totalCost = currentCost; // Start with the initial cost
                    float increaseFactor = 1.03f; // 3% increase

                    for (int j = 1; j < 100; j++) // Run the loop 9 more times (total of 10 increments)
                    {
                        currentCost *= increaseFactor; // Apply the 3% increase
                        totalCost += currentCost; // Add the updated cost to the total
                    }

                    // Check if the total cost exceeds the max limit, if so, cap it
                    MarketPurchaseCostCountValueCollectionTemp[i] = totalCost >= 999999999999999 ? 999999999999999 : totalCost;

                    maxIsSelected = false;
                    GoldCostCheckManualUpdate(i); // Update the UI or other relevant fields
                }
            }

            /* if (PurchaseCollectionID == 3) //Max
             {
                 for (int i = 0; i < MarketPurchaseCostCountValueCollection.Count; i++)
                 {
                     PurchaseButtonCollection[3].gameObject.SetActive(false);
                     MarketPurchaseCostCountValueCollectionTemp[i] = MarketPurchaseCostCountValueCollection[i];
                     TempMaxPurchaseGroupValue[i] = MainWallet.GoldWalletValue / MarketPurchaseCostCountValueCollectionTemp[i];

                     if (TempMaxPurchaseGroupValue[i] >= 1)
                     {
                         FinalTempMaxPurchaseGroupValue[i] = (int)TempMaxPurchaseGroupValue[i];

                         if (FinalTempMaxPurchaseGroupValue[i] <= -1)
                         {
                             FinalTempMaxPurchaseGroupValue[i] = 2000000000;
                         }

                         TempMaxMarketPurchaseCostGroupValue[i] = MarketPurchaseCostCountValueCollectionTemp[i] * FinalTempMaxPurchaseGroupValue[i];
                         MarketPurchaseCostCountValueCollectionTemp[i] = TempMaxMarketPurchaseCostGroupValue[i];

                         if (TempMaxMarketPurchaseCostGroupValue[i] >= 999999999999999)
                         {
                             TempMaxMarketPurchaseCostGroupValue[i] = 999999999999999;
                         }

                         if (MarketPurchaseCostCountValueCollectionTemp[i] < 999999999999999)
                         {
                             if (MainWallet.GoldWalletValue <= MarketPurchaseCostCountValueCollectionTemp[i])
                             {
                                 MarketPurchaseCostCountValueCollectionTemp[i] = MarketPurchaseCostCountValueCollection[i];
                             }
                         }

                         else if (MarketPurchaseCostCountValueCollectionTemp[i] >= 999999999999999)
                         {
                             MarketPurchaseCostCountValueCollectionTemp[i] = 999999999999999;
                         }
                     }
                     GoldCostCheckManualUpdate(i);

                 }
             }
            */
            if (PurchaseCollectionID == 3) //Max
            {
                for (int i = 0; i < MarketPurchaseCostCountValueCollection.Count; i++)
                {
                    PurchaseButtonCollection[3].gameObject.SetActive(false);
                    float currentCost = (MarketPurchaseCostCountValueCollection[i] * (1 - MainArtifact.DeflationTotalValue) - MainTalent.TotalReduceUpgradeCost);
                    float availableFunds = MainWallet.GoldWalletValue;
                    int maxPurchases = 0;
                    float totalCost = 0;

                    while (availableFunds >= currentCost)
                    {
                        availableFunds -= currentCost;  // Subtract the cost of the current level from available funds
                        totalCost += currentCost;       // Accumulate the total cost
                        currentCost *= 1.03f;           // Increment the cost for the next level by 3%
                        maxPurchases++;                 // Increment the count of maximum purchases
                    }



                    // Store the result of the number of max purchases possible
                    FinalTempMaxPurchaseGroupValue[i] = maxPurchases;
                    TempMaxMarketPurchaseCostGroupValue[i] = totalCost > 0 ? totalCost : MarketPurchaseCostCountValueCollection[i]; // Ensure that the minimum cost is the base cost if no purchases can be made

                    // Cap the total cost if it exceeds the max allowed value
                    if (TempMaxMarketPurchaseCostGroupValue[i] >= 999999999999999)
                    {
                        TempMaxMarketPurchaseCostGroupValue[i] = 999999999999999;
                    }

                    // Update the text on the button with both the number of levels and the total cost
                   
                    MarketPurchaseCostCountValueCollectionTemp[i] = (TempMaxMarketPurchaseCostGroupValue[i] * (1 - MainArtifact.DeflationTotalValue) - MainTalent.TotalReduceUpgradeCost);
                    

                    maxIsSelected = true;
                    GoldCostCheckManualUpdate(i);
                }
            }

        }



        public void TimeTravelActivate()
        {
            for (int i = 0; i < MarketPurchaseCountValueCollection.Count; i++)
            {

                if (MarketPurchaseCountValueCollection[i] >= 1)
                {
                    TimeTravelEarningsCountValueCollection[i] = 3600 / MarketTimerCDValueCollection[i];

                    TimeTravelEarningsCoinValueCollection[i] = TimeTravelEarningsCountValueCollection[i] * MarketIncomeValueCollection[i];
                }

                TotalTimeTravelEarningsCoinValue += TimeTravelEarningsCoinValueCollection[i];

				if (TotalTimeTravelEarningsCoinValue >= 999999999999999)
				{
					TotalTimeTravelEarningsCoinValue = 999999999999999;
				}
			}
            MainWallet.GoldWalletValue += TotalTimeTravelEarningsCoinValue;


            MainWallet.WalletValueManualUpdate();

            StartCoroutine("ResetTotalTimeTravelEarningsCoinValue");


        }

        IEnumerator ResetTotalTimeTravelEarningsCoinValue()
        {
            yield return new WaitForSeconds(.1f);
            TotalTimeTravelEarningsCoinValue = 0;

        }

        public void ResetPrestigeActivate()
        {
            for (int i = 0;  i < MarketIncomeValueCollection.Count; i++)
            {
                MarketIncomeValueCollection[i] = MarketIncomeValueCollectionReset[i];
            }

            for (int i = 0; i < MarketPurchaseCountValueCollection.Count; i++)
            {
                if (MarketPurchaseCountValueCollection[i] >= 1)
                {
                    MarketPurchaseCountValueCollection[i] = 1;
                }
              
            }

            for (int i = 0; i < MarketPurchaseCostCountValueCollection.Count; i++)
            {
                MarketPurchaseCostCountValueCollection[i] = MarketPurchaseCostCountValueCollectionReset[i];
            }

            for (int i = 0; i < VillageUpgradeLevel.Count; i++)
            {
                VillageUpgradeLevel[i] = 1;
            }

            VillageMarketUpgradeCurrentLevel = 0;
            VillageTownHallUpgradeCurrentLevel = 0;
            VillageMagesGuildUpgradeCurrentLevel = 0;
            VillageButcherUpgradeCurrentLevel = 0;
            VillageAlchemistUpgradeCurrentLevel = 0;
            VillageFisherMansHutUpgradeCurrentLevel = 0;
            VillageHarborUpgradeCurrentLevel = 0;
            VillageCowFarmUpgradeCurrentLevel = 0;
            VillageVolcanoUpgradeCurrentLevel = 0;
            VillageOurStatueUpgradeCurrentLevel = 0;
            VillageHoleInGroundUpgradeCurrentLevel = 0;
            VillageCourtUpgradeCurrentLevel = 0;
            VillageCenterSquareUpgradeCurrentLevel = 0;
            VillageTrainingGroundUpgradeCurrentLevel= 0;
            VillagePlainCottageUpgradeCurrentLevel = 0;
            VillageChurchUpgradeCurrentLevel = 0;
            VillageAltarUpgradeCurrentLevel = 0;
            VillageChariotParkingUpgradeCurrentLevel = 0;
            VillageShipUpgradeCurrentLevel = 0;
            VillageArmoryUpgradeCurrentLevel = 0;
            VillageClinicUpgradeCurrentLevel = 0;
            VillageIronWorksUpgradeCurrentLevel = 0;
            VillageArtifactShopUpgradeCurrentLevel = 0;
            VillageCemeteryUpgradeCurrentLevel = 0;
            VillagePowerStationUpgradeCurrentLevel = 0;
            VillageThievesGuildUpgradeCurrentLevel = 0;
            VillageGreenHouseUpgradeCurrentLevel = 0;
            VillageRobotFactoryUpgradeCurrentLevel = 0;
            VillageMonsterZooUpgradeCurrentLevel = 0;

            MainWallet.ResetPrestigeGoldActivate();

            PurchaseCollection(0);
            CheckPurchaseCostValueTextManualUpdate();
            ManualMarketValueCheckUpdate();
            MainHero.HeroResetPrestigeActivate();

        }

        public void DisplayMarketStatus(int MarketStatsID)
        {
            MarketStatusID = MarketStatsID;
            MainMarketStatus.MarketStatusDisplayText(MarketStatusID);
        }

        void CheckPurchaseCostValueTextManualUpdate()
        {
            for (int i = 0; i < MarketPurchaseCostCountValueCollectionText.Length; i++)
            {
                float TempValue = MarketPurchaseCostCountValueCollectionTemp[i] = MarketPurchaseCostCountValueCollection[i];

                if (TempValue >= 999999999999999)
                {
                    TempValue = 999999999999999;
                }

                if (TempValue <= 999)
                {
                    MarketPurchaseCostCountValueCollectionText[i].text = TempValue.ToString("F0");
                }
                else if (TempValue >= 1000 && TempValue <= 999999)
                {
                    MarketPurchaseCostCountValueCollectionText[i].text = (TempValue / 1000f).ToString("F2") + "K";
                }
                else if (TempValue >= 1000000 && TempValue <= 999999999)
                {
                    MarketPurchaseCostCountValueCollectionText[i].text = (TempValue / 1000000f).ToString("F2") + "M";
                }
                else if (TempValue >= 1000000000 && TempValue <= 999999999999)
                {
                    MarketPurchaseCostCountValueCollectionText[i].text = (TempValue / 1000000000f).ToString("F2") + "B";
                }
                else if (TempValue >= 1000000000000 && TempValue <= 999999999999999)
                {
                    MarketPurchaseCostCountValueCollectionText[i].text = (TempValue / 1000000000000f).ToString("F2") + "T";
                }
            }
        }

        void CheckPurchaseCountValueTextManualUpdate()
        {
            for (int i = 0; i < MarketPurchaseCountValueCollection.Count; i++)
            {
                float TempValue = (MarketPurchaseCountValueCollection[i] * (1 - MainArtifact.DeflationTotalValue) - MainTalent.TotalReduceUpgradeCost);

                if (TempValue >= 999999999999999)
                {
                    TempValue = 999999999999999;
                }

                if (TempValue <= 999)
                {
                    MarketPurchaseCountValueTextCollection[i].text = "LV" + TempValue.ToString("F0");
                }
                else if (TempValue >= 1000 && TempValue <= 999999)
                {
                    MarketPurchaseCountValueTextCollection[i].text = "LV" + (TempValue / 1000f).ToString("F2") + "K";
                }
                else if (TempValue >= 1000000 && TempValue <= 999999999)
                {
                    MarketPurchaseCountValueTextCollection[i].text = "LV" + (TempValue / 1000000f).ToString("F2") + "M";
                }
                else if (TempValue >= 1000000000 && TempValue <= 999999999999)
                {
                    MarketPurchaseCountValueTextCollection[i].text = "LV" + (TempValue / 1000000000f).ToString("F2") + "B";
                }
                else if (TempValue >= 1000000000000 && TempValue <= 999999999999999)
                {
                    MarketPurchaseCountValueTextCollection[i].text = "LV" + (TempValue / 1000000000000f).ToString("F2") + "T";
                }
            
            }
        }

        private void OnApplicationQuit()
        {
            if (MainBoost.ActivateOfflineEarningsCondition == true)
            {
                //if (DateAcquired == false)
                {
                    DateTime dateQuit = DateTime.Now;
                    PlayerPrefs.SetString("Date Quit", dateQuit.ToString());
                    Debug.Log(dateQuit);
                   // DateAcquired = true;
                }

            }

        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus == false)
            {
                if (MainBoost.ActivateOfflineEarningsCondition == true)
                {
                   // if (DateAcquired == false)
                    {
                        DateTime dateQuit = DateTime.Now;
                        PlayerPrefs.SetString("Date Quit", dateQuit.ToString());
                        Debug.Log(dateQuit);
                       // DateAcquired = true;
                    }

                }
            }
            
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause == true)
            {
                if (MainBoost.ActivateOfflineEarningsCondition == true)
                {
                    //if (DateAcquired == false)
                    {
                        DateTime dateQuit = DateTime.Now;
                        PlayerPrefs.SetString("Date Quit", dateQuit.ToString());
                        Debug.Log(dateQuit);
                       // DateAcquired = true;
                    }

                }
            }
           
        }
    }
}

