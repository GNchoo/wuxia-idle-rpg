using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SAMPLETEXT.Wallet.Manager;
using SAMPLETEXT.Data.Manager.Achievement;
using SAMPLETEXT.Gameplay.Manager.Enemy;
using SAMPLETEXT.Market.Manager;
using SAMPLETEXT.ItemPurchase.Manager.DailyAds;
using SAMPLETEXT.Account.Manager;
using SAMPLETEXT.Artifact.Manager;
using SAMPLETEXT.SubHeroUI.Manager;
using TMPro;

namespace SAMPLETEXT
{
    public class Achievment_NewSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] WalletManagerScript walletScript;
        [SerializeField] MarketManagerScript marketScript;
        [SerializeField] RewardsSystem postScript;
        [SerializeField] ArtifactManagerScript artifactScript;
        [SerializeField] SubHeroesManagerScript subHeroScript;

        [Header("Collection Achievment")]
        public Button[] achievmentButtons;
        public int[] rewardCollectionAchievment;
        public TMP_Text[] priceTextCollection;
        public TMP_Text[] quantityTextCollection;
        public bool[] canCollectAchievment;
        public bool[] collectedAchievment;
        public TMP_Text[] collectpriceTextCollection;

        [Header("Collection Active")]
        public bool manyBuildingsCollect; // 0 no
        public bool collectedGemsCollect; // 1 no
        public bool earnedGoldCollect; // 2 no
        public bool firstBuildingCollect; // 3 yes 
        public bool overallDamageCollect; // 4 no 
        public bool maximumHeroEvolveCollect;// 5 no
        public bool collectFromPostCollect;// 6 yes 
        public bool collectTalentPointsCollect; // 7 yes
        public bool artifactCollect; // 8 yes
        public bool collectedArtifactCountsCollect; // 9 no
        public bool firstPrestigeCollect; // 10 yes
        public bool collectAllHeroesCollect; // 11 yes
        public bool firstHeroCollect; // 12 yes
        public bool collectedHeroesCollect; //13 no
        public bool kingOfTheCastleCollect; // 14 yes
        public bool legendaryHeroCollect; // 15 yes
        public bool buildingUpgradesCollect; // 16 no
        public bool unlockAchievmentCollect; // 17 yes
        public bool heroesUpgradeDiamonCollect; //18 no
        public bool heroesMaxDpsCollect; // 19 no
        public bool collectCalendarCollect; // 20 yes
        public bool wavesCollectionCollect; // 21 no

        [Header("Target For Achievments")]
        public int[] achievmentCollectionValue;

        [SerializeField]
        Image collectedImageKnob;

        void Start()
        {
            // Ensure arrays are initialized with the correct length if they are not already initialized
            collectedAchievment = new bool[22];
            canCollectAchievment = new bool[22];

            // Load collectedAchievment and canCollectAchievment arrays
            for (int i = 0; i < collectedAchievment.Length; i++)
            {
                collectedAchievment[i] = PlayerPrefs.GetInt("collectedAchievment" + i, 0) == 1;
                canCollectAchievment[i] = PlayerPrefs.GetInt("canCollectAchievment" + i, 0) == 1;
            }

            // Load integer values
            for (int i = 0; i < achievmentCollectionValue.Length; i++)
            {
                achievmentCollectionValue[i] = PlayerPrefs.GetInt("AchievmentValue" + i, 0);
            }

            FirstCheck();
        }

        public void FirstCheck()
        {
            for (int i = 0; i < achievmentButtons.Length; i++)
            {
                if (canCollectAchievment[i] == true)
                {
                    achievmentButtons[i].interactable = true;
                }
                else
                {
                    achievmentButtons[i].interactable = false;
                }

                UpdateText();
            }
        }

        public void ButtonCollectionActive()
        {
            for (int i = 0; i < achievmentButtons.Length; i++)
            {
                if (canCollectAchievment[i] == true)
                {
                    achievmentButtons[i].interactable = true;
                }
                else
                {
                    achievmentButtons[i].interactable = false;
                }
            }
        }

        public void UpdateText()
        {
            bool showKnob = false; // Flag to determine if the knob should be shown

            for (int i = 0; i < achievmentButtons.Length; i++)
            {
                //Price Text
                priceTextCollection[i].text = rewardCollectionAchievment[i].ToString("F0");

                if (collectedAchievment[i] == true)
                {
                    collectpriceTextCollection[i].gameObject.SetActive(false);
                    priceTextCollection[i].text = "COLLECTED";
                    //canCollectAchievment[i] = false;
                    achievmentButtons[i].interactable = false;
                }
                else
                {
                    collectpriceTextCollection[i].gameObject.SetActive(true); // Ensure this is visible if not collected
                }

                // Check if canCollectAchievment and collectedAchievment do not match
                if (canCollectAchievment[i] && !collectedAchievment[i])
                {
                    showKnob = true; // Set the flag to true if any pair doesn't match
                }

            }
            // Update the knob's visibility based on the showKnob flag
            collectedImageKnob.enabled = showKnob;



            quantityTextCollection[3].text = achievmentCollectionValue[3] + " / " + "1";
            quantityTextCollection[6].text = achievmentCollectionValue[6] + " / " + "1";
            quantityTextCollection[7].text = achievmentCollectionValue[7] + " / " + "1";
            quantityTextCollection[8].text = achievmentCollectionValue[8] + " / " + "1";
            quantityTextCollection[10].text = achievmentCollectionValue[10] + " / " + "1";
            quantityTextCollection[11].text = achievmentCollectionValue[11] + " / " + "1";
            quantityTextCollection[12].text = achievmentCollectionValue[12] + " / " + "1";
            quantityTextCollection[14].text = achievmentCollectionValue[14] + " / " + "1";
            quantityTextCollection[15].text = achievmentCollectionValue[15] + " / " + "1";
            quantityTextCollection[17].text = achievmentCollectionValue[17] + " / " + "1";
            quantityTextCollection[20].text = achievmentCollectionValue[20] + " / " + "1";


            SaveAchievments();

        }

        public void SaveAchievments()
        {

            // Save integer values
            for (int i = 0; i < achievmentCollectionValue.Length; i++)
            {
                PlayerPrefs.SetInt("AchievmentValue" + i, achievmentCollectionValue[i]);
            }

            // Save collectedAchievment and canCollectAchievment arrays
            for (int i = 0; i < collectedAchievment.Length; i++)
            {
                PlayerPrefs.SetInt("collectedAchievment" + i, collectedAchievment[i] ? 1 : 0);
                PlayerPrefs.SetInt("canCollectAchievment" + i, canCollectAchievment[i] ? 1 : 0);
            }

            PlayerPrefs.Save(); // Always call this to write to disk
        }

        public void CollectFirstBuildingButton()
        {

            if (canCollectAchievment[3] == true)
            {
                achievmentCollectionValue[3] += 1;
                walletScript.GemWalletValue = walletScript.GemWalletValue + rewardCollectionAchievment[3];
                collectedAchievment[3] = true;
            }
            UnlockAchievmentCheck();
            FirstCheck();
        }

        public void CollectFromPostButton()
        {
            if (canCollectAchievment[6] == true)
            {
                achievmentCollectionValue[6] += 1;
                walletScript.GemWalletValue = walletScript.GemWalletValue + rewardCollectionAchievment[6];
                collectedAchievment[6] = true;
            }
            UnlockAchievmentCheck();
            FirstCheck();
        }

        public void CollectTalentPointButton()
        {
            if (canCollectAchievment[7] == true)
            {
                achievmentCollectionValue[7] += 1;
                walletScript.GemWalletValue = walletScript.GemWalletValue + rewardCollectionAchievment[7];
                collectedAchievment[7] = true;
            }
            UnlockAchievmentCheck();
            FirstCheck();
        }

        public void CollectTalentPointPrestigeCheck()
        {
            canCollectAchievment[7] = true;
            canCollectAchievment[10] = true;
            FirstCheck();
        }

        public void ArtifactCollectButton()
        {
            if (canCollectAchievment[8] == true)
            {
                achievmentCollectionValue[8] += 1;
                walletScript.GemWalletValue = walletScript.GemWalletValue + rewardCollectionAchievment[8];
                collectedAchievment[8] = true;
            }
            UnlockAchievmentCheck();
            FirstCheck();
        }

        public void ArtifactCollectedCheck()
        {
            canCollectAchievment[8] = true;
            FirstCheck();
        }

        public void FirstPrestigeCollectButton()
        {
            if (canCollectAchievment[10] == true)
            {
                achievmentCollectionValue[10] += 1;
                walletScript.GemWalletValue = walletScript.GemWalletValue + rewardCollectionAchievment[10];
                collectedAchievment[10] = true;
            }
            UnlockAchievmentCheck();
            FirstCheck();
        }

        public void CollectAllHeroesButton()
        {
            if (canCollectAchievment[11] == true)
            {
                achievmentCollectionValue[11] += 1;
                walletScript.GemWalletValue = walletScript.GemWalletValue + rewardCollectionAchievment[11];
                collectedAchievment[11] = true;
            }
            UnlockAchievmentCheck();
            FirstCheck();
        }

        public void FirstHeroCollectButton()
        {
            if (canCollectAchievment[12] == true)
            {
                achievmentCollectionValue[12] += 1;
                walletScript.GemWalletValue = walletScript.GemWalletValue + rewardCollectionAchievment[12];
                collectedAchievment[12] = true;
            }
            UnlockAchievmentCheck();
            FirstCheck();
        }

        public void FirstHeroCheck()
        {
            canCollectAchievment[12] = true;
            FirstCheck();
        }

        public void KingOfTheCastleCollectButton()
        {
            if (canCollectAchievment[14] == true)
            {
                achievmentCollectionValue[14] += 1;
                walletScript.GemWalletValue = walletScript.GemWalletValue + rewardCollectionAchievment[14];
                collectedAchievment[14] = true;
            }
            UnlockAchievmentCheck();
            FirstCheck();
        }

        public void LegendaryHeroCollectButton()
        {
            if (canCollectAchievment[15] == true)
            {
                achievmentCollectionValue[15] += 1;
                walletScript.GemWalletValue = walletScript.GemWalletValue + rewardCollectionAchievment[15];
                collectedAchievment[15] = true;
            }
            UnlockAchievmentCheck();
            FirstCheck();
        }


        public void UnlockAchievmentButton()
        {
            if (canCollectAchievment[17] == true)
            {
                achievmentCollectionValue[17] += 1;
                walletScript.GemWalletValue = walletScript.GemWalletValue + rewardCollectionAchievment[17];
                collectedAchievment[17] = true;
            }

            FirstCheck();
        }

        public void UnlockAchievmentCheck()
        {
            canCollectAchievment[17] = true;
            FirstCheck();
        }

        public void CollectCalendarButton()
        {
            if (canCollectAchievment[20] == true)
            {
                achievmentCollectionValue[20] += 1;
                walletScript.GemWalletValue = walletScript.GemWalletValue + rewardCollectionAchievment[20];
                collectedAchievment[20] = true;
            }
            UnlockAchievmentCheck();
            FirstCheck();
        }

        public void CollectCalendarCheck()
        {
            canCollectAchievment[20] = true;
            FirstCheck();
        }

        // Update is called once per frame
        void Update()
        {
            //FIRST BUILDING
            if (marketScript.TotalIncomeValue > 20 && collectedAchievment[3] == false)
            {
                canCollectAchievment[3] = true;
                FirstCheck();

            }
            else
            {
                canCollectAchievment[3] = false;
            }


            //POST
            if (postScript.collectedReward == true && collectedAchievment[6] == false)
            {
                canCollectAchievment[6] = true;
                FirstCheck();
            }
            else
            {
                canCollectAchievment[6] = false;
            }

            if (subHeroScript.SubHeroActive[8] || subHeroScript.SubHeroActive[22] || subHeroScript.SubHeroActive[28] || subHeroScript.SubHeroActive[36] || subHeroScript.SubHeroActive[37]
                || subHeroScript.SubHeroActive[38] || subHeroScript.SubHeroActive[40] || subHeroScript.SubHeroActive[41] || subHeroScript.SubHeroActive[43] || subHeroScript.SubHeroActive[46])
            {
                canCollectAchievment[15] = true;
                FirstCheck();
            }
            else
            {
                canCollectAchievment[15] = false;
            }




        }
    }
}