using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAMPLETEXT.Market.Manager;
using SAMPLETEXT.Market.Manager.Status;
using SAMPLETEXT.ItemPurchase.Manager.Boost;
using SAMPLETEXT.Wallet.Manager;
using SAMPLETEXT.ItemPurchase.Manager.DailyAds;
using SAMPLETEXT.Gameplay.Manager.Enemy;
using SAMPLETEXT.Artifact.Manager;
using SAMPLETEXT.ItemPurchase.Manager.Item;
using SAMPLETEXT.TabCollection.Manager;
using SAMPLETEXT.Data.Manager.Wallet;
using SAMPLETEXT.SubHeroUI.Manager;
using SAMPLETEXT.Talent.Manager;
using TMPro;
using SAMPLETEXT;


namespace SAMPLETEXT
{

    public class Tutorial_Manager : MonoBehaviour
    {
        [Header("Main Tutorial Window")]
        public GameObject tutorialPanel;
        public GameObject skipButton;
        public GameObject VillageButton;
        public GameObject GameModesButton;
        public GameObject GameplayButton;
        public GameObject TeamButton;
        public GameObject shopButton;

        [Header("References")]
        public MarketManagerScript marketManagerScript;
        public WalletManagerScript walletManagerScript;
        public DailyRewardSystem rewardSystemScript;
        public SubHeroesManagerScript heroManagerScript;
        public ArtifactManagerScript artifactManagerScript;
        public TalentsManagerScript talentManagerScript;


        [Header("Conditions")]
        public GameObject shopVillage1;
        public GameObject tab5Panel;
        public bool gaveGemsReward1;
        public GameObject tab5boostTab;
        public GameObject tab5ItemsTab;
        public GameObject tab5gemsTab;
        public bool gaveGemsReward2;
        public bool purchasedHero;
        public bool purchasedArtifact;
        public GameObject gameplayTab;
        public GameObject gameplayNextButtonRef;
        public GameObject teamTab;
        public GameObject mainHeroInventoryTab;
        public GameObject selectHeroTab;
        public GameObject teamUpgradeTab;
        public GameObject artifactTab;
        public GameObject talentsTab;
        public GameObject buttonNextTalents;
        public bool purchasedTalent;
        public GameObject profileTab;

        [Header("TurnedOffGroups")]
        public GameObject[] disableGroup1;
        public GameObject enableItemsButton;
        public GameObject enableBuyGemsButton;
        public GameObject[] enableActiveTeamButtons;
        public GameObject enableArtifactsButton;
        public GameObject enableTalentsButton;
        public GameObject enableProfileButton;


        [Header("Tutorials")]
        public GameObject[] activeTutPanel;

        [Header("Tutorials Save")]
        public bool[] activeTutPanelSave;
        public bool missionStarted;


        // Start is called before the first frame update
        void Start()
        {
            // Load the saved state of MissionStarted
            missionStarted = PlayerPrefs.GetInt("MissionStarted", 0) == 1;
            activeTutPanelSave[0] = PlayerPrefs.GetInt("ActiveTutPanel0", 0) == 1; //intro
            activeTutPanelSave[1] = PlayerPrefs.GetInt("ActiveTutPanel1", 0) == 1; //castle buy building
            activeTutPanelSave[2] = PlayerPrefs.GetInt("ActiveTutPanel2", 0) == 1; //collect money from castle
            activeTutPanelSave[3] = PlayerPrefs.GetInt("ActiveTutPanel3", 0) == 1; //show village quest
            activeTutPanelSave[4] = PlayerPrefs.GetInt("ActiveTutPanel4", 0) == 1; //open tab 5 boosts tab
            activeTutPanelSave[5] = PlayerPrefs.GetInt("ActiveTutPanel5", 0) == 1; //time travel mission
            activeTutPanelSave[6] = PlayerPrefs.GetInt("ActiveTutPanel6", 0) == 1; //items chest mission
            activeTutPanelSave[7] = PlayerPrefs.GetInt("ActiveTutPanel7", 0) == 1; //gems mission
            activeTutPanelSave[8] = PlayerPrefs.GetInt("ActiveTutPanel8", 0) == 1; //gems mission
            activeTutPanelSave[9] = PlayerPrefs.GetInt("ActiveTutPanel9", 0) == 1; //gems mission
            activeTutPanelSave[10] = PlayerPrefs.GetInt("ActiveTutPanel10", 0) == 1; //gems mission
            activeTutPanelSave[11] = PlayerPrefs.GetInt("ActiveTutPanel11", 0) == 1; //gems mission
            activeTutPanelSave[12] = PlayerPrefs.GetInt("ActiveTutPanel12", 0) == 1; //gems mission
            activeTutPanelSave[13] = PlayerPrefs.GetInt("ActiveTutPanel13", 0) == 1; //gems mission
            activeTutPanelSave[14] = PlayerPrefs.GetInt("ActiveTutPanel14", 0) == 1; //gems mission
            activeTutPanelSave[15] = PlayerPrefs.GetInt("ActiveTutPanel15", 0) == 1; //gems mission
            activeTutPanelSave[16] = PlayerPrefs.GetInt("ActiveTutPanel16", 0) == 1; //gems mission
            activeTutPanelSave[17] = PlayerPrefs.GetInt("ActiveTutPanel17", 0) == 1; //gems mission
            activeTutPanelSave[18] = PlayerPrefs.GetInt("ActiveTutPanel18", 0) == 1; //gems mission
            gaveGemsReward1 = PlayerPrefs.GetInt("GaveGemReward1", 0) == 1;
            gaveGemsReward2 = PlayerPrefs.GetInt("GaveGemReward2", 0) == 1;


            //On Start disable all buttons for tutorial
            foreach (GameObject button in disableGroup1)
            {
                button.SetActive(false);
            }


            LoadMissionStartedState();
        }

        public void ActivateByIndex(int index)
        {
            if (index >= 0 && index < activeTutPanel.Length)
            {
                ActivateExclusive(activeTutPanel[index]);
            }
            else
            {
                Debug.LogError("Index out of range: " + index);
            }
        }

        public void ActivateExclusive(GameObject activeObject)
        {
            foreach (GameObject obj in activeTutPanel)
            {
                obj.SetActive(obj == activeObject);
            }
        }

        // Update is called once per frame
        void Update()
        {
            //Open Village CASTLE menu
            if (shopVillage1.activeInHierarchy == true && activeTutPanelSave[0] == false)
            {
                activeTutPanelSave[0] = true;
                activeTutPanel[0].SetActive(false);
                activeTutPanel[1].SetActive(true);
                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(false);
                TeamButton.SetActive(false);
                shopButton.SetActive(false);

                //Disable Buttons System
                //Double Check if they are all disabled in Update method first tutorial
                foreach (GameObject button in disableGroup1)
                {
                    button.SetActive(false);
                }

            }

            //WAit if income is bigger than 0 then activate this quest
            if (marketManagerScript.TotalIncomeValue > 0 && activeTutPanelSave[1] == false)
            {
                activeTutPanelSave[1] = true;
                activeTutPanel[1].SetActive(false);
                activeTutPanel[2].SetActive(true);
                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(false);
                TeamButton.SetActive(false);
                shopButton.SetActive(false);

            }

            //If we closed castle menu window - activate this quest
            if (activeTutPanelSave[1] == true && shopVillage1.activeInHierarchy == false && activeTutPanelSave[2] == false)
            {
                activeTutPanelSave[2] = true;
                activeTutPanel[2].SetActive(false);
                activeTutPanel[3].SetActive(true);
                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(false);
                TeamButton.SetActive(false);
                shopButton.SetActive(true);

            }

            //if We opened Tab 5 window - activate this quest
            if (activeTutPanelSave[2] == true && tab5Panel.activeInHierarchy == true && activeTutPanelSave[3] == false)
            {
                activeTutPanel[3].SetActive(false);
                activeTutPanel[4].SetActive(true);
                activeTutPanelSave[3] = true;

                if (gaveGemsReward1 == false && walletManagerScript.GemWalletValue < 1f)
                {
                    walletManagerScript.GemWalletValue += 1000f;
                    gaveGemsReward1 = true;
                    SaveMissionStartedState();
                }

                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(false);
                TeamButton.SetActive(false);
                shopButton.SetActive(true);

                
            }

            //if We opened Tab 5 window - activate this quest
            if (activeTutPanelSave[3] == true && tab5Panel.activeInHierarchy == true && walletManagerScript.GoldWalletValue >= 45000f && activeTutPanelSave[4] == false)
            {
                activeTutPanel[4].SetActive(false);
                activeTutPanel[5].SetActive(true);
                activeTutPanelSave[4] = true;
                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(false);
                TeamButton.SetActive(false);
                shopButton.SetActive(true);

                //Enable button Items in Shop
                enableItemsButton.SetActive(true);
            }

            //if We opened Tab 5 window - activate this quest
            if (activeTutPanelSave[4] == true && tab5Panel.activeInHierarchy == true && tab5ItemsTab.activeInHierarchy && activeTutPanelSave[5] == false)
            {
                activeTutPanel[5].SetActive(false);
                activeTutPanel[6].SetActive(true);
                activeTutPanelSave[5] = true;

                if (gaveGemsReward2 == false)
                {
                    walletManagerScript.GemWalletValue += 3500f;
                    gaveGemsReward2 = true;
                    SaveMissionStartedState();
                }
                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(false);
                TeamButton.SetActive(false);
                shopButton.SetActive(true);
            }

            //if We opened Tab 5 window - activate this quest
            if (activeTutPanelSave[5] == true && tab5Panel.activeInHierarchy == true && tab5ItemsTab.activeInHierarchy && activeTutPanelSave[6] == false)
            {
                if (gaveGemsReward2 == false)
                {
                    purchasedHero = false;
                }


                for (int i = 0; i < heroManagerScript.SubHeroActive.Count; i++)
                {
                    if (heroManagerScript.SubHeroActive[i] == true)
                    {
                        purchasedHero = true;
                    }
                }

                for (int i = 0; i < artifactManagerScript.ArtifactActiveCollection.Count; i++)
                {
                    if (artifactManagerScript.ArtifactActiveCollection[i] == true)
                    {
                        purchasedArtifact = true;
                    }
                }

                if (purchasedHero == true && purchasedArtifact == true)
                {
                    activeTutPanel[6].SetActive(false);
                    activeTutPanel[7].SetActive(true);
                    activeTutPanelSave[6] = true;

                    SaveMissionStartedState();

                    VillageButton.SetActive(true);
                    GameModesButton.SetActive(false);
                    GameplayButton.SetActive(false);
                    TeamButton.SetActive(false);
                    shopButton.SetActive(true);
                }

                //Enable button Gems in Shop
                enableBuyGemsButton.SetActive(true);

            }

            //quest gems
            if (activeTutPanelSave[6] == true && tab5Panel.activeInHierarchy == true && tab5gemsTab.activeInHierarchy && activeTutPanelSave[7] == false)
            {
                activeTutPanel[7].SetActive(false);
                activeTutPanel[8].SetActive(true);
                activeTutPanelSave[7] = true;

                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(false);
                TeamButton.SetActive(false);
                shopButton.SetActive(true);

                
            }

            //quest village milestone 10x
            if (activeTutPanelSave[7] == true && shopVillage1.activeInHierarchy && marketManagerScript.VillageUpgradeLevel[0] >= 10 && activeTutPanelSave[8] == false)
            {
                activeTutPanel[8].SetActive(false);
                activeTutPanel[9].SetActive(false);
                activeTutPanel[10].SetActive(true);
                activeTutPanelSave[8] = true;

                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(false);
                shopButton.SetActive(true);
            }

            //quest gameplay
            if (activeTutPanelSave[8] == true && gameplayTab.activeInHierarchy && activeTutPanelSave[9] == false)
            {
                skipButton.SetActive(false);
                gameplayNextButtonRef.SetActive(true);
                activeTutPanel[10].SetActive(true);
                activeTutPanelSave[9] = true;

                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);
            }

            //quest first touch with team tab
            if (activeTutPanelSave[9] == true && teamTab.activeInHierarchy && activeTutPanelSave[10] == false)
            {
                skipButton.SetActive(false);
                activeTutPanel[10].SetActive(false);
                activeTutPanel[11].SetActive(true);
                activeTutPanelSave[10] = true;

                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);
            }

            //quest team tab
            if (activeTutPanelSave[9] == true && teamTab.activeInHierarchy && activeTutPanelSave[10] == false)
            {
                skipButton.SetActive(false);
                activeTutPanel[10].SetActive(false);
                activeTutPanel[11].SetActive(true);
                activeTutPanelSave[10] = true;

                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);
            }

            //quest player inventory
            if (activeTutPanelSave[10] == true && teamTab.activeInHierarchy && mainHeroInventoryTab.GetComponent<Canvas>().enabled && activeTutPanelSave[11] == false)
            {
                skipButton.SetActive(false);
                activeTutPanel[11].SetActive(false);
                activeTutPanel[12].SetActive(true);
                activeTutPanelSave[11] = true;

                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);
            }

            //quest closed player inventory
            if (activeTutPanelSave[11] == true && teamTab.activeInHierarchy && mainHeroInventoryTab.GetComponent<Canvas>().enabled == false && activeTutPanelSave[12] == false)
            {
                skipButton.SetActive(false);

                activeTutPanel[12].SetActive(false);
                activeTutPanel[13].SetActive(true);
                activeTutPanelSave[12] = true;

                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);

                //Enable button ActiveTeam
                foreach (GameObject button in enableActiveTeamButtons)
                {
                    button.SetActive(true);
                }
            }
            //quest open team selection
            if (activeTutPanelSave[12] == true && teamTab.activeInHierarchy && selectHeroTab.activeInHierarchy && activeTutPanelSave[13] == false)
            {
                skipButton.SetActive(false);
                activeTutPanel[13].SetActive(false);
                activeTutPanel[14].SetActive(true);
                activeTutPanelSave[13] = true;

                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);
            }
            //quest selection hero menu opened
            if (activeTutPanelSave[13] == true && teamTab.activeInHierarchy && selectHeroTab.activeInHierarchy == false && activeTutPanelSave[14] == false)
            {
                skipButton.SetActive(false);
                activeTutPanel[11].SetActive(false);
                activeTutPanel[14].SetActive(false);
                activeTutPanel[15].SetActive(false);
                activeTutPanel[16].SetActive(true);
                activeTutPanelSave[14] = true;

                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);

                //Enable button Artifacts in Team
                enableArtifactsButton.SetActive(true);
            }

            //quest artifacts opened
            if (activeTutPanelSave[14] == true && teamTab.activeInHierarchy && artifactTab.activeInHierarchy == true && activeTutPanelSave[15] == false)
            {
                skipButton.SetActive(false);
                activeTutPanel[16].SetActive(false);
                activeTutPanel[17].SetActive(true);
                activeTutPanelSave[15] = true;

                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);

                //Enable button Talents in Team
                enableTalentsButton.SetActive(true);
            }
            //quest talents opened
            if (activeTutPanelSave[15] == true && teamTab.activeInHierarchy && talentsTab.activeInHierarchy == true && activeTutPanelSave[16] == false)
            {
                skipButton.SetActive(false);
                activeTutPanel[16].SetActive(false);
                activeTutPanel[17].SetActive(false);
                activeTutPanel[18].SetActive(true);
                activeTutPanelSave[16] = true;

                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);
            }
            //if we spent a talent point - open this quest
            if (activeTutPanelSave[16] == true && teamTab.activeInHierarchy && talentsTab.activeInHierarchy == true && activeTutPanelSave[17] == false)
            {
                for (int i = 0; i < talentManagerScript.TalentCollectionValue.Count; i++)
                {
                    if (talentManagerScript.TalentCollectionValue[i] >= 1)
                    {
                        purchasedTalent = true;
                    }
                }

                if (purchasedTalent == true)
                {
                    skipButton.SetActive(false);
                    activeTutPanel[18].SetActive(false);
                    activeTutPanel[19].SetActive(true);
                    buttonNextTalents.SetActive(true);
                    activeTutPanelSave[17] = true;

                    SaveMissionStartedState();

                    VillageButton.SetActive(true);
                    GameModesButton.SetActive(false);
                    GameplayButton.SetActive(true);
                    TeamButton.SetActive(true);
                    shopButton.SetActive(true);

                    //Enable button Talents in Team
                    enableProfileButton.SetActive(true);
                }
            }

            //if we opened profile window - start this quest
            if (activeTutPanelSave[17] == true && profileTab.activeInHierarchy && activeTutPanelSave[18] == false)
            {
                skipButton.SetActive(false);
                activeTutPanel[19].SetActive(false);
                activeTutPanel[20].SetActive(true);
                activeTutPanelSave[18] = true;

                SaveMissionStartedState();

                VillageButton.SetActive(true);
                GameModesButton.SetActive(true);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);
            }

        }

        public void CompletedTutorial()
        {
            walletManagerScript.GemWalletValue += 5000f;

            //On Complete enable all buttons for tutorial
            foreach (GameObject button in disableGroup1)
            {
                button.SetActive(true);
            }

            VillageButton.SetActive(true);
            GameModesButton.SetActive(true);
            GameplayButton.SetActive(true);
            TeamButton.SetActive(true);
            shopButton.SetActive(true);

            missionStarted = true;
            profileTab.SetActive(false);
            teamTab.SetActive(false);
            SaveMissionStartedState();
            tutorialPanel.SetActive(false);

           

        }

        public void SkipTutorial()
        {

            //On Complete enable all buttons for tutorial
            foreach (GameObject button in disableGroup1)
            {
                button.SetActive(true);
            }


            VillageButton.SetActive(true);
            GameModesButton.SetActive(true);
            GameplayButton.SetActive(true);
            TeamButton.SetActive(true);
            shopButton.SetActive(true);

            missionStarted = true;
            profileTab.SetActive(false);
            teamTab.SetActive(false);
            SaveMissionStartedState();

           

            tutorialPanel.SetActive(false);
        }

        // Function to save the state of MissionStarted
        public void SaveMissionStartedState()
        {
            // Save the state of MissionStarted
            PlayerPrefs.SetInt("MissionStarted", missionStarted ? 1 : 0);

            if (activeTutPanelSave[0] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel0", activeTutPanelSave[0] ? 1 : 0);
            }

            if (activeTutPanelSave[1] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel1", activeTutPanelSave[1] ? 1 : 0);
            }

            if (activeTutPanelSave[2] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel2", activeTutPanelSave[2] ? 1 : 0);
            }

            if (activeTutPanelSave[3] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel3", activeTutPanelSave[3] ? 1 : 0);
            }

            if (activeTutPanelSave[4] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel4", activeTutPanelSave[4] ? 1 : 0);
            }

            if (gaveGemsReward1 == true)
            {
                PlayerPrefs.SetInt("GaveGemReward1", gaveGemsReward1 ? 1 : 0);
            }

            if (activeTutPanelSave[5] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel5", activeTutPanelSave[5] ? 1 : 0);
            }

            if (activeTutPanelSave[6] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel6", activeTutPanelSave[6] ? 1 : 0);
            }
            if (gaveGemsReward2 == true)
            {
                PlayerPrefs.SetInt("GaveGemReward2", gaveGemsReward2 ? 1 : 0);
            }

            if (activeTutPanelSave[7] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel7", activeTutPanelSave[7] ? 1 : 0);
            }
            if (activeTutPanelSave[8] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel8", activeTutPanelSave[8] ? 1 : 0);
            }
            if (activeTutPanelSave[9] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel9", activeTutPanelSave[9] ? 1 : 0);
            }
            if (activeTutPanelSave[10] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel10", activeTutPanelSave[10] ? 1 : 0);
            }
            if (activeTutPanelSave[11] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel11", activeTutPanelSave[11] ? 1 : 0);
            }
            if (activeTutPanelSave[12] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel12", activeTutPanelSave[12] ? 1 : 0);
            }
            if (activeTutPanelSave[13] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel13", activeTutPanelSave[13] ? 1 : 0);
            }
            if (activeTutPanelSave[14] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel14", activeTutPanelSave[14] ? 1 : 0);
            }
            if (activeTutPanelSave[15] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel15", activeTutPanelSave[15] ? 1 : 0);
            }
            if (activeTutPanelSave[16] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel16", activeTutPanelSave[16] ? 1 : 0);
            }
            if (activeTutPanelSave[17] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel17", activeTutPanelSave[17] ? 1 : 0);
            }
            if (activeTutPanelSave[18] == true)
            {
                PlayerPrefs.SetInt("ActiveTutPanel18", activeTutPanelSave[18] ? 1 : 0);
            }

            //Save All
            PlayerPrefs.Save();
        }

        public void LoadMissionStartedState()
        {

            if (missionStarted == true)
            {
                //On Start disable all buttons for tutorial
                foreach (GameObject button in disableGroup1)
                {
                    button.SetActive(true);
                }

                VillageButton.SetActive(true);
                GameModesButton.SetActive(true);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);

                missionStarted = true;
                tutorialPanel.SetActive(false);
            }


            if (activeTutPanelSave[0] == false && !missionStarted) // intro
            {
                tutorialPanel.SetActive(true);
                missionStarted = false;
                ActivateByIndex(0);

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(false);
                TeamButton.SetActive(false);
                shopButton.SetActive(false);

                foreach (GameObject button in disableGroup1)
                {
                    button.SetActive(false);
                }
            }
            else if (activeTutPanelSave[0] == true && activeTutPanelSave[1] == false && !missionStarted) //castle
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(1);
                shopVillage1.SetActive(true); //activate castle if this quest is active

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(false);
                TeamButton.SetActive(false);
                shopButton.SetActive(false);

                foreach (GameObject button in disableGroup1)
                {
                    button.SetActive(false);
                }

            }
            else if (activeTutPanelSave[1] == true && activeTutPanelSave[2] == false && !missionStarted) // close castle quest
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(2);
                shopVillage1.SetActive(true); //activate castle if this quest is active

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(false);
                TeamButton.SetActive(false);
                shopButton.SetActive(true);

                foreach (GameObject button in disableGroup1)
                {
                    button.SetActive(false);
                }
            }
            else if (activeTutPanelSave[2] == true && activeTutPanelSave[3] == false && !missionStarted) //castle closed
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(3);

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(false);
                TeamButton.SetActive(false);
                shopButton.SetActive(true);

                foreach (GameObject button in disableGroup1)
                {
                    button.SetActive(false);
                }

            }
            else if (activeTutPanelSave[3] == true && activeTutPanelSave[4] == false && !missionStarted) //Tab 5 mission before time travel buy
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(4);
                tab5Panel.SetActive(true); //open tab 5 quest if its active
                tab5boostTab.SetActive(true);
                tab5ItemsTab.SetActive(false);
                tab5gemsTab.SetActive(false);


                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(false);
                TeamButton.SetActive(false);
                shopButton.SetActive(true);

                foreach (GameObject button in disableGroup1)
                {
                    button.SetActive(false);
                }
            }
            else if (activeTutPanelSave[4] == true && activeTutPanelSave[5] == false && !missionStarted) //Tab 5 mission boosts after time travel
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(5);
                tab5Panel.SetActive(true); //open tab 5 quest if its active
                tab5boostTab.SetActive(true);
                tab5ItemsTab.SetActive(false);
                tab5gemsTab.SetActive(false);


                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(false);
                TeamButton.SetActive(false);
                shopButton.SetActive(true);

                //Enable button Items in Shop
                enableItemsButton.SetActive(true);


            }
            else if (activeTutPanelSave[5] == true && activeTutPanelSave[6] == false && !missionStarted) //Tab 5 mission items buy chest
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(6);
                tab5Panel.SetActive(true); //open tab 5 quest if its active
                tab5ItemsTab.SetActive(true);
                tab5boostTab.SetActive(false);
                tab5ItemsTab.SetActive(true);
                tab5gemsTab.SetActive(false);


                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(false);
                TeamButton.SetActive(false);
                shopButton.SetActive(true);

                //Enable button Items in Shop
                enableItemsButton.SetActive(true);

            }
            else if (activeTutPanelSave[6] == true && activeTutPanelSave[7] == false && !missionStarted) //Tab 5 mission gem open
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(7);
                tab5Panel.SetActive(true); //open tab 5 quest if its active
                tab5ItemsTab.SetActive(true);
                tab5boostTab.SetActive(false);
                tab5ItemsTab.SetActive(false);
                tab5gemsTab.SetActive(true);


                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(false);
                TeamButton.SetActive(false);
                shopButton.SetActive(true);


                //Enable button Items in Shop
                enableItemsButton.SetActive(true);
                //Enable button Gems in Shop
                enableBuyGemsButton.SetActive(true);
            }
            else if (activeTutPanelSave[7] == true && activeTutPanelSave[8] == false && !missionStarted) //Tab 5 mission open gems tab
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(8);
                tab5Panel.SetActive(true); //open tab 5 quest if its active
                tab5ItemsTab.SetActive(true);
                tab5boostTab.SetActive(false);
                tab5ItemsTab.SetActive(false);
                tab5gemsTab.SetActive(true);


                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(false);
                TeamButton.SetActive(false);
                shopButton.SetActive(true);

                //Enable button Items in Shop
                enableItemsButton.SetActive(true);
                //Enable button Gems in Shop
                enableBuyGemsButton.SetActive(true);

            }
            else if (activeTutPanelSave[8] == true && activeTutPanelSave[9] == false && !missionStarted) //village quest up to 10th milestone market
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(10);


                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(false);
                shopButton.SetActive(true);

                //Enable button Items in Shop
                enableItemsButton.SetActive(true);
                //Enable button Gems in Shop
                enableBuyGemsButton.SetActive(true);
            }
            else if (activeTutPanelSave[9] == true && activeTutPanelSave[10] == false && !missionStarted) //gameplay tab quest
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(10);
                gameplayTab.SetActive(true);
                gameplayNextButtonRef.SetActive(true);
                skipButton.SetActive(false);


                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);

                //Enable button Items in Shop
                enableItemsButton.SetActive(true);
                //Enable button Gems in Shop
                enableBuyGemsButton.SetActive(true);
            }
            else if (activeTutPanelSave[10] == true && activeTutPanelSave[11] == false && !missionStarted) //team tab quest open inventory
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(11);
                teamTab.SetActive(true);
                skipButton.SetActive(false);
                mainHeroInventoryTab.GetComponent<Canvas>().enabled = true;

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);

                //Enable button Items in Shop
                enableItemsButton.SetActive(true);
                //Enable button Gems in Shop
                enableBuyGemsButton.SetActive(true);
            }
            else if (activeTutPanelSave[11] == true && activeTutPanelSave[12] == false && !missionStarted) //team tab quest  close inventory
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(12);
                teamTab.SetActive(true);
                skipButton.SetActive(false);
                mainHeroInventoryTab.GetComponent<Canvas>().enabled = false;

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);

                //Enable button Items in Shop
                enableItemsButton.SetActive(true);
                //Enable button Gems in Shop
                enableBuyGemsButton.SetActive(true);

                //Enable button ActiveTeam
                foreach (GameObject button in enableActiveTeamButtons)
                {
                    button.SetActive(true);
                }
            }
            else if (activeTutPanelSave[12] == true && activeTutPanelSave[13] == false && !missionStarted) //team tab quest 
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(13);
                teamTab.SetActive(true);
                skipButton.SetActive(false);
                mainHeroInventoryTab.GetComponent<Canvas>().enabled = false;

                teamUpgradeTab.SetActive(true);
                artifactTab.SetActive(false);
                talentsTab.SetActive(false);

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);

                //Enable button Items in Shop
                enableItemsButton.SetActive(true);
                //Enable button Gems in Shop
                enableBuyGemsButton.SetActive(true);

                //Enable button ActiveTeam
                foreach (GameObject button in enableActiveTeamButtons)
                {
                    button.SetActive(true);
                }


            }
            else if (activeTutPanelSave[13] == true && activeTutPanelSave[14] == false && !missionStarted) //team tab quest 
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(14);
                teamTab.SetActive(true);
                skipButton.SetActive(false);


                teamUpgradeTab.SetActive(true);
                artifactTab.SetActive(false);
                talentsTab.SetActive(false);

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);

                //Enable button Items in Shop
                enableItemsButton.SetActive(true);
                //Enable button Gems in Shop
                enableBuyGemsButton.SetActive(true);

                //Enable button ActiveTeam
                foreach (GameObject button in enableActiveTeamButtons)
                {
                    button.SetActive(true);
                }


            }
            else if (activeTutPanelSave[14] == true && activeTutPanelSave[15] == false && !missionStarted) //artifact tab
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(16);
                teamTab.SetActive(true);
                skipButton.SetActive(false);
                mainHeroInventoryTab.GetComponent<Canvas>().enabled = false;
                selectHeroTab.SetActive(true);

                teamUpgradeTab.SetActive(false);
                artifactTab.SetActive(true);
                talentsTab.SetActive(false);

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);

                //Enable button Items in Shop
                enableItemsButton.SetActive(true);
                //Enable button Gems in Shop
                enableBuyGemsButton.SetActive(true);

                //Enable button ActiveTeam
                foreach (GameObject button in enableActiveTeamButtons)
                {
                    button.SetActive(true);
                }

                //Enable button Artifacts in Team
                enableArtifactsButton.SetActive(true);
            }
            else if (activeTutPanelSave[15] == true && activeTutPanelSave[16] == false && !missionStarted) //artifact tab
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(17);
                teamTab.SetActive(true);
                skipButton.SetActive(false);

                teamUpgradeTab.SetActive(false);
                artifactTab.SetActive(true);
                talentsTab.SetActive(false);

                VillageButton.SetActive(true);
                GameModesButton.SetActive(false);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);

                //Enable button Items in Shop
                enableItemsButton.SetActive(true);
                //Enable button Gems in Shop
                enableBuyGemsButton.SetActive(true);

                //Enable button ActiveTeam
                foreach (GameObject button in enableActiveTeamButtons)
                {
                    button.SetActive(true);
                }

                //Enable button Artifacts in Team
                enableArtifactsButton.SetActive(true);
            }
            else if (activeTutPanelSave[16] == true && activeTutPanelSave[17] == false && !missionStarted) //artifact tab
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(18);
                teamTab.SetActive(true);
                skipButton.SetActive(false);

                teamUpgradeTab.SetActive(false);
                artifactTab.SetActive(false);
                talentsTab.SetActive(true);

                VillageButton.SetActive(true);
                GameModesButton.SetActive(true);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);

                //Enable button Items in Shop
                enableItemsButton.SetActive(true);
                //Enable button Gems in Shop
                enableBuyGemsButton.SetActive(true);

                //Enable button ActiveTeam
                foreach (GameObject button in enableActiveTeamButtons)
                {
                    button.SetActive(true);
                }

                //Enable button Artifacts in Team
                enableArtifactsButton.SetActive(true);

                //Enable button Talents in Team
                enableTalentsButton.SetActive(true);
            }
            else if (activeTutPanelSave[17] == true && activeTutPanelSave[18] == false && !missionStarted) //profile tab
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(19);
                skipButton.SetActive(false);
                profileTab.SetActive(true);


                VillageButton.SetActive(true);
                GameModesButton.SetActive(true);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);

                //Enable button Items in Shop
                enableItemsButton.SetActive(true);
                //Enable button Gems in Shop
                enableBuyGemsButton.SetActive(true);

                //Enable button ActiveTeam
                foreach (GameObject button in enableActiveTeamButtons)
                {
                    button.SetActive(true);
                }

                //Enable button Artifacts in Team
                enableArtifactsButton.SetActive(true);

                //Enable button Talents in Team
                enableTalentsButton.SetActive(true);

                //Enable button Talents in Team
                enableProfileButton.SetActive(true);
            }
            else if (activeTutPanelSave[18] == true && !missionStarted) //profile tab
            {
                missionStarted = false;
                tutorialPanel.SetActive(true);
                ActivateByIndex(20);
                skipButton.SetActive(false);
                profileTab.SetActive(true);


                VillageButton.SetActive(true);
                GameModesButton.SetActive(true);
                GameplayButton.SetActive(true);
                TeamButton.SetActive(true);
                shopButton.SetActive(true);

                //Enable button Items in Shop
                enableItemsButton.SetActive(true);
                //Enable button Gems in Shop
                enableBuyGemsButton.SetActive(true);

                //Enable button ActiveTeam
                foreach (GameObject button in enableActiveTeamButtons)
                {
                    button.SetActive(true);
                }

                //Enable button Artifacts in Team
                enableArtifactsButton.SetActive(true);

                //Enable button Talents in Team
                enableTalentsButton.SetActive(true);

                //Enable button Talents in Team
                enableProfileButton.SetActive(true);
            }





            //IF ActiveTutPanelSave is true - then activate that GameObject (Load Tutorial State)
        }
    }

}