using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SAMPLETEXT.Wallet.Manager;
using SAMPLETEXT.ItemPurchase.Manager.Item;
using SAMPLETEXT.ItemPurchase.Manager.Boost;
using SAMPLETEXT.ItemPurchase.Manager.DailyBundle;
using SAMPLETEXT.Achievement.Manager;
using SAMPLETEXT.Gameplay.Manager.MainHero;
using SAMPLETEXT.Gameplay.Skills.Manager;
using SAMPLETEXT.Talent.Manager;
using SAMPLETEXT.ItemPurchase.Manager.DailyAds;
using SAMPLETEXT.Gameplay.Manager.Enemy;
using UnityEngine.Purchasing;
using System;

namespace SAMPLETEXT.ItemPurchase.Manager.Gem
{
    public class DiamondPurchaseManagerScript : MonoBehaviour, IStoreListener
    {


        [SerializeField]
        ItemsPurchaseArtifactManagerScript MainArtifactPurchase;
        [SerializeField]
        ItemsPurchaseBoostManagerScript MainBoostPurchase;
        [SerializeField]
        ItemsPurchaseHeroManagerScript MainHeroPurchase;
        [SerializeField]
        WalletManagerScript MainWallet;
        [SerializeField]
        DailyBundlePurchaseManagerScript MainDailyBundlePurchase;
        [Header("Gem, Price and Reward Settings")]
        [SerializeField]
        float[] PurchaseCostValueCollection;
        [SerializeField]
        TextMeshProUGUI[] PurchaseCostValueCollectionText;
        [SerializeField]
        float[] GemValueRewardCollection;

        [Header("Ads Settings")]
        public bool AdsDisable;
        [SerializeField]
        Button AdsButton;

        [Header("VIP Settings")]
        public float VIPPurchaseValue;
        [SerializeField]
        float VIPMaxPurchaseValue;
        public float VIPCountValue;
        [SerializeField]
        TextMeshProUGUI VIPCountValueText;
        [SerializeField]
        Image VIPBarImage;
        [SerializeField]
        Image FireImage;
        [SerializeField]
        Button VIPButton;

        [Header("Pop Up Purchase Window Settings")]
        [SerializeField]
        GameObject PopUpPurchaseObj;
        [SerializeField]
        Image PopUpIconImage;
        [SerializeField]
        Sprite PopUpGemImage;
        [SerializeField]
        Sprite PopUpAdsImage;
        [SerializeField]
        TextMeshProUGUI PopUpTextValue;

        [Header("Achievement Settings")]
        [SerializeField]
        AchievementManagerScript MainAchievement;

        [Header("Gem Purchase Max Settings")]
        [SerializeField]
        Button[] GemPurchaseButtons;
        [SerializeField]
        // Start is called before the first frame update

        [Header("VIP SYSTEM")]
        public DailyRewardSystem dailyRewardScript;
        public GameplayMainHeroManagerScript mainHeroScript;
        public GameplaySkillsManagerScript skillsManagerScript;
        public TalentsManagerScript talentScript;

        //NEW PURCHASING GOOGLE PLAY SYSTEM
        private static IStoreController m_StoreController;
        private static IExtensionProvider m_StoreExtensionProvider;
        void Start()
        {
            FirshCheck();


            //NEW PURCHASING SYSTEM GOOGLE PLAY
            if (m_StoreController == null)
            {
                InitializePurchasing();
            }

        }

        //NEW PURCHASING SYSTEM GOOGLE PLAY
        void InitializePurchasing()
        {
            if (IsInitialized())
                return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct("noads_purchase", ProductType.NonConsumable);
            builder.AddProduct("250gems", ProductType.Consumable);
            builder.AddProduct("1000gems", ProductType.Consumable);
            builder.AddProduct("2070gems", ProductType.Consumable);
            builder.AddProduct("5000gems", ProductType.Consumable);
            builder.AddProduct("10000gems", ProductType.Consumable);
            builder.AddProduct("25000gems", ProductType.Consumable);
            builder.AddProduct("50000gems", ProductType.Consumable);

            UnityPurchasing.Initialize(this, builder);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            string productId = args.purchasedProduct.definition.id;
            switch (productId)
            {
                case "noads_purchase":
                    PurchaseGem(7);
                    FindObjectOfType<Settings>().PlayPurchaseSound();
                    break;
                case "250gems":
                    PurchaseGem(0);
                    FindObjectOfType<Settings>().PlayPurchaseSound();
                    break;
                case "1000gems":
                    PurchaseGem(1);
                    FindObjectOfType<Settings>().PlayPurchaseSound();
                    break;
                case "2070gems":
                    PurchaseGem(2);
                    FindObjectOfType<Settings>().PlayPurchaseSound();
                    break;
                case "5000gems":
                    PurchaseGem(3);
                    FindObjectOfType<Settings>().PlayPurchaseSound();
                    break;
                case "10000gems":
                    PurchaseGem(4);
                    FindObjectOfType<Settings>().PlayPurchaseSound();
                    break;
                case "25000gems":
                    PurchaseGem(5);
                    FindObjectOfType<Settings>().PlayPurchaseSound();
                    break;
                case "50000gems":
                    PurchaseGem(6);
                    FindObjectOfType<Settings>().PlayPurchaseSound();
                    break;
                default:
                    Debug.Log("Unrecognized productId: " + productId);
                    return PurchaseProcessingResult.Pending;
            }
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log($"OnPurchaseFailed: {product.definition.id} due to {failureReason}");
        }

        private bool IsInitialized()
        {
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        public void FirshCheck()
        {
            for (int i = 0; i < PurchaseCostValueCollection.Length; i++)
            {
                PurchaseCostValueCollectionText[i].text = PurchaseCostValueCollection[i].ToString() + "$";
            }

            if (AdsDisable == true)
            {
                AdsButton.interactable = false;

            }

            for (int i = 0; i < GemPurchaseButtons.Length; i++)
            {
                if (MainWallet.GemWalletValue >= 999999999999999)
                {
                    GemPurchaseButtons[i].interactable = false;
                }

                if (MainWallet.GemWalletValue < 999999999999999)
                {
                    GemPurchaseButtons[i].interactable = true;
                }

            }

            //Check Max Value for VIP

            if (VIPCountValue == 0)
            {
                VIPMaxPurchaseValue = 1;
            }
            if (VIPCountValue == 1)
            {
                VIPMaxPurchaseValue = 2;
            }
            if (VIPCountValue == 2) // LVL 2
            {
                VIPMaxPurchaseValue = 4;
               
            }
            if (VIPCountValue == 3)
            {
                VIPMaxPurchaseValue = 6;
            }
            if (VIPCountValue == 4)
            {
                VIPMaxPurchaseValue = 12;
            }
            if (VIPCountValue == 5)
            {
                VIPMaxPurchaseValue = 18;
            }
            if (VIPCountValue == 6)
            {
                VIPMaxPurchaseValue = 30;
            }
            if (VIPCountValue == 7)
            {
                VIPMaxPurchaseValue = 50;
            }
            if (VIPCountValue == 8)
            {
                VIPMaxPurchaseValue = 80;
            }
            if (VIPCountValue == 9)
            {
                VIPMaxPurchaseValue = 120;
            }
            if (VIPCountValue == 10)
            {
                VIPMaxPurchaseValue = 1;
            }

            if(VIPCountValue >= 3)
            {
                MainBoostPurchase.DoubleGoldIncomePurchaseButtonVIP();
            }



            VIPBarImage.fillAmount = VIPPurchaseValue / VIPMaxPurchaseValue;
            float fillAmountTemp = VIPPurchaseValue / VIPMaxPurchaseValue;

            // float fillAmountTemp = VIPPurchaseValue / 10;

            FireImage.rectTransform.anchorMin = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMin.y);
            FireImage.rectTransform.anchorMax = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMax.y);
            FireImage.rectTransform.anchoredPosition = new Vector2(0, 20f);

            VIPCountValueText.text = "VIP " + VIPCountValue.ToString();

            CheckVIPButtonManualUpdate();
        }

        public void PurchaseGem(int GemPurchaseId)
        {

            if (GemPurchaseId == 7)
            {
                AdsDisable = true;
                AdsButton.interactable = false;

                PopUpIconImage.sprite = null;
                PopUpIconImage.sprite = PopUpAdsImage;
                PopUpPurchaseObj.gameObject.SetActive(true);
                PopUpTextValue.text = "No Ads".ToString();



                if (VIPPurchaseValue >= VIPMaxPurchaseValue)
                {
                    // VIP Function
                    VIPCountValue += 2;

                    VIPCountValueText.text = "VIP " + VIPCountValue.ToString();
                }
            }
            else
            {
                MainAchievement.CollectedGemsAchievementCheckManualUpdate();
                MainWallet.GemWalletValue += GemValueRewardCollection[GemPurchaseId];
                MainWallet.WalletValueManualUpdate();
                MainDailyBundlePurchase.DailyBundleButtonCheckManualUpdate();

                PopUpIconImage.sprite = null;
                PopUpIconImage.sprite = PopUpGemImage;
                PopUpPurchaseObj.gameObject.SetActive(true);
                PopUpTextValue.text = GemValueRewardCollection[GemPurchaseId].ToString();
                MainArtifactPurchase.FirstCheck();
                MainBoostPurchase.FirstCheck();
                MainHeroPurchase.ButtonCheckUpdate();
            }

            CheckPurchaseGemButtonsManualUpdate();

            if (GemPurchaseId == 0)
            {
                VIPPurchaseValue += 0.25f; // Add points to current VIP Purchase Value

                while (VIPPurchaseValue >= VIPMaxPurchaseValue)
                {
                    VIPPurchaseValue -= VIPMaxPurchaseValue; // Subtract the required points to level up
                    VIPCountValue++; // Increase VIP level

                    // Update max value for the new VIP level - this should ideally be in a separate method
                    switch (VIPCountValue)
                    {
                        case 1: VIPMaxPurchaseValue = 2; break;
                        case 2: VIPMaxPurchaseValue = 4; break;
                        case 3: VIPMaxPurchaseValue = 6; break;
                        case 4: VIPMaxPurchaseValue = 12; break;
                        case 5: VIPMaxPurchaseValue = 18; break;
                        case 6: VIPMaxPurchaseValue = 30; break;
                        case 7: VIPMaxPurchaseValue = 50; break;
                        case 8: VIPMaxPurchaseValue = 80; break;
                        case 9: VIPMaxPurchaseValue = 120; break;
                        case 10: VIPMaxPurchaseValue = 1; break; // Confirm this logic
                        default: VIPMaxPurchaseValue *= 2; break; // Adjust as necessary for higher levels
                    }

                    // Update VIP level text
                    VIPCountValueText.text = "VIP " + VIPCountValue.ToString();
                }

                // Update progress bar after all calculations
                VIPBarImage.fillAmount = VIPPurchaseValue / VIPMaxPurchaseValue;
                float fillAmountTemp = VIPPurchaseValue / VIPMaxPurchaseValue;

                // Update UI elements
                FireImage.rectTransform.anchorMin = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMin.y);
                FireImage.rectTransform.anchorMax = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMax.y);
                FireImage.rectTransform.anchoredPosition = new Vector2(0, 20f);

                if (VIPCountValue >= 3)
                {
                    MainBoostPurchase.DoubleGoldIncomePurchaseButtonVIP();
                }
            }
            if (GemPurchaseId == 1)
            {
                VIPPurchaseValue += 0.9f; // Add points to current VIP Purchase Value

                while (VIPPurchaseValue >= VIPMaxPurchaseValue)
                {
                    VIPPurchaseValue -= VIPMaxPurchaseValue; // Subtract the required points to level up
                    VIPCountValue++; // Increase VIP level

                    // Update max value for the new VIP level - this should ideally be in a separate method
                    switch (VIPCountValue)
                    {
                        case 1: VIPMaxPurchaseValue = 2; break;
                        case 2: VIPMaxPurchaseValue = 4; break;
                        case 3: VIPMaxPurchaseValue = 6; break;
                        case 4: VIPMaxPurchaseValue = 12; break;
                        case 5: VIPMaxPurchaseValue = 18; break;
                        case 6: VIPMaxPurchaseValue = 30; break;
                        case 7: VIPMaxPurchaseValue = 50; break;
                        case 8: VIPMaxPurchaseValue = 80; break;
                        case 9: VIPMaxPurchaseValue = 120; break;
                        case 10: VIPMaxPurchaseValue = 1; break; // Confirm this logic
                        default: VIPMaxPurchaseValue *= 2; break; // Adjust as necessary for higher levels
                    }

                    // Update VIP level text
                    VIPCountValueText.text = "VIP " + VIPCountValue.ToString();
                }

                // Update progress bar after all calculations
                VIPBarImage.fillAmount = VIPPurchaseValue / VIPMaxPurchaseValue;
                float fillAmountTemp = VIPPurchaseValue / VIPMaxPurchaseValue;

                // Update UI elements
                FireImage.rectTransform.anchorMin = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMin.y);
                FireImage.rectTransform.anchorMax = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMax.y);
                FireImage.rectTransform.anchoredPosition = new Vector2(0, 20f);

                if (VIPCountValue >= 3)
                {
                    MainBoostPurchase.DoubleGoldIncomePurchaseButtonVIP();
                }
            }

            if (GemPurchaseId == 2)
            {
                VIPPurchaseValue += 3.0f; // Add points to current VIP Purchase Value

                while (VIPPurchaseValue >= VIPMaxPurchaseValue)
                {
                    VIPPurchaseValue -= VIPMaxPurchaseValue; // Subtract the required points to level up
                    VIPCountValue++; // Increase VIP level

                    // Update max value for the new VIP level - this should ideally be in a separate method
                    switch (VIPCountValue)
                    {
                        case 1: VIPMaxPurchaseValue = 2; break;
                        case 2: VIPMaxPurchaseValue = 4; break;
                        case 3: VIPMaxPurchaseValue = 6; break;
                        case 4: VIPMaxPurchaseValue = 12; break;
                        case 5: VIPMaxPurchaseValue = 18; break;
                        case 6: VIPMaxPurchaseValue = 30; break;
                        case 7: VIPMaxPurchaseValue = 50; break;
                        case 8: VIPMaxPurchaseValue = 80; break;
                        case 9: VIPMaxPurchaseValue = 120; break;
                        case 10: VIPMaxPurchaseValue = 1; break; // Confirm this logic
                        default: VIPMaxPurchaseValue *= 2; break; // Adjust as necessary for higher levels
                    }

                    // Update VIP level text
                    VIPCountValueText.text = "VIP " + VIPCountValue.ToString();
                }

                // Update progress bar after all calculations
                VIPBarImage.fillAmount = VIPPurchaseValue / VIPMaxPurchaseValue;
                float fillAmountTemp = VIPPurchaseValue / VIPMaxPurchaseValue;

                // Update UI elements
                FireImage.rectTransform.anchorMin = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMin.y) ;
                FireImage.rectTransform.anchorMax = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMax.y);
                FireImage.rectTransform.anchoredPosition = new Vector2(0, 20f);

                if (VIPCountValue >= 3)
                {
                    MainBoostPurchase.DoubleGoldIncomePurchaseButtonVIP();
                }
            }
            if (GemPurchaseId == 3)
            {
                VIPPurchaseValue += 5.0f; // Add points to current VIP Purchase Value

                while (VIPPurchaseValue >= VIPMaxPurchaseValue)
                {
                    VIPPurchaseValue -= VIPMaxPurchaseValue; // Subtract the required points to level up
                    VIPCountValue++; // Increase VIP level

                    // Update max value for the new VIP level - this should ideally be in a separate method
                    switch (VIPCountValue)
                    {
                        case 1: VIPMaxPurchaseValue = 2; break;
                        case 2: VIPMaxPurchaseValue = 4; break;
                        case 3: VIPMaxPurchaseValue = 6; break;
                        case 4: VIPMaxPurchaseValue = 12; break;
                        case 5: VIPMaxPurchaseValue = 18; break;
                        case 6: VIPMaxPurchaseValue = 30; break;
                        case 7: VIPMaxPurchaseValue = 50; break;
                        case 8: VIPMaxPurchaseValue = 80; break;
                        case 9: VIPMaxPurchaseValue = 120; break;
                        case 10: VIPMaxPurchaseValue = 1; break; // Confirm this logic
                        default: VIPMaxPurchaseValue *= 2; break; // Adjust as necessary for higher levels
                    }

                    // Update VIP level text
                    VIPCountValueText.text = "VIP " + VIPCountValue.ToString();
                }

                // Update progress bar after all calculations
                VIPBarImage.fillAmount = VIPPurchaseValue / VIPMaxPurchaseValue;
                float fillAmountTemp = VIPPurchaseValue / VIPMaxPurchaseValue;

                // Update UI elements
                FireImage.rectTransform.anchorMin = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMin.y);
                FireImage.rectTransform.anchorMax = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMax.y);
                FireImage.rectTransform.anchoredPosition = new Vector2(0, 20f);

                if (VIPCountValue >= 3)
                {
                    MainBoostPurchase.DoubleGoldIncomePurchaseButtonVIP();
                }
            }
            if (GemPurchaseId == 4)
            {
                VIPPurchaseValue += 10.0f; // Add points to current VIP Purchase Value

                while (VIPPurchaseValue >= VIPMaxPurchaseValue)
                {
                    VIPPurchaseValue -= VIPMaxPurchaseValue; // Subtract the required points to level up
                    VIPCountValue++; // Increase VIP level

                    // Update max value for the new VIP level - this should ideally be in a separate method
                    switch (VIPCountValue)
                    {
                        case 1: VIPMaxPurchaseValue = 2; break;
                        case 2: VIPMaxPurchaseValue = 4; break;
                        case 3: VIPMaxPurchaseValue = 6; break;
                        case 4: VIPMaxPurchaseValue = 12; break;
                        case 5: VIPMaxPurchaseValue = 18; break;
                        case 6: VIPMaxPurchaseValue = 30; break;
                        case 7: VIPMaxPurchaseValue = 50; break;
                        case 8: VIPMaxPurchaseValue = 80; break;
                        case 9: VIPMaxPurchaseValue = 120; break;
                        case 10: VIPMaxPurchaseValue = 1; break; // Confirm this logic
                        default: VIPMaxPurchaseValue *= 2; break; // Adjust as necessary for higher levels
                    }

                    // Update VIP level text
                    VIPCountValueText.text = "VIP " + VIPCountValue.ToString();
                }

                // Update progress bar after all calculations
                VIPBarImage.fillAmount = VIPPurchaseValue / VIPMaxPurchaseValue;
                float fillAmountTemp = VIPPurchaseValue / VIPMaxPurchaseValue;

                // Update UI elements
                FireImage.rectTransform.anchorMin = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMin.y);
                FireImage.rectTransform.anchorMax = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMax.y);
                FireImage.rectTransform.anchoredPosition = new Vector2(0, 20f);

                if (VIPCountValue >= 3)
                {
                    MainBoostPurchase.DoubleGoldIncomePurchaseButtonVIP();
                }
            }
            if (GemPurchaseId == 5)
            {
                VIPPurchaseValue += 15.0f; // Add points to current VIP Purchase Value

                while (VIPPurchaseValue >= VIPMaxPurchaseValue)
                {
                    VIPPurchaseValue -= VIPMaxPurchaseValue; // Subtract the required points to level up
                    VIPCountValue++; // Increase VIP level

                    // Update max value for the new VIP level - this should ideally be in a separate method
                    switch (VIPCountValue)
                    {
                        case 1: VIPMaxPurchaseValue = 2; break;
                        case 2: VIPMaxPurchaseValue = 4; break;
                        case 3: VIPMaxPurchaseValue = 6; break;
                        case 4: VIPMaxPurchaseValue = 12; break;
                        case 5: VIPMaxPurchaseValue = 18; break;
                        case 6: VIPMaxPurchaseValue = 30; break;
                        case 7: VIPMaxPurchaseValue = 50; break;
                        case 8: VIPMaxPurchaseValue = 80; break;
                        case 9: VIPMaxPurchaseValue = 120; break;
                        case 10: VIPMaxPurchaseValue = 1; break; // Confirm this logic
                        default: VIPMaxPurchaseValue *= 2; break; // Adjust as necessary for higher levels
                    }

                    // Update VIP level text
                    VIPCountValueText.text = "VIP " + VIPCountValue.ToString();
                }

                // Update progress bar after all calculations
                VIPBarImage.fillAmount = VIPPurchaseValue / VIPMaxPurchaseValue;
                float fillAmountTemp = VIPPurchaseValue / VIPMaxPurchaseValue;

                // Update UI elements
                FireImage.rectTransform.anchorMin = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMin.y);
                FireImage.rectTransform.anchorMax = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMax.y);
                FireImage.rectTransform.anchoredPosition = new Vector2(0, 20f);

                if (VIPCountValue >= 3)
                {
                    MainBoostPurchase.DoubleGoldIncomePurchaseButtonVIP();
                }
            }
            if (GemPurchaseId == 6)
            {
                VIPPurchaseValue += 25.0f; // Add points to current VIP Purchase Value

                while (VIPPurchaseValue >= VIPMaxPurchaseValue)
                {
                    VIPPurchaseValue -= VIPMaxPurchaseValue; // Subtract the required points to level up
                    VIPCountValue++; // Increase VIP level

                    // Update max value for the new VIP level - this should ideally be in a separate method
                    switch (VIPCountValue)
                    {
                        case 1: VIPMaxPurchaseValue = 2; break;
                        case 2: VIPMaxPurchaseValue = 4; break;
                        case 3: VIPMaxPurchaseValue = 6; break;
                        case 4: VIPMaxPurchaseValue = 12; break;
                        case 5: VIPMaxPurchaseValue = 18; break;
                        case 6: VIPMaxPurchaseValue = 30; break;
                        case 7: VIPMaxPurchaseValue = 50; break;
                        case 8: VIPMaxPurchaseValue = 80; break;
                        case 9: VIPMaxPurchaseValue = 120; break;
                        case 10: VIPMaxPurchaseValue = 1; break; // Confirm this logic
                        default: VIPMaxPurchaseValue *= 2; break; // Adjust as necessary for higher levels
                    }

                    // Update VIP level text
                    VIPCountValueText.text = "VIP " + VIPCountValue.ToString();
                }

                // Update progress bar after all calculations
                VIPBarImage.fillAmount = VIPPurchaseValue / VIPMaxPurchaseValue;
                float fillAmountTemp = VIPPurchaseValue / VIPMaxPurchaseValue;

                // Update UI elements
                FireImage.rectTransform.anchorMin = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMin.y);
                FireImage.rectTransform.anchorMax = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMax.y);
                FireImage.rectTransform.anchoredPosition = new Vector2(0, 20f);

                if (VIPCountValue >= 3)
                {
                    MainBoostPurchase.DoubleGoldIncomePurchaseButtonVIP();
                }
            }



            /*
           if (GemPurchaseId == 1)
           {
               VIPPurchaseValue = (VIPPurchaseValue + 0.9f);
               // VIPPurchaseValue += 0.9f;
               VIPBarImage.fillAmount = VIPPurchaseValue / VIPMaxPurchaseValue;

               float fillAmountTemp = VIPPurchaseValue / VIPMaxPurchaseValue;

               FireImage.rectTransform.anchorMin = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMin.y);
               FireImage.rectTransform.anchorMax = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMax.y);
               FireImage.rectTransform.anchoredPosition = Vector2.zero;

               if (VIPPurchaseValue >= VIPMaxPurchaseValue)
               {
                   // VIP Function
                   VIPPurchaseValue = 0;
                   VIPCountValue += 1;
                   VIPBarImage.fillAmount = VIPPurchaseValue / VIPMaxPurchaseValue;

                   fillAmountTemp = VIPPurchaseValue;

                   FireImage.rectTransform.anchorMin = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMin.y);
                   FireImage.rectTransform.anchorMax = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMax.y);
                   FireImage.rectTransform.anchoredPosition = Vector2.zero;

                   VIPCountValueText.text = "VIP " + VIPCountValue.ToString();
               }
           }*/
            /*
			VIPPurchaseValue += 1;
            VIPBarImage.fillAmount = VIPPurchaseValue / VIPMaxPurchaseValue;

            float fillAmountTemp = VIPPurchaseValue/10;

            FireImage.rectTransform.anchorMin = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMin.y);
            FireImage.rectTransform.anchorMax = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMax.y);
            FireImage.rectTransform.anchoredPosition = Vector2.zero;

            if (VIPPurchaseValue >= VIPMaxPurchaseValue)
            {
                // VIP Function
                VIPPurchaseValue = 0;
				VIPCountValue += 1;
                VIPBarImage.fillAmount = VIPPurchaseValue / VIPMaxPurchaseValue;

                //float fillAmountTemp = VIPPurchaseValue;

                FireImage.rectTransform.anchorMin = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMin.y);
                FireImage.rectTransform.anchorMax = new Vector2(fillAmountTemp, FireImage.rectTransform.anchorMax.y);
                FireImage.rectTransform.anchoredPosition = Vector2.zero;

				VIPCountValueText.text = "VIP " + VIPCountValue.ToString();
			}*/


            CheckVIPButtonManualUpdate();
        }

        public void VIPActivate()
        {
            VIPCountValue -= 1;

            //Other Function Here

            CheckVIPButtonManualUpdate();
        }

        void CheckPurchaseGemButtonsManualUpdate()
        {
            for (int i = 0; i < GemPurchaseButtons.Length; i++)
            {
                if (MainWallet.GemWalletValue >= 999999999999999)
                {
                    GemPurchaseButtons[i].interactable = false;
                }

                if (MainWallet.GemWalletValue < 999999999999999)
                {
                    GemPurchaseButtons[i].interactable = true;
                }
            }
        }

        //In case you want to improve the system to be only visible after purchase
        void CheckVIPButtonManualUpdate()
        {
            if (VIPCountValue < 0)
            {
                //VIPButton.interactable = false;
            }

            else if (VIPCountValue >= 0) //change this to 1 and uncomment aboves code
            {
                VIPButton.interactable = true;
            }


        }

        // Update is called once per frame
        void Update()
        {
            //Check Max Value for VIP

            if (VIPCountValue == 0)
            {
                VIPMaxPurchaseValue = 1;
            }
            if (VIPCountValue == 1)
            {
                VIPMaxPurchaseValue = 2;
            }
            if (VIPCountValue == 2)
            {
                VIPMaxPurchaseValue = 4;
            }
            if (VIPCountValue == 3)
            {
                VIPMaxPurchaseValue = 6;
            }
            if (VIPCountValue == 4)
            {
                VIPMaxPurchaseValue = 12;
            }
            if (VIPCountValue == 5)
            {
                VIPMaxPurchaseValue = 18;
            }
            if (VIPCountValue == 6)
            {
                VIPMaxPurchaseValue = 30;
            }
            if (VIPCountValue == 7)
            {
                VIPMaxPurchaseValue = 50;
            }
            if (VIPCountValue == 8)
            {
                VIPMaxPurchaseValue = 80;
            }
            if (VIPCountValue == 9)
            {
                VIPMaxPurchaseValue = 120;
            }
            if (VIPCountValue == 10)
            {
                VIPMaxPurchaseValue = 1;
            }

            //VIP PERKS
            if(VIPCountValue >= 2) //VIP 2 PERKS
            {

                for (int i = 0; i < skillsManagerScript.CurrentTimerCountdownValueCollection.Length; i++) //Activate Skills Automatically 
                {
                    if (talentScript.TalentCollectionValue[10] == 1)
                    {
                        if (skillsManagerScript.CurrentTimerCountdownValueCollection[0] < 0.0) //lightning
                        {
                            skillsManagerScript.SkillActivate(0);
                        }
                    }
                    if (talentScript.TalentCollectionValue[11] == 1)
                    {
                        if (skillsManagerScript.CurrentTimerCountdownValueCollection[1] < 0.0) //meteor
                        {
                            skillsManagerScript.SkillActivate(1);
                        }
                    }
                    if (talentScript.TalentCollectionValue[12] == 1)
                    {
                        if (skillsManagerScript.CurrentTimerCountdownValueCollection[2] < 0.0) //ice spikes
                        {
                            skillsManagerScript.SkillActivate(2);
                        }
                    }
                    if (talentScript.TalentCollectionValue[13] == 1)
                    {
                        if (skillsManagerScript.CurrentTimerCountdownValueCollection[3] < 0.0) //scream
                        {
                            skillsManagerScript.SkillActivate(3);
                        }
                    }
                }
            }

            
        }


        public void BuyProductID(string productId)
        {
            if (IsInitialized())
            {
                Product product = m_StoreController.products.WithID(productId);
                if (product != null && product.availableToPurchase)
                {
                    m_StoreController.InitiatePurchase(product);
                }
                else
                {
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase.");
                }
            }
            else
            {
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.Log($"OnInitializeFailed: {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log($"OnInitializeFailed: {error}");
        }

        public void BuyNoAds()
        {
            BuyProductID("noads_purchase");
        }

        public void Buy250Gems()
        {
            BuyProductID("250gems");
        }

        public void Buy1000Gems()
        {
            BuyProductID("1000gems");
        }

        public void Buy2070Gems()
        {
            BuyProductID("2070gems");
        }

        public void Buy5000Gems()
        {
            BuyProductID("5000gems");
        }

        public void Buy10000Gems()
        {
            BuyProductID("10000gems");
        }

        public void Buy25000Gems()
        {
            BuyProductID("25000gems");
        }

        public void Buy50000Gems()
        {
            BuyProductID("50000gems");
        }
    }
}

