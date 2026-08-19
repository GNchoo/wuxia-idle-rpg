using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAMPLETEXT.Gameplay.Manager.Enemy;
using SAMPLETEXT.Wallet.Manager;
using SAMPLETEXT.Talent.Manager;
using SAMPLETEXT.Artifact.Manager;
using UnityEngine.UI;
using SAMPLETEXT.SubHeroUI.Manager;
using SAMPLETEXT.ItemPurchase.Manager.Boost;
using SAMPLETEXT.ItemPurchase.Manager.Gem;
using TMPro;

namespace SAMPLETEXT.Gameplay.Manager.MainHero
{
    public class GameplayMainHeroManagerScript : MonoBehaviour
    {
        [SerializeField]
        ArtifactManagerScript MainArtifact;
        [SerializeField]
        TalentsManagerScript MainTalent;
        [SerializeField]
        WalletManagerScript MainWallet;
        [Header("Main Hero Settings")]
        [SerializeField]
        GameplayEnemyManagerScript MainEnemyStats;
        //public float MainHeroDPSMinDamageValue;
      
        [HideInInspector]
        public float MainHeroDPSDamageValue;
        [SerializeField]
        GameObject MainHeroAttackObj;
        [SerializeField]
        Transform MainHeroAttackSpawnPos;
        [SerializeField]
        TextMeshProUGUI MainHeroDPSMaxDamageValueText;
        //public float SubHeroTotalDPS;
        public float TempValueHeroDamage;

        [Header("Attack Damage Text Spawn Settings")]
        [SerializeField]
        GameObject MainHeroDamageTextObj;
        [SerializeField]
        Transform MainHeroDamageSpawnPos;

        //[Header("For Testing Only")]
        //[SerializeField]
        //public float MaxTimer;
        //float Timer;
        //bool TimerControl;

        [Header("Speed Attack Settings")]
        public float AttackSpeedValue;
        [SerializeField]
        Animator MainHeroAnim;

        [Header("Main Character Base DPS Settings")]
        [SerializeField]
        Button DPSBaseUpgradeButton;
        [SerializeField]
        TextMeshProUGUI DPSBasePurchaseCostText;
        TextMeshProUGUI DPSLevelsPurchaseCost;
        public float DPSBasePurchaseCostValue;
        public float MainHeroDPSMaxDamageValue;
        public int MainHeroLevel;
        [SerializeField]
        TextMeshProUGUI MainHeroLevelText;

        [Header("Main Hero Upgrade Collection Settings")]
        [SerializeField]
        SubHeroesManagerScript MainSubHero;
        [SerializeField]
        float DPSBasePurchaseCostValueTemp;

        [Header("Main Hero Prestige Reset Settings")]
        [SerializeField]
        float DPSBasePurchaseCostValueReset;
        [SerializeField]
        float MainHeroDPSMaxDamageValueReset;

        [Header("Main Player Inventory Settings")]
        public float InventoryAdditionalDPS;
        public float InventoryAdditionalAttackSpeed;
        public float InventoryCriticalDamageIncrease;

        [Header("Group Purchase Settings")]
        [SerializeField]
        float TempMaxPurchaseGroupValue;
        [SerializeField]
        int FinalTempMaxPurchaseGroupValue;
        [SerializeField]
        float TempMaxMainHeroLevelPurchaseCostGroupValue;

        [Header("Miss System")]
        [SerializeField]
        public float mainHeroDamageMiss;
        [SerializeField]
        public float mainHeroDamageChanceCurrent = 95f;
        [SerializeField]
        public float mainHeroDamageChanceMax = 100f;
        [SerializeField] public bool missedAttack = false;

        [Header("Critical System")]
        [SerializeField]
        public float criticalChance;
        [SerializeField]
        public float criticalDamage;
        [SerializeField]
        public bool criticalAttacked = false;
        [SerializeField]
        public float criticalAttackChance;

        //DRYMARTI
        [Header("Double Damage")]
        [SerializeField]
        ItemsPurchaseBoostManagerScript boostPurchaseScript;
        [SerializeField]
        DiamondPurchaseManagerScript gemsScript;


        // Start is called before the first frame update
        void Start()
        {
            FirstCheck();
        }

        public void FirstCheck()
        {
            float TempScaryScreamDamage = 0;
            if (MainTalent.ScaryScreamSkillActivate == true)
            {
                TempScaryScreamDamage = MainTalent.ScaryScreamReferenceValue * TempValueHeroDamage;
            }

            if(boostPurchaseScript.ActivateDoubleGoldIncomeCondition == false && gemsScript.VIPCountValue < 2)
            {
                float TempValue = MainHeroDPSMaxDamageValue + MainTalent.TotalAttackValue + MainTalent.TotalAdditionalDamageFromEnemyHealth   +  (MainHeroDPSMaxDamageValue * MainArtifact.TrippleSwordTotalValue) + TempScaryScreamDamage;// + SubHeroTotalDPS;
                MainWallet.MainHeroTotalDPS = TempValue;

                if (TempValue <= 999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + TempValue.ToString("F0");
                }

                else if (TempValue <= 999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + TempValue.ToString("F2") + "K";
                }

                else if (TempValue <= 999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + TempValue.ToString("F2") + "M";
                }

                else if (TempValue <= 999999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + TempValue.ToString("F2") + "B";
                }
                else if (TempValue <= 999999999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + TempValue.ToString("F2") + "T";
                }
            }
            else if (boostPurchaseScript.ActivateDoubleGoldIncomeCondition == true || gemsScript.VIPCountValue >= 2)
            {
                float TempValue = (MainHeroDPSMaxDamageValue + MainTalent.TotalAttackValue + MainTalent.TotalAdditionalDamageFromEnemyHealth + (MainHeroDPSMaxDamageValue * MainArtifact.TrippleSwordTotalValue) + TempScaryScreamDamage) * 2;// + SubHeroTotalDPS; double damage
                MainWallet.MainHeroTotalDPS = TempValue;

                if (TempValue <= 999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + TempValue.ToString("F0") + " X2";
                }

                else if (TempValue >= 1000f && TempValue <= 999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + TempValue.ToString("F2") + "K" + " X2";
                }

                else if (TempValue >= 10000000f && TempValue <= 999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + TempValue.ToString("F2") + "M" + " X2";
                }

                else if (TempValue >= 100000000f && TempValue <= 999999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + TempValue.ToString("F2") + "B" + " X2";
                }
                else if (TempValue >= 100000000000f && TempValue <= 999999999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + TempValue.ToString("F2") + "T" + " X2";
                }
            }
           

           
            if (DPSBasePurchaseCostValue <= 999f)
            {
                DPSBasePurchaseCostText.text = "UPGRADE" + "\n" + DPSBasePurchaseCostValue.ToString("F0");
            }

            else if (DPSBasePurchaseCostValue >= 1000f && DPSBasePurchaseCostValue <= 999999f)
            {
                DPSBasePurchaseCostText.text = "UPGRADE" + "\n" + DPSBasePurchaseCostValue.ToString("F2") + "K";
            }

            else if (DPSBasePurchaseCostValue >= 10000000f && DPSBasePurchaseCostValue <= 999999999f)
            {
                DPSBasePurchaseCostText.text = "UPGRADE" + "\n" + DPSBasePurchaseCostValue.ToString("F2") + "M";
            }

            else if (DPSBasePurchaseCostValue >= 100000000f && DPSBasePurchaseCostValue <= 999999999999f)
            {
                DPSBasePurchaseCostText.text = "UPGRADE" + "\n" + DPSBasePurchaseCostValue.ToString("F2") + "B";
            }
            else if (DPSBasePurchaseCostValue >= 100000000000f && DPSBasePurchaseCostValue <= 999999999999999f)
            {
                DPSBasePurchaseCostText.text = "UPGRADE" + "\n" + DPSBasePurchaseCostValue.ToString("F2") + "T";
            }

            MainHeroLevelText.text = "LVL:" + MainHeroLevel.ToString();
            //MainHeroDPSMaxDamageValueText.text = MainHeroDPSMaxDamageValue.ToString("F0");

            DPSBasePurchaseCostValueTemp = DPSBasePurchaseCostValue;

            MainWallet.WalletValueManualUpdate();
            CheckUpgradeTextManualUpdate();
        }

        public void ManualUpdateDPS()
        {
            float TempScaryScreamDamage = 0;
            if (MainTalent.ScaryScreamSkillActivate == true)
            {
                TempScaryScreamDamage = MainTalent.ScaryScreamReferenceValue * TempValueHeroDamage;
            }

            if (boostPurchaseScript.ActivateDoubleGoldIncomeCondition == false && gemsScript.VIPCountValue < 2)
            {
                float TempValue = MainHeroDPSMaxDamageValue + MainTalent.TotalAttackValue + MainTalent.TotalAdditionalDamageFromEnemyHealth + (MainHeroDPSMaxDamageValue * MainArtifact.TrippleSwordTotalValue) + TempScaryScreamDamage;// + SubHeroTotalDPS;
                MainWallet.MainHeroTotalDPS = TempValue;

                if (TempValue <= 999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + TempValue.ToString("F0");
                }

                else if (TempValue >= 1000f && TempValue <= 999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + (TempValue / 1000f).ToString("F2") + "K";
                }

                else if (TempValue >= 1000000f && TempValue <= 999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + (TempValue / 1000000f).ToString("F2") + "M";
                }

                else if (TempValue >= 1000000000f && TempValue <= 999999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + (TempValue / 1000000000f).ToString("F2") + "B";
                }
                else if (TempValue >= 1000000000000f && TempValue <= 999999999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + (TempValue / 1000000000000f).ToString("F2") + "T";
                }

            }
            else if (boostPurchaseScript.ActivateDoubleGoldIncomeCondition == true || gemsScript.VIPCountValue >= 2)// double damage
            {
                float TempValue = (MainHeroDPSMaxDamageValue + MainTalent.TotalAttackValue + MainTalent.TotalAdditionalDamageFromEnemyHealth + (MainHeroDPSMaxDamageValue * MainArtifact.TrippleSwordTotalValue) + TempScaryScreamDamage) * 2;// + SubHeroTotalDPS;
                MainWallet.MainHeroTotalDPS = TempValue;

                if (TempValue <= 999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + TempValue.ToString("F0") + " X2";
                }

                else if (TempValue >= 1000f && TempValue <= 999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + (TempValue / 1000f).ToString("F2") + "K" + " X2";
                }

                else if (TempValue >= 1000000f && TempValue <= 999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + (TempValue / 1000000f).ToString("F2") + "M" + " X2";
                }

                else if (TempValue >= 1000000000f && TempValue <= 999999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + (TempValue / 1000000000f).ToString("F2") + "B" + " X2";
                }

                else if (TempValue >= 1000000000000f && TempValue <= 999999999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + (TempValue / 1000000000000f).ToString("F2") + "T" + " X2";
                }

            }


            MainHeroLevelText.text = "LVL:" + MainHeroLevel.ToString();

            MainHeroAnim.speed = AttackSpeedValue;

            if (MainHeroAnim.speed >= 1.5f)
            {
                MainHeroAnim.speed = 1.5f;
            }

            MainWallet.WalletValueManualUpdate();
            
        }

        void TimerCountDownTempUpdate()
        {
            //if (TimerControl == false)
            //{
            //    Timer = MaxTimer - MainTalent.TotalAttackSpeed;
            //    TimerControl = true;
            //}

            //Timer -= Time.deltaTime;

            //if (Timer <= 0)
            //{
            //    if (TimerControl == true)
            //    {
            //        Instantiate(MainHeroAttackObj, MainHeroAttackSpawnPos.transform.position, MainHeroAttackSpawnPos.rotation);

            //        TimerControl = false;
            //    }
            //}

            
        }
        public void AttackActivate()
        {
            Instantiate(MainHeroAttackObj, MainHeroAttackSpawnPos.transform.position, MainHeroAttackSpawnPos.rotation);
        }
        // Update is called once per frame
        void Update()
        {
                // TimerCountDownTempUpdate();
                //CheckUpgradeTextManualUpdate();

            }

        public void EnemyDamage()
        {
            //float i = Random.Range(MainHeroDPSMinDamageValue, MainHeroDPSMaxDamageValue);

            float TempMainHeroDPS = MainHeroDPSMaxDamageValue * InventoryAdditionalDPS;
            float TempMainHeroAttackSpeedValue = AttackSpeedValue * InventoryAdditionalAttackSpeed;
            float TempMainHeroCriticalDamage = (MainHeroDPSMaxDamageValue * AttackSpeedValue) * InventoryCriticalDamageIncrease;

            float TempScaryScreamDamage = 0;
            if (MainTalent.ScaryScreamSkillActivate == true)
            {
                TempScaryScreamDamage = MainTalent.ScaryScreamReferenceValue * TempValueHeroDamage;
                ManualUpdateDPS();
            }

            if (MainTalent.ScaryScreamSkillActivate == false)
            {
                ManualUpdateDPS();
            }

            mainHeroDamageMiss = Random.Range(0, mainHeroDamageChanceMax); //miss chance
            criticalChance = Random.Range(0, mainHeroDamageChanceMax);
            criticalAttackChance = MainArtifact.InevitableVictoryTotalValue; //critical artifact chance
            criticalDamage = ((MainHeroDPSMaxDamageValue * AttackSpeedValue) * ((MainArtifact.BloodySkullTotalValue + MainTalent.TotalCriticalDamage) / 100f));

            if (mainHeroDamageMiss >= mainHeroDamageChanceCurrent)
            {
                missedAttack = true;
                criticalAttacked = false;
                Instantiate(MainHeroDamageTextObj, MainHeroDamageSpawnPos.transform.position, MainHeroDamageSpawnPos.rotation);
            }
            else if (criticalChance > criticalAttackChance)
            {
                missedAttack = false;
                criticalAttacked = false;
                //DOUBLE DAMAGE
                if (boostPurchaseScript.ActivateDoubleGoldIncomeCondition == false && gemsScript.VIPCountValue < 2)
                {
                    TempValueHeroDamage = (MainHeroDPSMaxDamageValue + TempMainHeroDPS + TempMainHeroCriticalDamage + MainTalent.TotalAttackValue + MainTalent.TotalAdditionalDamageFromEnemyHealth + MainTalent.TotalAdditionalDamageFromEnemyHealth + (MainHeroDPSMaxDamageValue * MainArtifact.TrippleSwordTotalValue)) * AttackSpeedValue + TempMainHeroAttackSpeedValue + TempScaryScreamDamage;// + SubHeroTotalDPS;
                }
                else if (boostPurchaseScript.ActivateDoubleGoldIncomeCondition == true || gemsScript.VIPCountValue >= 2)
                {
                    TempValueHeroDamage = ((MainHeroDPSMaxDamageValue + TempMainHeroDPS + TempMainHeroCriticalDamage + MainTalent.TotalAttackValue + MainTalent.TotalAdditionalDamageFromEnemyHealth + MainTalent.TotalAdditionalDamageFromEnemyHealth + (MainHeroDPSMaxDamageValue * MainArtifact.TrippleSwordTotalValue)) * AttackSpeedValue + TempMainHeroAttackSpeedValue + TempScaryScreamDamage) * 2;// + SubHeroTotalDPS;
                }
                    
                    MainHeroDPSDamageValue = TempValueHeroDamage;
                MainEnemyStats.EnemyHealthValue -= TempValueHeroDamage;
                MainEnemyStats.EnemyHealthCheckReduction();
                Instantiate(MainHeroDamageTextObj, MainHeroDamageSpawnPos.transform.position, MainHeroDamageSpawnPos.rotation);
                
            }
            else if (criticalChance <= criticalAttackChance) //CRITICAL ATTACK
            {
                missedAttack = false;
                criticalAttacked = true;
                //DOUBLE DAMAGE
                if (boostPurchaseScript.ActivateDoubleGoldIncomeCondition == false && gemsScript.VIPCountValue < 2)
                {
                    TempValueHeroDamage = (MainHeroDPSMaxDamageValue + TempMainHeroDPS + TempMainHeroCriticalDamage + MainTalent.TotalAttackValue + MainTalent.TotalAdditionalDamageFromEnemyHealth + MainTalent.TotalAdditionalDamageFromEnemyHealth + (MainHeroDPSMaxDamageValue * (MainArtifact.BloodySkullTotalValue / 100f)) + (MainHeroDPSMaxDamageValue * MainArtifact.TrippleSwordTotalValue) + MainTalent.TotalCriticalDamage / 100f) * AttackSpeedValue + TempMainHeroAttackSpeedValue + TempScaryScreamDamage;// + SubHeroTotalDPS;
                }
                else if (boostPurchaseScript.ActivateDoubleGoldIncomeCondition == true || gemsScript.VIPCountValue >= 2)
                {
                    TempValueHeroDamage = ((MainHeroDPSMaxDamageValue + TempMainHeroDPS + TempMainHeroCriticalDamage + MainTalent.TotalAttackValue + MainTalent.TotalAdditionalDamageFromEnemyHealth + MainTalent.TotalAdditionalDamageFromEnemyHealth + (MainHeroDPSMaxDamageValue * (MainArtifact.BloodySkullTotalValue / 100f)) + (MainHeroDPSMaxDamageValue * MainArtifact.TrippleSwordTotalValue) + MainTalent.TotalCriticalDamage) * AttackSpeedValue + TempMainHeroAttackSpeedValue + TempScaryScreamDamage) * 2;// + SubHeroTotalDPS;
                }

                MainHeroDPSDamageValue = TempValueHeroDamage;
                MainEnemyStats.EnemyHealthValue -= TempValueHeroDamage;
                MainEnemyStats.EnemyHealthCheckReduction();
                Instantiate(MainHeroDamageTextObj, MainHeroDamageSpawnPos.transform.position, MainHeroDamageSpawnPos.rotation);
            }

               
        }

        public void BaseDPSUpgradeButton()
        {
            MainWallet.GoldWalletValue -= DPSBasePurchaseCostValueTemp;
            int levelsPurchased = 0; // Determine levels based on upgrade ID

            switch (MainSubHero.SubHeroUpgradeID)
            {
                case 0: // x1
                    levelsPurchased = 1;
                    break;
                case 1: // x10
                    levelsPurchased = 10;
                    break;
                case 2: // x100
                    levelsPurchased = 100;
                    break;
                case 3: // MAX
                    float currentCost = DPSBasePurchaseCostValue;
                    while (MainWallet.GoldWalletValue >= currentCost)
                    {
                        MainWallet.GoldWalletValue -= currentCost;
                        currentCost *= 1.03f; // Assuming a 2% increase per level
                        levelsPurchased++;
                    }
                    break;
            }

            float cumulativeCost = DPSBasePurchaseCostValue;
            float cumulativeDPS = MainHeroDPSMaxDamageValue;
            if (maxIsSelected)
            {
                for (int i = 0; i < FinalTempMaxPurchaseGroupValue; i++)
                {
                    cumulativeCost *= 1.03f; // Incremental cost
                    cumulativeDPS *= 1.03f;  // Incremental DPS
                }
            }
            else
            {
                for (int i = 0; i < levelsPurchased; i++)
                {
                    cumulativeCost *= 1.03f; // Incremental cost
                    cumulativeDPS *= 1.03f;  // Incremental DPS
                }
            }
            /*
            for (int i = 0; i < levelsPurchased; i++)
            {
                cumulativeCost *= 1.03f; // Incremental cost
                cumulativeDPS *= 1.02f;  // Incremental DPS
            }*/

            // Update DPS and cost after calculating total increments
            if (maxIsSelected)
            {
                MainHeroLevel += FinalTempMaxPurchaseGroupValue; //levelsPurchased; FIX DRYMARTI
            }
            else
            {
                MainHeroLevel += levelsPurchased;
            }
           
            MainHeroLevelText.text = "LVL:" + MainHeroLevel.ToString();
            DPSBasePurchaseCostValue = cumulativeCost;
            MainHeroDPSMaxDamageValue = cumulativeDPS;

            // Refresh the display according to the last used multiplier after making the purchase
            CheckUpgradeMainHeroSetCollection(); // This ensures the display is updated properly
            

            if (MainHeroAnim.speed >= 1.5f)
            {
                MainHeroAnim.speed = 1.5f;
            }

            MainWallet.WalletValueManualUpdate();

        }




        public bool maxIsSelected = false;
        void CheckUpgradeTextManualUpdate()
        {
            if(maxIsSelected == false)
            {
                // DPS Base Purchase Cost
                if (DPSBasePurchaseCostValueTemp <= 999)
                {
                    DPSBasePurchaseCostText.text = "UPGRADE" + "\n" + DPSBasePurchaseCostValueTemp.ToString("F0");
                }
                else if (DPSBasePurchaseCostValueTemp <= 999999)
                {
                    DPSBasePurchaseCostText.text = "UPGRADE" + "\n" + (DPSBasePurchaseCostValueTemp / 1000f).ToString("F2") + "K";
                }
                else if (DPSBasePurchaseCostValueTemp <= 999999999f)
                {
                    DPSBasePurchaseCostText.text = "UPGRADE" + "\n" + (DPSBasePurchaseCostValueTemp / 1000000f).ToString("F2") + "M";
                }
                else if (DPSBasePurchaseCostValueTemp <= 999999999999f)
                {
                    DPSBasePurchaseCostText.text = "UPGRADE" + "\n" + (DPSBasePurchaseCostValueTemp / 1000000000f).ToString("F2") + "B";
                }
                else if (DPSBasePurchaseCostValueTemp <= 999999999999999f)
                {
                    DPSBasePurchaseCostText.text = "UPGRADE" + "\n" + (DPSBasePurchaseCostValueTemp / 1000000000000f).ToString("F2") + "T";
                }
            }
            else
            {

                // DPS Base Purchase Cost
                if (DPSBasePurchaseCostValueTemp <= 999)
                {
                    DPSBasePurchaseCostText.text = "Count: " + FinalTempMaxPurchaseGroupValue.ToString("F0") + "\n" + DPSBasePurchaseCostValueTemp.ToString("F0");
                }
                else if (DPSBasePurchaseCostValueTemp <= 999999)
                {
                    DPSBasePurchaseCostText.text = "Count: " + FinalTempMaxPurchaseGroupValue.ToString("F0") + "\n" + (DPSBasePurchaseCostValueTemp / 1000f).ToString("F2") + "K";
                }
                else if (DPSBasePurchaseCostValueTemp <= 999999999f)
                {
                    DPSBasePurchaseCostText.text = "Count: " + FinalTempMaxPurchaseGroupValue.ToString("F0") + "\n" + (DPSBasePurchaseCostValueTemp / 1000000f).ToString("F2") + "M";
                }
                else if (DPSBasePurchaseCostValueTemp <= 999999999999f)
                {
                    DPSBasePurchaseCostText.text = "Count: " + FinalTempMaxPurchaseGroupValue.ToString("F0") + "\n" + (DPSBasePurchaseCostValueTemp / 1000000000f).ToString("F2") + "B";
                }
                else if (DPSBasePurchaseCostValueTemp <= 999999999999999f)
                {
                    DPSBasePurchaseCostText.text = "Count: " + FinalTempMaxPurchaseGroupValue.ToString("F0") + "\n" + (DPSBasePurchaseCostValueTemp / 1000000000000f).ToString("F2") + "T";
                }
            }


            if (boostPurchaseScript.ActivateDoubleGoldIncomeCondition == false && gemsScript.VIPCountValue < 2)
            {
                float TempValue = MainHeroDPSMaxDamageValue + MainTalent.TotalAttackValue + MainTalent.TotalAdditionalDamageFromEnemyHealth + (MainHeroDPSMaxDamageValue * MainArtifact.TrippleSwordTotalValue);// + SubHeroTotalDPS;

                // Main Hero DPS Max Damage Value
                if (MainHeroDPSMaxDamageValue <= 999)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + TempValue.ToString("F0");//MainHeroDPSMaxDamageValue.ToString("F0");
                }
                else if (MainHeroDPSMaxDamageValue <= 999999)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + (TempValue / 1000f).ToString("F2") + "K";
                }
                else if (MainHeroDPSMaxDamageValue <= 999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + (TempValue / 1000000f).ToString("F2") + "M";
                }
                else if (MainHeroDPSMaxDamageValue <= 999999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + (TempValue / 1000000000f).ToString("F2") + "B";
                }
                else if (MainHeroDPSMaxDamageValue <= 999999999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + (TempValue / 1000000000000f).ToString("F2") + "T";
                }
            }
            else if (boostPurchaseScript.ActivateDoubleGoldIncomeCondition == true || gemsScript.VIPCountValue >= 2)
            {

                float TempValue = (MainHeroDPSMaxDamageValue + MainTalent.TotalAttackValue + MainTalent.TotalAdditionalDamageFromEnemyHealth + (MainHeroDPSMaxDamageValue * MainArtifact.TrippleSwordTotalValue)) * 2;// + SubHeroTotalDPS;

                // Main Hero DPS Max Damage Value
                if (MainHeroDPSMaxDamageValue <= 999)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + TempValue.ToString("F0") + " X2";//MainHeroDPSMaxDamageValue.ToString("F0");
                }
                else if (MainHeroDPSMaxDamageValue <= 999999)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + (TempValue / 1000f).ToString("F2") + "K" + " X2";
                }
                else if (MainHeroDPSMaxDamageValue <= 999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + (TempValue / 1000000f).ToString("F2") + "M" + " X2";
                }
                else if (MainHeroDPSMaxDamageValue <= 999999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + (TempValue / 1000000000f).ToString("F2") + "B" + " X2";
                }
                else if (MainHeroDPSMaxDamageValue <= 999999999999999f)
                {
                    MainHeroDPSMaxDamageValueText.text = "DPS:" + (TempValue / 1000000000000f).ToString("F2") + "T" + " X2";
                }
            }
           


        }

        public void CheckDPSUpgradeButton()
        {
            if (MainWallet.GoldWalletValue >= DPSBasePurchaseCostValueTemp)
            {
                DPSBaseUpgradeButton.interactable = true;
            }

            if (MainWallet.GoldWalletValue < DPSBasePurchaseCostValueTemp)
            {
                DPSBaseUpgradeButton.interactable = false;
            }
        }

        public void CheckUpgradeMainHeroSetCollection()
        {
            float baseCost = DPSBasePurchaseCostValue;
            switch (MainSubHero.SubHeroUpgradeID)
            {
                case 0: // x1
                    maxIsSelected = false;
                    DPSBasePurchaseCostValueTemp = baseCost;
                    break;
                case 1: // x10
                    maxIsSelected = false;
                    DPSBasePurchaseCostValueTemp = CalculateIncrementalCost(baseCost, 10);
                    break;
                case 2: // x100
                    maxIsSelected = false;
                    DPSBasePurchaseCostValueTemp = CalculateIncrementalCost(baseCost, 100);
                    break;
                case 3: // MAX
                    maxIsSelected = true;
                    int maxLevels = CalculateMaxLevels(baseCost, MainWallet.GoldWalletValue);
                    DPSBasePurchaseCostValueTemp = CalculateIncrementalCost(baseCost, maxLevels);
                    FinalTempMaxPurchaseGroupValue = maxLevels;
                    if (FinalTempMaxPurchaseGroupValue < 0)
                    {
                        FinalTempMaxPurchaseGroupValue = 2000000000;  // Default to a very high value if negative
                    }
                    break;
            }
            CheckUpgradeTextManualUpdate();  // This will update the display according to the current multiplier
        
        /*
        if (MainSubHero.SubHeroUpgradeID == 0)
        {
            DPSBasePurchaseCostValueTemp = DPSBasePurchaseCostValue;
            DPSBasePurchaseCostValueTemp *= 1;
            CheckUpgradeTextManualUpdate();
        }

        if (MainSubHero.SubHeroUpgradeID == 1)
        {
            DPSBasePurchaseCostValueTemp = DPSBasePurchaseCostValue;
            DPSBasePurchaseCostValueTemp *= 10;
            CheckUpgradeTextManualUpdate();
        }

        if (MainSubHero.SubHeroUpgradeID == 2)
        {
            DPSBasePurchaseCostValueTemp = DPSBasePurchaseCostValue;
            DPSBasePurchaseCostValueTemp *= 100;
            CheckUpgradeTextManualUpdate();
        }

        if (MainSubHero.SubHeroUpgradeID == 3)
        {
            DPSBasePurchaseCostValueTemp = DPSBasePurchaseCostValue;

            TempMaxPurchaseGroupValue = MainWallet.GoldWalletValue / DPSBasePurchaseCostValueTemp;

            FinalTempMaxPurchaseGroupValue = (int)TempMaxPurchaseGroupValue;

            if (FinalTempMaxPurchaseGroupValue <= -1)
            {
                FinalTempMaxPurchaseGroupValue = 2000000000;
            }

            if (TempMaxPurchaseGroupValue >= 1)
            {
                //FinalTempMaxPurchaseGroupValue = (int)TempMaxPurchaseGroupValue;

                TempMaxMainHeroLevelPurchaseCostGroupValue = DPSBasePurchaseCostValueTemp * FinalTempMaxPurchaseGroupValue;

                DPSBasePurchaseCostValueTemp = TempMaxMainHeroLevelPurchaseCostGroupValue;

                if (MainWallet.GoldWalletValue < DPSBasePurchaseCostValueTemp)
                {
                    DPSBasePurchaseCostValueTemp = DPSBasePurchaseCostValue;
                }

            }

            //DPSBasePurchaseCostValueTemp *= 1000;
            CheckUpgradeTextManualUpdate();
        }
        */
    }


        private float CalculateIncrementalCost(float initialCost, int levels)
        {
            float totalCost = initialCost;
            float currentCost = initialCost;
            for (int i = 1; i < levels; i++)
            {
                currentCost *= 1.03f;  // Assuming a 2% cost increase per level
                totalCost += currentCost;
            }
            return totalCost;
        }

        private int CalculateMaxLevels(float costPerLevel, float availableFunds)
        {
            int levels = 0;
            while (availableFunds >= costPerLevel)
            {
                availableFunds -= costPerLevel;
                costPerLevel *= 1.03f;  // Assuming a 2% cost increase per level
                levels++;
            }
            return levels;
        }

        public void HeroResetPrestigeActivate()
        {
            AttackSpeedValue = 2;
            MainHeroAnim.speed = AttackSpeedValue;

            if (MainHeroAnim.speed >= 1.5f)
            {
                MainHeroAnim.speed = 1.5f;
            }

            for (int i = 0; i < MainSubHero.SubHeroActiveHeroConditionCollection.Count; i++)
            {
                if (MainSubHero.SubHeroActiveHeroConditionCollection[i])
                {
                    MainSubHero.UnEquipButtonID = i; // Assuming UnEquipButtonID needs to be set before calling
                    MainSubHero.PrestigeResetSubHero();
                }
            }



            //below works
            DPSBasePurchaseCostValue = DPSBasePurchaseCostValueReset;
            MainHeroDPSMaxDamageValue = MainHeroDPSMaxDamageValueReset;
            MainHeroLevel = 1;
            MainSubHero.SubHeroUpgradeID = 0;
            MainSubHero.SubHeroUpgradeCollection(0);
            CheckUpgradeMainHeroSetCollection();
            CheckUpgradeTextManualUpdate();
            CheckDPSUpgradeButton();
            FirstCheck();
        }
    }
}

