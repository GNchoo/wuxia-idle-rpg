using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SAMPLETEXT.Gameplay.Manager.MainHero;
using SAMPLETEXT.Wallet.Manager;
using SAMPLETEXT.Talent.Manager;
using SAMPLETEXT.Achievement.Manager;
using System;
using SAMPLETEXT.ItemPurchase.Manager.Boost;
using SAMPLETEXT.Artifact.Manager;
using SAMPLETEXT.FieldAttack;

namespace SAMPLETEXT.SubHeroUI.Manager
{
    public class SubHeroesManagerScript : MonoBehaviour
    {
        [SerializeField]
        GameplayMainHeroManagerScript MainHero;
        public List<bool> SubHeroActive = new List<bool>(); // New JSON
        public List<float> SubHeroItemCount = new List<float>(); //JSON
        public List<GameObject> SubHeroUIObj = new List<GameObject>(); //JSON
        public List<TextMeshProUGUI> SubHeroDPSUIText = new List<TextMeshProUGUI>();
        public List<float> SubHeroAttackCount = new List<float>();
        [SerializeField]
        List<float> SubHeroDefaultAttackCount = new List<float>();//NonJSON
        public List<float> SubHeroEvolveRequirements = new List<float>();
        public List<int> SubHeroEvolveLevelID = new List<int>(); // New JSON
        public List<int> SubHeroEvolveRequirementsID = new List<int>(); // New JSON
        public List<Button> SubHeroButtonCollection = new List<Button>();
       

        [SerializeField]
        public GameObject HeroActiveParentObj;
        [SerializeField]
        public GameObject[] backgroundRaritySelection;
        [SerializeField]
        GameObject HeroNonActiveParentObj;
        [SerializeField]
        Sprite HeroActiveImage;
        [SerializeField]
        Sprite HeroNonActiveImage;

        [Header ("Sub Hero Reference Settings")]
        public List<float> SubHeroLegendaryReferenceDPS = new List<float>();
        public List<float> SubHeroEpicReferenceDPS = new List<float>();
        public List<float> SubHeroRareReferenceDPS = new List<float>();
        public List<float> SubHeroCommonReferenceDPS = new List<float>();

        public List<float> SubHeroLegendaryReferenceRequireEvolve = new List<float>();
        public List<float> SubHeroEpicReferenceRequireEvolve = new List<float>();
        public List<float> SubHeroRareReferenceRequireEvolve  = new List<float>();
        public List<float> SubHeroCommonReferenceRequireEvolve = new List<float>();
        enum SubHeroType
        {
            Legendary,
            Epic,
            Rare,
            Common
        };
        [SerializeField]
        List<SubHeroType> SubHeroReferenceType = new List<SubHeroType>();

        [Header("PopUp Window Settings")]
        [SerializeField]
        GameObject PopUpWindowObj;

        [Header("New System Popup Quick Equip")]
        // Assuming the following elements are already serialized in your existing script
        [SerializeField] public GameObject popUpPanel; // Your new popup panel
                                                       // UI Components linked via the Unity Inspector
        public Button equipButton;
        public Button detailsButton;
        public Button closeButton;
        public TextMeshProUGUI equipButtonText;

        public int currentHeroID = -1; // Tracks the currently selected hero ID

        //DESCRIPTIONS FOR HERO
        public TMP_Text descriptionText;
        public TMP_Text descriptionTextActive;
        // Dictionary to store description texts for each hero
        private Dictionary<int, string> descriptionDatabase = new Dictionary<int, string>();

        [SerializeField]
        Image PopUpImage;
        [SerializeField]
        Sprite[] HeroImageCollection;
        [SerializeField]
        TextMeshProUGUI HeroNameText;
        [SerializeField]
        string[] HeroNameCollection;
        int ActiveHeroID;
        [SerializeField]
        TextMeshProUGUI SubHeroDPSVaueText;
        [SerializeField]
        GameObject[] StarsCollectionObj;
        [SerializeField]
        TextMeshProUGUI EvolveDetailsValueText;
        [SerializeField]
        GameObject LabelCollectedContent;
        [SerializeField]
        string[] AnimationCollection;
        [SerializeField]
        Image[] PreviewCollection;
        //[SerializeField]
        //Animator PopUpWindowAnim;
        [SerializeField]
        Image PopUpWindowAnim;
        [SerializeField]
        Image PopUpCircleBGImage;

        [Header("Equip Settings")]
        [SerializeField]
        Button EquipButton;
        [SerializeField]
        TextMeshProUGUI EquipButtonText;
        [SerializeField]
        Image[] SlotListImageCollection;
        [SerializeField]
        Sprite SlotListDefaultImage;
        [SerializeField]
        GameObject[] SlotListCoverImageObj;
        public List<int> SubHeroActiveImageIDCollection = new List<int>(); //JSON
        [SerializeField]
        Button[] SubHeroActiveButtonCollection;
        [SerializeField]
        GameObject[] SubHeroRarityActive;
        public List<bool> SubHeroActiveHeroConditionCollection = new List<bool>(); //JSON
        public List<float> SubHeroActiveAttack = new List<float>(); // JSON
        [SerializeField]
        TextMeshProUGUI[] SubHeroActiveAttackTextDisplay;
        [SerializeField]
        public List<FieldAttackScript> SubHeroFieldAttackScriptCollection = new List<FieldAttackScript>();
        public float PlayerActiveCountInField;
        [SerializeField]
        FieldAttackScript[] SubHeroFieldScript;

        [Header("Equip Sub Hero Front Page Settings")]
        [SerializeField]
        Image[] SubHeroFrontImageCollection;
        [SerializeField]
        Sprite SubHeroFrontImageDefault;
        [SerializeField]
        Button[] SubHeroFrontButtonCollection;
        [SerializeField]
        GameObject[] FrontSlotListCoverImageObj;
        [SerializeField]
        TextMeshProUGUI[] SubHeroFrontActiveAttackTextDisplay;
        [SerializeField]
        TextMeshProUGUI[] SubHeroFrontActiveNameTextDisplay;
        [SerializeField]
        Animator[] SubHeroSlotAnimator;
        [SerializeField]
        Animator[] SubHeroFrontSlotAnimator;
        

        [Header("Sub Hero Field Settings")]
        [SerializeField]
        GameObject[] SubHeroInFieldObj;
        [SerializeField]
        SpriteRenderer[] SubHeroInFieldSprite;
        [SerializeField]
        string[] AnimationFieldCollection;
        [SerializeField]
        Animator[] SubHeroFieldAnimator;
        public List<float> SubHeroAnimatorAttackSpeed = new List<float>(); // New JSON
        [SerializeField]
        Animator[] SubHeroAnimatorInField;
        [SerializeField]
        public float[] SubHeroAttackSpeedCollection;
        [SerializeField]
        public float[] SubHeroAttackSpeedCollectionDefault;


        [Header("UnEquip Settings")]
        [SerializeField]
        TextMeshProUGUI SubHeroNameSlotText;
        [SerializeField]
        Image PopUpSlotImage;
        [SerializeField]
        GameObject UnEquipPopUpWindowObj;
        [SerializeField]
        Button UnEquipButton;
        public int UnEquipButtonID;
        [SerializeField]
        TextMeshProUGUI UnequipSubHeroDPSVaueText;
        [SerializeField]
        GameObject[] UnequipStarsCollectionObj;
        [SerializeField]
        TextMeshProUGUI UnequipEvolveDetailsValueText;
        public List<int> UnEquipActiveHeroID = new List<int>();// New JSON
        //[SerializeField]
        //Animator UnEquipPopUpWindowAnim;
        [SerializeField]
        Image UnEquipPopUpWindowAnim;
        [SerializeField]
        Image UnEquipPopUpCircleBGImage;

        [Header("TAB4 BG Hero Rarity")]
        [SerializeField] Image UnEquipPopUpCircleBGImage1;
        [SerializeField] Image UnEquipPopUpCircleBGImage2;
        [SerializeField] Image UnEquipPopUpCircleBGImage3;
        [SerializeField] Image UnEquipPopUpCircleBGImage4;

        [Header("Bonus Rarity Show (Circles)")]
        [SerializeField] GameObject circleImage1;
        [SerializeField] GameObject circleImage2;
        [SerializeField] GameObject circleImage3;
        [SerializeField] GameObject circleImage12;
        [SerializeField] GameObject circleImage22;
        [SerializeField] GameObject circleImage32;

        [Header("Evolve Button Settings")]
        [SerializeField]
        Button[] EvolveButton;
        [SerializeField]
        float SubHeroTotalAttackValue;

        [Header("Front Slot Sub Hero Upgrade Settings")]
        [SerializeField]
        WalletManagerScript MainWallet;
        [SerializeField]
        TextMeshProUGUI[] SubHeroFrontSlotUpgradeText;
        [SerializeField]
        Button[] SubHeroFrontSlotButtonCollection;
        public List<float> SubHeroDPSBaseLevel = new List<float>();
        public List<int> SubHeroLevel = new List<int>();
        public List<float> SubHeroLevelPurchaseCost = new List<float>();
        [SerializeField]
        TextMeshProUGUI[] SubHeroLevelText;
        

        [Header("Sub Hero Upgrade Bonus Text Settings")]
        [SerializeField]
        TextMeshProUGUI UnequipPopUpWindowMileStoneText;
        [SerializeField]
        TextMeshProUGUI EquipPopUpWindowMileStoneText;
        [SerializeField]
        int DefaultSubHeroLevel;
        [SerializeField]
        float DefaultSubHeroLevelPurchaseCost;
        [SerializeField]
        float DefaultSubHeroLevelMilestoneTarget;
        [SerializeField]
        float DefaultSubHeroDPSBaseLevel;
        public List<float> SubHeroTempLevelCount = new List<float>();//New JSON
        public List<float> SubHeroDPSBonusDamage = new List<float>();// New JSON
        public List<float> SubHeroLevelMilestoneTarget = new List<float>();// New JSON;

        [Header("Sub Hero Upgrade Collection Settings")]
        public int SubHeroUpgradeID;
        [SerializeField]
        GameObject[] SubHeroUpgradeButtonCoverCollection;
        [SerializeField]
        float[] SubHeroLevelPurchaseCostTemp;

        [Header("Group Purchase Settings")]
        [SerializeField]
        float[] TempMaxPurchaseGroupValue;
        [SerializeField]
        int[] FinalTempMaxPurchaseGroupValue;
        [SerializeField]
        float[] TempMaxSubHeroLevelPurchaseCostGroupValue;

        [Header("Skill Settings")]
        [SerializeField]
        TalentsManagerScript MainTalent;
        [SerializeField]
        ArtifactManagerScript artifactScript;

		[Header("Achievement Settings")]
		[SerializeField]
		AchievementManagerScript MainAchievement;

        //DRYMARTI
        [Header("Double Damage")]
       [SerializeField] ItemsPurchaseBoostManagerScript boostPurchaseScript;
        [SerializeField] Image[] activeTeamBackground;

       


        //ACTIVE TEAM FIX COLOUR BG
        [Header("Hero Rarity Colors")]
        public Color legendaryColor = new Color(255, 150, 0, 255); // Example: Orange
        public Color epicColor = new Color(28, 0, 128, 255); // Example: Purple
        public Color rareColor = new Color32(75, 178, 250, 250); // Example: Blue
        public Color commonColor = Color.green; // Example: Green


        // Start is called before the first frame update
        void Start()
        {
                    FirstCheck();
            ChangeBackgroundRarityColour(); // DRYMARTI Fix
                                            // Initialize listeners for the popup buttons
            equipButton.onClick.AddListener(ToggleEquipStatus);
            detailsButton.onClick.AddListener(ShowDetails);
            closeButton.onClick.AddListener(ClosePopup);

            popUpPanel.SetActive(false); // Ensure the popup is hidden initially
            InitializeDescriptionDatabase();

        }

        // Function to open the popup and set up necessary data
        public void OpenPopup(int heroID)
        {
            currentHeroID = heroID;
            popUpPanel.SetActive(true);
        }

        private void ToggleEquipStatus()
        {
            ActiveHeroID = currentHeroID;
            EquipSubHero();  // Equip the hero with currentHeroID
            ClosePopup();  // Close the popup after equipping
        }

        private void ShowDetails()
        {
            DisplaySubHeroDetails(currentHeroID);
            ClosePopup();  // Optionally close the popup after showing details
        }

        private void ClosePopup()
        {
            popUpPanel.SetActive(false);
        }


        public void FirstCheck()
        {
            //for (int i = 0; i < SubHeroUIObj.Count; i++)
            for (int i = 0; i < SubHeroActive.Count; i++)
            {
                //if (SubHeroItemCount[i] >= 1)
                if (SubHeroActive[i] == true)
                {
                    SubHeroUIObj[i].transform.parent = HeroActiveParentObj.transform;

                    SubHeroUIObj[i].transform.SetSiblingIndex(0);
                   SubHeroButtonCollection[i].interactable = true;
                    Image AITemp = SubHeroUIObj[i].GetComponent<Image>();
                    AITemp.sprite = HeroActiveImage;
                    SubHeroDPSUIText[i].gameObject.SetActive(true);
                    backgroundRaritySelection[i].SetActive(true);
                }

                //if (SubHeroItemCount[i] <= 0)
                if (SubHeroActive[i] == false)
                {
                    SubHeroUIObj[i].transform.parent = HeroNonActiveParentObj.transform;
                    SubHeroButtonCollection[i].interactable = false;
                    Image NAITemp = SubHeroUIObj[i].GetComponent<Image>();
                    NAITemp.sprite = HeroNonActiveImage;
                    SubHeroDPSUIText[i].gameObject.SetActive(false);
                    backgroundRaritySelection[i].SetActive(false);
                }
            }

            float MainAttack = 0;
            //Attack Value and Current Evolve Requirement Value
            for (int a = 0; a < SubHeroReferenceType.Count; a++)
            {
                if (SubHeroReferenceType[a] == SubHeroType.Epic)
                {
                    float TempAttack = SubHeroEpicReferenceDPS[SubHeroEvolveLevelID[a]] + SubHeroDPSBaseLevel[a] + SubHeroDPSBonusDamage[a];
                    SubHeroAttackCount[a] = TempAttack;
                    float TempDefaultAttack = (SubHeroEpicReferenceDPS[SubHeroEvolveLevelID[a]] + DefaultSubHeroDPSBaseLevel);
                    SubHeroDefaultAttackCount[a] = TempDefaultAttack;
                    MainAttack = TempAttack;
                    
                    SubHeroEvolveRequirements[a] = SubHeroEpicReferenceRequireEvolve[SubHeroEvolveLevelID[a]];
                }

                if (SubHeroReferenceType[a] == SubHeroType.Common)
                {
                    float TempAttack = SubHeroCommonReferenceDPS[SubHeroEvolveLevelID[a]] + SubHeroDPSBaseLevel[a] + SubHeroDPSBonusDamage[a];
                    SubHeroAttackCount[a] = TempAttack;
                    float TempDefaultAttack = (SubHeroEpicReferenceDPS[SubHeroEvolveLevelID[a]] + DefaultSubHeroDPSBaseLevel);
                    SubHeroDefaultAttackCount[a] = TempDefaultAttack;
                    MainAttack = TempAttack;

                    SubHeroEvolveRequirements[a] = SubHeroCommonReferenceRequireEvolve[SubHeroEvolveLevelID[a]];
                }

                if (SubHeroReferenceType[a] == SubHeroType.Legendary)
                {
                    float TempAttack = SubHeroLegendaryReferenceDPS[SubHeroEvolveLevelID[a]] + SubHeroDPSBaseLevel[a] + SubHeroDPSBonusDamage[a];
                    SubHeroAttackCount[a] = TempAttack;
                    float TempDefaultAttack = (SubHeroEpicReferenceDPS[SubHeroEvolveLevelID[a]] + DefaultSubHeroDPSBaseLevel);
                    SubHeroDefaultAttackCount[a] = TempDefaultAttack;
                    MainAttack = TempAttack;

                    SubHeroEvolveRequirements[a] = SubHeroLegendaryReferenceRequireEvolve[SubHeroEvolveLevelID[a]];
                }

                if (SubHeroReferenceType[a] == SubHeroType.Rare)
                {
                    float TempAttack = SubHeroRareReferenceDPS[SubHeroEvolveLevelID[a]] + SubHeroDPSBaseLevel[a];
                    SubHeroAttackCount[a] = TempAttack;
                    float TempDefaultAttack = (SubHeroEpicReferenceDPS[SubHeroEvolveLevelID[a]] + DefaultSubHeroDPSBaseLevel);
                    SubHeroDefaultAttackCount[a] = TempDefaultAttack;
                    MainAttack = TempAttack ;

                    SubHeroEvolveRequirements[a] = SubHeroRareReferenceRequireEvolve[SubHeroEvolveLevelID[a]];
                }
            }

            for (int a = 0; a < SubHeroDPSUIText.Count; a++)
            {

                if (boostPurchaseScript.ActivateDoubleGoldIncomeCondition == false) //double damage inactive
                {
                    float TempAttack = SubHeroAttackCount[a] + SubHeroDPSBaseLevel[a] + SubHeroDPSBonusDamage[a] + MainTalent.TotalAttackValueSubHero + (SubHeroAttackCount[a] * artifactScript.TrippleSwordTotalValue);

                    if (TempAttack <= 999)
                    {
                        SubHeroDPSUIText[a].text = "DPS: " + TempAttack.ToString("F0");
                    }
                    else if (TempAttack <= 999999)
                    {
                        SubHeroDPSUIText[a].text = "DPS: " + (TempAttack / 1000f).ToString("F0") + "K";
                    }
                    else if (TempAttack <= 999999999)
                    {
                        SubHeroDPSUIText[a].text = "DPS: " + (TempAttack / 1000000f).ToString("F0") + "M";
                    }
                    else if (TempAttack <= 9999999999)
                    {
                        SubHeroDPSUIText[a].text = "DPS: " + (TempAttack / 1000000000f).ToString("F0") + "B";
                    }
                    else if (TempAttack <= 999999999999999)
                    {
                        SubHeroDPSUIText[a].text = "DPS: " + (TempAttack / 1000000000000f).ToString("F0") + "T";
                    }
                }
                else //double damage
                {
                    float TempAttack = (SubHeroAttackCount[a] + SubHeroDPSBaseLevel[a] + SubHeroDPSBonusDamage[a] + MainTalent.TotalAttackValueSubHero + (SubHeroAttackCount[a] * artifactScript.TrippleSwordTotalValue)) * 2;

                    if (TempAttack <= 999)
                    {
                        SubHeroDPSUIText[a].text = "DPS: " + TempAttack.ToString("F0") + " X2";
                    }
                    else if (TempAttack <= 999999)
                    {
                        SubHeroDPSUIText[a].text = "DPS: " + (TempAttack / 1000f).ToString("F0") + "K" + " X2";
                    }
                    else if (TempAttack <= 999999999)
                    {
                        SubHeroDPSUIText[a].text = "DPS: " + (TempAttack / 1000000f).ToString("F0") + "M" + " X2";
                    }
                    else if (TempAttack <= 9999999999)
                    {
                        SubHeroDPSUIText[a].text = "DPS: " + (TempAttack / 1000000000f).ToString("F0") + "B" + " X2";
                    }
                    else if (TempAttack <= 999999999999999)
                    {
                        SubHeroDPSUIText[a].text = "DPS: " + (TempAttack / 1000000000000f).ToString("F0") + "T" + " X2";
                    }
                }
                //float TempAttack = MainAttack;

                ResetSpeedTalent();
               

            }

            for (int i = 0; i < SubHeroLevelPurchaseCost.Count; i++)
            {
                SubHeroLevelPurchaseCostTemp[i] = SubHeroLevelPurchaseCost[i];
            }

            CheckUnEquipSubButton();
            CheckEquipLoadSubHero();
        }

        public void ResetSpeedTalent()
        {
            //Attack speed from talents
            for (int j = 0; j < SubHeroAttackSpeedCollection.Length; j++)
            {
                if(SubHeroAttackSpeedCollection[j] == 1.8f)
                {
                    SubHeroAttackSpeedCollection[j] = 1.8f + MainTalent.TotalAttackSpeedSubHero;
                }
                if (SubHeroAttackSpeedCollection[j] == 1.9f)
                {
                    SubHeroAttackSpeedCollection[j] = 1.9f + MainTalent.TotalAttackSpeedSubHero;
                }
                if (SubHeroAttackSpeedCollection[j] == 2f)
                {
                    SubHeroAttackSpeedCollection[j] = 2f + MainTalent.TotalAttackSpeedSubHero;
                }
                if (SubHeroAttackSpeedCollection[j] == 2.1f)
                {
                    SubHeroAttackSpeedCollection[j] = 2.1f + MainTalent.TotalAttackSpeedSubHero;
                }

            }
        }

        public void ResetSpeedTalent2()
        {
            for (int j = 0; j < SubHeroAttackSpeedCollection.Length; j++)
            {
                SubHeroAttackSpeedCollection[j] = SubHeroAttackSpeedCollectionDefault[j];
            }
        }

        public void DisplaySubHeroDetails(int HeroID)
        {
            //Debug.Log(HeroID);

            //foreach(string AC in AnimationCollection)
            // {
            //     PopUpWindowAnim.SetBool(AC, false);
            // }

            if (HeroID >= 0 && HeroID < HeroImageCollection.Length)
            {
                PopUpWindowAnim.sprite = HeroImageCollection[HeroID]; // Set the image directly

                // Display the description text if it exists in the database        SAMPLE TEXT                                              
                if (descriptionDatabase.ContainsKey(HeroID))
                {
                    descriptionText.text = descriptionDatabase[HeroID];
                    descriptionTextActive.text = descriptionDatabase[HeroID];
                }
                else
                {
                    descriptionText.text = "No description available.";
                    descriptionTextActive.text = "No Description available";
                }

                //CIRCLES (BONUSES) SYSTEM TO SHOW CIRCLES BASED ON THE RARITY
                circleImage12.SetActive(false);
                circleImage22.SetActive(false);
                circleImage32.SetActive(false);

                // Determine the number of circles to activate based on hero rarity
                int circlesToShow = 0;
                switch (SubHeroReferenceType[HeroID])
                {
                    case SubHeroType.Common:
                        circlesToShow = 0; // 0 circles for common
                        break;
                    case SubHeroType.Rare:
                        circlesToShow = 1; // 1 circle for rare
                        break;
                    case SubHeroType.Epic:
                        circlesToShow = 2; // 2 circles for epic
                        break;
                    case SubHeroType.Legendary:
                        circlesToShow = 3; // 3 circles for legendary
                        break;
                }

                // Activate the appropriate number of circle GameObjects
                if (circlesToShow > 0)
                {

                    circleImage12.SetActive(true);
                }
                if (circlesToShow > 1)
                {
                    circleImage22.SetActive(true);
                }
                if (circlesToShow > 2)
                {
                    circleImage32.SetActive(true);
                }
            }



            HeroNameText.text = HeroNameCollection[HeroID];


            foreach (GameObject SCO in StarsCollectionObj)
            {
                SCO.gameObject.SetActive(false);
            }

            for (int i = 0; i < StarsCollectionObj.Length; i++)
            {
                if (i <= SubHeroEvolveLevelID[HeroID])
                {
                    if (SubHeroEvolveLevelID[HeroID] >= 1)
                    {
                        int TempValue = i - 1;
                        if (TempValue >= 0 && TempValue < 4)
                        {
                            StarsCollectionObj[TempValue].gameObject.SetActive(true);

                        }
                    }
                    if (SubHeroEvolveLevelID[HeroID] >= 5)
                    {
                        foreach (GameObject SCO in StarsCollectionObj)
                        {
                            SCO.gameObject.SetActive(true);
                        }
                    }

                    //if (SubHeroEvolveLevelID[HeroID] <= 0)
                    //{
                    //    foreach (GameObject SCO in StarsCollectionObj)
                    //    {
                    //        SCO.gameObject.SetActive(false);
                    //    }
                    //}

                }


            }

            

            ActiveHeroID = HeroID;
            PopUpWindowObj.gameObject.SetActive(true);
            //PopUpQuickEquip.gameObject.SetActive(true);
            PopUpImage.sprite = null;
            PopUpImage.sprite = HeroImageCollection[HeroID];
            HeroNameText.text = string.Empty;
            HeroNameText.text = HeroNameCollection[HeroID];
            //PopUpWindowAnim.SetBool(AnimationCollection[ActiveHeroID], true);

            if (boostPurchaseScript.ActivateDoubleGoldIncomeCondition == false)
            {
                float AttackTemp = SubHeroAttackCount[HeroID] + SubHeroDPSBaseLevel[HeroID] + SubHeroDPSBonusDamage[HeroID] + MainTalent.TotalAttackValueSubHero +(SubHeroAttackCount[HeroID] * artifactScript.TrippleSwordTotalValue);

                // Sub Hero DPS Value Text
                if (AttackTemp <= 999)
                {
                    SubHeroDPSVaueText.text = "DPS:" + AttackTemp.ToString("F0");
                }
                else if (AttackTemp <= 999999)
                {
                    SubHeroDPSVaueText.text = "DPS:" + (AttackTemp / 1000f).ToString("F0") + "K";
                }
                else if (AttackTemp <= 999999999)
                {
                    SubHeroDPSVaueText.text = "DPS:" + (AttackTemp / 1000000f).ToString("F0") + "M";
                }
                else if (AttackTemp <= 999999999999)
                {
                    SubHeroDPSVaueText.text = "DPS:" + (AttackTemp / 1000000000f).ToString("F0") + "B";
                }
                else if (AttackTemp <= 999999999999999)
                {
                    SubHeroDPSVaueText.text = "DPS:" + (AttackTemp / 1000000000000f).ToString("F0") + "T";
                }
            }
            else //double damage
            {
                float AttackTemp = (SubHeroAttackCount[HeroID] + SubHeroDPSBaseLevel[HeroID] + SubHeroDPSBonusDamage[HeroID] + MainTalent.TotalAttackValueSubHero + (SubHeroAttackCount[HeroID] * artifactScript.TrippleSwordTotalValue)) * 2;

                // Sub Hero DPS Value Text
                if (AttackTemp <= 999)
                {
                    SubHeroDPSVaueText.text = "DPS:" + AttackTemp.ToString("F0") + " X2";
                }
                else if (AttackTemp <= 999999)
                {
                    SubHeroDPSVaueText.text = "DPS:" + (AttackTemp / 1000f).ToString("F0") + "K" + " X2";
                }
                else if (AttackTemp <= 999999999)
                {
                    SubHeroDPSVaueText.text = "DPS:" + (AttackTemp / 1000000f).ToString("F0") + "M" + " X2";
                }
                else if (AttackTemp <= 999999999999)
                {
                    SubHeroDPSVaueText.text = "DPS:" + (AttackTemp / 1000000000f).ToString("F0") + "B" + " X2";
                }
                else if (AttackTemp <= 999999999999999)
                {
                    SubHeroDPSVaueText.text = "DPS:" + (AttackTemp / 1000000000000f).ToString("F0") + "T" + " X2";
                }
            }

            //SubHeroDPSVaueText.text = "DPS:" + AttackTemp.ToString();

           

            EvolveDetailsValueText.text = "EVOLVE: " + SubHeroItemCount[HeroID] + "/" + SubHeroEvolveRequirements[HeroID];


            if (SubHeroEvolveLevelID[ActiveHeroID] >= 5)
            {
                foreach (Button EB in EvolveButton)
                {
                    EB.interactable = false;
                }
            }
            if (SubHeroEvolveLevelID[ActiveHeroID] >= 0 && SubHeroEvolveLevelID[ActiveHeroID] <= 4)
            {
                if (SubHeroItemCount[ActiveHeroID] >= SubHeroEvolveRequirements[ActiveHeroID])
                {
                    foreach (Button EB in EvolveButton)
                    {
                        EB.interactable = true;
                    }
                }
                if (SubHeroItemCount[ActiveHeroID] < SubHeroEvolveRequirements[ActiveHeroID])
                {
                    foreach (Button EB in EvolveButton)
                    {
                        EB.interactable = false;
                    }
                }
            }


            


            if (SubHeroReferenceType[HeroID] == SubHeroType.Epic)
            {
                PopUpCircleBGImage.color = new Color(28, 0, 128, 255);
            }

            if (SubHeroReferenceType[HeroID] == SubHeroType.Common)
            {
                PopUpCircleBGImage.color = Color.green;
            }

            if (SubHeroReferenceType[HeroID] == SubHeroType.Legendary)
            {
                PopUpCircleBGImage.color = new Color(255, 150, 0, 255);
            }

            if (SubHeroReferenceType[HeroID] == SubHeroType.Rare)
            {
                PopUpCircleBGImage.color = new Color32(75, 178, 250, 250);
            }


            CheckUnEquipSubButton();
            CheckSubHeroEquipButton();
        }

        public void CloseDisplaySubheroDetails()
        {
            PopUpImage.sprite = null;
            HeroNameText.text = string.Empty;
            PopUpWindowObj.gameObject.SetActive(false);
        }

        public void CloseDisplaySubheroDetailsSlot()
        {
            PopUpSlotImage.sprite = null;
            SubHeroNameSlotText.text = string.Empty;
            UnEquipPopUpWindowObj.gameObject.SetActive(false);
        }

        void CheckSubHeroEquipButton()
        {
            for (int i = 0; i < SubHeroActiveHeroConditionCollection.Count; i++)
            {
                if (SubHeroActiveHeroConditionCollection[i] == false)
                {
                    EquipButton.interactable = true;
                    break;
                }

                if (SubHeroActiveHeroConditionCollection[i] == true)
                {
                    EquipButton.interactable = false;
                }
            }
        }

        public bool maxIsSelected;
        void CheckUnEquipSubButton()
        {
            for (int i = 0; i < SubHeroActiveHeroConditionCollection.Count; i++)
            {
                if (SubHeroActiveHeroConditionCollection[i] == false)
                {
                    SubHeroSlotAnimator[i].SetBool(AnimationCollection[UnEquipActiveHeroID[i]], false);
                    SubHeroFrontSlotAnimator[i].SetBool(AnimationCollection[UnEquipActiveHeroID[i]], false);

                    SubHeroActiveButtonCollection[i].interactable = false;
                    SlotListImageCollection[i].sprite = SlotListDefaultImage;
                    SlotListCoverImageObj[i].gameObject.SetActive(true);
                    SubHeroFrontImageCollection[i].sprite = SubHeroFrontImageDefault;
                    FrontSlotListCoverImageObj[i].gameObject.SetActive(true);

                    //Front and Field
                    SubHeroFrontButtonCollection[i].interactable = false;
                    SubHeroInFieldObj[i].gameObject.SetActive(false);
                    SubHeroFrontSlotButtonCollection[i].interactable = false;
                    SubHeroFrontSlotUpgradeText[i].gameObject.SetActive(false);



                }

                if (SubHeroActiveHeroConditionCollection[i] == true)
                {
                    SubHeroActiveButtonCollection[i].interactable = true;
                   
                   
                    SubHeroUIObj[SubHeroActiveImageIDCollection[i]].gameObject.SetActive(false);

                    SubHeroFrontButtonCollection[i].interactable = true;
                    SlotListImageCollection[i].sprite = HeroImageCollection[SubHeroActiveImageIDCollection[i]];
                    SlotListCoverImageObj[i].gameObject.SetActive(false);
                    SubHeroFrontImageCollection[i].sprite = HeroImageCollection[SubHeroActiveImageIDCollection[i]];
                    FrontSlotListCoverImageObj[i].gameObject.SetActive(false);

                    //Front and Field
                    SubHeroInFieldObj[i].gameObject.SetActive(true);
                    SubHeroInFieldSprite[i].sprite = HeroImageCollection[SubHeroActiveImageIDCollection[i]]; SubHeroFrontSlotButtonCollection[i].interactable = true;
                    SubHeroFrontSlotUpgradeText[i].gameObject.SetActive(true);

                    /*
                    if (!maxIsSelected)
                    {
                        // Sub Hero Front Slot Upgrade Text
                        if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]].ToString("F0");
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000f).ToString("F2") + "K";
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000f).ToString("F2") + "M";
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 9999999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000f).ToString("F2") + "B";
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000000f).ToString("F2") + "T";
                        }
                    }
                    else
                    {

                            // Sub Hero Front Slot Upgrade Text
                            if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999)
                            {
                                SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]].ToString("F0");
                            }
                            else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999)
                            {
                                SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000f).ToString("F2") + "K";
                            }
                            else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999)
                            {
                                SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000f).ToString("F2") + "M";
                            }
                            else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 9999999999)
                            {
                                SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]]  + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000f).ToString("F2") + "B";
                            }
                            else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999999999)
                            {
                                SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000000f).ToString("F2") + "T";
                            }
                        
                    }
                   */
                    ChangeTextUpgrade();

                    //Attack Value
                    //SubHeroActiveAttack[i] = SubHeroAttackCount[SubHeroActiveImageIDCollection[i]];
                    ChangeBackgroundRarityColour(); // DRYMARTI Fix
                }
            }
            CheckEquipLoadSubHero();
        }

        public void ChangeTextUpgrade()
        {
            for (int i = 0; i < SubHeroActiveHeroConditionCollection.Count; i++)
            {
                if (!maxIsSelected)
                {
                    // Sub Hero Front Slot Upgrade Text
                    if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]].ToString("F0");
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000f).ToString("F2") + "K";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000f).ToString("F2") + "M";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 9999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000f).ToString("F2") + "B";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000000f).ToString("F2") + "T";
                    }
                }
                else
                {

                    // Sub Hero Front Slot Upgrade Text
                    if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]].ToString("F0");
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000f).ToString("F2") + "K";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000f).ToString("F2") + "M";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 9999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000f).ToString("F2") + "B";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000000f).ToString("F2") + "T";
                    }

                }
            }
           
        }

        public void ChangeTextUpgrade2()
        {
            //ANOTHER ONE:
            for (int i = 0; i < SubHeroActiveHeroConditionCollection.Count; i++)
            {
                if (!maxIsSelected)
                {
                    if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]].ToString("F0");
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000f).ToString("F2") + "K";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000f).ToString("F2") + "M";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000f).ToString("F2") + "B";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000000f).ToString("F2") + "T";
                    }
                }
                else
                {


                    if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]].ToString("F0");
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000f).ToString("F2") + "K";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000f).ToString("F2") + "M";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000f).ToString("F2") + "B";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000000f).ToString("F2") + "T";
                    }
                }
            }
           
        }

        public void ChangeTextUpgrade3()
        {
            for (int i = 0; i < SubHeroActiveHeroConditionCollection.Count; i++)
            {
                //Another 3:
                if (!maxIsSelected)
                {
                    if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]].ToString("F0");
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000f).ToString("F2") + "K";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000f).ToString("F2") + "M";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 9999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000f).ToString("F2") + "B";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000000f).ToString("F2") + "T";
                    }

                }
                else
                {


                    if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]].ToString("F0");
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000f).ToString("F2") + "K";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000f).ToString("F2") + "M";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 9999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000f).ToString("F2") + "B";
                    }
                    else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999999999)
                    {
                        SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000000f).ToString("F2") + "T";
                    }
                }
            }
        }

        public void UnEquipSubHeroButtonSlot(int UnEquipButtonIDTemp)
        {
            //Debug.Log(ActiveHeroID);
           
            ActiveHeroID = UnEquipActiveHeroID[UnEquipButtonIDTemp];

            //foreach (string AC in AnimationCollection)
            // {
            //    UnEquipPopUpWindowAnim.SetBool(AC, false);
            // }

            if (UnEquipButtonIDTemp >= 0 && UnEquipButtonIDTemp < HeroImageCollection.Length)
            {
                UnEquipPopUpWindowAnim.sprite = HeroImageCollection[UnEquipButtonIDTemp]; // Set the image directly
            }

            SubHeroNameSlotText.text = HeroNameCollection[UnEquipButtonIDTemp];

            foreach (GameObject SCO in UnequipStarsCollectionObj)
            {
                SCO.gameObject.SetActive(false);
            }

            for (int i = 0; i < UnequipStarsCollectionObj.Length; i++)
            {
                if (i <= SubHeroEvolveLevelID[ActiveHeroID])
                {
                    if (SubHeroEvolveLevelID[ActiveHeroID] >= 1)
                    {
                        int TempValue = i - 1;
                        if (TempValue >= 0 && TempValue < 4)
                        {
                            UnequipStarsCollectionObj[TempValue].gameObject.SetActive(true);

                        }
                    }
                    if (SubHeroEvolveLevelID[ActiveHeroID] >= 5)
                    {
                        foreach (GameObject SCO in UnequipStarsCollectionObj)
                        {
                            SCO.gameObject.SetActive(true);
                        }

                        foreach (Button EB in EvolveButton)
                        {
                            EB.interactable = false;
                        }
                    }

                    //UnequipStarsCollectionObj[i].gameObject.SetActive(true);
                }
            }

            if (boostPurchaseScript.ActivateDoubleGoldIncomeCondition == false)
            {
                float AttackTemp = (SubHeroAttackCount[ActiveHeroID] + SubHeroDPSBaseLevel[ActiveHeroID] + SubHeroDPSBonusDamage[ActiveHeroID] + MainTalent.TotalAttackValueSubHero + (SubHeroAttackCount[ActiveHeroID] * artifactScript.TrippleSwordTotalValue));

                if (AttackTemp <= 999)
                {
                    UnequipSubHeroDPSVaueText.text = "DPS:" + AttackTemp.ToString("F0");
                }
                else if (AttackTemp <= 999999)
                {
                    UnequipSubHeroDPSVaueText.text = "DPS:" + (AttackTemp / 1000f).ToString("F2") + "K";
                }
                else if (AttackTemp <= 999999999)
                {
                    UnequipSubHeroDPSVaueText.text = "DPS:" + (AttackTemp / 1000000f).ToString("F2") + "M";
                }
                else if (AttackTemp <= 999999999999)
                {
                    UnequipSubHeroDPSVaueText.text = "DPS:" + (AttackTemp / 1000000000f).ToString("F2") + "B";
                }
                else if (AttackTemp <= 999999999999999)
                {
                    UnequipSubHeroDPSVaueText.text = "DPS:" + (AttackTemp / 1000000000000f).ToString("F2") + "T";
                }
            }
            else //double damage 
            {
                float AttackTemp = ((SubHeroAttackCount[ActiveHeroID] + SubHeroDPSBaseLevel[ActiveHeroID] + SubHeroDPSBonusDamage[ActiveHeroID] + MainTalent.TotalAttackValueSubHero + (SubHeroAttackCount[ActiveHeroID] * artifactScript.TrippleSwordTotalValue))) * 2;

                if (AttackTemp <= 999)
                {
                    UnequipSubHeroDPSVaueText.text = "DPS:" + AttackTemp.ToString("F0") + " X2";
                }
                else if (AttackTemp <= 999999)
                {
                    UnequipSubHeroDPSVaueText.text = "DPS:" + (AttackTemp / 1000f).ToString("F2") + "K" + " X2";
                }
                else if (AttackTemp <= 999999999)
                {
                    UnequipSubHeroDPSVaueText.text = "DPS:" + (AttackTemp / 1000000f).ToString("F2") + "M" + " X2";
                }
                else if (AttackTemp <= 999999999999)
                {
                    UnequipSubHeroDPSVaueText.text = "DPS:" + (AttackTemp / 1000000000f).ToString("F2") + "B" + " X2";
                }
                else if (AttackTemp <= 999999999999999)
                {
                    UnequipSubHeroDPSVaueText.text = "DPS:" + (AttackTemp / 1000000000000f).ToString("F2") + "T" + " X2";
                }
            }

           



            UnEquipButtonID = UnEquipButtonIDTemp;
            PopUpSlotImage.sprite = null;
            PopUpSlotImage.sprite = HeroImageCollection[SubHeroActiveImageIDCollection[UnEquipButtonID]];
            SubHeroNameSlotText.text = string.Empty;
            SubHeroNameSlotText.text = HeroNameCollection[SubHeroActiveImageIDCollection[UnEquipButtonID]];
            UnEquipPopUpWindowObj.gameObject.SetActive(true);
            UnEquipButton.interactable = true;
          //  UnEquipPopUpWindowAnim.SetBool(AnimationCollection[SubHeroActiveImageIDCollection[UnEquipButtonID]], true);

            UnequipEvolveDetailsValueText.text = "EVOLVE: " + SubHeroItemCount[SubHeroActiveImageIDCollection[UnEquipButtonID]] + "/" + SubHeroEvolveRequirements[SubHeroActiveImageIDCollection[UnEquipButtonID]];

            if (SubHeroEvolveLevelID[ActiveHeroID] >= 5)
            {
                foreach (Button EB in EvolveButton)
                {
                    EB.interactable = false;
                }
            }
            if (SubHeroEvolveLevelID[ActiveHeroID] >= 0 && SubHeroEvolveLevelID[ActiveHeroID] <= 4)
            {
                if (SubHeroItemCount[ActiveHeroID] >= SubHeroEvolveRequirements[ActiveHeroID])
                {
                    foreach (Button EB in EvolveButton)
                    {
                        EB.interactable = true;
                    }
                }
                if (SubHeroItemCount[ActiveHeroID] < SubHeroEvolveRequirements[ActiveHeroID])
                {
                    foreach (Button EB in EvolveButton)
                    {
                        EB.interactable = false;
                    }
                }
            }

            // Display the description text if it exists in the database        SAMPLE TEXT                                              
            if (descriptionDatabase.ContainsKey(ActiveHeroID))
            {
                descriptionText.text = descriptionDatabase[ActiveHeroID];
                descriptionTextActive.text = descriptionDatabase[ActiveHeroID];
            }
            else
            {
                descriptionText.text = "No description available.";
                descriptionTextActive.text = "No Description available";
            }

            //CIRCLES (BONUSES) SYSTEM TO SHOW CIRCLES BASED ON THE RARITY
            circleImage1.SetActive(false);
            circleImage2.SetActive(false);
            circleImage3.SetActive(false);


            // Determine the number of circles to activate based on hero rarity
            int circlesToShow = 0;
            switch (SubHeroReferenceType[ActiveHeroID])
            {
                case SubHeroType.Common:
                    circlesToShow = 0; // 0 circles for common
                    break;
                case SubHeroType.Rare:
                    circlesToShow = 1; // 1 circle for rare
                    break;
                case SubHeroType.Epic:
                    circlesToShow = 2; // 2 circles for epic
                    break;
                case SubHeroType.Legendary:
                    circlesToShow = 3; // 3 circles for legendary
                    break;
            }

            // Activate the appropriate number of circle GameObjects
            if (circlesToShow > 0)
            {
                circleImage1.SetActive(true);

            }
            if (circlesToShow > 1)
            {
                circleImage2.SetActive(true);

            }
            if (circlesToShow > 2)
            {
                circleImage3.SetActive(true);

            }


            if (SubHeroReferenceType[ActiveHeroID] == SubHeroType.Epic)
            {
                UnEquipPopUpCircleBGImage.color = new Color(28, 0, 128, 255);
            }

            if (SubHeroReferenceType[ActiveHeroID] == SubHeroType.Common)
            {
                UnEquipPopUpCircleBGImage.color = Color.green;
            }

            if (SubHeroReferenceType[ActiveHeroID] == SubHeroType.Legendary)
            {
                UnEquipPopUpCircleBGImage.color = new Color(255, 150, 0, 255);
            }

            if (SubHeroReferenceType[ActiveHeroID] == SubHeroType.Rare)
            {
                UnEquipPopUpCircleBGImage.color = new Color32(75, 178, 250, 250);
            }

            CheckUnEquipSubButton();
        }
        public void UnEquipSubHeroButton()
        {
            PlayerActiveCountInField -= .1f;

            if (PlayerActiveCountInField <= 0.0f)
            {
                PlayerActiveCountInField = .1f;
            }

            SubHeroActiveHeroConditionCollection[UnEquipButtonID] = false;

            SubHeroSlotAnimator[UnEquipButtonID].SetBool(AnimationCollection[UnEquipActiveHeroID[UnEquipButtonID]], false);
            SubHeroFrontSlotAnimator[UnEquipButtonID].SetBool(AnimationCollection[UnEquipActiveHeroID[UnEquipButtonID]], false);

            SubHeroFieldAnimator[UnEquipButtonID].SetBool(AnimationFieldCollection[UnEquipActiveHeroID[UnEquipButtonID]], false);

            //Bonus DPS
            SubHeroDPSBonusDamage[UnEquipActiveHeroID[UnEquipButtonID]] = 0;
            SubHeroTempLevelCount[UnEquipActiveHeroID[UnEquipButtonID]] = 0;
            SubHeroLevel[UnEquipActiveHeroID[UnEquipButtonID]] = DefaultSubHeroLevel;
            SubHeroLevelPurchaseCost[UnEquipActiveHeroID[UnEquipButtonID]] = DefaultSubHeroLevelPurchaseCost;

            SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[UnEquipButtonID]] = SubHeroLevelPurchaseCost[UnEquipActiveHeroID[UnEquipButtonID]];

            SubHeroDPSBaseLevel[UnEquipActiveHeroID[UnEquipButtonID]] = DefaultSubHeroDPSBaseLevel;
            SubHeroLevelMilestoneTarget[UnEquipActiveHeroID[UnEquipButtonID]] = DefaultSubHeroLevelMilestoneTarget;

            SlotListImageCollection[UnEquipButtonID].sprite = SlotListDefaultImage;
            SlotListCoverImageObj[UnEquipButtonID].gameObject.SetActive(true);
            SubHeroFrontImageCollection[UnEquipButtonID].sprite = SubHeroFrontImageDefault;
            FrontSlotListCoverImageObj[UnEquipButtonID].gameObject.SetActive(true);
            float TempSubHeroTotalAttackValue = (SubHeroActiveAttack[UnEquipButtonID] + SubHeroDPSBaseLevel[UnEquipActiveHeroID[UnEquipButtonID]] + SubHeroDPSBonusDamage[UnEquipButtonID]);
            SubHeroActiveAttack[UnEquipButtonID] = 0; //Attack
            //float TempSubHeroTotalAttackValue = (SubHeroAttackCount[UnEquipActiveHeroID[UnEquipButtonID]] + SubHeroDPSBaseLevel[UnEquipActiveHeroID[UnEquipButtonID]] + SubHeroDPSBonusDamage[UnEquipButtonID]);
            SubHeroAttackCount[UnEquipActiveHeroID[UnEquipButtonID]] = SubHeroDefaultAttackCount[UnEquipActiveHeroID[UnEquipButtonID]];

            float TempSubHeroAttackCount = SubHeroAttackCount[UnEquipActiveHeroID[UnEquipButtonID]] + DefaultSubHeroDPSBaseLevel;
            SubHeroDPSUIText[UnEquipActiveHeroID[UnEquipButtonID]].text = "DPS:" + TempSubHeroAttackCount;


            SubHeroTotalAttackValue -= TempSubHeroTotalAttackValue;
            SubHeroFieldAttackScriptCollection[UnEquipButtonID].AttackDamage = 0; //Attack
            SubHeroAnimatorAttackSpeed[UnEquipButtonID] = 0; // Attack Speed


            MainWallet.SubHeroTotalDPS = SubHeroTotalAttackValue;

            UnEquipActiveHeroID[UnEquipButtonID] = 0;

            SubHeroActiveAttackTextDisplay[UnEquipButtonID].text = "DPS";
            SubHeroFrontActiveAttackTextDisplay[UnEquipButtonID].text = "DPS";
            SubHeroLevelText[UnEquipButtonID].text = "LVL:";
            SubHeroLevelText[UnEquipButtonID].gameObject.SetActive(false);


            SubHeroFrontActiveNameTextDisplay[UnEquipButtonID].text = string.Empty;

            //foreach (GameObject SCO in UnequipStarsCollectionObj)
            //{
            //    SCO.gameObject.SetActive(false);
            //}
            //UnequipSubHeroDPSVaueText.text = "DPS:";

            SubHeroFrontSlotButtonCollection[UnEquipButtonID].interactable = false;
            SubHeroFrontSlotUpgradeText[UnEquipButtonID].gameObject.SetActive(false);


            SlotListImageCollection[UnEquipButtonID].sprite = SlotListDefaultImage;
            SlotListCoverImageObj[UnEquipButtonID].gameObject.SetActive(true);
            SubHeroFrontImageCollection[UnEquipButtonID].sprite = SubHeroFrontImageDefault;
            FrontSlotListCoverImageObj[UnEquipButtonID].gameObject.SetActive(true);
            SubHeroFrontButtonCollection[UnEquipButtonID].interactable = false;
            SubHeroActiveButtonCollection[UnEquipButtonID].interactable = false;
            UnEquipButton.interactable = false;
            SubHeroUIObj[SubHeroActiveImageIDCollection[UnEquipButtonID]].gameObject.SetActive(true);
            SubHeroInFieldObj[UnEquipButtonID].gameObject.SetActive(false);
            SubHeroActiveImageIDCollection[UnEquipButtonID] = 0;
            LabelCollectedContent.gameObject.SetActive(false);
            //SubHeroSlotAnimator[UnEquipButtonID].SetBool(AnimationCollection[SubHeroActiveImageIDCollection[UnEquipButtonID]], false);
           

            //MainHero.SubHeroTotalDPS -= SubHeroAttackCount[ActiveHeroID];
            //MainHero.SubHeroTotalDPS = SubHeroTotalAttackValue;
            CheckEquipLoadSubHero();

            foreach (Button EB in EvolveButton)
            {
                EB.interactable = false;
            }

            //MainHero.ManualUpdateDPS();

            StartCoroutine("DelayLabelDisplay");
        }

        public void PrestigeResetSubHero()
        {
            PlayerActiveCountInField -= .1f;

            if (PlayerActiveCountInField <= 0.0f)
            {
                PlayerActiveCountInField = .1f;
            }

           // SubHeroActiveHeroConditionCollection[UnEquipButtonID] = false;

           // SubHeroSlotAnimator[UnEquipButtonID].SetBool(AnimationCollection[UnEquipActiveHeroID[UnEquipButtonID]], false);
           // SubHeroFrontSlotAnimator[UnEquipButtonID].SetBool(AnimationCollection[UnEquipActiveHeroID[UnEquipButtonID]], false);

           // SubHeroFieldAnimator[UnEquipButtonID].SetBool(AnimationFieldCollection[UnEquipActiveHeroID[UnEquipButtonID]], false);

            //Bonus DPS
            SubHeroDPSBonusDamage[UnEquipActiveHeroID[UnEquipButtonID]] = 0;
            SubHeroTempLevelCount[UnEquipActiveHeroID[UnEquipButtonID]] = 0;
            SubHeroLevel[UnEquipActiveHeroID[UnEquipButtonID]] = DefaultSubHeroLevel;
            SubHeroLevelPurchaseCost[UnEquipActiveHeroID[UnEquipButtonID]] = DefaultSubHeroLevelPurchaseCost;

            SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[UnEquipButtonID]] = SubHeroLevelPurchaseCost[UnEquipActiveHeroID[UnEquipButtonID]];

            SubHeroDPSBaseLevel[UnEquipActiveHeroID[UnEquipButtonID]] = DefaultSubHeroDPSBaseLevel;
            SubHeroLevelMilestoneTarget[UnEquipActiveHeroID[UnEquipButtonID]] = DefaultSubHeroLevelMilestoneTarget;

           // SlotListImageCollection[UnEquipButtonID].sprite = SlotListDefaultImage;
           // SlotListCoverImageObj[UnEquipButtonID].gameObject.SetActive(true);
           // SubHeroFrontImageCollection[UnEquipButtonID].sprite = SubHeroFrontImageDefault;
          //  FrontSlotListCoverImageObj[UnEquipButtonID].gameObject.SetActive(true);
            float TempSubHeroTotalAttackValue = (SubHeroActiveAttack[UnEquipButtonID] + SubHeroDPSBaseLevel[UnEquipActiveHeroID[UnEquipButtonID]] + SubHeroDPSBonusDamage[UnEquipButtonID]);
            SubHeroActiveAttack[UnEquipButtonID] = 0; //Attack
            //float TempSubHeroTotalAttackValue = (SubHeroAttackCount[UnEquipActiveHeroID[UnEquipButtonID]] + SubHeroDPSBaseLevel[UnEquipActiveHeroID[UnEquipButtonID]] + SubHeroDPSBonusDamage[UnEquipButtonID]);
            SubHeroAttackCount[UnEquipActiveHeroID[UnEquipButtonID]] = SubHeroDefaultAttackCount[UnEquipActiveHeroID[UnEquipButtonID]];

            float TempSubHeroAttackCount = SubHeroAttackCount[UnEquipActiveHeroID[UnEquipButtonID]] + DefaultSubHeroDPSBaseLevel;
            SubHeroDPSUIText[UnEquipActiveHeroID[UnEquipButtonID]].text = "DPS:" + TempSubHeroAttackCount;


            SubHeroTotalAttackValue -= TempSubHeroTotalAttackValue;
            SubHeroFieldAttackScriptCollection[UnEquipButtonID].AttackDamage = 0; //Attack
            SubHeroAnimatorAttackSpeed[UnEquipButtonID] = 0; // Attack Speed


            MainWallet.SubHeroTotalDPS = SubHeroTotalAttackValue;

           // UnEquipActiveHeroID[UnEquipButtonID] = 0;

           // SubHeroActiveAttackTextDisplay[UnEquipButtonID].text = "DPS";
           // SubHeroFrontActiveAttackTextDisplay[UnEquipButtonID].text = "DPS";
           // SubHeroLevelText[UnEquipButtonID].text = "LVL:";
           // SubHeroLevelText[UnEquipButtonID].gameObject.SetActive(false);


           // SubHeroFrontActiveNameTextDisplay[UnEquipButtonID].text = string.Empty;

            //foreach (GameObject SCO in UnequipStarsCollectionObj)
            //{
            //    SCO.gameObject.SetActive(false);
            //}
            //UnequipSubHeroDPSVaueText.text = "DPS:";

           // SubHeroFrontSlotButtonCollection[UnEquipButtonID].interactable = false;
           // SubHeroFrontSlotUpgradeText[UnEquipButtonID].gameObject.SetActive(false);


           // SlotListImageCollection[UnEquipButtonID].sprite = SlotListDefaultImage;
          //  SlotListCoverImageObj[UnEquipButtonID].gameObject.SetActive(true);
          //  SubHeroFrontImageCollection[UnEquipButtonID].sprite = SubHeroFrontImageDefault;
          //  FrontSlotListCoverImageObj[UnEquipButtonID].gameObject.SetActive(true);
          //  SubHeroFrontButtonCollection[UnEquipButtonID].interactable = false;
          //  SubHeroActiveButtonCollection[UnEquipButtonID].interactable = false;
          //  UnEquipButton.interactable = false;
          //  SubHeroUIObj[SubHeroActiveImageIDCollection[UnEquipButtonID]].gameObject.SetActive(true);
          //  SubHeroInFieldObj[UnEquipButtonID].gameObject.SetActive(false);
          //  SubHeroActiveImageIDCollection[UnEquipButtonID] = 0;
          //  LabelCollectedContent.gameObject.SetActive(false);
            //SubHeroSlotAnimator[UnEquipButtonID].SetBool(AnimationCollection[SubHeroActiveImageIDCollection[UnEquipButtonID]], false);


            //MainHero.SubHeroTotalDPS -= SubHeroAttackCount[ActiveHeroID];
            //MainHero.SubHeroTotalDPS = SubHeroTotalAttackValue;
           // CheckEquipLoadSubHero();

          //  foreach (Button EB in EvolveButton)
          //  {
          //      EB.interactable = false;
          //  }

            //MainHero.ManualUpdateDPS();

          //  StartCoroutine("DelayLabelDisplay");
        }

        IEnumerator DelayLabelDisplay()
        {
            yield return new WaitForSeconds(.1f);
            LabelCollectedContent.gameObject.SetActive(true);
        }


        void CheckEquipButton()
        {
            //if (SubHeroItemCount[ActiveHeroID] >= 1)
            if (SubHeroActive[ActiveHeroID] == true)
            {
                for (int i = 0; i < SubHeroActiveHeroConditionCollection.Count; i++)
                {
                    EquipButtonText.text = "EQUIP".ToString();
                    if (SubHeroActiveHeroConditionCollection[i] == false)
                    {

                        if (SubHeroUIObj[ActiveHeroID].activeSelf == true)
                        {
                            EquipButton.interactable = true;
                        }

                        break;
                    }

                    if (SubHeroActiveHeroConditionCollection[i] == true)
                    {

                        if (SubHeroUIObj[ActiveHeroID].activeSelf == true)
                        {
                            EquipButton.interactable = false;


                            float TempAttack = SubHeroActiveAttack[i] + SubHeroDPSBaseLevel[UnEquipActiveHeroID[i]] + SubHeroDPSBonusDamage[i] + MainTalent.TotalAttackValueSubHero + (SubHeroAttackCount[i] * artifactScript.TrippleSwordTotalValue);

                            float TempScaryScream = 0;
                            if (MainTalent.ScaryScreamSkillActivate == true)
                            {
                                TempScaryScream = TempAttack * MainTalent.ScaryScreamReferenceValue;
                            }

                            TempAttack += TempScaryScream;

                            SubHeroFieldAttackScriptCollection[i].AttackDamage = TempAttack * SubHeroAttackSpeedCollection[UnEquipActiveHeroID[i]];
                            SubHeroFieldAttackScriptCollection[i].CheckDamageManualUpdate();
                        }

                        
                    }
                        
                }
                    
               
            }
            //if (SubHeroItemCount[ActiveHeroID] <= 0)
            if (SubHeroActive[ActiveHeroID] == false)
            {
                EquipButtonText.text = "EQUIP".ToString();
                EquipButton.interactable = false;
            }
        }

        void CheckEquipLoadSubHero()
        {
            float TempValue = 0;
            PlayerActiveCountInField = .1f;
            for (int i = 0; i < SubHeroActiveHeroConditionCollection.Count; i++)
            {
                if (SubHeroActiveHeroConditionCollection[i] == true)
                {
                    SubHeroFieldScript[i].SubHeroID = UnEquipActiveHeroID[i];

                    PlayerActiveCountInField += .1f;

                    if (boostPurchaseScript.ActivateDoubleGoldIncomeCondition == false) //double damage
                    {

                        float TempAttack = SubHeroAttackCount[UnEquipActiveHeroID[i]] + SubHeroDPSBaseLevel[UnEquipActiveHeroID[i]] + SubHeroDPSBonusDamage[i] + MainTalent.TotalAttackValueSubHero + (SubHeroAttackCount[UnEquipActiveHeroID[i]] * artifactScript.TrippleSwordTotalValue);

                        float TempScaryScream = 0;
                        if (MainTalent.ScaryScreamSkillActivate == true)
                        {
                            TempScaryScream = TempAttack * MainTalent.ScaryScreamReferenceValue;
                        }

                        TempAttack += TempScaryScream;

                        SubHeroFieldAttackScriptCollection[i].AttackDamage = TempAttack * SubHeroAttackSpeedCollection[UnEquipActiveHeroID[i]];

                        SubHeroActiveAttack[i] = TempAttack;
                        SubHeroAnimatorAttackSpeed[i] = SubHeroAttackSpeedCollection[UnEquipActiveHeroID[i]];
                        //if (SubHeroAnimatorAttackSpeed[i] <= 0)
                        //{
                        //    SubHeroAnimatorAttackSpeed[i] = 1.2f;
                        //}

                        SubHeroAnimatorInField[i].speed = SubHeroAttackSpeedCollection[UnEquipActiveHeroID[i]];

                        if (SubHeroAnimatorInField[i].speed >= 1.5f)
                        {
                            SubHeroAnimatorInField[i].speed = 1.5f;
                        }


                        if (TempAttack <= 999f)
                        {
                            SubHeroActiveAttackTextDisplay[i].text = "DPS:" + TempAttack.ToString("F0");
                            SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + TempAttack.ToString("F0");
                        }
                        else if (TempAttack <= 999999f)
                        {
                            SubHeroActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000f).ToString("F2") + "K";
                            SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000f).ToString("F2") + "K";
                        }
                        else if (TempAttack <= 999999999f)
                        {
                            SubHeroActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000f).ToString("F2") + "M";
                            SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000f).ToString("F2") + "M";
                        }
                        else if (TempAttack <= 9999999999f)
                        {
                            SubHeroActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000000f).ToString("F2") + "B";
                            SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000000f).ToString("F2") + "B";
                        }
                        else if (TempAttack <= 999999999999999f)
                        {
                            SubHeroActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000000000f).ToString("F2") + "T";
                            SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000000000f).ToString("F2") + "T";
                        }

                    }
                    else //double damage 
                    {
                        float TempAttack = (SubHeroAttackCount[UnEquipActiveHeroID[i]] + SubHeroDPSBaseLevel[UnEquipActiveHeroID[i]] + SubHeroDPSBonusDamage[i] + MainTalent.TotalAttackValueSubHero + (SubHeroAttackCount[UnEquipActiveHeroID[i]] * artifactScript.TrippleSwordTotalValue)) * 2;

                        float TempScaryScream = 0;
                        if (MainTalent.ScaryScreamSkillActivate == true)
                        {
                            TempScaryScream = TempAttack * MainTalent.ScaryScreamReferenceValue;
                        }

                        TempAttack += TempScaryScream;

                        SubHeroFieldAttackScriptCollection[i].AttackDamage = TempAttack * SubHeroAttackSpeedCollection[UnEquipActiveHeroID[i]];

                        SubHeroActiveAttack[i] = TempAttack;
                        SubHeroAnimatorAttackSpeed[i] = SubHeroAttackSpeedCollection[UnEquipActiveHeroID[i]];
                        //if (SubHeroAnimatorAttackSpeed[i] <= 0)
                        //{
                        //    SubHeroAnimatorAttackSpeed[i] = 1.2f;
                        //}

                        SubHeroAnimatorInField[i].speed = SubHeroAttackSpeedCollection[UnEquipActiveHeroID[i]];

                        if (SubHeroAnimatorInField[i].speed >= 1.5f)
                        {
                            SubHeroAnimatorInField[i].speed = 1.5f;
                        }


                        if (TempAttack <= 999f)
                        {
                            SubHeroActiveAttackTextDisplay[i].text = "DPS:" + TempAttack.ToString("F0") + " X2";
                            SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + TempAttack.ToString("F0") + " X2";
                        }
                        else if (TempAttack <= 999999f)
                        {
                            SubHeroActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000f).ToString("F2") + "K" + " X2";
                            SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000f).ToString("F2") + "K" + " X2";
                        }
                        else if (TempAttack <= 999999999f)
                        {
                            SubHeroActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000f).ToString("F2") + "M" + " X2";
                            SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000f).ToString("F2") + "M" + " X2";
                        }
                        else if (TempAttack <= 9999999999f)
                        {
                            SubHeroActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000000f).ToString("F2") + "B" + " X2";
                            SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000000f).ToString("F2") + "B" + " X2";
                        }
                        else if (TempAttack <= 999999999999999f)
                        {
                            SubHeroActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000000000f).ToString("F2") + "T" + " X2";
                            SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000000000f).ToString("F2") + "T" + " X2";
                        }

                    }

                    /*
                    float TempScaryScream = 0;
                    if (MainTalent.ScaryScreamSkillActivate == true)
                    {
                        TempScaryScream = TempAttack * MainTalent.ScaryScreamReferenceValue;
                    }

                    TempAttack += TempScaryScream;

                    SubHeroFieldAttackScriptCollection[i].AttackDamage = TempAttack * SubHeroAttackSpeedCollection[UnEquipActiveHeroID[i]];

                    SubHeroActiveAttack[i] = TempAttack;
                    SubHeroAnimatorAttackSpeed[i] = SubHeroAttackSpeedCollection[UnEquipActiveHeroID[i]];
                    //if (SubHeroAnimatorAttackSpeed[i] <= 0)
                    //{
                    //    SubHeroAnimatorAttackSpeed[i] = 1.2f;
                    //}

                    SubHeroAnimatorInField[i].speed = SubHeroAttackSpeedCollection[UnEquipActiveHeroID[i]];

                    if (SubHeroAnimatorInField[i].speed >= 1.5f)
                    {
                        SubHeroAnimatorInField[i].speed = 1.5f;
                    }


                    if (TempAttack <= 999)
                    {
                        SubHeroActiveAttackTextDisplay[i].text = "DPS:" + TempAttack.ToString("F0");
                        SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + TempAttack.ToString("F0");
                    }
                    else if (TempAttack <= 999999)
                    {
                        SubHeroActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000f).ToString("F2") + "K";
                        SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000f).ToString("F2") + "K";
                    }
                    else if (TempAttack <= 999999999)
                    {
                        SubHeroActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000f).ToString("F2") + "M";
                        SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000f).ToString("F2") + "M";
                    }
                    else if (TempAttack <= 9999999999)
                    {
                        SubHeroActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000000f).ToString("F2") + "B";
                        SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000000f).ToString("F2") + "B";
                    }
                    else if (TempAttack <= 999999999999999)
                    {
                        SubHeroActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000000000f).ToString("F2") + "T";
                        SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + (TempAttack / 1000000000000f).ToString("F2") + "T";
                    }*/


                    SubHeroFrontActiveNameTextDisplay[i].text = HeroNameCollection[UnEquipActiveHeroID[i]];
                    TempValue += SubHeroActiveAttack[i] + SubHeroDPSBaseLevel[UnEquipActiveHeroID[i]] + SubHeroDPSBonusDamage[UnEquipActiveHeroID[i]];
                    //TempValue = SubHeroActiveAttack[i] + SubHeroDPSBonusDamage[UnEquipActiveHeroID[i]];
                    SubHeroTotalAttackValue = TempValue;
                    //SubHeroActiveAttack[i] = TempAttack;
                    //MainHero.SubHeroTotalDPS += SubHeroAttackCount[i];
                    //MainHero.SubHeroTotalDPS = SubHeroTotalAttackValue;
                    MainWallet.SubHeroTotalDPS = SubHeroTotalAttackValue;

                    SubHeroSlotAnimator[i].SetBool(AnimationCollection[UnEquipActiveHeroID[i]], true);
                    SubHeroFrontSlotAnimator[i].SetBool(AnimationCollection[UnEquipActiveHeroID[i]], true);

                    SubHeroFieldAnimator[i].SetBool(AnimationFieldCollection[UnEquipActiveHeroID[i]], true);

                    SubHeroFrontSlotButtonCollection[i].interactable = true;
                    SubHeroFrontSlotUpgradeText[i].gameObject.SetActive(true);

                    SubHeroLevelText[i].gameObject.SetActive(true);
                    SubHeroLevelText[i].text = "LVL:" + SubHeroLevel[UnEquipActiveHeroID[i]].ToString();

                    /*
                    if (!maxIsSelected)
                    {
                        if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]].ToString("F0");
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000f).ToString("F2") + "K";
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000f).ToString("F2") + "M";
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000f).ToString("F2") + "B";
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000000f).ToString("F2") + "T";
                        }
                    }
                    else
                    {
                        

                        if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]].ToString("F0");
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000f).ToString("F2") + "K";
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000f).ToString("F2") + "M";
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000f).ToString("F2") + "B";
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000000f).ToString("F2") + "T";
                        }
                    }
                   */
                    ChangeTextUpgrade2();

                    MainHero.ManualUpdateDPS();

                    SubHeroFieldAttackScriptCollection[i].CheckDamageManualUpdate();
                    //SubHeroTotalAttackValue = TempValue;

                }

                

                if (SubHeroActiveHeroConditionCollection[i] == false)
                {
                    //SubHeroAnimatorAttackSpeed[i] = 0;
                    TempValue += SubHeroActiveAttack[i];
                   
                    //MainHero.SubHeroTotalDPS = SubHeroTotalAttackValue;
                    MainWallet.SubHeroTotalDPS = SubHeroTotalAttackValue;
                    SubHeroFrontSlotButtonCollection[i].interactable = false;
                    SubHeroFrontSlotUpgradeText[i].gameObject.SetActive(false);
                    MainHero.ManualUpdateDPS();
                }

              

            }


            //MainHero.ManualUpdateDPS();
        }

        void LoadEquipSubHero(int TempUnEquipButtonID)
        {
            //Debug.Log(ActiveHeroID);
            float AttackTemp = (SubHeroAttackCount[ActiveHeroID] + SubHeroDPSBaseLevel[ActiveHeroID] + SubHeroDPSBonusDamage[ActiveHeroID] + MainTalent.TotalAttackValueSubHero + (SubHeroAttackCount[ActiveHeroID] * artifactScript.TrippleSwordTotalValue)) * SubHeroAttackSpeedCollection[ActiveHeroID];
            UnequipSubHeroDPSVaueText.text = "DPS:" + AttackTemp.ToString();
            UnequipEvolveDetailsValueText.text = "EVOLVE: " + SubHeroItemCount[SubHeroActiveImageIDCollection[TempUnEquipButtonID]] + "/" + SubHeroEvolveRequirements[SubHeroActiveImageIDCollection[TempUnEquipButtonID]];


            for (int i = 0; i < UnequipStarsCollectionObj.Length; i++)
            {
                //if (i <= SubHeroEvolveLevelID[ActiveHeroID])
                //{
                //    UnequipStarsCollectionObj[i].gameObject.SetActive(true);
                //}

                if (i <= SubHeroEvolveLevelID[ActiveHeroID])
                {
                    if (SubHeroEvolveLevelID[ActiveHeroID] >= 1)
                    {
                        int TempValue = i - 1;
                        if (TempValue >= 0 && TempValue < 4)
                        {
                            UnequipStarsCollectionObj[TempValue].gameObject.SetActive(true);

                        }
                    }
                    if (SubHeroEvolveLevelID[ActiveHeroID] >= 5)
                    {
                        foreach (GameObject SCO in UnequipStarsCollectionObj)
                        {
                            SCO.gameObject.SetActive(true);
                        }
                    }

                    //UnequipStarsCollectionObj[i].gameObject.SetActive(true);
                }
            }

            //SubHeroActiveAttackTextDisplay[TempUnEquipButtonID].text = SubHeroActiveAttack[ActiveHeroID] + " DPS";
            if (boostPurchaseScript.ActivateDoubleGoldIncomeCondition == false)
            {
                float TempAttack = SubHeroAttackCount[ActiveHeroID] + SubHeroDPSBaseLevel[UnEquipActiveHeroID[TempUnEquipButtonID]] + SubHeroDPSBonusDamage[ActiveHeroID] + MainTalent.TotalAttackValueSubHero + (SubHeroAttackCount[ActiveHeroID] * artifactScript.TrippleSwordTotalValue);

                float TempScaryScream = 0;
                if (MainTalent.ScaryScreamSkillActivate == true)
                {
                    TempScaryScream = TempAttack * MainTalent.ScaryScreamReferenceValue;
                }

                TempAttack += TempScaryScream;


                if (TempAttack <= 999)
                {
                    SubHeroActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + TempAttack.ToString("F0");
                    SubHeroFrontActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + SubHeroAttackCount[ActiveHeroID].ToString("F0") + TempAttack.ToString("F0");
                }
                else if (TempAttack <= 999999)
                {
                    SubHeroActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + TempAttack.ToString("F2") + "K";
                    SubHeroFrontActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + SubHeroAttackCount[ActiveHeroID].ToString("F0") + TempAttack.ToString("F2") + "K";
                }
                else if (TempAttack <= 999999999)
                {
                    SubHeroActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + TempAttack.ToString("F2") + "M";
                    SubHeroFrontActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + SubHeroAttackCount[ActiveHeroID].ToString("F0") + TempAttack.ToString("F2") + "M";
                }
                else if (TempAttack <= 9999999999)
                {
                    SubHeroActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + TempAttack.ToString("F0") + "B";
                    SubHeroFrontActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + SubHeroAttackCount[ActiveHeroID].ToString("F0") + TempAttack.ToString("F0") + "B";
                }
                else if (TempAttack <= 999999999999999)
                {
                    SubHeroActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + TempAttack.ToString("F0") + "T";
                    SubHeroFrontActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + SubHeroAttackCount[ActiveHeroID].ToString("F0") + TempAttack.ToString("F0") + "T";
                }

                SubHeroFrontActiveNameTextDisplay[TempUnEquipButtonID].text = HeroNameCollection[UnEquipActiveHeroID[TempUnEquipButtonID]];

                SubHeroFrontSlotButtonCollection[TempUnEquipButtonID].interactable = true;
                SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].gameObject.SetActive(true);



                if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] <= 999)
                {
                    SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].text = "UPGRADE" + "\n" + SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]].ToString("F0");
                }
                else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] <= 999999)
                {
                    SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] / 1000f).ToString("F2") + "K";
                }
                else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] <= 999999999)
                {
                    SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] / 1000000f).ToString("F2") + "M";
                }
                else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] <= 9999999999)
                {
                    SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] / 1000000000f).ToString("F2") + "B";
                }
                else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] <= 999999999999999)
                {
                    SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] / 1000000000000f).ToString("F2") + "T";
                }

                SubHeroActiveAttack[TempUnEquipButtonID] = TempAttack;
            }
            else //double damage 
            {
                float TempAttack = (SubHeroAttackCount[ActiveHeroID] + SubHeroDPSBaseLevel[UnEquipActiveHeroID[TempUnEquipButtonID]] + SubHeroDPSBonusDamage[ActiveHeroID] + MainTalent.TotalAttackValueSubHero + (SubHeroAttackCount[ActiveHeroID] * artifactScript.TrippleSwordTotalValue)) * 2;

                float TempScaryScream = 0;
                if (MainTalent.ScaryScreamSkillActivate == true)
                {
                    TempScaryScream = TempAttack * MainTalent.ScaryScreamReferenceValue;
                }

                TempAttack += TempScaryScream;


                if (TempAttack <= 999)
                {
                    SubHeroActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + TempAttack.ToString("F0") + " X2";
                    SubHeroFrontActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + SubHeroAttackCount[ActiveHeroID].ToString("F0") + TempAttack.ToString("F0") + " X2";
                }
                else if (TempAttack <= 999999)
                {
                    SubHeroActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + TempAttack.ToString("F2") + "K" + " X2";
                    SubHeroFrontActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + SubHeroAttackCount[ActiveHeroID].ToString("F0") + TempAttack.ToString("F2") + "K" + " X2";
                }
                else if (TempAttack <= 999999999)
                {
                    SubHeroActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + TempAttack.ToString("F2") + "M" + " X2";
                    SubHeroFrontActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + SubHeroAttackCount[ActiveHeroID].ToString("F0") + TempAttack.ToString("F2") + "M" + " X2";
                }
                else if (TempAttack <= 9999999999)
                {
                    SubHeroActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + TempAttack.ToString("F0") + "B" + " X2";
                    SubHeroFrontActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + SubHeroAttackCount[ActiveHeroID].ToString("F0") + TempAttack.ToString("F0") + "B" + " X2";
                }
                else if (TempAttack <= 999999999999999)
                {
                    SubHeroActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + TempAttack.ToString("F0") + "T" + " X2";
                    SubHeroFrontActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + SubHeroAttackCount[ActiveHeroID].ToString("F0") + TempAttack.ToString("F0") + "T" + " X2";
                }

                SubHeroFrontActiveNameTextDisplay[TempUnEquipButtonID].text = HeroNameCollection[UnEquipActiveHeroID[TempUnEquipButtonID]];

                SubHeroFrontSlotButtonCollection[TempUnEquipButtonID].interactable = true;
                SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].gameObject.SetActive(true);

                if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] <= 999)
                {
                    SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].text = "UPGRADE" + "\n" + SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]].ToString("F0");
                }
                else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] <= 999999)
                {
                    SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] / 1000f).ToString("F2") + "K";
                }
                else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] <= 999999999)
                {
                    SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] / 1000000f).ToString("F2") + "M";
                }
                else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] <= 9999999999)
                {
                    SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] / 1000000000f).ToString("F2") + "B";
                }
                else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] <= 999999999999999)
                {
                    SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] / 1000000000000f).ToString("F2") + "T";
                }

                SubHeroActiveAttack[TempUnEquipButtonID] = TempAttack;
            }
            /*
             float TempScaryScream = 0;
             if (MainTalent.ScaryScreamSkillActivate == true)
             {
                 TempScaryScream = TempAttack * MainTalent.ScaryScreamReferenceValue;
             }

             TempAttack += TempScaryScream;


             if (TempAttack <= 999)
             {
                 SubHeroActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + TempAttack.ToString("F0");
                 SubHeroFrontActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + SubHeroAttackCount[ActiveHeroID].ToString("F0") + TempAttack.ToString("F0");
             }
             else if (TempAttack <= 999999)
             {
                 SubHeroActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + TempAttack.ToString("F2") + "K";
                 SubHeroFrontActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + SubHeroAttackCount[ActiveHeroID].ToString("F0") + TempAttack.ToString("F2") + "K";
             }
             else if (TempAttack <= 999999999)
             {
                 SubHeroActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + TempAttack.ToString("F2") + "M";
                 SubHeroFrontActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + SubHeroAttackCount[ActiveHeroID].ToString("F0") + TempAttack.ToString("F2") + "M";
             }
             else if (TempAttack <= 9999999999)
             {
                 SubHeroActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + TempAttack.ToString("F0") + "B";
                 SubHeroFrontActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + SubHeroAttackCount[ActiveHeroID].ToString("F0") + TempAttack.ToString("F0") + "B";
             }
             else if (TempAttack <= 999999999999999)
             {
                 SubHeroActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + TempAttack.ToString("F0") + "T";
                 SubHeroFrontActiveAttackTextDisplay[TempUnEquipButtonID].text = "DPS:" + SubHeroAttackCount[ActiveHeroID].ToString("F0") + TempAttack.ToString("F0") + "T";
             }


             SubHeroFrontActiveNameTextDisplay[TempUnEquipButtonID].text = HeroNameCollection[UnEquipActiveHeroID[TempUnEquipButtonID]];

             SubHeroFrontSlotButtonCollection[TempUnEquipButtonID].interactable = true;
             SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].gameObject.SetActive(true);

             if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] <= 999)
             {
                 SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].text = "UPGRADE" + "\n" + SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]].ToString("F0");
             }
             else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] <= 999999)
             {
                 SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] / 1000f).ToString("F2") + "K";
             }
             else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] <= 999999999)
             {
                 SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] / 1000000f).ToString("F2") + "M";
             }
             else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] <= 9999999999)
             {
                 SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] / 1000000000f).ToString("F2") + "B";
             }
             else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] <= 999999999999999)
             {
                 SubHeroFrontSlotUpgradeText[TempUnEquipButtonID].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[TempUnEquipButtonID]] / 1000000000000f).ToString("F2") + "T";
             }

             SubHeroActiveAttack[TempUnEquipButtonID] = TempAttack;

              */

            if (SubHeroItemCount[ActiveHeroID] >= SubHeroEvolveRequirements[ActiveHeroID])
            {
                foreach (Button EB in EvolveButton)
                {
                    EB.interactable = true;
                }

            }

            if (SubHeroItemCount[ActiveHeroID] < SubHeroEvolveRequirements[ActiveHeroID])
            {
                foreach (Button EB in EvolveButton)
                {
                    EB.interactable = false;
                }
            }
            CheckEquipLoadSubHero();
        }
        public void ChangeBackgroundRarityColour()
        {
            // Mapping each slot to its corresponding background Image component
            Image[] slotBackgrounds = new Image[] { UnEquipPopUpCircleBGImage1, UnEquipPopUpCircleBGImage2, UnEquipPopUpCircleBGImage3, UnEquipPopUpCircleBGImage4 };

            for (int i = 0; i < SubHeroActiveImageIDCollection.Count; i++)
            {
                if (SubHeroActiveImageIDCollection[i] >= 0)
                {
                    SubHeroType heroRarity = SubHeroReferenceType[SubHeroActiveImageIDCollection[i]];

                    // Determine the color based on hero rarity
                    Color colorToApply = Color.white; // Default to white, adjust as necessary
                    switch (heroRarity)
                    {
                        //Epic: new Color(211, 54, 226, 180);
                        //Green : new Color(73, 188, 87, 180);
                        //Rare: new Color(44 , 156, 224, 180);
                        //Legendary: new Color(200, 100, 25, 180);
                        //Common: new Color(170, 205, 226,180);
                        case SubHeroType.Legendary:
                            colorToApply = new Color32(200, 100, 25, 250); // Adjust as necessary
                            break;
                        case SubHeroType.Epic:
                            colorToApply = new Color32(211, 54, 226, 250); // Adjust as necessary
                            break;
                        case SubHeroType.Rare:
                            colorToApply = new Color32(75, 178, 250, 250); // Adjust as necessary
                            break;
                        case SubHeroType.Common:
                            colorToApply = Color.green; // Adjust as necessary
                            break;
                        default:
                            // Use default color if needed
                            break;
                    }

                    // Apply the color to the corresponding slot background
                    if (i < slotBackgrounds.Length)
                    {
                        slotBackgrounds[i].color = colorToApply;
                    }

                    activeTeamBackground[0].color = UnEquipPopUpCircleBGImage1.color;
                    activeTeamBackground[1].color = UnEquipPopUpCircleBGImage2.color;
                    activeTeamBackground[2].color = UnEquipPopUpCircleBGImage3.color;
                    activeTeamBackground[3].color = UnEquipPopUpCircleBGImage4.color;
                }
                else
                {
                    // Optional: Reset to default color if no hero is assigned
                    if (i < slotBackgrounds.Length)
                    {
                        slotBackgrounds[i].color = Color.white; // Or any default color you choose
                    }
                }
            }
        }


        public void EquipSubHero()
        {
            for (int i = 0; i < SubHeroActiveHeroConditionCollection.Count; i++)
            {
                if (SubHeroActiveHeroConditionCollection[i] == false)
                {
                    SlotListImageCollection[i].sprite = HeroImageCollection[ActiveHeroID];
                    SlotListCoverImageObj[i].gameObject.SetActive(false);
                    SubHeroActiveHeroConditionCollection[i] = true;
                    SubHeroUIObj[ActiveHeroID].gameObject.SetActive(false);
                    SubHeroActiveImageIDCollection[i] = ActiveHeroID;
                    EquipButton.interactable = false;

                    UnEquipActiveHeroID[i] = ActiveHeroID;

                    foreach (Button EB in EvolveButton)
                    {
                        EB.interactable = false;
                    }



                    SubHeroSlotAnimator[i].SetBool(AnimationCollection[ActiveHeroID], true);
                    SubHeroFrontSlotAnimator[i].SetBool(AnimationCollection[ActiveHeroID], true);

                    SubHeroFieldAnimator[i].SetBool(AnimationFieldCollection[UnEquipActiveHeroID[i]], true);
                    SubHeroLevelText[i].gameObject.SetActive(true);
                    SubHeroLevelText[i].text = "LVL:" + SubHeroLevel[UnEquipActiveHeroID[i]].ToString();
                    

                    SubHeroFrontSlotButtonCollection[i].interactable = true;
                    SubHeroFrontSlotUpgradeText[i].gameObject.SetActive(true);
                    CheckUpgradeSetCollection();

                    /*
                    if (!maxIsSelected)
                    {
                        if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]].ToString("F0");
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000f).ToString("F2") + "K";
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000f).ToString("F2") + "M";
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 9999999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000f).ToString("F2") + "B";
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "UPGRADE" + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000000f).ToString("F2") + "T";
                        }

                    }
                    else
                    {
                        

                        if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]].ToString("F0");
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000f).ToString("F2") + "K";
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000f).ToString("F2") + "M";
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 9999999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000f).ToString("F2") + "B";
                        }
                        else if (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] <= 999999999999999)
                        {
                            SubHeroFrontSlotUpgradeText[i].text = "Count: " + FinalTempMaxPurchaseGroupValue[UnEquipActiveHeroID[i]] + "\n" + (SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]] / 1000000000000f).ToString("F2") + "T";
                        }
                    }*/
                    ChangeTextUpgrade3();


                    //Front Page



                    SubHeroFrontImageCollection[i].sprite = HeroImageCollection[ActiveHeroID];
                    FrontSlotListCoverImageObj[i].gameObject.SetActive(false);

                    if (boostPurchaseScript.ActivateDoubleGoldIncomeCondition == false)
                    {
                        float TempAttack = (SubHeroAttackCount[ActiveHeroID] + SubHeroDPSBaseLevel[ActiveHeroID] + SubHeroDPSBonusDamage[ActiveHeroID] + MainTalent.TotalAttackValueSubHero + (SubHeroAttackCount[ActiveHeroID] * artifactScript.TrippleSwordTotalValue)) * SubHeroAttackSpeedCollection[UnEquipActiveHeroID[i]];

                        float TempScaryScream = 0;
                        if (MainTalent.ScaryScreamSkillActivate == true)
                        {
                            TempScaryScream = TempAttack * MainTalent.ScaryScreamReferenceValue;
                        }

                        TempAttack += TempScaryScream;

                        SubHeroActiveAttack[i] = TempAttack;
                        SubHeroFieldAttackScriptCollection[i].AttackDamage = SubHeroActiveAttack[i];

                        SubHeroActiveAttackTextDisplay[i].text = "DPS:" + SubHeroActiveAttack[i].ToString("F0");
                        SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + SubHeroActiveAttack[i].ToString("F0");
                        SubHeroFrontActiveNameTextDisplay[i].text = HeroNameCollection[ActiveHeroID];
                    }
                    else //double damage 
                    {
                        float TempAttack = ((SubHeroAttackCount[ActiveHeroID] + SubHeroDPSBaseLevel[ActiveHeroID] + SubHeroDPSBonusDamage[ActiveHeroID] + MainTalent.TotalAttackValueSubHero + (SubHeroAttackCount[ActiveHeroID] * artifactScript.TrippleSwordTotalValue)) * SubHeroAttackSpeedCollection[UnEquipActiveHeroID[i]]) * 2;

                        float TempScaryScream = 0;
                        if (MainTalent.ScaryScreamSkillActivate == true)
                        {
                            TempScaryScream = TempAttack * MainTalent.ScaryScreamReferenceValue;
                        }

                        TempAttack += TempScaryScream;

                        SubHeroActiveAttack[i] = TempAttack;
                        SubHeroFieldAttackScriptCollection[i].AttackDamage = SubHeroActiveAttack[i];

                        SubHeroActiveAttackTextDisplay[i].text = "DPS:" + SubHeroActiveAttack[i].ToString("F0") + " X2";
                        SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + SubHeroActiveAttack[i].ToString("F0") + " X2";
                        SubHeroFrontActiveNameTextDisplay[i].text = HeroNameCollection[ActiveHeroID];

                    }

                    /*
                    float TempScaryScream = 0;
                    if (MainTalent.ScaryScreamSkillActivate == true)
                    {
                        TempScaryScream = TempAttack * MainTalent.ScaryScreamReferenceValue;
                    }

                    TempAttack += TempScaryScream;

                    SubHeroActiveAttack[i] = TempAttack;
                    SubHeroFieldAttackScriptCollection[i].AttackDamage = SubHeroActiveAttack[i];

                    SubHeroActiveAttackTextDisplay[i].text = "DPS:" + SubHeroActiveAttack[i].ToString("F0");
                    SubHeroFrontActiveAttackTextDisplay[i].text = "DPS:" + SubHeroActiveAttack[i].ToString("F0");
                    SubHeroFrontActiveNameTextDisplay[i].text = HeroNameCollection[ActiveHeroID];
                    */
                    //MainHero.SubHeroTotalDPS += SubHeroAttackCount[ActiveHeroID];
                    //MainHero.SubHeroTotalDPS = SubHeroTotalAttackValue;
                    CheckEquipLoadSubHero();

                    //MainHero.ManualUpdateDPS();

                    SubHeroFieldAttackScriptCollection[i].CheckDamageManualUpdate();
                    //Field
                    SubHeroInFieldObj[i].gameObject.SetActive(true);
                    SubHeroInFieldSprite[i].sprite = HeroImageCollection[ActiveHeroID];

                    ChangeBackgroundRarityColour();

                    break;
                }
            }

                    


                CheckUnEquipSubButton();

        }

        public void EvolveButtonActivate()
        {
            if (SubHeroEvolveLevelID[ActiveHeroID] <= 4)
            {
                SubHeroItemCount[ActiveHeroID] -= SubHeroEvolveRequirements[ActiveHeroID];
                SubHeroEvolveLevelID[ActiveHeroID] += 1;
                //UnEquipSubHeroButtonSlot(UnEquipButtonID);
                FirstCheck();
                DisplaySubHeroDetails(ActiveHeroID);
                CheckEquipLoadSubHero();
                //GOOD!!!
            }

            if (SubHeroEvolveLevelID[ActiveHeroID] >= 5)
            {
				MainAchievement.MaximumHeroEvolveCheckManualUpdate();

				foreach (Button EB in EvolveButton)
                {
                    EB.interactable = false;
                }
            }

        }

        public void UnEquipEvolveButtonActivate()
        {
            if (SubHeroEvolveLevelID[ActiveHeroID] <= 4)
            {
                SubHeroItemCount[ActiveHeroID] -= SubHeroEvolveRequirements[ActiveHeroID];
                SubHeroEvolveLevelID[ActiveHeroID] += 1;
                FirstCheck();
                //UnEquipSubHeroButtonSlot(UnEquipButtonID);
                LoadEquipSubHero(UnEquipButtonID);
                //CheckEquipLoadSubHero();
                //DisplaySubHeroDetails(ActiveHeroID);
            }
            if (SubHeroEvolveLevelID[ActiveHeroID] >= 5)
            {
                foreach (Button EB in EvolveButton)
                {
                    EB.interactable = false;
                }
            }

        }
        public void SubHeroUpgradeCollection(int SubHeroUpgradeCollectionID)
        {
            foreach (GameObject SHUBC in SubHeroUpgradeButtonCoverCollection)
            {
                SHUBC.gameObject.SetActive(true);
            }
            SubHeroUpgradeButtonCoverCollection[SubHeroUpgradeCollectionID].gameObject.SetActive(false);
            SubHeroUpgradeID = SubHeroUpgradeCollectionID;
            if (SubHeroUpgradeCollectionID == 0)
            {
                maxIsSelected = false;
                for (int i = 0; i < SubHeroLevelPurchaseCostTemp.Length; i++)
                {
                    SubHeroLevelPurchaseCostTemp[i] = SubHeroLevelPurchaseCost[i];
                    SubHeroLevelPurchaseCostTemp[i] *= 1;
                    CheckEquipLoadSubHero();
                    MainHero.CheckUpgradeMainHeroSetCollection();
                }
            }

            if (SubHeroUpgradeCollectionID == 1)
            {
                maxIsSelected = false;
                for (int i = 0; i < SubHeroLevelPurchaseCostTemp.Length; i++)
                {
                    float baseCost = SubHeroLevelPurchaseCost[i];  // Get the initial cost
                    float cumulativeCost = baseCost;  // Initialize cumulative cost with the base cost
                    float currentCost = baseCost;  // This will track the cost increment at each step
                    for (int j = 1; j < 10; j++)  // We already have the first cost, so start from 1
                    {
                        currentCost *= 1.03f;  // Increase the current cost by 3%
                        cumulativeCost += currentCost;  // Add the updated current cost to the cumulative cost
                    }
                    SubHeroLevelPurchaseCostTemp[i] = cumulativeCost;  // Set the calculated cumulative cost
                    
                    CheckEquipLoadSubHero();
                    MainHero.CheckUpgradeMainHeroSetCollection();
                }
            }

            if (SubHeroUpgradeCollectionID == 2)
            {
                maxIsSelected = false;
                for (int i = 0; i < SubHeroLevelPurchaseCostTemp.Length; i++)
                {
                    float baseCost = SubHeroLevelPurchaseCost[i];  // Get the initial cost
                    float cumulativeCost = baseCost;  // Initialize cumulative cost with the base cost
                    float currentCost = baseCost;  // This will track the cost increment at each step
                    for (int j = 1; j < 100; j++)  // We already have the first cost, so start from 1
                    {
                        currentCost *= 1.03f;  // Increase the current cost by 3%
                        cumulativeCost += currentCost;  // Add the updated current cost to the cumulative cost
                    }
                    SubHeroLevelPurchaseCostTemp[i] = cumulativeCost;  // Set the calculated cumulative cost
                  
                    CheckEquipLoadSubHero();
                    MainHero.CheckUpgradeMainHeroSetCollection();
                }
            }
      

                if (SubHeroUpgradeCollectionID == 3)
                {
                maxIsSelected = true;
                for (int i = 0; i < SubHeroLevelPurchaseCostTemp.Length; i++)
                {
                    float baseCost = SubHeroLevelPurchaseCost[i];  // Initial cost of upgrading
                    float fundsAvailable = MainWallet.GoldWalletValue;  // Total gold available for upgrades
                    int totalLevelsPurchased = 0;
                    float cumulativeCost = 0;
                    float currentCost = baseCost;

                    while (fundsAvailable >= currentCost)
                    {
                        fundsAvailable -= currentCost;  // Deduct the cost of the current upgrade from available funds
                        cumulativeCost += currentCost;  // Add the cost of the current upgrade to the cumulative cost
                        currentCost *= 1.03f;  // Increase the cost for the next level
                        totalLevelsPurchased++;  // Increment the number of upgrades purchased
                    }

                    SubHeroLevelPurchaseCostTemp[i] = cumulativeCost;  // Update the temp cost with the total cost for maximum upgrades
                    FinalTempMaxPurchaseGroupValue[i] = totalLevelsPurchased;  // Store the total levels purchased

                    // Check if at least one level can be purchased; if not, reset to the base cost
                    if (totalLevelsPurchased == 0)
                    {
                        SubHeroLevelPurchaseCostTemp[i] = baseCost;
                    }
                    
                    CheckEquipLoadSubHero();
                    MainHero.CheckUpgradeMainHeroSetCollection();
                }
            }


        }

        public void UpgradeHeroBaseDPS(int SlotID)
        {
            int heroID = UnEquipActiveHeroID[SlotID];
            int levelsToPurchase = DetermineLevelsPurchased(SubHeroUpgradeID, heroID);
            float increaseFactor = 1.03f; // Assuming 2% increase per level

            float cumulativeCost = 0;
            float currentCost = SubHeroLevelPurchaseCost[heroID];
            float cumulativeDPS = SubHeroDPSBaseLevel[heroID];
            float currentDPS = cumulativeDPS;

            // Calculate cumulative cost and DPS for the specified number of levels
            for (int j = 0; j < levelsToPurchase; j++)
            {
                cumulativeCost += currentCost;
                currentCost *= increaseFactor;

                cumulativeDPS += currentDPS;
                currentDPS *= increaseFactor;
            }

            // Check if there's enough gold to proceed
            if (MainWallet.GoldWalletValue >= cumulativeCost)
            {
                MainWallet.GoldWalletValue -= cumulativeCost;
                SubHeroLevelPurchaseCost[heroID] = currentCost; // Update to the new base cost after all increments
                SubHeroDPSBaseLevel[heroID] = currentDPS; // Update to the new DPS after all increments

                SubHeroLevel[heroID] += levelsToPurchase; // Add the total levels purchased
                SubHeroLevelText[SlotID].text = "LVL:" + SubHeroLevel[heroID];
            }
            else
            {
                // Not enough gold, handle as needed (e.g., show error message)
            }

            SubHeroLevelText[SlotID].text = "LVL:" + SubHeroLevel[UnEquipActiveHeroID[SlotID]].ToString();


            //Sub Hero DPS Bonus Damage
            SubHeroTempLevelCount[UnEquipActiveHeroID[SlotID]] += 1; 

            if (SubHeroTempLevelCount[UnEquipActiveHeroID[SlotID]] >= SubHeroLevelMilestoneTarget[SlotID])
            {
                SubHeroLevelMilestoneTarget[UnEquipActiveHeroID[SlotID]] += 100;
                float TempBonusAttack = SubHeroDPSBaseLevel[UnEquipActiveHeroID[SlotID]] * 2;
                SubHeroDPSBonusDamage[UnEquipActiveHeroID[SlotID]] = TempBonusAttack;
            }

            //float TempAttack = SubHeroAttackCount[SlotID] + SubHeroDPSBaseLevel[SlotID];

            //SubHeroActiveAttack[SlotID] = TempAttack;
            CheckUpgradeSetCollection();
            MainWallet.WalletValueManualUpdate();
           
        }

        private int DetermineLevelsPurchased(int upgradeID, int heroID)
        {
            switch (upgradeID)
            {
                case 1: // x10
                    
                    return 10;
                case 2: // x100
                    
                    return 100;
                case 3: // MAX
                    
                    float fundsAvailable = MainWallet.GoldWalletValue;
                    float currentCost = SubHeroLevelPurchaseCost[heroID];
                    int maxLevels = 0;
                    while (fundsAvailable >= currentCost)
                    {
                        fundsAvailable -= currentCost;
                        currentCost *= 1.03f; // Increase cost by 2% for the next level
                        maxLevels++;
                    }
                    return maxLevels;
                default: // x1
                    
                    return 1;
            }
        }

        void CheckUpgradeSetCollection()
        {
            float increaseFactor = 1.03f; // Assuming a 3% increase per level as before

            for (int i = 0; i < SubHeroLevelPurchaseCostTemp.Length; i++)
            {
                float baseCost = SubHeroLevelPurchaseCost[i];
                int levelsToPurchase = DetermineLevelsPurchased(SubHeroUpgradeID, i);
                float cumulativeCost = baseCost;

                for (int j = 1; j < levelsToPurchase; j++) // Start from 1 as the first level cost is already included
                {
                    cumulativeCost += baseCost * (float)Math.Pow(increaseFactor, j);
                }

                SubHeroLevelPurchaseCostTemp[i] = cumulativeCost;
                CheckEquipLoadSubHero(); // Update UI to reflect the new potential upgrade cost
            }

            CheckEquipLoadSubHero(); // Ensure the UI is updated after all changes
        }

        public void CheckPurchaseButton()
        {
            for (int i = 0; i < SubHeroActiveHeroConditionCollection.Count; i++)
            {
                if (SubHeroActiveHeroConditionCollection[i] == true)
                {
                    if (MainWallet.GoldWalletValue >= SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]])
                    {
                        SubHeroFrontSlotButtonCollection[i].interactable = true;
                    }

                    if (MainWallet.GoldWalletValue < SubHeroLevelPurchaseCostTemp[UnEquipActiveHeroID[i]])
                    {
                        SubHeroFrontSlotButtonCollection[i].interactable = false;
                    }
                }
            }
        }

        public void LoadAnimation()
        {
            StartCoroutine("DelayLoadAnimation");
        }

        public void ResetDefaultImage(int SlotID)
        {
            SlotListImageCollection[SlotID].sprite = SlotListDefaultImage;
            SubHeroFrontImageCollection[SlotID].sprite = SubHeroFrontImageDefault;


            //for (int i = 0; i < SubHeroActiveHeroConditionCollection.Count; i++)
            //{
            //    if (SubHeroActiveHeroConditionCollection[i] == false)
            //    {
            //        //SubHeroSlotAnimator[i].SetBool(AnimationCollection[UnEquipActiveHeroID[i]], false);
            //        //SubHeroFrontSlotAnimator[i].SetBool(AnimationCollection[UnEquipActiveHeroID[i]], false);

            //        SubHeroActiveButtonCollection[i].interactable = false;
            //        SlotListImageCollection[i].sprite = SlotListDefaultImage;
            //        SubHeroFrontImageCollection[i].sprite = SubHeroFrontImageDefault;

            //        //Front and Field
            //        SubHeroFrontButtonCollection[i].interactable = false;
            //        SubHeroInFieldObj[i].gameObject.SetActive(false);


            //    }

            //}
        }

        IEnumerator DelayLoadAnimation()
        {
            yield return new WaitForSeconds(.1f);
            CheckUnEquipSubButton();
            CheckEquipLoadSubHero();
        }


        // Function to initialize the description database
        private void InitializeDescriptionDatabase()
        {
            // Add description texts for each hero ID
            descriptionDatabase.Add(0, "It uses the power of steam to suck out your thoughts. \n \n Attack: Mind Projectiles \n \n Rarity: EPIC \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(1, "Fearless warrior! \n \n Attack: Spear \n \n Rarity: Rare \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(2, "Agile as a deer, elusive as a snake \n \n Attack: Screaming Wave \n \n Rarity: Rare \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(3, "Excellent archer, a great tracker. \n \n Attack: Shooting Bow \n \n Rarity: Rare \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(4, "Meeting him ends badly. \n \n Attack: Shadow Spikes \n \n Rarity: Epic \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(5, "Don't be fooled by its small stature! \n \n Attack: Shurikens Throw \n \n Rarity: Epic \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(6, "He may be a glutton, but he breathes fire! \n \n Attack: Magma Spits \n \n Rarity: Common \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(7, "The nightmare of every enemy of the West \n \n Attack: Grenades Throw \n \n Rarity: Rare \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(8, "It was supposed to be played by Halle Berry. \n \n Attack: Crossbow \n \n Rarity: Legendary \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(9, "The most dangerous creature of the forest. \n \n Attack: Boomerang \n \n Rarity: Epic \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(10, "Hocussss Pocussss, my friend! \n \n Attack: Magic Cards Deck \n \n Rarity: Epic \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(11, "Accept this offering, oh Gowroth! \n \n Attack: Dark Energy \n \n Rarity: Rare \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(12, "The latest technological achievement, a flying marvel. \n \n Attack: Bomb Attack \n \n Rarity: Rare \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(13, "Vikings! Shield wall! \n \n Attack: Glyphs Projectiles \n \n Rarity: Common \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(14, "Pure, unbridled fire. \n \n Attack: Fireballs \n \n Rarity: Common \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(15, "Not only crazy but also furious! \n \n Attack: Spells Book \n \n Rarity: Rare \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(16, "She has the potential to create gold! \n \n Attack: Potions Throw \n \n Rarity: Rare \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(17, "Not a single word has come out of his mouth in the last 30 years. \n \n Attack: Wind Storm \n \n Rarity: Rare \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(18, "Be cautious when he offers you tea. \n \n Attack: Toxic Cloud \n \n Rarity: Rare \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(19, "He surrounds himself with bony friends. \n \n Attack: Magic Staff \n \n Rarity: Epic \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(20, "My God, give me strength! \n \n Attack: Staff \n \n Rarity: Epic \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(21, "His hunger is insatiable. \n \n Attack: Souls Eater \n \n Rarity: Epic \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(22, "Pure, untamed, utter chaos. \n \n Attack: Devil Eyes \n \n Rarity: Legendary \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(23, "He could fly to Mordor for Frodo. \n \n Attack: Claws \n \n Rarity: Common \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(24, "For the honor of my Clan! \n \n Attack: Sword \n \n Rarity: Rare \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(25, "Beware of his mighty club! \n \n Attack: Mace \n \n Rarity: Epic \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(26, "He can't reach the throat, but other valuable organs are in grave danger \n \n Attack: Daggers \n \n Rarity: Rare \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(27, "Is there a princess to rescue here? \n \n Attack: Sword and Shield \n \n Rarity: Epic \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(28, "Behold me, the bringer of death. \n \n Attack: Massive Sword \n \n Rarity: Legendary \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(29, "Few dared to be lucky enough to tell about me. \n \n Attack: Paws \n \n Rarity: Common \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(30, "Attention! A charge is coming! Pikes down! \n \n Attack: Pike \n \n Rarity: Common \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(31, "It has one fundamental advantage: it doesn't bleed. \n \n Attack: Mince \n \n Rarity: Rare \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(32, "A resident of the nearby cemeteries. \n \n Attack: Deadly Hug \n \n Rarity: Rare \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(33, "Come here, little dog, come! \n \n Attack: Triple Bite \n \n Rarity: Epic \n \n FUTURE UPDATE: Bonuses ");
            descriptionDatabase.Add(34, "Thunder from a clear sky! \n \n Attack: Shooting Daggers \n \n Rarity: Common \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(35, "That doesn't compute! \n \n Attack: Throwing Fists \n \n Rarity: Epic \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(36, "Those aren't human bones... or are they? \n \n Attack: Ice Breath \n \n Rarity: Legendary \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(37, "Eastern, Colorful, beautiful, so... Angry!!!!! \n \n Attack: Fire Breath \n \n Rarity: Legendary \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(38, "Oops.. I lost my 7th head again... \n \n Attack: Toxic Breath \n \n Rarity: Legendary \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(39, "ROCKS... I NEED ROCKS... \n \n Attack: Rocks Throw \n \n Rarity: Epic \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(40, "When I die... I rise stronger twice! \n \n Attack: Inferno Waves \n \n Rarity: Legendary \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(41, "Nature was my nature. Now I am the nature! \n \n Attack: Trident \n \n Rarity: Legendary \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(42, "Heaven was never an option for you my dear... \n \n Attack: Fiery whip \n \n Rarity: Epic \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(43, "Gods have nothing to say to me. I am the chosen one! \n \n Attack: God's Hoops \n \n Rarity: Legendary \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(44, "Why everyone asks me to take my mask of... \n \n Attack: Claws \n \n Rarity: Epic \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(45, "One small step for man, one giant leap for mankind! \n \n Attack: Drones Projectiles \n \n Rarity: Epic \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(46, "Shouldn't use it for drinking... \n \n Attack: Screaming Waves \n \n Rarity: Legendary \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(47, "The best friend of Hercules \n \n Attack: Hoof Attack \n \n Rarity: Rare \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(48, "It resides in a far far galaxy. \n \n Attack: Flamethrower \n \n Rarity: Rare \n \n FUTURE UPDATE: Bonuses");
            descriptionDatabase.Add(49, "Silent and deadly. \n \n Attack: Knife Stab \n \n Rarity: Common \n \n FUTURE UPDATE: Bonuses");
        }

        // Update is called once per frame
        void Update()
        {
            //ChangeTextUpgrade();
            //ChangeTextUpgrade2();
            //ChangeTextUpgrade3();
            if (maxIsSelected)
            {
                SubHeroUpgradeCollection(3);
            }
            else
            {
                return;
            }
            
        }
    }
}

