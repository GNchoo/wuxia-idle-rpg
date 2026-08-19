using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAMPLETEXT.ItemPurchase.Manager.Boost;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using SAMPLETEXT.Wallet.Manager;


namespace SAMPLETEXT.ItemPurchase.Manager.DailyAds
{
    public class RewardsSystem : MonoBehaviour
    {
        [Header("Ads Bonus Settings")]
        [SerializeField]
        WalletManagerScript MainWallet;
        [SerializeField]
        ItemsPurchaseBoostManagerScript MainItemPurchaseBoost;

        //REWARD TYPES
        [Header("Reward Types")]
        public int[] rewardTypes;
        [SerializeField] public Button[] rewardButtons;
        [SerializeField] public Image notCollectedKnob;
        [SerializeField] public Image[] messagesNotCollectedKnob;
        [SerializeField] public Button collectAllButton;  // New Collect All button

        private const string RewardCollectedKey = "RewardCollected";

        [Header("Achievment Note")]
        public bool collectedReward;


        void Start()
        {
            //FirstCheck();
            InitializeRewardButtons();

            // Check if the reward has already been collected for each button
            for (int i = 0; i < rewardButtons.Length; i++)
            {
                int buttonIndex = i;
                if (PlayerPrefs.GetInt(RewardCollectedKey + buttonIndex, 0) == 1)
                {
                    rewardButtons[buttonIndex].interactable = false;
                    messagesNotCollectedKnob[buttonIndex].enabled = false;
                }
                else
                {
                    rewardButtons[buttonIndex].onClick.AddListener(() => OnRewardButtonClick(buttonIndex));
                }
            }

            collectAllButton.onClick.AddListener(CollectAllRewards);
            if (rewardButtons[0].interactable == false && rewardButtons[1].interactable == false
                && rewardButtons[2].interactable == false && rewardButtons[3].interactable == false)
            {
                notCollectedKnob.enabled = false;
            }
            else
            {
                notCollectedKnob.enabled = true;
            }

            UpdateCollectAllButton();
        }

        private void InitializeRewardButtons()
        {
            // Ensure rewardButtons array is properly assigned in the inspector
            if (rewardButtons == null || rewardButtons.Length == 0)
            {
                Debug.LogWarning("Reward buttons not assigned in the inspector.");
                return;
            }
        }

        private void OnRewardButtonClick(int buttonIndex)
        {
            DisableButton(buttonIndex);
        }

        public void DisableButton(int buttonIndex)
        {
            if (buttonIndex < rewardButtons.Length)
            {
                rewardButtons[buttonIndex].interactable = false;
                messagesNotCollectedKnob[buttonIndex].enabled = false;
                PlayerPrefs.SetInt(RewardCollectedKey + buttonIndex, 1);
                PlayerPrefs.Save();
            }

            if (rewardButtons[0].interactable == false && rewardButtons[1].interactable == false
                && rewardButtons[2].interactable == false && rewardButtons[3].interactable == false)
            {
                notCollectedKnob.enabled = false;
            }
            else
            {
                notCollectedKnob.enabled = true;
            }

            UpdateCollectAllButton();
        }


        public void GiveReward(int rewardType)
        {
            collectedReward = true;
            DelayLoad(rewardType);
        }

        public void DelayLoad(int rewardType)
        {
           // yield return new WaitForSeconds(.8f);

            switch (rewardType)
            {
                case 0: //1000 GEMS REWARD
                    MainWallet.GemWalletValue += 1000f;
                    break;
                case 1:
                    MainWallet.GemWalletValue += 100f;
                    MainItemPurchaseBoost.DoubleGoldIncomePurchaseButton();
                    break;
                case 2:
                    MainWallet.GemWalletValue += 100f;
                    MainItemPurchaseBoost.DoubleVillageIncomePurchaseButton();
                    break;
                case 3:
                    MainWallet.GemWalletValue += 500f;
                    MainItemPurchaseBoost.TimeTravelPurchaseButton();
                    break;
                default:
                    Debug.LogWarning("Invalid reward type");
                    break;
            }

            // Disable the button and save the state
            if (rewardType < rewardButtons.Length)
            {
                rewardButtons[rewardType].interactable = false;
                PlayerPrefs.SetInt(RewardCollectedKey + rewardType, 1);
                PlayerPrefs.Save();
            }



        }

        public void CollectAllRewards()
        {
            for (int i = 0; i < rewardButtons.Length; i++)
            {
                if (rewardButtons[i].interactable)
                {
                    GiveReward(i);
                    DisableButton(i);
                }
            }

            if (rewardButtons[0].interactable == false && rewardButtons[1].interactable == false
               && rewardButtons[2].interactable == false && rewardButtons[3].interactable == false)
            {
                notCollectedKnob.enabled = false;
            }
            else
            {
                notCollectedKnob.enabled = true;
            }
        }

        private void UpdateCollectAllButton()
        {
            bool anyInteractable = false;
            for (int i = 0; i < rewardButtons.Length; i++)
            {
                if (rewardButtons[i].interactable)
                {
                    anyInteractable = true;
                    break;
                }
            }
            collectAllButton.gameObject.SetActive(anyInteractable);
        }
    }
}

