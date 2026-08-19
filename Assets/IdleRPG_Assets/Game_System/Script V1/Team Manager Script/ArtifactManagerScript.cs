using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SAMPLETEXT.Gameplay.Manager.MainHero;
using SAMPLETEXT.Market.Manager;
using SAMPLETEXT.Gameplay.Manager.Enemy;
using SAMPLETEXT.Talent.Manager;
using SAMPLETEXT.Achievement.Manager;

namespace SAMPLETEXT.Artifact.Manager
{
    public class ArtifactManagerScript : MonoBehaviour
    {
        [SerializeField]
        GameplayEnemyManagerScript MainEnemy;
        [SerializeField]
        MarketManagerScript MainMarket;
        [SerializeField]
        GameplayMainHeroManagerScript MainHero;
        [Header("Artifact Settings")]
        public List<bool> ArtifactActiveCollection = new List<bool>();
        public List<float> ArtifactSkillValueCollection = new List<float>();
        public List<float> ArtifactEvolveCountCollection = new List<float>();
        public List<float> ArtifactEvolveRequireCountCollection = new List<float>();
        public List<float> ArtifactEvolveCountInputCollection = new List<float>();
        [SerializeField]
        float[] ArtifactSkillIncreaseValueCollection;
        [SerializeField]
        string[] ArtifactDescriptionCollection;
        [SerializeField]
        string[] ArtifactNameCollection;
        [SerializeField]
        GameObject[] ArtifactObjCollection;
        [SerializeField]
        Sprite[] ArtifactImageCollection;
        [SerializeField]

        [Header("Artifact Default Settings")]
        Sprite DefaultImage;
       

        [Header("Golden Seeds Extra Settings")]
        public float GoldenSeedsBossDropValue;

        [Header("Pop Up Window Settings")]
        [SerializeField]
        TextMeshProUGUI NameText;
        [SerializeField]
        Image ArtifactImage;
        [SerializeField]
        TextMeshProUGUI DescriptionText;
        [SerializeField]
        TextMeshProUGUI ArtifactCountText;
        [SerializeField]
        Button EvolveArtifactButton;
        int ArtifactButtonID;

        [Header("Artifact Total Value Settings")]
        public float BloodySkullTotalValue;
        public float DeflationTotalValue;
        public float GodsHourGlassTotalValue;
        public float GoldenSeedsTotalValue;
        public float HugeOpportunityTotalValue;
        public float InevitableVictoryTotalValue;
        public float HornOfPlentyTotalValue;
        public float TalentedTotalValue;
        public float TrippleSwordTotalValue;

       

        [Header("Talents Settings")]
        [SerializeField]
        TalentsManagerScript MainTalent;

		[Header("Achievement Settings")]
		[SerializeField]
		AchievementManagerScript MainAchievement;
        // Start is called before the first frame update
        void Start()
        {
            PopUpWindowFirstDisplay();
            FirstCheck();

        }

        void PopUpWindowFirstDisplay()
        {
            NameText.text = string.Empty;
            DescriptionText.text = string.Empty;
            ArtifactCountText.text = string.Empty;
            EvolveArtifactButton.interactable = false;

        }

        public void FirstCheck()
        {
            for (int i = 0; i < ArtifactObjCollection.Length; i ++)
            {
                if (ArtifactActiveCollection[i] == false)
                {
                    ArtifactObjCollection[i].gameObject.SetActive(false);
                }

                if (ArtifactActiveCollection[i] == true)
                {
                    
                         ArtifactObjCollection[i].gameObject.SetActive(true);
                    
                   

                    if (ArtifactEvolveCountCollection[i] >= ArtifactEvolveRequireCountCollection[i])
                    {
                        EvolveArtifactButton.interactable = true;
                    }

                    if (ArtifactEvolveCountCollection[i] < ArtifactEvolveRequireCountCollection[i])
                    {
                        EvolveArtifactButton.interactable = false;
                    }
                }
            }

            NameText.text = string.Empty;
            DescriptionText.text = string.Empty;
            ArtifactCountText.text = string.Empty;
            ArtifactImage.sprite = DefaultImage;
          

            if (ArtifactActiveCollection[0] == true) //critical damage increase
            {
                float TempValue = ArtifactSkillValueCollection[0];

                BloodySkullTotalValue = TempValue;
            }

            if (ArtifactActiveCollection[1] == true)
            {
                //float TempValue = MainMarket.TotalCostValue * ArtifactSkillValueCollection[1];

                //DeflationTotalValue = TempValue;

                DeflationTotalValue = (ArtifactSkillValueCollection[1] * .100f);
            }

            if (ArtifactActiveCollection[3] == true)
            {
                float TempValue = MainMarket.TotalIncomeValue * ArtifactSkillValueCollection[3] * .100f;

                GoldenSeedsTotalValue = TempValue;
            }

            if (ArtifactActiveCollection[4] == true)
            {
                float TempValue = ArtifactSkillValueCollection[4];

                HugeOpportunityTotalValue = TempValue;
            }

            if (ArtifactActiveCollection[5] == true) // critical chance
            {
                float TempValue = ArtifactSkillValueCollection[5];

                InevitableVictoryTotalValue = TempValue;
            }

            if (ArtifactActiveCollection[7] == true)
            {
                float TempValue = (ArtifactSkillValueCollection[7]);

                float TempValueWhole = Mathf.Round(TempValue);

                TalentedTotalValue = TempValueWhole;
                MainTalent.TotalAdditionalArtifactTalentValue = TalentedTotalValue;
            }

            if (ArtifactActiveCollection[8] == true)
            {
                float TempValue = (ArtifactSkillValueCollection[8] * 0.01f);

                TrippleSwordTotalValue = TempValue;
            }
        }

        void RecheckArtifactTotalManualUpdate()
        {
            if (ArtifactActiveCollection[0] == true)
            {
                float TempValue = ArtifactSkillValueCollection[0];

                BloodySkullTotalValue = TempValue;
            }

            if (ArtifactActiveCollection[1] == true)
            {
                float TempValue = ArtifactSkillValueCollection[1] * .100f;

                DeflationTotalValue = TempValue;
            }

            if(ArtifactActiveCollection[2] == true)
            {

            }

            if (ArtifactActiveCollection[3] == true)
            {
                float TempValue = MainMarket.TotalIncomeValue * ArtifactSkillValueCollection[3];

                GoldenSeedsTotalValue = TempValue;
            }

            if (ArtifactActiveCollection[4] == true)
            {
                float TempValue = ArtifactSkillValueCollection[4];

                HugeOpportunityTotalValue = TempValue;
            }

            if (ArtifactActiveCollection[5] == true) //critical attack
            {
                float TempValue =  ArtifactSkillValueCollection[5];

                InevitableVictoryTotalValue = TempValue;
            }

            if (ArtifactActiveCollection[7] == true) //talented
            {
                float TempValue = (ArtifactSkillValueCollection[7]);

                float TempValueWhole = Mathf.Round(TempValue);

                TalentedTotalValue = TempValueWhole;
                MainTalent.TotalAdditionalArtifactTalentValue = TalentedTotalValue;
            }

            if (ArtifactActiveCollection[8] == true)
            {
                float TempValue = (ArtifactSkillValueCollection[8] * 0.01f);

                TrippleSwordTotalValue = TempValue;
            }
        }

        public void ArtifactItemCheck(int ArtifactID)
        {
            ArtifactButtonID = ArtifactID;
            NameText.text = string.Empty;
            NameText.text = ArtifactNameCollection[ArtifactID];
            DescriptionText.text = string.Empty;
          

            if (ArtifactID == 3)
            {
                DescriptionText.text = ArtifactDescriptionCollection[ArtifactID] + ArtifactSkillValueCollection[ArtifactID] + "%" + " BOSS DROP:" + GoldenSeedsBossDropValue +"%".ToString();
            }
            else
            {
                DescriptionText.text = ArtifactDescriptionCollection[ArtifactID] + ArtifactSkillValueCollection[ArtifactID] + "%".ToString();
            }

            ArtifactImage.sprite = null;
            ArtifactImage.sprite = ArtifactImageCollection[ArtifactID];

            ArtifactCountText.text = string.Empty;
            ArtifactCountText.text = "EVOLVE " + ArtifactEvolveCountCollection[ArtifactID] + "/" + ArtifactEvolveRequireCountCollection[ArtifactID];

            if (ArtifactEvolveCountCollection[ArtifactID] >= ArtifactEvolveRequireCountCollection[ArtifactID])
            {

                EvolveArtifactButton.interactable = true;
            }

            if (ArtifactEvolveCountCollection[ArtifactID] < ArtifactEvolveRequireCountCollection[ArtifactID])
            {
                EvolveArtifactButton.interactable = false;
            }
        }

        public void EvolveButton()
        {
			MainAchievement.ArtifactLevelCheckManualUpdate();
			ArtifactSkillValueCollection[ArtifactButtonID] += ArtifactSkillIncreaseValueCollection[ArtifactButtonID];

            if (ArtifactButtonID == 3)
            {
                GoldenSeedsBossDropValue += ArtifactSkillIncreaseValueCollection[ArtifactButtonID];
            }

            ArtifactEvolveCountInputCollection[ArtifactButtonID] += 1;

            if (ArtifactEvolveCountInputCollection[ArtifactButtonID] >= 10)
            {
                ArtifactEvolveRequireCountCollection[ArtifactButtonID] += 1;
                ArtifactEvolveCountInputCollection[ArtifactButtonID] = 0;
            }

            ArtifactEvolveCountCollection[ArtifactButtonID] -= ArtifactEvolveRequireCountCollection[ArtifactButtonID];

            ArtifactItemCheck(ArtifactButtonID);
            RecheckArtifactTotalManualUpdate();
            MainHero.ManualUpdateDPS();
            MainMarket.ManualMarketValueCheckUpdate();
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}

