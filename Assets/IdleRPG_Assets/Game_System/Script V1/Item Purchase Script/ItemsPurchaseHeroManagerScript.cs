using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SAMPLETEXT.Wallet.Manager;
using UnityEngine.UI;
using SAMPLETEXT.SubHeroUI.Manager;
using SAMPLETEXT.Achievement.Manager;

namespace SAMPLETEXT.ItemPurchase.Manager.Item
{
    public class ItemsPurchaseHeroManagerScript : MonoBehaviour
    {
        [SerializeField]
        SubHeroesManagerScript MainSubHero;
        [SerializeField]
        WalletManagerScript MainWallet;
        [Header("Hero Chest Settings")]
        [SerializeField]
        int[] SubHeroPurchaseValueCount;
        [SerializeField]
        int[] SubHeroPurchaseCostValue;
        [SerializeField]
        TextMeshProUGUI[] SubHeroPurchaseCostValueText;
        public Sprite[] SubHeroImageCollection;
        public string[] SubHeroAnimationCollection;
        [SerializeField]
        bool[] SubHeroAnimationConditionCollection;
        [SerializeField]
        Animator SubHeroCurrentAnimator;
        [SerializeField]
        Button[] HeroPurchaseButton;
        public string[] HeroNameCollection;
        bool GemDeduction;
        enum SubHeroType
        {
            Legendary,
            Epic,
            Rare,
            Common
        };
        [SerializeField]
        List<SubHeroType> SubHeroReferenceType = new List<SubHeroType>();
        public List<int> SubHeroEpicID = new List<int>();
        public List<int> SubHeroRareID = new List<int>();
        public List<int> SubHeroCommonID = new List<int>();
        public List<int> SubHeroLegendaryID = new List<int>();


        [Header("Pop Up Window Settings")]
        [SerializeField]
        GameObject PopUpWindowObj;
        [SerializeField]
        Image PopUpWindowHeroImage;
        [SerializeField]
        TextMeshProUGUI PopUpWindowHeroNameText;
[SerializeField]
        TextMeshProUGUI PurchaseHeroValueCountText;
        [SerializeField]
        TextMeshProUGUI PurchaseHeroItemValueCountText;
        [SerializeField]
        Image PopUpCircleBGImage;

        int CollectCountButtonReference;

		[Header("Achievement Settings")]
		[SerializeField]
		AchievementManagerScript MainAchievement;

        // Start is called before the first frame update
        void Start()
        {
            //SubHeroCurrentAnimator.runtimeAnimatorController = SubHeroAnimatorControllerCollection[0];

            for (int i = 0; i < SubHeroReferenceType.Count; i++)
            {
                if (SubHeroReferenceType[i] == SubHeroType.Epic)
                {
                    SubHeroEpicID.Add(i);
                }

                if (SubHeroReferenceType[i] == SubHeroType.Rare)
                {
                    SubHeroRareID.Add(i);
                }

                if (SubHeroReferenceType[i] == SubHeroType.Common)
                {
                    SubHeroCommonID.Add(i);
                }

                if (SubHeroReferenceType[i] == SubHeroType.Legendary)
                {
                    SubHeroLegendaryID.Add(i);
                }
            }

            ButtonCheckUpdate();
        }

        public void ButtonCheckUpdate()
        {
           for (int i = 0; i < SubHeroPurchaseCostValue.Length; i++)
            {
                

                if (MainWallet.GemWalletValue >= SubHeroPurchaseCostValue[i])
                {
                    HeroPurchaseButton[i].interactable = true;
                }

                if (MainWallet.GemWalletValue < SubHeroPurchaseCostValue[i])
                {
                    HeroPurchaseButton[i].interactable = false;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
           
        }

        public void PurchaseHero(int HeroCount)
        {
			MainAchievement.FindFirstHeroCheckManualUpdate();
			MainAchievement.CollectedHeroesCheckManualUpdate();

			if (GemDeduction == false)
            {
                MainWallet.GemWalletValue -= SubHeroPurchaseCostValue[HeroCount];
                MainWallet.WalletValueManualUpdate();
                ButtonCheckUpdate();
                GemDeduction = true;
            }
            
            CollectCountButtonReference = HeroCount;
            PopUpWindowHeroImage.sprite = null;
            PurchaseHeroValueCountText.text = string.Empty;
            PopUpWindowHeroNameText.text = string.Empty;

            foreach(string SHAC in SubHeroAnimationCollection)
            {
                SubHeroCurrentAnimator.SetBool(SHAC, false);
            }

            int i = Random.Range(0, 50);
            Debug.Log(i);


            if (i == 49) // Legendary Chance (between 49 and 49 random number generated = 1 * 2 = 2%)
            {
				MainAchievement.LegendaryHeroCheckManualUpdate();

				int a = Random.Range(0, SubHeroLegendaryID.Count);
                Debug.Log(a);
                PopUpWindowHeroImage.sprite = SubHeroImageCollection[SubHeroLegendaryID[a]];

                PopUpWindowHeroNameText.text = HeroNameCollection[SubHeroLegendaryID[a]];

                PopUpWindowObj.gameObject.SetActive(true);
                PurchaseHeroValueCountText.text = "Remaining " + SubHeroPurchaseValueCount[HeroCount].ToString();

                SubHeroPurchaseValueCount[HeroCount] -= 1;

                SubHeroCurrentAnimator.SetBool(SubHeroAnimationCollection[SubHeroLegendaryID[a]], true);

                int x = Random.Range(1, 4);
                MainSubHero.SubHeroItemCount[SubHeroLegendaryID[a]] += x;

                PurchaseHeroItemValueCountText.text = "X" + x;

                if (MainSubHero.SubHeroActive[SubHeroLegendaryID[a]] == false)
                {
                    MainSubHero.SubHeroActive[SubHeroLegendaryID[a]] = true;
                }

                PopUpCircleBGImage.color = new Color(255, 104, 0, 255);
                MainSubHero.SubHeroUIObj[SubHeroLegendaryID[a]].transform.parent = MainSubHero.HeroActiveParentObj.transform;
                MainSubHero.SubHeroUIObj[SubHeroRareID[a]].transform.SetSiblingIndex(0);
                //MainSubHero.FirstCheck();
            }

            if (i >= 45 && i <= 48) // Epic Chance (between 45 and 48 random number generated = 40 * 2 = 8%)
            {
                int a = Random.Range(0, SubHeroEpicID.Count);
                Debug.Log(a);
                PopUpWindowHeroImage.sprite = SubHeroImageCollection[SubHeroEpicID[a]];

                PopUpWindowHeroNameText.text = HeroNameCollection[SubHeroEpicID[a]];

                PopUpWindowObj.gameObject.SetActive(true);
                PurchaseHeroValueCountText.text = "Remaining " + SubHeroPurchaseValueCount[HeroCount].ToString();

                SubHeroPurchaseValueCount[HeroCount] -= 1;

                SubHeroCurrentAnimator.SetBool(SubHeroAnimationCollection[SubHeroEpicID[a]], true);

                int x = Random.Range(2, 2);
                MainSubHero.SubHeroItemCount[SubHeroEpicID[a]] += x;

                PurchaseHeroItemValueCountText.text = "X" + x;

                if (MainSubHero.SubHeroActive[SubHeroEpicID[a]] == false)
                {
                    MainSubHero.SubHeroActive[SubHeroEpicID[a]] = true;

                }

                PopUpCircleBGImage.color = new Color(28, 0, 128, 255);

                MainSubHero.SubHeroUIObj[SubHeroEpicID[a]].transform.parent = MainSubHero.HeroActiveParentObj.transform;
                MainSubHero.SubHeroUIObj[SubHeroRareID[a]].transform.SetSiblingIndex(0);
                //MainSubHero.FirstCheck();

            }

            if (i >= 25 && i <= 44) // Rare Chance (between 25 and 44 random number generated = 20 * 2 = 40%)
            {
               
                int a = Random.Range(0, SubHeroRareID.Count);
                Debug.Log(a);
                PopUpWindowHeroImage.sprite = SubHeroImageCollection[SubHeroRareID[a]];

                PopUpWindowHeroNameText.text = HeroNameCollection[SubHeroRareID[a]];

                PopUpWindowObj.gameObject.SetActive(true);
                PurchaseHeroValueCountText.text = "Remaining " + SubHeroPurchaseValueCount[HeroCount].ToString();

                SubHeroPurchaseValueCount[HeroCount] -= 1;

                SubHeroCurrentAnimator.SetBool(SubHeroAnimationCollection[SubHeroRareID[a]], true);

                int x = Random.Range(3, 9);
                MainSubHero.SubHeroItemCount[SubHeroRareID[a]] += x;

                PurchaseHeroItemValueCountText.text = "X" + x;

                if (MainSubHero.SubHeroActive[SubHeroRareID[a]] == false)
                {
                    MainSubHero.SubHeroActive[SubHeroRareID[a]] = true;
                }

                PopUpCircleBGImage.color = new Color32(75, 178, 250, 250);

                MainSubHero.SubHeroUIObj[SubHeroRareID[a]].transform.parent = MainSubHero.HeroActiveParentObj.transform;
                MainSubHero.SubHeroUIObj[SubHeroRareID[a]].transform.SetSiblingIndex(0);
                //MainSubHero.FirstCheck();
            }

            if (i >= 0 && i <= 24) // Common Chance (between 0 and 24 random number generated = 25 * 2 = 50%)
            {
                int a = Random.Range(0, SubHeroCommonID.Count);
                Debug.Log(a);
                PopUpWindowHeroImage.sprite = SubHeroImageCollection[SubHeroCommonID[a]];

                PopUpWindowHeroNameText.text = HeroNameCollection[SubHeroCommonID[a]];

                PopUpWindowObj.gameObject.SetActive(true);
                PurchaseHeroValueCountText.text = "Remaining " + SubHeroPurchaseValueCount[HeroCount].ToString();

                SubHeroPurchaseValueCount[HeroCount] -= 1;

                SubHeroCurrentAnimator.SetBool(SubHeroAnimationCollection[SubHeroCommonID[a]], true);

                int x = Random.Range(4, 11);
                MainSubHero.SubHeroItemCount[SubHeroCommonID[a]] += x;

                PurchaseHeroItemValueCountText.text = "X" + x;


                if (MainSubHero.SubHeroActive[SubHeroCommonID[a]] == false)
                {
                    MainSubHero.SubHeroActive[SubHeroCommonID[a]] = true;
                }

                PopUpCircleBGImage.color = Color.green;

                MainSubHero.SubHeroUIObj[SubHeroCommonID[a]].transform.parent = MainSubHero.HeroActiveParentObj.transform;
                MainSubHero.SubHeroUIObj[SubHeroCommonID[a]].transform.SetSiblingIndex(0);
                //MainSubHero.FirstCheck();
            }

            //PopUpWindowHeroImage.sprite = SubHeroImageCollection[i];
            
            //PopUpWindowHeroNameText.text = HeroNameCollection[i];

            //PopUpWindowObj.gameObject.SetActive(true);
            //PurchaseHeroValueCountText.text = "Remaining " + SubHeroPurchaseValueCount[HeroCount].ToString();

            //SubHeroPurchaseValueCount[HeroCount] -= 1;
            //MainSubHero.SubHeroItemCount[i] += 1;
            //MainSubHero.FirstCheck();
        }

        public void PurchaseHeroCollectButton()
        {
            foreach (string SHAC in SubHeroAnimationCollection)
            {
                SubHeroCurrentAnimator.SetBool(SHAC, false);
            }
            if (CollectCountButtonReference == 0)
            {
                if (SubHeroPurchaseValueCount[0] <= 0)
                {
                    PopUpWindowObj.gameObject.SetActive(false);
                    SubHeroPurchaseValueCount[0] = 1;
                    GemDeduction = false;
                    MainSubHero.FirstCheck();
                }
                
            }

            if (CollectCountButtonReference == 1)
            {

                if (SubHeroPurchaseValueCount[1] <= 0)
                {
                    PopUpWindowObj.gameObject.SetActive(false);
                    SubHeroPurchaseValueCount[1] = 10;
                    GemDeduction = false;
                    MainSubHero.FirstCheck();
                }

                else if (SubHeroPurchaseValueCount[1] >= 1)
                {
                    PurchaseHero(1);
                }
            }



        }
    }
}

