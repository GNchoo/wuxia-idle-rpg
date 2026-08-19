using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SAMPLETEXT.Data.Manager.Wallet;
using SAMPLETEXT.Gameplay.Manager.MainHero;
using SAMPLETEXT.SubHeroUI.Manager;
using SAMPLETEXT.Market.Manager;
using SAMPLETEXT.ItemPurchase.Manager.Item;
using SAMPLETEXT.ItemPurchase.Manager.DailyBundle;
using SAMPLETEXT.ItemPurchase.Manager.Boost;
using SAMPLETEXT.Achievement.Manager;
using System.Numerics;

namespace SAMPLETEXT.Wallet.Manager
{
    public class WalletManagerScript : MonoBehaviour
    {
        [SerializeField]
        MarketManagerScript MainMarketManager;
        [SerializeField]
        GameplayMainHeroManagerScript MainHero;
        [SerializeField]
        SubHeroesManagerScript MainSubHero;
        [SerializeField]
        JSONGameplayWalletManagerScript WalletSave;
        [Header("Wallet Settings")]
        [SerializeField]
        TextMeshProUGUI GoldWalletValueText;
        public float GoldWalletValue;
        [SerializeField]
        TextMeshProUGUI GemWalletValueText;
        public float GemWalletValue;
        [SerializeField]
        TextMeshProUGUI DPSWalletValueText;
        public float DPSWalletValue;
        public float MainHeroTotalDPS;
        public float SubHeroTotalDPS;

        [Header("Reset Prestige Settings")]
        [SerializeField]
        float CoinPercentageReset;

        [Header("Check Diamond Settings")]
        [SerializeField]
        ItemsPurchaseHeroManagerScript MainHeroPurchase;
        [SerializeField]
        DailyBundlePurchaseManagerScript MainDailyBundlePurchase;
        [SerializeField]
        ItemsPurchaseArtifactManagerScript MainArtifactPurchase;
        [SerializeField]
        ItemsPurchaseBoostManagerScript MainBoostPurchase;

		[Header("Achievement Settings")]
		[SerializeField]
		AchievementManagerScript MainAchievement;
		public float AchievementGoldWalletValue;
        // Start is called before the first frame update
        void Start()
        {
			//WalletSave = GameObject.FindObjectOfType<JSONGameplayWalletManagerScript>();
			WalletValueManualUpdate();
        }

        public void WalletDataSave()
        {
            WalletSave.SaveFile();
        }

        void WalletUpdate()
        {

        }

        public void WalletValueManualUpdate()
        {
            DPSWalletValue = MainHeroTotalDPS + SubHeroTotalDPS;

            // Limits
            if (GoldWalletValue > 999999999999999)
            {
                GoldWalletValue = 999999999999999;
            }

            if (GemWalletValue > 999999999999999)
            {
                GemWalletValue = 999999999999999;
            }

            if (DPSWalletValue > 999999999999999)
            {
                DPSWalletValue = 999999999999999;
            }

            // Gold
            if (GoldWalletValue <= 999)
            {
                GoldWalletValueText.text = GoldWalletValue.ToString("F0");
            }
            else if (GoldWalletValue <= 999999)
            {
                GoldWalletValueText.text = (GoldWalletValue / 1000f).ToString("F2") + "K";
            }
            else if (GoldWalletValue <= 999999999)
            {
                GoldWalletValueText.text = (GoldWalletValue / 1000000f).ToString("F2") + "M";
            }
            else if (GoldWalletValue <= 999999999999)
            {
                GoldWalletValueText.text = (GoldWalletValue / 1000000000f).ToString("F2") + "B";
            }
            else if (GoldWalletValue <= 999999999999999)
            {
                GoldWalletValueText.text = (GoldWalletValue / 1000000000000f).ToString("F2") + "T";
            }

            // Gem
            if (GemWalletValue <= 999)
            {
                GemWalletValueText.text = GemWalletValue.ToString();
            }
            else if (GemWalletValue <= 999999)
            {
                GemWalletValueText.text = (GemWalletValue / 1000f).ToString("F2") + "K";
            }
            else if (GemWalletValue <= 999999999)
            {
                GemWalletValueText.text = (GemWalletValue / 1000000f).ToString("F2") + "M";
            }
            else if (GemWalletValue <= 999999999999)
            {
                GemWalletValueText.text = (GemWalletValue / 1000000000f).ToString("F2") + "B";
            }
            else if (GemWalletValue <= 999999999999999)
            {
                GemWalletValueText.text = (GemWalletValue / 1000000000000f).ToString("F2") + "T";
            }

            // DPS
            if (DPSWalletValue <= 999)
            {
                DPSWalletValueText.text = DPSWalletValue.ToString("F0");
            }
            else if (DPSWalletValue <= 999999)
            {
                DPSWalletValueText.text = (DPSWalletValue / 1000f).ToString("F2") + "K";
            }
            else if (DPSWalletValue <= 999999999)
            {
                DPSWalletValueText.text = (DPSWalletValue / 1000000f).ToString("F2") + "M";
            }
            else if (DPSWalletValue <= 999999999999)
            {
                DPSWalletValueText.text = (DPSWalletValue / 1000000000f).ToString("F2") + "B";
            }
            else if (DPSWalletValue <= 999999999999999)
            {
                DPSWalletValueText.text = (DPSWalletValue / 1000000000000f).ToString("F2") + "T";
            }

            CheckSubHeroLevelPurchase();
        
    }

        void CheckSubHeroLevelPurchase()
        {
            MainSubHero.CheckPurchaseButton();
            MainHero.CheckDPSUpgradeButton();

            MainDailyBundlePurchase.DailyBundleButtonCheckManualUpdate();
            MainArtifactPurchase.FirstCheck();
            MainBoostPurchase.FirstCheck();
            MainHeroPurchase.ButtonCheckUpdate();
        }

		public void AchievementEarnedGoldQuantityActivate()
		{
			MainAchievement.EarnedGoldQuantityAchievementCheckManualUpdate();
		}

        public void ResetPrestigeGoldActivate()
        {
            float TempCoin = GoldWalletValue * CoinPercentageReset;
            GoldWalletValue = TempCoin;
            WalletValueManualUpdate();
        }

        // Update is called once per frame
        void Update()
        {
          
        }
    }
}

