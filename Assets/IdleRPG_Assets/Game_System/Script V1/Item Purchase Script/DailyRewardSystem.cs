using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SAMPLETEXT.ItemPurchase.Manager.Boost;
using SAMPLETEXT.Wallet.Manager;
using SAMPLETEXT.ItemPurchase.Manager.DailyAds;
using SAMPLETEXT.Gameplay.Manager.Enemy;
using SAMPLETEXT.Artifact.Manager;
using SAMPLETEXT.ItemPurchase.Manager.Item;
using SAMPLETEXT.TabCollection.Manager;
using TMPro;

namespace SAMPLETEXT
{
    public class DailyRewardSystem : MonoBehaviour
    {
        [Header("Reward Buttons")]
        [SerializeField]
        private Button[] rewardButtons; // Array of reward buttons, should be 30 buttons

        [Header("Reward Bonus Settings")]
        [SerializeField]
        private WalletManagerScript MainWallet;
        [SerializeField]
        private ItemsPurchaseBoostManagerScript MainItemPurchaseBoost;
        [SerializeField]
        private GameplayEnemyManagerScript gameplayScript; // Talent Points
        [SerializeField]
        private ItemsPurchaseArtifactManagerScript itemArtifactScript;
        [SerializeField]
        private ItemsPurchaseHeroManagerScript itemHeroScript;
        [SerializeField]
        private Button PayTabButton;
        [SerializeField]
        private Button PayTabItemsButton;
        [SerializeField]
        private TMP_Text dayText; // Reference to the TMP_Text component to display the current day

        [Header("Active Reward Image")]
        [SerializeField]
        private Sprite activeDailyRewardImage; // Image for the active daily reward button
        [SerializeField]
        private Sprite inactiveDailyImage;
        [SerializeField]
        Image collectedImageKnob;

        [Header("Collected Reward Image")]
        [SerializeField]
        private Sprite collectedImage; // Image for the collected reward

        private const string RewardCollectedKey = "RewardCollected_";
        private const string LastActiveDayKey = "LastActiveDay";
        private const string LastLoginDateKey = "LastLoginDate";

        public int currentDay;

        void Start()
        {

            InitializeRewardButtons();
            CheckAndSetCurrentDay();
            CheckAndDisableButtons();
            ChangeTextDay();
        }

        void ChangeTextDay()
        {
            dayText.text = "DAY: " + (currentDay).ToString();
        }

        private void InitializeRewardButtons()
        {
            // Ensure rewardButtons array is properly assigned in the inspector
            if (rewardButtons == null || rewardButtons.Length != 30)
            {
                Debug.LogError("Reward buttons array must have exactly 30 buttons assigned.");
                return;
            }
        }

        private void CheckAndSetCurrentDay()
        {
            string lastLoginDateStr = PlayerPrefs.GetString(LastLoginDateKey, "");
            DateTime lastLoginDate;

            if (DateTime.TryParse(lastLoginDateStr, out lastLoginDate))
            {
                if ((DateTime.Now.Date - lastLoginDate.Date).Days > 1)
                {
                    // Reset to day 1 if more than one day has passed since last login
                    currentDay = 1;
                    PlayerPrefs.SetInt(LastActiveDayKey, 1);
                    // Clear all collected rewards
                    for (int i = 1; i <= 30; i++)
                    {
                        PlayerPrefs.SetInt(RewardCollectedKey + i, 0);
                    }
                }
                else if ((DateTime.Now.Date - lastLoginDate.Date).Days == 1)
                {
                    // Increment the day if exactly one day has passed since last login
                    currentDay = PlayerPrefs.GetInt(LastActiveDayKey, 1) + 1;
                    if (currentDay > 30) currentDay = 1;
                    PlayerPrefs.SetInt(LastActiveDayKey, currentDay);
                }
                else
                {
                    // Same day, cannot collect another reward
                    currentDay = PlayerPrefs.GetInt(LastActiveDayKey, 1);
                }
            }
            else
            {
                // First login or no saved data, start from day 1
                currentDay = 1;
                PlayerPrefs.SetInt(LastActiveDayKey, 1);
            }

            PlayerPrefs.SetString(LastLoginDateKey, DateTime.Now.Date.ToString());
            PlayerPrefs.Save();
        }

        public void CheckAndDisableButtons()
        {
            bool rewardAvailable = false; // Flag to track if there's a reward to collect


            for (int i = 0; i < rewardButtons.Length; i++)
            {
                int buttonIndex = i + 1; // Button index starts from 1 to 30

                if (PlayerPrefs.GetInt(RewardCollectedKey + buttonIndex, 0) == 1)
                {
                    // Reward already collected for this day
                    rewardButtons[i].interactable = false;
                    rewardButtons[i].GetComponent<Image>().sprite = collectedImage;
                }
                else if (buttonIndex == currentDay)
                {
                    // Reward available to collect
                    rewardButtons[i].interactable = true;
                    rewardButtons[i].onClick.RemoveAllListeners(); // Clear previous listeners
                    rewardButtons[i].onClick.AddListener(() => OnRewardButtonClick(buttonIndex));
                    rewardButtons[i].GetComponent<Image>().sprite = activeDailyRewardImage;

                    // Update the dayText to show the current day
                    if (dayText != null)
                    {
                        dayText.text = "DAY " + currentDay;
                    }

                    rewardAvailable = true; // Set flag to true since there's a reward to collect
                }
                else
                {
                    rewardButtons[i].interactable = false;
                    rewardButtons[i].GetComponent<Image>().sprite = inactiveDailyImage; // Reset to default sprite
                }
            }
            // Enable or disable the collectedImageKnob based on reward availability
            if (collectedImageKnob != null)
            {
                collectedImageKnob.enabled = rewardAvailable;
            }
        }

        public void OnRewardButtonClick(int buttonIndex)
        {
            if (PlayerPrefs.GetInt(RewardCollectedKey + buttonIndex, 0) == 1)
            {
                // Reward already collected for this day, do nothing
                return;
            }

            DisableButton(buttonIndex);
            GiveReward(buttonIndex - 1); // Reward type index is 0-based, button index is 1-based
            PlayerPrefs.SetInt(RewardCollectedKey + buttonIndex, 1); // Mark reward as collected
            PlayerPrefs.Save();

            // Disable the collectedImageKnob since the reward is collected
            if (collectedImageKnob != null)
            {
                collectedImageKnob.enabled = false;
            }
        }

        private void DisableButton(int buttonIndex)
        {
            if (buttonIndex <= rewardButtons.Length)
            {
                rewardButtons[buttonIndex - 1].interactable = false; // Button index is 1-based, array is 0-based
                                                                     // rewardButtons[buttonIndex - 1].GetComponent<Image>().sprite = inactiveDailyImage; // Reset to default sprite
                rewardButtons[buttonIndex - 1].GetComponent<Image>().sprite = collectedImage; // Set to collected sprite
            }
        }

        public void GiveReward(int rewardType)
        {
            StartCoroutine(DelayLoad(rewardType));
        }

        private IEnumerator DelayLoad(int rewardType)
        {
            // Your reward logic here
            // yield return new WaitForSeconds(.8f);

            switch (rewardType)
            {
                case 0:
                    // Your reward logic here
                    MainWallet.GemWalletValue += 100f; //GEMS REWARDS
                    Debug.Log("Reward 1 given");
                    break;
                case 1:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    MainWallet.GemWalletValue += 100f;
                    Debug.Log("Reward 2 given");
                    break;
                case 2:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    MainWallet.GemWalletValue += 500f;
                    Debug.Log("Reward 3 given");
                    break;
                case 3:
                    // Your reward logic here
                    //TALENT POINTS
                    gameplayScript.TempPointsCount = gameplayScript.TempPointsCount + 2; //Two talent point
                    Debug.Log("Reward 4 given");
                    break;
                case 4:
                    // Your reward logic here
                    MainWallet.GemWalletValue += 200f; //GEMS REWARDS
                    Debug.Log("Reward 4 given");
                    break;
                case 5:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleVillageIncomePurchaseButton(); // DOUBLE VILLAGE INCOOME REWARD
                    MainWallet.GemWalletValue += 100f;
                    Debug.Log("Reward 5 given");
                    break;
                case 6:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    PayTabItemsButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.2f);
                    itemArtifactScript.PurchaseArtifact(0); //ARTIFACT BLOODY SKULL x1 buy
                    MainWallet.GemWalletValue += 2500f;
                    Debug.Log("Reward 7 given");
                    break;
                case 7:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    PayTabItemsButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.2f);
                    itemHeroScript.PurchaseHero(0); //1 CHEST PURCHASE
                    MainWallet.GemWalletValue += 150f;
                    Debug.Log("Reward 8 given");
                    break;
                case 8:
                    // Your reward logic here
                    MainWallet.GemWalletValue += 500f; //GEMS REWARDS
                    Debug.Log("Reward 9 given");
                    break;
                case 9:
                    // Your reward logic here
                    //PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    MainWallet.GemWalletValue += 200f;
                    Debug.Log("Reward 10 given");
                    break;
                case 10:
                    // Your reward logic here
                    //TALENT POINTS X5
                    gameplayScript.TempPointsCount = gameplayScript.TempPointsCount + 5; //Two talent point
                    Debug.Log("Reward 11 given");
                    break;
                case 11:
                    // Your reward logic here
                    //PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    MainWallet.GemWalletValue += 1000f;
                    Debug.Log("Reward 12 given");
                    break;
                case 12:
                    // Your reward logic here
                    MainWallet.GemWalletValue += 1000f; //GEMS REWARDS
                    Debug.Log("Reward 13 given");
                    break;
                case 13:
                    // Your reward logic here
                    //PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleVillageIncomePurchaseButton(); // DOUBLE VILLAGE INCOOME REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleVillageIncomePurchaseButton(); // DOUBLE VILLAGE INCOOME REWARD
                    MainWallet.GemWalletValue += 200f;
                    Debug.Log("Reward 14 given");
                    break;
                case 14:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    PayTabItemsButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.2f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.2f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    MainWallet.GemWalletValue += 5000f;
                    Debug.Log("Reward 15 given");
                    break;
                case 15:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    PayTabItemsButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.2f);
                    itemHeroScript.PurchaseHero(1); //1 CHEST PURCHASE
                    MainWallet.GemWalletValue += 1350f;
                    Debug.Log("Reward 16 given");
                    break;
                case 16:
                    // Your reward logic here

                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    MainWallet.GemWalletValue += 400f;

                    Debug.Log("Reward 17 given");
                    break;
                case 17:
                    // Your reward logic here
                    MainWallet.GemWalletValue += 2000f; //GEMS REWARDS
                    Debug.Log("Reward 18 given");
                    break;
                case 18:
                    // Your reward logic here
                    //PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    MainWallet.GemWalletValue += 2000f;
                    Debug.Log("Reward 19 given");
                    break;
                case 19:
                    // Your reward logic here
                    //TALENT POINTS X8
                    gameplayScript.TempPointsCount = gameplayScript.TempPointsCount + 8; //Two talent point
                    Debug.Log("Reward 20 given");
                    break;
                case 20:
                    // Your reward logic here
                    MainWallet.GemWalletValue += 4000f; //GEMS REWARDS
                    Debug.Log("Reward 21 given");
                    break;
                case 21:
                    // Your reward logic here
                    //PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleVillageIncomePurchaseButton(); // DOUBLE VILLAGE INCOOME REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleVillageIncomePurchaseButton(); // DOUBLE VILLAGE INCOOME REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleVillageIncomePurchaseButton(); // DOUBLE VILLAGE INCOOME REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleVillageIncomePurchaseButton(); // DOUBLE VILLAGE INCOOME REWARD
                    MainWallet.GemWalletValue += 400f;
                    Debug.Log("Reward 22 given");
                    break;
                case 22:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    PayTabItemsButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.2f);
                    itemArtifactScript.PurchaseArtifact(0); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.2f);
                    itemArtifactScript.PurchaseArtifact(0); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.2f);
                    itemArtifactScript.PurchaseArtifact(0); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.2f);
                    itemArtifactScript.PurchaseArtifact(0); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.2f);
                    itemArtifactScript.PurchaseArtifact(0); //ARTIFACT DEFLATION  x1 buy
                    MainWallet.GemWalletValue += 12500f;
                    Debug.Log("Reward 23 given");
                    break;
                case 23:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    PayTabItemsButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.2f);
                    itemHeroScript.PurchaseHero(1); //1 CHEST PURCHASE
                    MainWallet.GemWalletValue += 1350f;
                    Debug.Log("Reward 24 given");
                    break;
                case 24:
                    // Your reward logic here
                    MainWallet.GemWalletValue += 8000f; //GEMS REWARDS
                    Debug.Log("Reward 25 given");
                    break;
                case 25:
                    // Your reward logic here
                    //PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.15f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.15f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.15f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.15f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.15f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.15f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.15f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.15f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    MainWallet.GemWalletValue += 800f;
                    Debug.Log("Reward 26 given");
                    break;
                case 26:
                    // Your reward logic here
                    //PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    MainWallet.GemWalletValue += 4000f;
                    Debug.Log("Reward 27 given");
                    break;
                case 27:
                    // Your reward logic here
                    gameplayScript.TempPointsCount = gameplayScript.TempPointsCount + 10; //Two talent point
                                                                                          //TALENT POINTS X10
                    Debug.Log("Reward 28 given");
                    break;
                case 28:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    PayTabItemsButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.2f);
                    itemHeroScript.PurchaseHero(1); //1 CHEST PURCHASE
                    MainWallet.GemWalletValue += 1350f;
                    Debug.Log("Reward 29 given");
                    break;
                case 29:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    PayTabItemsButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    MainWallet.GemWalletValue += 25000f;
                    Debug.Log("Reward 30 given");
                    break;
                default:
                    Debug.LogWarning("Invalid reward type");
                    break;
            }
        }

        void Update()
        {
            // This method is no longer needed since we handle day changes in Start
        }
    }



    /*using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;
    using ItemPurchase.Manager.Boost;
    using Wallet.Manager;
    using ItemPurchase.Manager.DailyAds;
    using Gameplay.Manager.Enemy;
    using Artifact.Manager;
    using ItemPurchase.Manager.Item;
    using TabCollection.Manager;
    using SampleText.;

    public class DailyRewardSystem : MonoBehaviour
    {
        [Header("Reward Buttons")]
        [SerializeField]
        private Button[] rewardButtons; // Array of reward buttons, should be 30 buttons

        private const string RewardCollectedKey = "RewardCollected_";
        private const string LastActiveDayKey = "LastActiveDay";

        [Header("Reward Bonus Settings")]
        [SerializeField]
        WalletManagerScript MainWallet;
        [SerializeField]
        ItemsPurchaseBoostManagerScript MainItemPurchaseBoost;
        [SerializeField]
        GameplayEnemyManagerScript gameplayScript; //Talent Points

        [SerializeField]
        ItemsPurchaseArtifactManagerScript itemArtifactScript;
        [SerializeField]
        ItemsPurchaseHeroManagerScript itemHeroScript;
        [SerializeField]
        Button PayTabButton;
        [SerializeField]
        Button PayTabItemsButton;

        [SerializeField]
        private TMP_Text dayText; // Reference to the TMP_Text component to display the current day



        [Header("Active Reward Image")]
        [SerializeField]
        private Sprite activeDailyRewardImage; // Image for the active daily reward button
        [SerializeField]
        private Sprite inactiveDailyImage;

        void Start()
        {
            InitializeRewardButtons();
            CheckAndDisableButtons();
        }

        private void InitializeRewardButtons()
        {
            // Ensure rewardButtons array is properly assigned in the inspector
            if (rewardButtons == null || rewardButtons.Length != 30)
            {
                Debug.LogError("Reward buttons array must have exactly 30 buttons assigned.");
                return;
            }
        }

        private void CheckAndDisableButtons()
        {
            int currentDayOfMonth = DateTime.Now.Day;

            for (int i = 0; i < rewardButtons.Length; i++)
            {
                int buttonIndex = i + 1; // Button index starts from 1 to 30

                // Disable previously collected rewards
                if (PlayerPrefs.GetInt(RewardCollectedKey + buttonIndex, 0) == 1)
                {
                    rewardButtons[i].interactable = false;

                    // Reset the background image to the default
                    rewardButtons[i].GetComponent<Image>().sprite = inactiveDailyImage; // Replace null with your default sprite if needed
                }
                else if (buttonIndex == currentDayOfMonth)
                {
                    rewardButtons[i].interactable = true;
                    rewardButtons[i].onClick.RemoveAllListeners(); // Clear previous listeners
                    rewardButtons[i].onClick.AddListener(() => OnRewardButtonClick(buttonIndex));

                    // Set the background image to activeDailyRewardImage
                    rewardButtons[i].GetComponent<Image>().sprite = activeDailyRewardImage;

                    // Update the dayText to show the current day
                    if (dayText != null)
                    {
                        dayText.text = "DAY " + currentDayOfMonth;
                    }


                }
                else
                {
                    rewardButtons[i].interactable = false;
                    // Reset the background image to the default
                    rewardButtons[i].GetComponent<Image>().sprite = inactiveDailyImage; // Replace null with your default sprite if needed
                }
            }

            // Save the last active day to reset the system if the day changes
            PlayerPrefs.SetInt(LastActiveDayKey, currentDayOfMonth);
            PlayerPrefs.Save();
        }

        public void OnRewardButtonClick(int buttonIndex)
        {
            DisableButton(buttonIndex);
            GiveReward(buttonIndex - 1); // Reward type index is 0-based, button index is 1-based
        }

        private void DisableButton(int buttonIndex)
        {
            if (buttonIndex <= rewardButtons.Length)
            {
                rewardButtons[buttonIndex - 1].interactable = false; // Button index is 1-based, array is 0-based
                rewardButtons[buttonIndex - 1].GetComponent<Image>().sprite = inactiveDailyImage; // Reset to default sprite
                PlayerPrefs.SetInt(RewardCollectedKey + buttonIndex, 1);
                PlayerPrefs.Save();
            }
        }

        private void GiveReward(int rewardType)
        {
            StartCoroutine(DelayLoad(rewardType));
        }

        private IEnumerator DelayLoad(int rewardType)
        {
           // yield return new WaitForSeconds(.8f);

            switch (rewardType)
            {
                case 0:
                    // Your reward logic here
                    MainWallet.GemWalletValue += 100f; //GEMS REWARDS
                    Debug.Log("Reward 1 given");
                    break;
                case 1:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    MainWallet.GemWalletValue += 100f;
                    Debug.Log("Reward 2 given");
                    break;
                case 2:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    MainWallet.GemWalletValue += 500f;
                    Debug.Log("Reward 3 given");
                    break;
                case 3:
                    // Your reward logic here
                    //TALENT POINTS
                    gameplayScript.TempPointsCount = gameplayScript.TempPointsCount + 2; //Two talent point
                    Debug.Log("Reward 4 given");
                    break;
                case 4:
                    // Your reward logic here
                    MainWallet.GemWalletValue += 200f; //GEMS REWARDS
                    Debug.Log("Reward 4 given");
                    break;
                case 5:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleVillageIncomePurchaseButton(); // DOUBLE VILLAGE INCOOME REWARD
                    MainWallet.GemWalletValue += 100f;
                    Debug.Log("Reward 5 given");
                    break;
                case 6:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    PayTabItemsButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.2f);
                    itemArtifactScript.PurchaseArtifact(0); //ARTIFACT BLOODY SKULL x1 buy
                    MainWallet.GemWalletValue += 2500f;
                    Debug.Log("Reward 7 given");
                    break;
                case 7:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    PayTabItemsButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.2f);
                    itemHeroScript.PurchaseHero(0); //1 CHEST PURCHASE
                    MainWallet.GemWalletValue += 150f;
                    Debug.Log("Reward 8 given");
                    break;
                case 8:
                    // Your reward logic here
                    MainWallet.GemWalletValue += 500f; //GEMS REWARDS
                    Debug.Log("Reward 9 given");
                    break;
                case 9:
                    // Your reward logic here
                    //PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    MainWallet.GemWalletValue += 200f;
                    Debug.Log("Reward 10 given");
                    break;
                case 10:
                    // Your reward logic here
                    //TALENT POINTS X5
                    gameplayScript.TempPointsCount = gameplayScript.TempPointsCount + 5; //Two talent point
                    Debug.Log("Reward 11 given");
                    break;
                case 11:
                    // Your reward logic here
                    //PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    MainWallet.GemWalletValue += 1000f;
                    Debug.Log("Reward 12 given");
                    break;
                case 12:
                    // Your reward logic here
                    MainWallet.GemWalletValue += 1000f; //GEMS REWARDS
                    Debug.Log("Reward 13 given");
                    break;
                case 13:
                    // Your reward logic here
                    //PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleVillageIncomePurchaseButton(); // DOUBLE VILLAGE INCOOME REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleVillageIncomePurchaseButton(); // DOUBLE VILLAGE INCOOME REWARD
                    MainWallet.GemWalletValue += 200f;
                    Debug.Log("Reward 14 given");
                    break;
                case 14:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    PayTabItemsButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.2f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.2f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    MainWallet.GemWalletValue += 5000f;
                    Debug.Log("Reward 15 given");
                    break;
                case 15:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    PayTabItemsButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.2f);
                    itemHeroScript.PurchaseHero(1); //1 CHEST PURCHASE
                    MainWallet.GemWalletValue += 1350f;
                    Debug.Log("Reward 16 given");
                    break;
                case 16:
                    // Your reward logic here

                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    MainWallet.GemWalletValue += 400f;

                    Debug.Log("Reward 17 given");
                    break;
                case 17:
                    // Your reward logic here
                    MainWallet.GemWalletValue += 2000f; //GEMS REWARDS
                    Debug.Log("Reward 18 given");
                    break;
                case 18:
                    // Your reward logic here
                    //PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    MainWallet.GemWalletValue += 2000f;
                    Debug.Log("Reward 19 given");
                    break;
                case 19:
                    // Your reward logic here
                    //TALENT POINTS X8
                    gameplayScript.TempPointsCount = gameplayScript.TempPointsCount + 8; //Two talent point
                    Debug.Log("Reward 20 given");
                    break;
                case 20:
                    // Your reward logic here
                    MainWallet.GemWalletValue += 4000f; //GEMS REWARDS
                    Debug.Log("Reward 21 given");
                    break;
                case 21:
                    // Your reward logic here
                    //PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleVillageIncomePurchaseButton(); // DOUBLE VILLAGE INCOOME REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleVillageIncomePurchaseButton(); // DOUBLE VILLAGE INCOOME REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleVillageIncomePurchaseButton(); // DOUBLE VILLAGE INCOOME REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.DoubleVillageIncomePurchaseButton(); // DOUBLE VILLAGE INCOOME REWARD
                    MainWallet.GemWalletValue += 400f;
                    Debug.Log("Reward 22 given");
                    break;
                case 22:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    PayTabItemsButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.2f);
                    itemArtifactScript.PurchaseArtifact(0); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.2f);
                    itemArtifactScript.PurchaseArtifact(0); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.2f);
                    itemArtifactScript.PurchaseArtifact(0); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.2f);
                    itemArtifactScript.PurchaseArtifact(0); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.2f);
                    itemArtifactScript.PurchaseArtifact(0); //ARTIFACT DEFLATION  x1 buy
                    MainWallet.GemWalletValue += 12500f;
                    Debug.Log("Reward 23 given");
                    break;
                case 23:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    PayTabItemsButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.2f);
                    itemHeroScript.PurchaseHero(1); //1 CHEST PURCHASE
                    MainWallet.GemWalletValue += 1350f;
                    Debug.Log("Reward 24 given");
                    break;
                case 24:
                    // Your reward logic here
                    MainWallet.GemWalletValue += 8000f; //GEMS REWARDS
                    Debug.Log("Reward 25 given");
                    break;
                case 25:
                    // Your reward logic here
                    //PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.15f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.15f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.15f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.15f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.15f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.15f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.15f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    yield return new WaitForSeconds(0.15f);
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton(); //DOUBLE DAMAGE REWARD
                    MainWallet.GemWalletValue += 800f;
                    Debug.Log("Reward 26 given");
                    break;
                case 26:
                    // Your reward logic here
                    //PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    yield return new WaitForSeconds(0.2f);
                    MainItemPurchaseBoost.TimeTravelPurchaseButton(); //TIME TRAVEL REWARD
                    MainWallet.GemWalletValue += 4000f;
                    Debug.Log("Reward 27 given");
                    break;
                case 27:
                    // Your reward logic here
                    gameplayScript.TempPointsCount = gameplayScript.TempPointsCount + 10; //Two talent point
                    //TALENT POINTS X10
                    Debug.Log("Reward 28 given");
                    break;
                case 28:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    PayTabItemsButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.2f);
                    itemHeroScript.PurchaseHero(1); //1 CHEST PURCHASE
                    MainWallet.GemWalletValue += 1350f;
                    Debug.Log("Reward 29 given");
                    break;
                case 29:
                    // Your reward logic here
                    PayTabButton.onClick.Invoke(); // Call the OnClick method of PayTabButton
                    PayTabItemsButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    yield return new WaitForSeconds(0.1f);
                    itemArtifactScript.PurchaseArtifact(1); //ARTIFACT DEFLATION  x1 buy
                    MainWallet.GemWalletValue += 25000f;
                    Debug.Log("Reward 30 given");
                    break;
                default:
                    Debug.LogWarning("Invalid reward type");
                    break;
            }
        }

        void Update()
        {
            // Check if the day has changed
            int lastActiveDay = PlayerPrefs.GetInt(LastActiveDayKey, -1);
            int currentDayOfMonth = DateTime.Now.Day;

            if (lastActiveDay != currentDayOfMonth)
            {
                CheckAndDisableButtons();
            }
        }
    }
    */
}