using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SAMPLETEXT.Wallet.Manager;
using SAMPLETEXT.ItemPurchase.Manager.Boost;
using SAMPLETEXT.Account.Manager.Profile;
using SAMPLETEXT.Talent.Manager;
using SAMPLETEXT.Artifact.Manager;
using SAMPLETEXT.Inventory.Manager;
using SAMPLETEXT.Achievement.Manager;
using SAMPLETEXT.ItemPurchase.Manager.Gem;
using TMPro;

namespace SAMPLETEXT.Gameplay.Manager.Enemy
{
    public class GameplayEnemyManagerScript : MonoBehaviour
    {
        [SerializeField] DiamondPurchaseManagerScript gemScript;
        [SerializeField]
        ArtifactManagerScript MainArtifact;
        [SerializeField]
        ItemsPurchaseBoostManagerScript MainBoost;
        [SerializeField]
        WalletManagerScript MainWallet;
        [Header("Wave Settings")]
        public float WaveMinCountValue;
        public float WaveMaxCountValue;
        public float WavePointsCount;
        public float TempPointsCount;
        public float doubleTalentPointsChance;
        [SerializeField]
        TextMeshProUGUI[] WaveMinCountValueText;
        [SerializeField]
        TextMeshProUGUI[] WaveMaxCountValueText;
        [SerializeField]
        ProfileManagerScript MainAccountProfile;
        [SerializeField]
        TalentsManagerScript MainTalent;


        [Header("Enemy Health Settings")]
        public float EnemyMaxHealthValue;
        public float EnemyHealthValue;
        [SerializeField]
        TextMeshProUGUI EnemyHealthValueText;
        [SerializeField]
        TextMeshProUGUI EnemyHealthValueTextMAX;
        [SerializeField]
        Image EnemyHealthBarImage;

        [Header("Enemy Countdown Health Settings")]
        public float EnemyHealthMaxTimerCDValue;
        [SerializeField]
        float EnemyHealthTimerCDValue;
        [SerializeField]
        TextMeshProUGUI HealthTimerCDValueText;
        bool HealthTimerControl;

        [Header("Enemy Drop Bonus Gold Coin")]
        public float MinGoldCoinEnemyDropValue;
        public float MaxGoldCoinEnemyDropValue;
        public float CurrentGoldCoinEnemyDropValue;

        [Header("Change Enemy Image Settings")]
        [SerializeField]
        Sprite[] EnemyCollectionSprite;
        [SerializeField]
        SpriteRenderer EnemySprite;
        //[SerializeField]
        public int EnemyDeathCount; // New JSON
        //[SerializeField]
        //int EnemyMaxDeathCount;
        [SerializeField]
        string[] EnemyNameString;
        [SerializeField]
        TextMeshProUGUI EnemyNameText;
        public int EnemyNameID;// New JSON
        public int EnemyID; // New JSON
        public bool EnemySecondEvolve; //New JSON
        public bool EnemyThirdEvolve;// New JSON

        [Header("Change Background Image Settings")]
        [SerializeField]
        Sprite[] ImageBackgroundCollection;
        public int ImageBackgroundIDCount; // New JSON
        [SerializeField]
        Image ImageBackground;
        [SerializeField]
        GameObject changeBiomAnimation;

        [Header("Inventory Settings")]
        [SerializeField]
        InventoryManagerScript MainInventory;
        public float InventoryBossGoldDrop;

        [Header("Item Boss Drop Settings")]
        [SerializeField]
        GameObject ItemDisplayObj;
        [SerializeField]
        Image ItemImage;
        [SerializeField]
        TextMeshProUGUI ItemName;
        [SerializeField]
        int MaxItemDropRange;
        public float ItemChanceDropRange; //Non JSON
        public float TalentItemChanceDropAdditional; // NON JSON
        
        [Header("Item Chance Range Control")]
        [SerializeField]
       public float ItemChanceDropMaxValue;
        [SerializeField]
        public float ItemChanceDropMaxValueDefault = 5f;
        [SerializeField]
        float ItemChanceDropMinValue;
        [SerializeField]
        public int ItemChanceMaxRangeValue;
        public int ItemChanceCurrentRangeValue;
        enum BiomesClassificationType
        {
            LAKA_01,
            WEJSCIE_02,
            LAS_03,
            PALACY_04,
            LAWOWY_05,
            WULKANY_06,
            SKALISTY_07,
            JASKINIA_08,
            PODZIEMIA_09,
            LOCHY_10,
            KOSZMAR_11,
            PRZYSZIE_12,
            POSTAPO_13,
            PUSTYNNIA_14,
            OAZA_15,
            MIASTO_16,
            STATE_17,
            KRAINA_18,
            WEJSCIE_19,
            POZIOM_20
        }
        [SerializeField]
        List<BiomesClassificationType> BiomesClassificationList = new List<BiomesClassificationType>();
        [SerializeField]
        List<int> MinimumItemDropRangeList = new List<int>();
        [SerializeField]
        List<int> MaximumItemDropRangeList = new List<int>();

		[Header("Achievement Settings")]
		[SerializeField]
		AchievementManagerScript MainAchievement;
		public float AchievementMaxWaveCountValue;

        // Start is called before the first frame update
        void Start()
        {
            changeBiomAnimation.SetActive(false); // Drymarti
            FirstCheck();
        }

        public void FirstCheck()
        {
            if (WaveMinCountValue <= 0)
            {
                EnemyDeathCount = 1;
                WaveMinCountValue = 1;
                WaveMaxCountValue = 2;

                foreach(TextMeshProUGUI CurrentWave in WaveMinCountValueText)
                {
                    CurrentWave.text = "Wave " + WaveMinCountValue.ToString();
                }

                foreach (TextMeshProUGUI MaxWave in WaveMaxCountValueText)
                {
                    MaxWave.text = "Wave " + WaveMaxCountValue.ToString();
                }

                EnemyMaxHealthValue = 1000; //Here is the Max health value of the enemy by default (first enemy has 1000) and it increases by 3% with each evolve and new enemy
                EnemyHealthValue = EnemyMaxHealthValue;
                EnemyHealthTextUpdate();




                    // EnemyHealthValueText.text = Mathf.Round(EnemyHealthValue) + "/" + Mathf.Round(EnemyMaxHealthValue);
                EnemyHealthBarImage.fillAmount = EnemyHealthValue / EnemyMaxHealthValue;


                EnemyHealthMaxTimerCDValue = 60;
                EnemyHealthTimerCDValue = EnemyHealthMaxTimerCDValue;

                MinGoldCoinEnemyDropValue = 30;
                MaxGoldCoinEnemyDropValue = 50;

                EnemySprite.sprite = EnemyCollectionSprite[0];
                EnemyNameText.text = EnemyNameString[0].ToString();
            }
            else
            {
                foreach (TextMeshProUGUI CurrentWave in WaveMinCountValueText)
                {
                    CurrentWave.text = "Wave " + WaveMinCountValue.ToString();
                }

                foreach (TextMeshProUGUI MaxWave in WaveMaxCountValueText)
                {
                    MaxWave.text = "Wave " + WaveMaxCountValue.ToString();
                }
                EnemyHealthValue = EnemyMaxHealthValue;


                //ADDED K,M,B,T TO MAX HEALTH
                EnemyHealthTextUpdate();
                //EnemyHealthValueText.text = Mathf.Round(EnemyHealthValue) + "/" + Mathf.Round(EnemyMaxHealthValue);

                EnemyHealthBarImage.fillAmount = EnemyHealthValue / EnemyMaxHealthValue;

                EnemyHealthTimerCDValue = EnemyHealthMaxTimerCDValue;

                EnemySprite.sprite = EnemyCollectionSprite[EnemyID];
                EnemyNameText.text = EnemyNameString[EnemyNameID].ToString();
            }

            ImageBackground.sprite = ImageBackgroundCollection[ImageBackgroundIDCount];
        }

        void EnemyHealthTimerCountDownUpdate()
        {
            if (WaveMinCountValue >= 1)
            {
                HealthTimerCDValueText.text = "" + Mathf.Round(EnemyHealthTimerCDValue) +"s";
                if (HealthTimerControl == false)
                {
                    EnemyHealthTimerCDValue = EnemyHealthMaxTimerCDValue;
                    HealthTimerControl = true;
                }

                EnemyHealthTimerCDValue -= Time.deltaTime;

                if (EnemyHealthTimerCDValue <= 0)
                {
                    if (HealthTimerControl == true)
                    {
                        EnemyHealthTimerCDValue = 0;
                        EnemyWaveDecrease();
                    }
                }
            }
          
        }

        public void EnemyHealthCheckReduction()
        {
            EnemyHealthBarImage.fillAmount = EnemyHealthValue / EnemyMaxHealthValue;

            //ADDED K,M,B,T TO MAX HEALTH
            EnemyHealthTextUpdate();
            //EnemyHealthValueText.text = Mathf.Round(EnemyHealthValue) + "/" + Mathf.Round(EnemyMaxHealthValue);

            if (EnemyHealthValue <= 0)
            {
                EnemyHealthValue = 0;


                EnemyHealthTextUpdate();
                //EnemyHealthValueText.text = Mathf.Round(EnemyHealthValue) + "/" + Mathf.Round(EnemyMaxHealthValue);
                EnemyWaveIncrease();
            }
        }
        public void ResetEnemyWave()
        {
			MainAchievement.FirstPrestigeAchievementCheckManualUpdate();
            EnemySprite.gameObject.transform.localScale = new Vector3(.3f, .3f, .3f);
            EnemyID = 0;
            EnemyNameID = 0;
            EnemySprite.sprite = EnemyCollectionSprite[0];
            EnemyNameText.text = EnemyNameString[0];
            WaveMinCountValue = 1;
            WaveMaxCountValue = 2;
            WavePointsCount = 0;
            EnemyDeathCount = 1;
            EnemySecondEvolve = false;
            EnemyThirdEvolve = false;
            foreach (TextMeshProUGUI CurrentWave in WaveMinCountValueText)
            {
                CurrentWave.text = "Wave " + WaveMinCountValue.ToString();
            }

            foreach (TextMeshProUGUI MaxWave in WaveMaxCountValueText)
            {
                MaxWave.text = "Wave " + WaveMaxCountValue.ToString();
            }

            //int i = Random.Range(150, 350);
            EnemyMaxHealthValue = 1000;

            EnemyHealthValue = EnemyMaxHealthValue;


            EnemyHealthTextUpdate();
            // EnemyHealthValueText.text = Mathf.Round(EnemyHealthValue) + "/" + Mathf.Round(EnemyMaxHealthValue);
            EnemyHealthBarImage.fillAmount = EnemyHealthValue / EnemyMaxHealthValue;

            //int a = Random.Range(10, 20);
            //EnemyHealthMaxTimerCDValue = a;
            EnemyHealthTimerCDValue = EnemyHealthMaxTimerCDValue;
            HealthTimerControl = false;

            MinGoldCoinEnemyDropValue = 100;
            MaxGoldCoinEnemyDropValue = 200;

            doubleTalentPointsChance = UnityEngine.Random.Range(0, MainArtifact.TalentedTotalValue);
            if(doubleTalentPointsChance > MainArtifact.TalentedTotalValue)
            {
                MainAccountProfile.PointsValue += TempPointsCount;

                MainTalent.TalentPointValue += TempPointsCount;
            }
            else if (doubleTalentPointsChance <= MainArtifact.TalentedTotalValue)
            {
                MainAccountProfile.PointsValue += (TempPointsCount * 2);

                MainTalent.TalentPointValue += (TempPointsCount * 2);
            }
            

            MainTalent.PointsTextManualUpdate();
            TempPointsCount = 0;
            //MainAccountProfile.PointsTextManualUpdate();
            //RESET BIOMS
            ImageBackgroundIDCount = 0;
            ImageBackground.sprite = ImageBackgroundCollection[0];
            
        }

      
        void EnemyWaveIncrease()
        {
            WaveMinCountValue += 1;
            WaveMaxCountValue += 1;

			if (AchievementMaxWaveCountValue <= WaveMaxCountValue)
			{
				AchievementMaxWaveCountValue += 1;
				MainAchievement.WavesConquerorQuantityAchievementCheckManualUpdate();
			}
			WavePointsCount += 1;

            EnemyDeathCount += 1;

            if (EnemyDeathCount <= 5)
            {
                EnemySprite.gameObject.transform.localScale = new Vector3(.3f, .3f, .3f);
            }

            else if (EnemyDeathCount <= 10)
            {
                EnemySprite.gameObject.transform.localScale = new Vector3(.4f, .4f, .4f);
            }

            else if (EnemyDeathCount <= 15)
            {
                if (EnemySecondEvolve == false)
                {
                    EnemyID += 1;
                    EnemySprite.gameObject.transform.localScale = new Vector3(.3f, .3f, .3f);
                    EnemySecondEvolve = true;
                }
               
            }

            else if (EnemyDeathCount <= 20)
            {
                EnemySprite.gameObject.transform.localScale = new Vector3(.5f, .5f, .5f);
            }

            else if (EnemyDeathCount <= 25)
            {
                if (EnemyThirdEvolve == false)
                {
                    EnemyID += 1;
                    EnemySprite.gameObject.transform.localScale = new Vector3(.3f, .3f, .3f);
                    EnemyThirdEvolve = true;
                }

            }

           else if (EnemyDeathCount >= 26)
            {
                EnemySprite.gameObject.transform.localScale = new Vector3(.3f, .3f, .3f);
                EnemyDeathCount = 1;
                EnemyID += 1;
                EnemyNameID += 1;
                EnemySecondEvolve = false;
                EnemyThirdEvolve = false;
            }

            float TempEnemyCount = 0;
            for (int x = 0; x < EnemyCollectionSprite.Length; x++)
            {
                TempEnemyCount = x;
            }

            if (EnemyNameID > TempEnemyCount)
            {
                EnemyID = 0;
            }

            float TempEnemyNameCount = 0;

            for (int z = 0; z < EnemyNameString.Length; z++)
            {
                TempEnemyNameCount = z;
            }

            if (EnemyNameID > TempEnemyNameCount)
            {
                EnemyNameID = 0;
            }

            EnemyNameText.text = EnemyNameString[EnemyNameID];
            EnemySprite.sprite = EnemyCollectionSprite[EnemyID];

            if (WavePointsCount >= 100)
            {
                //int a = Random.Range(0, ImageBackgroundCollection.Length);

                ImageBackgroundIDCount += 1;

                int TempImageCount = 0;
                for (int a = 0; a < ImageBackgroundCollection.Length; a++)
                {
                    TempImageCount = a;
                }

                if (ImageBackgroundIDCount > TempImageCount)
                {
                    ImageBackgroundIDCount = 0;
                }
                //DRYMNARTI ANIMATION BACKGROUND CHANGE
                //changeBiomAnimation.SetActive(true);
                //ImageBackground.sprite = ImageBackgroundCollection[ImageBackgroundIDCount];
                StartCoroutine(ChangeBackgroundCoroutine());

                WavePointsCount = 0;
                TempPointsCount += 1;
                MainAccountProfile.PrestigeCheckAcceptButton();
            }
            foreach (TextMeshProUGUI CurrentWave in WaveMinCountValueText)
            {
                CurrentWave.text = "Wave " + WaveMinCountValue.ToString();
            }

            foreach (TextMeshProUGUI MaxWave in WaveMaxCountValueText)
            {
                MaxWave.text = "Wave " + WaveMaxCountValue.ToString();
            }

            float i = EnemyMaxHealthValue * 1.02f; //% how many % increase on new evolution or new enemy (with each defeated wave)

            EnemyMaxHealthValue = i;
            EnemyHealthValue = EnemyMaxHealthValue;


            EnemyHealthTextUpdate();
            //EnemyHealthValueText.text = Mathf.Round(EnemyHealthValue) + "/" + Mathf.Round(EnemyMaxHealthValue);
            EnemyHealthBarImage.fillAmount = EnemyHealthValue / EnemyMaxHealthValue;

            //int a = Random.Range(1, 4);
            //EnemyHealthMaxTimerCDValue += a;
            EnemyHealthTimerCDValue = EnemyHealthMaxTimerCDValue;
            HealthTimerControl = false;

            float BonusCoin = Random.Range(MinGoldCoinEnemyDropValue, MaxGoldCoinEnemyDropValue);
            float TempBonusCoin = Mathf.Round(BonusCoin);
            float TempInventoryBossGoldDrop = InventoryBossGoldDrop * TempBonusCoin;
            CurrentGoldCoinEnemyDropValue = TempBonusCoin + TempInventoryBossGoldDrop;

            float TempTalentIceSpike = 0;
            if (MainTalent.IceSpikeSkillActivate == true)
            {
               TempTalentIceSpike = MainTalent.TotalAdditionalGoldValueIceSpike;
            }


            if (MainBoost.ActivateDoubleGoldIncomeCondition == true)
            {
                MainWallet.GoldWalletValue += (CurrentGoldCoinEnemyDropValue + TempTalentIceSpike) * 2;
				MainWallet.AchievementGoldWalletValue += (CurrentGoldCoinEnemyDropValue + TempTalentIceSpike) * 2;
            }

            if (MainBoost.ActivateDoubleGoldIncomeCondition == false)
            {
                MainWallet.GoldWalletValue += (CurrentGoldCoinEnemyDropValue + TempTalentIceSpike);
				MainWallet.AchievementGoldWalletValue += (CurrentGoldCoinEnemyDropValue + TempTalentIceSpike);
            }

			MainWallet.AchievementEarnedGoldQuantityActivate();
			MainWallet.WalletValueManualUpdate();
            float MinG = Random.Range(15, 20);
            float TempMinG =  Mathf.Round(MinG);
            MinGoldCoinEnemyDropValue += TempMinG;
            float MaxG = Random.Range(25, 30);
            float TempMaxG = Mathf.Round(MaxG);
            MaxGoldCoinEnemyDropValue += TempMaxG;

            //Inventory Drop

            ItemChanceCurrentRangeValue = Random.Range(0, ItemChanceMaxRangeValue);

            if(gemScript.VIPCountValue >= 4)
            {
                if (ItemChanceCurrentRangeValue <= (ItemChanceDropMaxValue * 2))
                {

                    float TempItemChanceDrop = ItemChanceDropRange + MainTalent.TotalAdditionalItemDropChance;

                    ItemChanceDropRange = ItemChanceDropRange + TempItemChanceDrop;

                    int TempDefeatBossItemDropChance = Random.Range(0, MaxItemDropRange);

                    if (TempDefeatBossItemDropChance <= ItemChanceDropRange)
                    {
                        ItemDisplayObj.gameObject.SetActive(false);
                        ItemDisplayObj.gameObject.SetActive(true);

                        int TempItemDrop = Random.Range(MinimumItemDropRangeList[ImageBackgroundIDCount], MaximumItemDropRangeList[ImageBackgroundIDCount]);

                        if (MainInventory.InventoryActiveCollectionList[TempItemDrop] == false)
                        {
                            MainInventory.InventoryActiveCollectionList[TempItemDrop] = true;
                        }

                        MainInventory.InventoryFragmentCollectionList[TempItemDrop] += 1;

                        MainInventory.FirstCheck();

                        ItemImage.sprite = null;
                        ItemImage.sprite = MainInventory.InventoryImageCollection[TempItemDrop];
                        ItemName.text = string.Empty;
                        ItemName.text = MainInventory.InventoryNameCollection[TempItemDrop];

                        StartCoroutine("HideItemDropDisplay");

                    }
                }
              
            }
            else
            {
                if (ItemChanceCurrentRangeValue <= ItemChanceDropMaxValue)
                {

                    float TempItemChanceDrop = ItemChanceDropRange + MainTalent.TotalAdditionalItemDropChance;

                    ItemChanceDropRange = ItemChanceDropRange + TempItemChanceDrop;

                    int TempDefeatBossItemDropChance = Random.Range(0, MaxItemDropRange);

                    if (TempDefeatBossItemDropChance <= ItemChanceDropRange)
                    {
                        ItemDisplayObj.gameObject.SetActive(false);
                        ItemDisplayObj.gameObject.SetActive(true);

                        int TempItemDrop = Random.Range(MinimumItemDropRangeList[ImageBackgroundIDCount], MaximumItemDropRangeList[ImageBackgroundIDCount]);

                        if (MainInventory.InventoryActiveCollectionList[TempItemDrop] == false)
                        {
                            MainInventory.InventoryActiveCollectionList[TempItemDrop] = true;
                        }

                        MainInventory.InventoryFragmentCollectionList[TempItemDrop] += 1;

                        MainInventory.FirstCheck();

                        ItemImage.sprite = null;
                        ItemImage.sprite = MainInventory.InventoryImageCollection[TempItemDrop];
                        ItemName.text = string.Empty;
                        ItemName.text = MainInventory.InventoryNameCollection[TempItemDrop];

                        StartCoroutine("HideItemDropDisplay");

                    }
                }
            }

        }

        //Drymarti Change Background
        public IEnumerator ChangeBackgroundCoroutine()
        {
            changeBiomAnimation.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            ImageBackground.sprite = ImageBackgroundCollection[ImageBackgroundIDCount];
            yield return new WaitForSeconds(0.5f);
            changeBiomAnimation.SetActive(false);
            
        }

        IEnumerator HideItemDropDisplay()
        {
            yield return new WaitForSeconds(2f);
            ItemDisplayObj.gameObject.SetActive(false);
        }

        void EnemyWaveDecrease()
        {
            WaveMinCountValue -= 1;
            WaveMaxCountValue -= 1;

            if (WaveMinCountValue <= 0)
            {
                FirstCheck();
            }
            else
            {
                foreach (TextMeshProUGUI CurrentWave in WaveMinCountValueText)
                {
                    CurrentWave.text = "Wave " + WaveMinCountValue.ToString();
                }

                foreach (TextMeshProUGUI MaxWave in WaveMaxCountValueText)
                {
                    MaxWave.text = "Wave " + WaveMaxCountValue.ToString();
                }

                //int i = Random.Range(100, 130);
               
                float i = EnemyMaxHealthValue * 1.02f; //% how many % increase on new evolution or new enemy (with each defeated wave)
                float TempI = EnemyMaxHealthValue - i;
                EnemyMaxHealthValue -= TempI;
                EnemyHealthValue = EnemyMaxHealthValue;



                EnemyHealthTextUpdate();
                //EnemyHealthValueText.text = Mathf.Round(EnemyHealthValue) + "/" + Mathf.Round(EnemyMaxHealthValue);
                EnemyHealthBarImage.fillAmount = EnemyHealthValue / EnemyMaxHealthValue;

                //int a = Random.Range(7, 10);
                //EnemyHealthMaxTimerCDValue -= a;
                EnemyHealthTimerCDValue = EnemyHealthMaxTimerCDValue;
                HealthTimerControl = false;

                float MinG = Random.Range(10, 13);
                float TempMinG = Mathf.Round(MinG);
                MinGoldCoinEnemyDropValue -= TempMinG;
                float MaxG = Random.Range(20, 23);
                float TempMaxG = Mathf.Round(MaxG);
                MaxGoldCoinEnemyDropValue -= TempMaxG;
            }
        }
        public void EnemyHealthTextUpdate()
        {
            //ADDED K,M,B,T TO MAX HEALTH
            if (EnemyHealthValue <= 999f)
            {
                EnemyHealthValueText.text = Mathf.Round(EnemyHealthValue) + " ";
            }
            else if (EnemyHealthValue <= 999999f)
            {
                EnemyHealthValueText.text = Mathf.Round(EnemyHealthValue/ 1f).ToString("F0");
            }
            else if (EnemyHealthValue <= 9999999f)
            {
                EnemyHealthValueText.text = Mathf.Round(EnemyHealthValue / 1f).ToString("F0");
            }
            else if (EnemyHealthValue <= 999999999f)
            {
                EnemyHealthValueText.text = Mathf.Round(EnemyHealthValue / 1000000f).ToString("F0") + "M";
            }
            else if (EnemyHealthValue <= 999999999999f)
            {
                EnemyHealthValueText.text = Mathf.Round(EnemyHealthValue / 1000000000f).ToString("F0") + "B";
            }
            else if (EnemyHealthValue <= 999999999999999f)
            {
                EnemyHealthValueText.text = Mathf.Round(EnemyHealthValue / 1000000000000f).ToString("F0") + "T";
            }


            if (EnemyMaxHealthValue <= 999f)
            {
                EnemyHealthValueText.text = " " + Mathf.Round(EnemyMaxHealthValue);
            }
            else if (EnemyMaxHealthValue <= 999999f)
            {
                EnemyHealthValueTextMAX.text = " " + Mathf.Round(EnemyMaxHealthValue / 1f).ToString("F0");
            }
            else if (EnemyMaxHealthValue <= 9999999f)
            {
                EnemyHealthValueTextMAX.text = " " + Mathf.Round(EnemyMaxHealthValue / 1f).ToString("F0");
            }
            else if (EnemyMaxHealthValue <= 999999999f)
            {
                EnemyHealthValueTextMAX.text = " " + Mathf.Round(EnemyMaxHealthValue / 1000000f).ToString("F0") + "M";
            }
            else if (EnemyMaxHealthValue <= 999999999999f)
            {
                EnemyHealthValueTextMAX.text = " " + Mathf.Round(EnemyMaxHealthValue / 1000000000f).ToString("F0") + "B";
            }
            else if (EnemyMaxHealthValue <= 999999999999999f)
            {
                EnemyHealthValueTextMAX.text = " " + Mathf.Round(EnemyMaxHealthValue / 1000000000000f).ToString("F0") + "T";
            }
        }


        // Update is called once per frame
        void Update()
        {
            //EnemyHealthTextUpdate();
            EnemyHealthTimerCountDownUpdate();
        }
    }
}

