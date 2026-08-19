using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAMPLETEXT.Wallet.Manager;
using UnityEngine.UI;
using TMPro;
using SAMPLETEXT.Artifact.Manager;
using SAMPLETEXT.Achievement.Manager;

namespace SAMPLETEXT.ItemPurchase.Manager.Item
{
    public class ItemsPurchaseArtifactManagerScript : MonoBehaviour
    {
        [SerializeField]
        ArtifactManagerScript MainArtifact;
        [SerializeField]
        WalletManagerScript MainWallet;
        [Header("Artifact Collection Settings")]
        //[SerializeField]
        public Sprite[] ArtifactCollectionImage;
        //[SerializeField]
        public string[] ArtifactCollectionName;
        [SerializeField]
        float[] ArtifactCostValueCollection;
        [SerializeField]
        Button[] ArtifactPurchaseButtonCollection;


        [Header("Pop Up Window Settings")]
        [SerializeField]
        GameObject PopUpWindowObj;
        [SerializeField]
        Image PopUpWindowArtifactImage;
        [SerializeField] TMP_Text artifactNameText;
        [SerializeField]
        TextMeshProUGUI PurchaseArtifactValueCountText;
        int CollectCountButtonReference;

		[Header("Achievement Settings")]
		[SerializeField]
		AchievementManagerScript MainAchievement;
        // Start is called before the first frame update
        void Start()
        {
            FirstCheck();
        }

        public void FirstCheck()
        {
            for (int i = 0;  i < ArtifactCostValueCollection.Length; i++)
            {
                if (MainWallet.GemWalletValue >= ArtifactCostValueCollection[i])
                {
                   // ArtifactPurchaseButtonCollection[i].interactable = true; //Drymarti
                    ArtifactPurchaseButtonCollection[0].interactable = true;
                    ArtifactPurchaseButtonCollection[2].interactable = true;
                    ArtifactPurchaseButtonCollection[5].interactable = true;
                    ArtifactPurchaseButtonCollection[7].interactable = true;
                    ArtifactPurchaseButtonCollection[8].interactable = true;

                }
                if (MainWallet.GemWalletValue < ArtifactCostValueCollection[i])
                {
                    ArtifactPurchaseButtonCollection[i].interactable = false;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void PurchaseArtifact(int ArtifactCountID)
        {
			MainAchievement.CollectedArtifactCheckManualUpdate();

			MainWallet.GemWalletValue -= ArtifactCostValueCollection[ArtifactCountID];
            MainWallet.WalletValueManualUpdate();

            if (MainArtifact.ArtifactActiveCollection[ArtifactCountID] == false)
            {
                MainArtifact.ArtifactActiveCollection[ArtifactCountID] = true;
            }

            else if (MainArtifact.ArtifactActiveCollection[ArtifactCountID] == true)
            {
                MainArtifact.ArtifactEvolveCountCollection[ArtifactCountID] += 1;
            }
               

            MainArtifact.FirstCheck();

            CollectCountButtonReference = ArtifactCountID;
            PopUpWindowArtifactImage.sprite = null;
            PurchaseArtifactValueCountText.text = string.Empty;

            PopUpWindowArtifactImage.sprite = ArtifactCollectionImage[ArtifactCountID];
            artifactNameText.text = ArtifactCollectionName[ArtifactCountID];

           PopUpWindowObj.gameObject.SetActive(true);
            PurchaseArtifactValueCountText.text = ArtifactCollectionName[ArtifactCountID];

        }

        public void PurchaseArtifactCollectButton()
        {
            PopUpWindowArtifactImage.sprite = null;
            PurchaseArtifactValueCountText.text = string.Empty;
           
            PopUpWindowObj.gameObject.SetActive(false);
            FirstCheck();
        }

    }
}


