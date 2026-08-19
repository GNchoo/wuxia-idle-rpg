using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAMPLETEXT.ItemPurchase.Manager.Boost;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using SAMPLETEXT.Wallet.Manager;
using SAMPLETEXT.ItemPurchase.Manager.Gem;
using System;
using TMPro;

namespace SAMPLETEXT.ItemPurchase.Manager.DailyAds
{
    // ADDED: IUnityAdsInitializationListener
    public class AdsFreeDailyGiftManagerScript : MonoBehaviour,
        IUnityAdsInitializationListener,
        IUnityAdsLoadListener,
        IUnityAdsShowListener
    {
        [Header("Ads Bonus Settings")]
        [SerializeField] WalletManagerScript MainWallet;
        [SerializeField] ItemsPurchaseBoostManagerScript MainItemPurchaseBoost;
        public int WatchAdsCount;
        [SerializeField] int WatchAdsDefaultValue = 5;
        [SerializeField] TextMeshProUGUI WatchAdsCountText;
        [SerializeField] Button WatchAddsButton;

        [Header("Ads Control Settings")]
        [SerializeField] bool ActivateAds = true;
        [SerializeField] string gameID;
        [SerializeField] string placement;
        [SerializeField] bool testMode = false;

        [Header("VIP SYSTEM")]
        public DiamondPurchaseManagerScript gemScript;

        private bool _adsInitialized = false;

        void Start()
        {
            if (ActivateAds)
            {
#if UNITY_ANDROID || UNITY_IOS
                // NOWA WERSJA: z listenerem „this”
                Advertisement.Initialize(gameID, testMode, this);
#else
                Debug.Log("[AdsFreeDailyGiftManager] Ads disabled on this platform.");
#endif
            }

            CheckAndResetAdsCount();
            FirstCheck();
        }

        // =========================================
        // INITIALIZATION LISTENER
        // =========================================

        public void OnInitializationComplete()
        {
            Debug.Log("[AdsFreeDailyGiftManager] Unity Ads initialization complete.");
            _adsInitialized = true;

            // Ładujemy reklamy dopiero po init
            LoadAd();
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.LogError($"[AdsFreeDailyGiftManager] Unity Ads initialization failed: {error} - {message}");
            _adsInitialized = false;
        }

        // =========================================
        // LOAD / SHOW
        // =========================================

        // Load the ad
        void LoadAd()
        {
            if (!ActivateAds)
                return;

            if (!_adsInitialized)
            {
                Debug.LogWarning("[AdsFreeDailyGiftManager] Cannot load ad, Ads not initialized yet.");
                return;
            }

            Advertisement.Load(placement, this);
        }

        // Show the ad if it's loaded
        void ShowAd()
        {
            if (!ActivateAds)
                return;

            if (!_adsInitialized)
            {
                Debug.LogWarning("[AdsFreeDailyGiftManager] Cannot show ad, Ads not initialized yet.");
                return;
            }

            Advertisement.Show(placement, this);
        }

        // Called when an ad has been successfully loaded
        public void OnUnityAdsAdLoaded(string placementId)
        {
            Debug.Log("[AdsFreeDailyGiftManager] Ad successfully loaded: " + placementId);
        }

        // Called when there was an error loading the ad
        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            Debug.Log($"[AdsFreeDailyGiftManager] Failed to load ad {placementId}: {error} - {message}");
        }

        // Called when an ad successfully shows
        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
            {
                Debug.Log("[AdsFreeDailyGiftManager] Ad finished successfully, rewarding player.");
                StartCoroutine(RunAds());
            }
        }

        // Called when the ad fails to show
        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            Debug.Log($"[AdsFreeDailyGiftManager] Failed to show ad {placementId}: {error} - {message}");
        }

        // Called when the ad starts showing
        public void OnUnityAdsShowStart(string placementId)
        {
            Debug.Log("[AdsFreeDailyGiftManager] Ad started showing: " + placementId);
        }

        // Called when the user clicks on the ad
        public void OnUnityAdsShowClick(string placementId)
        {
            Debug.Log("[AdsFreeDailyGiftManager] Ad clicked: " + placementId);
        }

        IEnumerator RunAds()
        {
            yield return new WaitForSeconds(.5f);
            // Reward the player after the ad has finished
            MainWallet.GemWalletValue += 200f;
            FirstCheck();
        }

        public void FirstCheck()
        {
            if (WatchAdsCount >= 1)
            {
                WatchAdsCountText.text = "WATCH ADS" + "\n" + WatchAdsCount + " Left";
                WatchAddsButton.interactable = true;
            }

            if (WatchAdsCount <= 0)
            {
                WatchAdsCountText.text = "COLLECTED TODAY";
                WatchAddsButton.interactable = false;
            }
        }

        public void WatchAdsActivate()
        {
            if (WatchAdsCount <= 0)
                return;

            if (gemScript != null && gemScript.VIPCountValue >= 2)
            {
                WatchAdsCount -= 1;
                StartCoroutine(RunAds());
            }
            else
            {
                ShowAd();
                WatchAdsCount -= 1;
                StartCoroutine(DelayLoad());
            }

            PlayerPrefs.SetInt("WatchAdsCount", WatchAdsCount);
            PlayerPrefs.Save();
        }

        IEnumerator DelayLoad()
        {
            yield return new WaitForSeconds(.8f);
            LoadAd();  // Reload ad after watching
        }

        void CheckAndResetAdsCount()
        {
            string lastResetDate = PlayerPrefs.GetString("LastResetDate", string.Empty);
            string currentDate = System.DateTime.Now.ToString("yyyy-MM-dd");

            if (lastResetDate != currentDate)
            {
                WatchAdsCount = WatchAdsDefaultValue;
                PlayerPrefs.SetInt("WatchAdsCount", WatchAdsCount);
                PlayerPrefs.SetString("LastResetDate", currentDate);
                PlayerPrefs.Save();
            }
            else
            {
                WatchAdsCount = PlayerPrefs.GetInt("WatchAdsCount", WatchAdsDefaultValue);
            }
        }
    }
}
