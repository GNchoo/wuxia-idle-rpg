using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SAMPLETEXT.Market.Manager;
using UnityEngine.UI;
using SAMPLETEXT.Wallet.Manager;
using SAMPLETEXT.Gameplay.Manager.Enemy;
using SAMPLETEXT.SubHeroUI.Manager;
using SAMPLETEXT.ItemPurchase.Manager.Gem;

namespace SAMPLETEXT.ItemPurchase.Manager.Boost
{
    public class ItemsPurchaseBoostManagerScript : MonoBehaviour
    {

        [Header("Boost Collection Settings")]
        [SerializeField]
        WalletManagerScript MainWallet;
        [SerializeField]
        Button[] BoostPurchaseButtonCollection;
        [SerializeField]
        float[] BoostCostValueCollection;
        [SerializeField]
        TextMeshProUGUI[] BoostCostValueCollectionText;

        [Header("Double Gold Income Settings")]
        [SerializeField] DiamondPurchaseManagerScript gemScript;
        public bool ActivateDoubleGoldIncomeCondition;
        public float DoubleGoldIncomeCountDownTimerValue;
        [SerializeField]
        TextMeshProUGUI[] DoubleGoldIncomeCountDownTimerValueText;
        [SerializeField]
        GameObject[] DoubleGoldIncomeObj;
        [SerializeField]
        public float DoubleGoldIncomeTimerCountdownReference;

        [Header("Double Village Income Settings")]
        [SerializeField]
        MarketManagerScript MainVillageMarket;
        public bool ActivateDoubleVillageIncomeCondition;
        public float DoubleVillageIncomeCountdownTimerValue;
        [SerializeField]
        TextMeshProUGUI[] DoubleVillageIncomeCountdownTimerValueText;
        [SerializeField]
        GameObject[] DoubleVillageIncomeObj;
        public float DoubleVillageIncomeCountdownReference;

        [Header("Offline Earning Settings")]
        public bool ActivateOfflineEarningsCondition;
        [SerializeField]
        public float OfflineEarningsCountdownReference;

        [Header("Time Travel Settings")]
        [SerializeField]
        GameplayEnemyManagerScript MainEnemyManager;
        [SerializeField]
        SubHeroesManagerScript MainSubHeroManager;
        float EnemyTotalHealthTimeTravel;
        float EnemyTotalGoldCoinTimeTravel;
        float PlayerFieldCountTimeTravel;
        [SerializeField]
        float EnemyTotalGoldIncomeTimeTravel;

        [Header("DailyBundle Pop Up Window Settings")]
        [SerializeField]
        GameObject DailyBundlePopUpWindowObj;
        [SerializeField]
        Image DailyBundlePopUpWindowBoostImage;
        [SerializeField]
        TextMeshProUGUI DailyBundlePurchaseBoostValueCountText;
        
        

        [Header("Boost Pop Up Window Settings")]
        [SerializeField]
        GameObject BoostPopUpWindowObj;
        [SerializeField]
        Image BoostPopUpWindowBoostImage;
        [SerializeField]
        TextMeshProUGUI BoostPurchaseBoostValueCountText;
        int DescriptionCountReference;
        public Sprite[] BoostImageCollection;
        //[SerializeField]
        public string[] BoostNameCollection;
        //[SerializeField]
        //TextMeshProUGUI BoostNameBoostText;


        // Start is called before the first frame update
        void Start()
        {
            FirstCheck();
            FirstCheckText();
        }

        void FirstCheckText()
        {
            for (int i = 0; i < BoostCostValueCollection.Length; i++)
            {
                BoostCostValueCollectionText[i].text = BoostCostValueCollection[i].ToString();
            }
        }

        public void FirstCheck()
        {
            if (ActivateDoubleGoldIncomeCondition == true)
            {
                foreach(GameObject DGIO in DoubleGoldIncomeObj)
                {
                    DGIO.gameObject.SetActive(true);
                }
                //DoubleGoldIncomeObj.gameObject.SetActive(true);
            }

            if (ActivateDoubleGoldIncomeCondition == false)
            {
                foreach (GameObject DGIO in DoubleGoldIncomeObj)
                {
                    DGIO.gameObject.SetActive(false);
                }
                //DoubleGoldIncomeObj.gameObject.SetActive(false);
            }

            if (ActivateDoubleVillageIncomeCondition == true)
            {
                foreach (GameObject DVIO in DoubleVillageIncomeObj)
                {
                    DVIO.gameObject.SetActive(true);
                }

                //DoubleVillageIncomeObj.gameObject.SetActive(true);
            }

            if (ActivateDoubleVillageIncomeCondition == false)
            {
                foreach (GameObject DVIO in DoubleVillageIncomeObj)
                {
                    DVIO.gameObject.SetActive(false);
                }
                //DoubleVillageIncomeObj.gameObject.SetActive(false);
            }

            
            for (int i = 0; i < BoostCostValueCollection.Length; i++)
            {
                if (MainWallet.GemWalletValue >= BoostCostValueCollection[i])
                {
                    if (BoostPurchaseButtonCollection[i] != BoostPurchaseButtonCollection[2])
                    {
                        BoostPurchaseButtonCollection[i].interactable = true;

                    }
                }

                if (MainWallet.GemWalletValue < BoostCostValueCollection[i])
                {
                    BoostPurchaseButtonCollection[i].interactable = false;
                }
            }
           
        }

        public void LoadFirstCheck()
        {
            MainVillageMarket.OfflineEarningFirstCheck();
        }

        void DoubleGoldIncomeCountDownTimerUpdate()
        {
            if (ActivateDoubleGoldIncomeCondition == true)
            {
                if(gemScript.VIPCountValue < 3)
                {
                    DoubleGoldIncomeCountDownTimerValue -= Time.deltaTime;

                    foreach (TextMeshProUGUI DGICTVT in DoubleGoldIncomeCountDownTimerValueText)
                    {
                        DGICTVT.text = Mathf.Floor(DoubleGoldIncomeCountDownTimerValue / 60).ToString("00") + ":" + Mathf.Floor(DoubleGoldIncomeCountDownTimerValue % 60).ToString("00");
                    }

                    //DoubleGoldIncomeCountDownTimerValueText.text = Mathf.Floor(DoubleGoldIncomeCountDownTimerValue/60).ToString("00") + ":" + Mathf.Floor(DoubleGoldIncomeCountDownTimerValue % 60).ToString("00");

                    if (DoubleGoldIncomeCountDownTimerValue <= 0)
                    {
                        ActivateDoubleGoldIncomeCondition = false;
                        DoubleGoldIncomeCountDownTimerValue = 0;
                        FirstCheck();
                    }
                }
                else
                {
                    foreach (TextMeshProUGUI DGICTVT in DoubleGoldIncomeCountDownTimerValueText)
                    {
                        DGICTVT.text = "FULL";
                    }

                }
               
            }
        }

        void DoubleVillageIncomeCountDownTimerUpdate()
        {
            if (ActivateDoubleVillageIncomeCondition == true)
            {
                DoubleVillageIncomeCountdownTimerValue -= Time.deltaTime;

                foreach (TextMeshProUGUI DVICTVT in DoubleVillageIncomeCountdownTimerValueText)
                {
                    DVICTVT.text = Mathf.Floor(DoubleVillageIncomeCountdownTimerValue / 60).ToString("00") + ":" + Mathf.Floor(DoubleVillageIncomeCountdownTimerValue % 60).ToString("00");
                }

                //DoubleVillageIncomeCountdownTimerValueText.text = Mathf.Floor(DoubleVillageIncomeCountdownTimerValue / 60).ToString("00") + ":" + Mathf.Floor(DoubleVillageIncomeCountdownTimerValue % 60).ToString("00");

                if (DoubleVillageIncomeCountdownTimerValue <= 0)
                {
                    ActivateDoubleVillageIncomeCondition = false;
                    DoubleVillageIncomeCountdownTimerValue = 0;
                    MainVillageMarket.DoubleVillageGoldIncomeManualCheckAllSet();
                    FirstCheck();
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            DoubleGoldIncomeCountDownTimerUpdate();
            DoubleVillageIncomeCountDownTimerUpdate();
        }

        public void DoubleGoldIncomePurchaseButtonVIP()
        {
            DescriptionCountReference = 0;
            //MainWallet.GemWalletValue -= BoostCostValueCollection[DescriptionCountReference];
            //MainWallet.WalletValueManualUpdate();
            DoubleGoldIncomeCountDownTimerValue += DoubleGoldIncomeTimerCountdownReference;
            ActivateDoubleGoldIncomeCondition = true;
            FirstCheck();
        }

        public void DoubleGoldIncomePurchaseButton()
        {
            DescriptionCountReference = 0;
            MainWallet.GemWalletValue -= BoostCostValueCollection[DescriptionCountReference];
            MainWallet.WalletValueManualUpdate();
            DoubleGoldIncomeCountDownTimerValue += DoubleGoldIncomeTimerCountdownReference;
            ActivateDoubleGoldIncomeCondition = true;
            FirstCheck();
            BoostPopUpWindowDisplayActivate();
        }

        public void DoubleVillageIncomePurchaseButton()
        {
            //Gem Deduction
            DescriptionCountReference = 1;
            MainWallet.GemWalletValue -= BoostCostValueCollection[DescriptionCountReference];
            MainWallet.WalletValueManualUpdate();
            DoubleVillageIncomeCountdownTimerValue += DoubleVillageIncomeCountdownReference;
            ActivateDoubleVillageIncomeCondition = true;
            MainVillageMarket.DoubleVillageGoldIncomeManualCheckAllSet();
            FirstCheck();
            BoostPopUpWindowDisplayActivate();
        }

        public void OfflineEarningsPurchaseButton()
        {
            DescriptionCountReference = 2;
            MainWallet.GemWalletValue -= BoostCostValueCollection[DescriptionCountReference];
            MainWallet.WalletValueManualUpdate();
            ActivateOfflineEarningsCondition = true;
            MainVillageMarket.TotalOfflineEarningHoursValue += OfflineEarningsCountdownReference;
            MainVillageMarket.OfflineEarningFirstCheck();
            BoostPopUpWindowDisplayActivate();
        }

        public void TimeTravelPurchaseButton()
        {
            DescriptionCountReference = 3;
            MainWallet.GemWalletValue -= BoostCostValueCollection[DescriptionCountReference];
            MainWallet.WalletValueManualUpdate();
            MainVillageMarket.TimeTravelActivate();
            EnemyBossTimeTravelIncome();
        }

        void DailyBundlePopUpWindowDisplayActivate()
        {
            DailyBundlePopUpWindowBoostImage.sprite = null;
            DailyBundlePopUpWindowBoostImage.sprite = BoostImageCollection[DescriptionCountReference];
            DailyBundlePurchaseBoostValueCountText.text = string.Empty;

            if(DescriptionCountReference == 3)
            {
                TimeTravelEarningsTextUpdate();
               
            }
            else
            {
                DailyBundlePurchaseBoostValueCountText.text = BoostNameCollection[DescriptionCountReference];
            }
            DailyBundlePopUpWindowObj.gameObject.SetActive(true);

        }

        void BoostPopUpWindowDisplayActivate()
        {
            BoostPopUpWindowBoostImage.sprite = null;
            BoostPopUpWindowBoostImage.sprite = BoostImageCollection[DescriptionCountReference];
            BoostPurchaseBoostValueCountText.text = string.Empty;
            BoostPurchaseBoostValueCountText.text = BoostNameCollection[DescriptionCountReference];
            if (DescriptionCountReference == 3)
            {
                TimeTravelEarningsTextUpdate();

            }
            BoostPopUpWindowObj.gameObject.SetActive(true);

        }
        public void DailyBundleEnemyBossTimeTravelIncome()
        {
            EnemyTotalGoldIncomeTimeTravel = 0;


            EnemyTotalGoldCoinTimeTravel = MainEnemyManager.MaxGoldCoinEnemyDropValue;
            PlayerFieldCountTimeTravel = MainSubHeroManager.PlayerActiveCountInField;

            int HealthCheckCount = 60;

            for (int i = 0; i < HealthCheckCount; i++)
            {
                if (i < HealthCheckCount)
                {
                    EnemyTotalHealthTimeTravel = MainEnemyManager.EnemyMaxHealthValue * 1.02f;
                    EnemyTotalGoldCoinTimeTravel = MainEnemyManager.MaxGoldCoinEnemyDropValue * 1.02f;
                }

            }

            float TempEnemyKill = EnemyTotalHealthTimeTravel * PlayerFieldCountTimeTravel;
            EnemyTotalGoldIncomeTimeTravel = TempEnemyKill * EnemyTotalGoldCoinTimeTravel;

            MainWallet.GoldWalletValue += EnemyTotalGoldIncomeTimeTravel;

            //BoostPopUpWindowDisplayActivate();


        }

        public void EnemyBossTimeTravelIncome()
        {
            EnemyTotalGoldIncomeTimeTravel = 0;

           
            EnemyTotalGoldCoinTimeTravel = MainEnemyManager.MaxGoldCoinEnemyDropValue;
            PlayerFieldCountTimeTravel = MainSubHeroManager.PlayerActiveCountInField;

            int HealthCheckCount = 60;

            for (int i = 0; i < HealthCheckCount; i++ )
            {
                if (i < HealthCheckCount)
                {
                    EnemyTotalHealthTimeTravel = MainEnemyManager.EnemyMaxHealthValue * 1.02f;
                    EnemyTotalGoldCoinTimeTravel = MainEnemyManager.MaxGoldCoinEnemyDropValue * 1.02f;
                }
               
            }

            float TempEnemyKill = EnemyTotalHealthTimeTravel * PlayerFieldCountTimeTravel;
            EnemyTotalGoldIncomeTimeTravel = TempEnemyKill * EnemyTotalGoldCoinTimeTravel;

            MainWallet.GoldWalletValue += EnemyTotalGoldIncomeTimeTravel;

            BoostPopUpWindowDisplayActivate();


        }


        void TimeTravelEarningsTextUpdate()
        {
            //if (MainVillageMarket.TotalTimeTravelEarningsCoinValue <= 0)
            //{
            //    GoldWalletValue = 0;
            //}
            float TotalIncomeTimeTravel = EnemyTotalGoldIncomeTimeTravel + MainVillageMarket.TotalTimeTravelEarningsCoinValue;
            if (TotalIncomeTimeTravel <= 999)
            {
                BoostPurchaseBoostValueCountText.text = BoostNameCollection[DescriptionCountReference] + " Coins: " + TotalIncomeTimeTravel.ToString("F0");
            }

            if (TotalIncomeTimeTravel >= 1000 && TotalIncomeTimeTravel <= 999999)
            {
                BoostPurchaseBoostValueCountText.text = BoostNameCollection[DescriptionCountReference] + " Coins: " + (TotalIncomeTimeTravel / 1000).ToString("F2") + "K";
            }

            if (TotalIncomeTimeTravel >= 1000000 && TotalIncomeTimeTravel <= 999999999)
            {

                BoostPurchaseBoostValueCountText.text = BoostNameCollection[DescriptionCountReference] + " Coins: " + (TotalIncomeTimeTravel / 1000000).ToString("F2") + "M";
            }

            if (TotalIncomeTimeTravel >= 1000000000 && TotalIncomeTimeTravel <= 999999999999)
            {
                BoostPurchaseBoostValueCountText.text = BoostNameCollection[DescriptionCountReference] + " Coins: " + (TotalIncomeTimeTravel / 1000000000).ToString("F2") + "B";
            }

			if (TotalIncomeTimeTravel >= 1000000000000 && TotalIncomeTimeTravel <= 999999999999999)
			{
				BoostPurchaseBoostValueCountText.text = BoostNameCollection[DescriptionCountReference] + " Coins: " + (TotalIncomeTimeTravel / 1000000000000).ToString("F2") + "T";
			}
		}

        public void PurchaseBoostCollectButton()
        {
            BoostPopUpWindowBoostImage.sprite = null;
            BoostPurchaseBoostValueCountText.text = string.Empty;
            BoostPopUpWindowObj.gameObject.SetActive(false);
            FirstCheck();
        }
    }
}

