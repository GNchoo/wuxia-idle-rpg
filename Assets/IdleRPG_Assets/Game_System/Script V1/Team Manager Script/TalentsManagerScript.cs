using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SAMPLETEXT.Account.Manager.Profile;
using SAMPLETEXT.Gameplay.Manager.MainHero;
using SAMPLETEXT.Market.Manager;
using SAMPLETEXT.Gameplay.Manager.Enemy;
using SAMPLETEXT.SkillsAnimation.Manager;
using SAMPLETEXT.SubHeroUI.Manager;

namespace SAMPLETEXT.Talent.Manager
{
    public class TalentsManagerScript : MonoBehaviour
    {
        [SerializeField]
        GameplayEnemyManagerScript MainEnemy;
        [SerializeField]
        MarketManagerScript MainMarket;
        [SerializeField]
        GameplayMainHeroManagerScript MainHero;
        [SerializeField]
        ProfileManagerScript MainProfileManager;
        [Header("Talent Settings")]
        [SerializeField]
        string[] TalentsDescriptionCollectionString;
        [SerializeField]
        string[] TalentsNameCollectionString;
        [SerializeField]
        TextMeshProUGUI TalentDescriptionText;
        [SerializeField]
        TextMeshProUGUI TalenNameText;
        [SerializeField]
        int TalentPageCount;
        public List<float> TalentSkillsCollectionValue; //JSON
        public float MeteorShowerTimerValue; //JSON
        public float IceSpikeTimerValue; //JSON
        public float ScaryScreamTimerValue; //JSON
        [Header("Talent Button Settings")]
        public List<float> TalentCollectionValue; //JSON
        public List<float> TalentCollectionRequireValue; //JSON
        public List<bool> TalentCollectionUnlockCondition; //JSON
        [SerializeField]
        Button TalentButton;
        [SerializeField]
        TextMeshProUGUI TalentButtonText;
        public float TalentPointValue; //JSON
        [SerializeField]
        TextMeshProUGUI TalentPointValueText;
        [SerializeField]
        GameObject[] TalentIconImageCollection;
        //[SerializeField]
        //Button[] TalentButtonCollection;

        [Header("Total Settings")]
        public float TotalAttackValue;
        public float TotalAttackValueSubHero;
        public float TotalAdditionalGoldValue;
        public float TotalAttackSpeed;
        public float TotalAttackSpeedSubHero;
        public float TotalReduceUpgradeCost;
        public float TotalAdditionalDamageFromEnemyHealth;
        public float TotalAdditionalDamageMeteorShowerFromEnemyHealth;
        public float TotalAdditionalGoldValueIceSpike;
        public float TotalAdditionalItemDropChance;
        public float TotalCriticalDamage;
        public float TotalCritialDamageSubHeroes;

        [Header("Artifact Talented Settings")]
        public float TotalAdditionalArtifactTalentValue;//NonJSON

        [Header("Skill Settings")]
        [SerializeField]
        GameObject LightningStrikeCollectionObj;
        [SerializeField]
        GameObject MeteorShowerCollectionObj;
        [SerializeField]
        GameObject IceSpikeCollectionObj;
        [SerializeField]
        GameObject ScaryScreamCollectionObj;
        [SerializeField]
        Button SkillButton;

        [Header("Lightning Strike Settings")]
        [SerializeField]
        LightningStrikeAnimationScript MainLightningStrike;

        [Header("Meteor Shower Settings")]
        [SerializeField]
        MeteorShowerAnimationScript MainMeteorShower;

        [Header("Ice Spike Settings")]
        [SerializeField]
        float CurrentIceSpikeCountDownTimer;
        [SerializeField]
        float IceSpikeCountDownTimerResetReference;
        public bool IceSpikeSkillActivate;

        [Header("Scary Scream Settings")]
        [SerializeField]
        public float ScaryScreamReferenceValue;
        [SerializeField]
        float CurrentScaryScreamCountDownTimer;
        [SerializeField]
        float ScaryScreamCountDownTimerResetReference;
        public bool ScaryScreamSkillActivate;

        [Header("SubHeroes Ref")]
        [SerializeField] SubHeroesManagerScript subheroScript;



        //[Header("Meteor Shower Settings")]


        // Start is called before the first frame update
        void Start()
        {
            //TalentPointValue = MainProfileManager.PointsValue;
            //FirstCheck();
        }

        public void FirstCheck()
        {
            for (int i = 0; i < TalentCollectionUnlockCondition.Count; i++)
            {
                if (TalentCollectionUnlockCondition[i] == true)
                {
                    TalentIconImageCollection[i].gameObject.SetActive(true);
                    //TalentButtonCollection[i].interactable = true;

                    
                }

                if (TalentCollectionUnlockCondition[i] == false)
                {
                    TalentIconImageCollection[i].gameObject.SetActive(false);
                    //TalentButtonCollection[i].interactable = false;

                }
            }

            for (int i = 1; i < TalentCollectionValue.Count; i++)
            {
                if (TalentCollectionValue[i] >= 1)
                {
                    TalentIconImageCollection[i].gameObject.SetActive(true);

                    if (TalentCollectionValue[10] >= 1)
                    {
                        LightningStrikeCollectionObj.gameObject.SetActive(true);
                    }

                    if (TalentCollectionValue[11] >= 1)
                    {
                        MeteorShowerCollectionObj.gameObject.SetActive(true);
                    }

                    if (TalentCollectionValue[12] >= 1)
                    {
                        IceSpikeCollectionObj.gameObject.SetActive(true);
                    }

                    if (TalentCollectionValue[13] >= 1)
                    {
                        ScaryScreamCollectionObj.gameObject.SetActive(true);
                    }
                }

                if (TalentCollectionValue[i] <= 0)
                {
                    TalentIconImageCollection[i].gameObject.SetActive(false);

                    if (TalentCollectionValue[10] <= 0)
                    {
                        LightningStrikeCollectionObj.gameObject.SetActive(false);
                    }

                    if (TalentCollectionValue[11] <= 0)
                    {
                        MeteorShowerCollectionObj.gameObject.SetActive(false);
                    }

                    if (TalentCollectionValue[12] <= 0)
                    {
                        IceSpikeCollectionObj.gameObject.SetActive(false);
                    }

                    if (TalentCollectionValue[13] <= 0)
                    {
                        ScaryScreamCollectionObj.gameObject.SetActive(false);
                    }
                }
            }

            //Skills

            TalentDescriptionText.text = string.Empty;
            TalenNameText.text = string.Empty;
            CheckSkillButtonManualUpdate();
            PointsTextManualUpdate();
            MainHero.ManualUpdateDPS();
        }

        public void PointsTextManualUpdate()
        {
            TalentPointValueText.text = "POINTS: " + TalentPointValue.ToString();
        }

        public void AdditionalBossDamageHealthPercentageUpdate()
        {
            float TempValue = MainEnemy.EnemyHealthValue * TalentSkillsCollectionValue[10];
            float TempAdditionalValue = TempValue; //* TotalAdditionalArtifactTalentValue;
            float TempTotalValue = TempValue + TempAdditionalValue;

            TotalAdditionalDamageFromEnemyHealth = TempTotalValue;

            MainLightningStrike.LightningStrikeDamageValue = TotalAdditionalDamageFromEnemyHealth;
        }

        void AdditionalBossDamageHealthPercentageMeteorShowerUpdate()
        {
            float TempValue = MainEnemy.EnemyHealthValue * TalentSkillsCollectionValue[11];
            float TempAdditionalValue = TempValue; //* TotalAdditionalArtifactTalentValue;
            float TempTotalValue = TempValue + TempAdditionalValue;

            TotalAdditionalDamageMeteorShowerFromEnemyHealth = TempTotalValue;

            MainMeteorShower.MeteorDamageValue = TotalAdditionalDamageMeteorShowerFromEnemyHealth;

        }
        public void TalentDescriptionButton(int TalentDescriptionID)
        {
            TalentPageCount = TalentDescriptionID;
            TalentDescriptionText.text = string.Empty;
            TalenNameText.text = string.Empty;

            TalenNameText.text = TalentsNameCollectionString[TalentDescriptionID].ToString();

            if (TalentPageCount == 10)
            {
                TalentDescriptionText.text = TalentsDescriptionCollectionString[TalentDescriptionID] + " " + Mathf.Round(TalentSkillsCollectionValue[TalentDescriptionID] * 100f) /100f + "%" + " OF BOSS HEALTH".ToString();
            }

            else if (TalentPageCount == 11)
            {
                TalentDescriptionText.text = TalentsDescriptionCollectionString[TalentDescriptionID] + " " + Mathf.Round(TalentSkillsCollectionValue[TalentDescriptionID] * 100f) / 100f + "%" + " OF BOSS HEALTH OVER " + MeteorShowerTimerValue + " SECONDS " + " DURATION" .ToString();
            }

            else if(TalentPageCount == 12)
            {
                TalentDescriptionText.text = TalentsDescriptionCollectionString[TalentDescriptionID] + " " + Mathf.Round(TalentSkillsCollectionValue[TalentDescriptionID] * 100f) / 100f + "%" +  " FOR " + IceSpikeTimerValue + " SECONDS".ToString();
            }

            else if(TalentPageCount == 13)
            {
                TalentDescriptionText.text = TalentsDescriptionCollectionString[TalentDescriptionID] + Mathf.Round(TalentSkillsCollectionValue[TalentDescriptionID] * 100f) / 100f + "%" + " MORE DAMAGE BY ALL HEROES FOR " + ScaryScreamTimerValue + " SECONDS".ToString();
            }

            else if(TalentPageCount == 0)
            {
                TalentDescriptionText.text = TalentsDescriptionCollectionString[TalentDescriptionID] + " " + TalentPointValue.ToString();
            }

            else
            {
                TalentDescriptionText.text = TalentsDescriptionCollectionString[TalentDescriptionID] + " " + Mathf.Round(TalentSkillsCollectionValue[TalentDescriptionID] * 100f) / 100f + "%".ToString();
            }

            ButtonCheck();
        }

        void ButtonCheck()
        {
            if (TalentPageCount != 0)
            {
                TalentButton.gameObject.SetActive(true);
                TalentButtonText.text = string.Empty;

                if (TalentPointValue >= TalentCollectionRequireValue[TalentPageCount] && TalentCollectionUnlockCondition[TalentPageCount] == true)
                {
                    TalentButtonText.text = string.Empty;
                    TalentButtonText.text = "UNLOCK: " + TalentPointValue + "/" + TalentCollectionRequireValue[TalentPageCount].ToString();
                    TalentButton.interactable = true;

                }

                if (TalentPointValue < TalentCollectionRequireValue[TalentPageCount] && TalentCollectionUnlockCondition[TalentPageCount] == true)
                {
                    TalentButtonText.text = "UNLOCK: " + TalentPointValue + "/" + TalentCollectionRequireValue[TalentPageCount].ToString();
                    TalentButton.interactable = false;

                   

                }

                if (TalentPointValue >= TalentCollectionRequireValue[TalentPageCount] && TalentCollectionUnlockCondition[TalentPageCount] == false)
                {
                    TalentButtonText.text = "LOCKED".ToString();
                    TalentButton.interactable = false;

                }

                if (TalentPointValue < TalentCollectionRequireValue[TalentPageCount] && TalentCollectionUnlockCondition[TalentPageCount] == false)
                {
                    TalentButtonText.text = "LOCKED".ToString();
                    TalentButton.interactable = false;

                }


            }

            if (TalentPageCount == 0)
            {
                TalentButton.interactable = true;
                TalentButton.gameObject.SetActive(true);
                TalentButtonText.text = "RESET".ToString();
            }
        }

        public void ButtonPurchase()
        {
            if (TalentPageCount != 0)
            {
                TalentPointValue -= 1;
                TalentCollectionValue[TalentPageCount] += 1;
                TalentSkillsCollectionValue[TalentPageCount] += 1f; //changed from .01f


                if (TalentPageCount == 1)
                {
                    //if (TalentCollectionValue[1] >= 1)
                    //{
                    //    TalentIconImageCollection[1].gameObject.SetActive(true);
                    //}

                    if (TalentCollectionUnlockCondition[2] == false)
                    {
                       

                        TalentCollectionUnlockCondition[2] = true;
                        //TalentIconImageCollection[2].gameObject.SetActive(true);
                        //TalentButtonCollection[2].interactable = true;
                    }

                    if (TalentCollectionUnlockCondition[3] == false)
                    {
                        TalentCollectionUnlockCondition[3] = true;

                        //TalentIconImageCollection[3].gameObject.SetActive(true);
                        //TalentButtonCollection[3].interactable = true;
                    }
                }

                if (TalentPageCount == 4)
                {
                    //if (TalentCollectionValue[4] >= 1)
                    //{
                    //    TalentIconImageCollection[4].gameObject.SetActive(true);
                    //}
                    if (TalentCollectionUnlockCondition[5] == false)
                    {
                        TalentCollectionUnlockCondition[5] = true;

                        //TalentIconImageCollection[5].gameObject.SetActive(true);
                        //TalentButtonCollection[5].interactable = true;
                    }

                    if (TalentCollectionUnlockCondition[6] == false)
                    {
                        TalentCollectionUnlockCondition[6] = true;

                        //TalentIconImageCollection[6].gameObject.SetActive(true);
                        //TalentButtonCollection[6].interactable = true;
                    }
                }
         
                if (TalentPageCount == 7)
                {
                    //if (TalentCollectionValue[7] >= 1)
                    //{
                    //    TalentIconImageCollection[7].gameObject.SetActive(true);
                    //}

                    if (TalentCollectionUnlockCondition[8] == false)
                    {
                        TalentCollectionUnlockCondition[8] = true;

                        //TalentIconImageCollection[8].gameObject.SetActive(true);
                        //TalentButtonCollection[8].interactable = true;
                    }

                    if (TalentCollectionUnlockCondition[9] == false)
                    {
                        TalentCollectionUnlockCondition[9] = true;

                        //TalentIconImageCollection[9].gameObject.SetActive(true);
                        //TalentButtonCollection[9].interactable = true;
                    }
                }

                if (TalentPageCount == 10)
                {
                    //if (TalentCollectionValue[10] >= 1)
                    //{
                    //    TalentIconImageCollection[10].gameObject.SetActive(true);
                    //}
                    
                    FirstCheck();
                    if (TalentCollectionUnlockCondition[11] == false)
                    {
                        TalentCollectionUnlockCondition[11] = true;

                        //TalentIconImageCollection[11].gameObject.SetActive(true);
                        //TalentButtonCollection[11].interactable = true;
                    }
                }

                if (TalentPageCount == 11)
                {
                    MeteorShowerTimerValue += 1;

                    FirstCheck();
                    if (TalentCollectionUnlockCondition[12] == false)
                    {
                        TalentCollectionUnlockCondition[12] = true;


                        //TalentIconImageCollection[12].gameObject.SetActive(true);
                        //TalentButtonCollection[12].interactable = true;
                    }


                }

                if (TalentPageCount == 12)
                {
                    IceSpikeTimerValue += 1;

                    FirstCheck();
                    if (TalentCollectionUnlockCondition[13] == false)
                    {
                        TalentCollectionUnlockCondition[13] = true;

                        //TalentIconImageCollection[13].gameObject.SetActive(true);
                        //TalentButtonCollection[13].interactable = true;
                    }
                }

                if (TalentPageCount == 13)
                {
                    FirstCheck();
                    ScaryScreamTimerValue += 1;
                }
                

                for (int i = 1; i < TalentCollectionValue.Count; i++)
                {
                    if (TalentCollectionValue[i] >= 1)
                    {
                        TalentIconImageCollection[i].gameObject.SetActive(true);
                    }

                    if (TalentCollectionValue[i] <= 0)
                    {
                        TalentIconImageCollection[i].gameObject.SetActive(false);
                    }
                }

                TalentDescriptionButton(TalentPageCount);
                PointsTextManualUpdate();
            }

            if (TalentPageCount == 0)
            {
                for (int i = 0; i < TalentCollectionValue.Count; i++)
                {
                    TalentCollectionValue[i] = 0;
                }

                for (int i = 0; i < TalentCollectionRequireValue.Count; i++)
                {
                    TalentCollectionRequireValue[i] = 1;
                }

                for (int i = 11; i < TalentCollectionUnlockCondition.Count; i++)
                {
                    TalentCollectionUnlockCondition[i] = false;
                }

                for (int i = 1; i < TalentSkillsCollectionValue.Count; i++)
                {
                    TalentSkillsCollectionValue[i] = 0.01f;
                }

                TalentPointValue = MainProfileManager.PointsValue;
                MeteorShowerTimerValue = 5;
                IceSpikeTimerValue = 5;
                ScaryScreamTimerValue = 5;

                PointsTextManualUpdate();

                TalentDescriptionButton(0);

                TotalAttackValue = 0;
                TotalAdditionalGoldValue = 0;
                TotalAttackSpeed = 0;
                TotalReduceUpgradeCost = 0;
                TotalAttackValueSubHero = 0;
                TotalAttackSpeedSubHero = 0;
                TotalCritialDamageSubHeroes = 0f;
                TotalCriticalDamage = 0f;
                TotalAdditionalItemDropChance = 0f;
                MainEnemy.ItemChanceDropMaxValue = MainEnemy.ItemChanceDropMaxValueDefault;

                foreach (GameObject TIIC in TalentIconImageCollection)
                {
                    TIIC.gameObject.SetActive(false);
                }

                TalentIconImageCollection[0].gameObject.SetActive(true);

                subheroScript.ResetSpeedTalent2();
            }

            TotalAttackValueManualUpdate();
            subheroScript.SubHeroUpgradeCollection(0);
            
        }

        public void TotalAttackValueManualUpdate()
        {
            if(TalentPageCount == 1) //extra damage for subheroes
            {
                    float TempValue = MainHero.MainHeroDPSMaxDamageValue * (TalentSkillsCollectionValue[TalentPageCount] / 100f);
                float TempAdditionalValue = TempValue;// * TotalAdditionalArtifactTalentValue;
                    float TempTotalValue = TempValue + TempAdditionalValue;
                    TotalAttackValueSubHero += TempTotalValue;

                subheroScript.SubHeroUpgradeCollection(0);

            }
            if (TalentPageCount == 4) //|| TalentPageCount == 4) //Attack Damage
            {
                float TempValue = MainHero.MainHeroDPSMaxDamageValue * (TalentSkillsCollectionValue[TalentPageCount] / 100f);
                float TempAdditionalValue = TempValue; //* TotalAdditionalArtifactTalentValue;
                float TempTotalValue = TempValue + TempAdditionalValue;
                TotalAttackValue += TempTotalValue;

                MainHero.ManualUpdateDPS();
            }

            if(TalentPageCount == 2) //attack speed for sub-heroes
            {
                for (int i = 0; i < subheroScript.SubHeroAttackSpeedCollection.Length; i++)
                {
                    float TempValue = 2f * (TalentSkillsCollectionValue[TalentPageCount] / 100f); //2f is attack speed default
                    float TempAdditionalValue = TempValue; //* TotalAdditionalArtifactTalentValue;
                    float TempTotalValue = TempValue + TempAdditionalValue;
                    TotalAttackSpeedSubHero = TempTotalValue;
                    subheroScript.SubHeroAttackSpeedCollection[i] += TotalAttackSpeedSubHero;
                }
                
               
                //TotalAttackSpeed += TempTotalValue;

                //MainHero.AttackSpeedValue += TempTotalValue;

                //MainHero.ManualUpdateDPS();
            }

            if (TalentPageCount == 5) // Attack Speed
            {
                float TempValue = MainHero.AttackSpeedValue * (TalentSkillsCollectionValue[TalentPageCount] / 100f);
                float TempAdditionalValue = TempValue; //* TotalAdditionalArtifactTalentValue;
                float TempTotalValue = TempValue + TempAdditionalValue;
                //TotalAttackSpeed += TempTotalValue;

                MainHero.AttackSpeedValue += TempTotalValue;

                MainHero.ManualUpdateDPS();
            }

            if (TalentPageCount == 6) //Critical Damage
            {
                float TempValue = MainHero.MainHeroDPSMaxDamageValue * (TalentSkillsCollectionValue[TalentPageCount] / 100f);
                float TempAdditionalValue = TempValue; //* TotalAdditionalArtifactTalentValue;
                float TempTotalValue = TempValue + TempAdditionalValue;
                TotalCriticalDamage += TempTotalValue;

               // MainHero.ManualUpdateDPS();
            }

            if (TalentPageCount == 3) //Critical Damage SUB HEROES
            {
                float TempValue = MainHero.MainHeroDPSMaxDamageValue * (TalentSkillsCollectionValue[TalentPageCount] / 100f);
                float TempAdditionalValue = TempValue; //* TotalAdditionalArtifactTalentValue;
                float TempTotalValue = TempValue + TempAdditionalValue;
                TotalCritialDamageSubHeroes += TempTotalValue;

                //MainHero.ManualUpdateDPS();
            }

            if (TalentPageCount == 7) //Drop item chance BUGGY MATHS
            {
                float TempTalentDropChance = MainEnemy.ItemChanceDropMaxValue * (TalentSkillsCollectionValue[TalentPageCount] / 100f); //incremental
               // float TempAdditionalValue = TempTalentDropChance * TotalAdditionalArtifactTalentValue;
               // float TempTotalValue = TempTalentDropChance + TempAdditionalValue;
                TotalAdditionalItemDropChance = TempTalentDropChance;

                MainEnemy.ItemChanceDropMaxValue += TotalAdditionalItemDropChance;
                //MainEnemy.TalentItemChanceDropAdditional = TotalAdditionalItemDropChance;
            }

            if (TalentPageCount == 8) // income village total 
            {
                float TempValue = MainMarket.TotalIncomeValue * (TalentSkillsCollectionValue[TalentPageCount] / 100f);
                float TempAdditionalValue = TempValue; //* TotalAdditionalArtifactTalentValue;
                float TempTotalValue = TempValue + TempAdditionalValue;
                TotalAdditionalGoldValue = TempTotalValue;

               // MainMarket.CheckTotalValueManualUpdate();

            }

            if (TalentPageCount == 9)
            {
                float TempValue = (TalentSkillsCollectionValue[TalentPageCount] / 100f);
                float TempAdditionalValue = TempValue; //* TotalAdditionalArtifactTalentValue;
                float TempTotalValue = TempValue + TempAdditionalValue;
                TotalReduceUpgradeCost = TempTotalValue;

                MainMarket.ManualMarketValueCheckUpdate();
            }

            if (TalentPageCount == 10) // Lightning Strike
            {
                //AdditionalBossDamageHealthPercentageUpdate();
                CheckSkillButtonManualUpdate();
            }

            if (TalentPageCount == 11) // Meteor Shower
            {
                AdditionalBossDamageHealthPercentageMeteorShowerUpdate();
            }

            if (TalentPageCount == 12) // Ice Spike
            {
                float TempValue = MainEnemy.CurrentGoldCoinEnemyDropValue * TalentSkillsCollectionValue[12];
                float TempAdditionalValue = TempValue; //* TotalAdditionalArtifactTalentValue;
                float TempTotalValue = TempValue + TempAdditionalValue;
                TotalAdditionalGoldValueIceSpike = TempTotalValue;
                
            }

            if (TalentPageCount == 13) // Scary Scream
            {
                
            }

        }

        void CheckSkillButtonManualUpdate()
        {
            if (LightningStrikeCollectionObj.activeSelf == true)
            {
                SkillButton.interactable = true;
            }

            if (LightningStrikeCollectionObj.activeSelf == false)
            {
                SkillButton.interactable = false;
            }
        }

        public void SkillValueManualUpdate(int SkillsID)
        {
            if (SkillsID == 10) // Lightning Strike
            {
               // AdditionalBossDamageHealthPercentageUpdate();
            }

            if (SkillsID == 11) // Meteor Shower
            {
                AdditionalBossDamageHealthPercentageMeteorShowerUpdate();
            }

            if (SkillsID == 12) // Ice Spike
            {
                float TempValue = MainEnemy.CurrentGoldCoinEnemyDropValue * TalentSkillsCollectionValue[12];
                float TempAdditionalValue = TempValue; //* TotalAdditionalArtifactTalentValue;
                float TempTotalValue = TempValue + TempAdditionalValue;
                TotalAdditionalGoldValueIceSpike = TempTotalValue;

                CurrentIceSpikeCountDownTimer = IceSpikeCountDownTimerResetReference;

            }

            if (SkillsID == 13) // Scary Scream
            {
                ScaryScreamReferenceValue = TalentSkillsCollectionValue[13];
                CurrentScaryScreamCountDownTimer = ScaryScreamCountDownTimerResetReference;
            }

        }

        // Update is called once per frame
        void Update()
        {
            if (CurrentIceSpikeCountDownTimer >= 0.01f)
            {
                CurrentIceSpikeCountDownTimer -= Time.deltaTime;
                IceSpikeSkillActivate = true;
            }

            if (CurrentIceSpikeCountDownTimer <= 0.0f)
            {
                CurrentIceSpikeCountDownTimer = 0.0f;
                IceSpikeSkillActivate = false;
            }

            if (CurrentScaryScreamCountDownTimer >= 0.01f)
            {
                CurrentScaryScreamCountDownTimer -= Time.deltaTime;
                ScaryScreamSkillActivate = true;
            }

            if (CurrentScaryScreamCountDownTimer <= 0.0f)
            {
                CurrentScaryScreamCountDownTimer = 0.0f;
                ScaryScreamSkillActivate = false;
            }
        }

    }
}

