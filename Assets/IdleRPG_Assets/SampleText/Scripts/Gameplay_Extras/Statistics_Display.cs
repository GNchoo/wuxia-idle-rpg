using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAMPLETEXT.Market.Manager;
using TMPro;
using SAMPLETEXT.Gameplay.Manager.Enemy;
using SAMPLETEXT.Talent.Manager;
using SAMPLETEXT.SubHeroUI.Manager;
using SAMPLETEXT.SubHero.Attack;
using SAMPLETEXT.FieldAttack;
using SAMPLETEXT.Artifact.Manager;
using SAMPLETEXT.Gameplay.Manager.MainHero;
using SAMPLETEXT.ItemPurchase.Manager.Gem;


namespace SAMPLETEXT
{

    public class Statistics_Display : MonoBehaviour
    {
        public GameObject statsActive;
        [Header("References")]
        [SerializeField] DiamondPurchaseManagerScript gemScript;
        [SerializeField] MarketManagerScript marketScript;
        [SerializeField] GameplayEnemyManagerScript enemyManager;
        [SerializeField] TalentsManagerScript talentScript;
        [SerializeField] FieldAttackScript subHeroAttackScript;
        [SerializeField] ArtifactManagerScript artifactScript;
        [SerializeField] SubHeroesManagerScript subheroManagerScript;
        [SerializeField] GameplayMainHeroManagerScript mainHeroScript;

        [Header("Economy Texts")]
        public TMP_Text totalIncomeText;
        public TMP_Text goldIncomeBossText;
        public TMP_Text dropChanceText;

        [Header("Sub Heroes Texts")]
        public TMP_Text subheroCritDamageText;
        public TMP_Text subheroCritChanceText;
        public TMP_Text subheroAttackSpeedText;
        public TMP_Text subheroExtraDamageStatText;

        [Header("Main Hero Texts")]
        public TMP_Text mainHeroBaseDPSText;
        public TMP_Text mainheroCritDamageText;
        public TMP_Text mainheroCritChanceText;
        public TMP_Text mainheroAttackSpeedText;
        public TMP_Text mainheroExtraDamageStatText;
        // Start is called before the first frame update
        void UpdateText()
        {
            //ECONOMY
            totalIncomeText.text = "Total village income: " + marketScript.TotalIncomeValue.ToString("F0");
            goldIncomeBossText.text = "Current boss income: " + enemyManager.CurrentGoldCoinEnemyDropValue.ToString("F0");

            if (gemScript.VIPCountValue >= 4)
            {
                dropChanceText.text = "Drop item chance: " + enemyManager.ItemChanceDropMaxValue.ToString("F2") + " X2";
            }
            else
            {
                dropChanceText.text = "Drop item chance: " + enemyManager.ItemChanceDropMaxValue.ToString("F2");
            }




            //HEROES

            subheroCritDamageText.text = "Critical damage: " + subHeroAttackScript.subHerocriticalDamage.ToString("F0");
            subheroCritChanceText.text = "Critical damage chance: " + artifactScript.InevitableVictoryTotalValue.ToString("F1");
            subheroAttackSpeedText.text = "Attack Speed: " + subheroManagerScript.SubHeroAttackSpeedCollection[0].ToString("F2");
            subheroExtraDamageStatText.text = "Additional Damage: " + (talentScript.TotalAttackValueSubHero + (subHeroAttackScript.AttackDamage * artifactScript.TrippleSwordTotalValue));

            //MAIN HERO

            mainHeroBaseDPSText.text = "Damage Per Second: " + mainHeroScript.MainHeroDPSMaxDamageValue.ToString("F0");
            mainheroCritDamageText.text = "Critical damage: " + mainHeroScript.criticalDamage.ToString("F0");
            mainheroCritChanceText.text = "Critical damage chance: " + artifactScript.InevitableVictoryTotalValue.ToString("F1");
            mainheroAttackSpeedText.text = "Attack Speed: " + mainHeroScript.AttackSpeedValue.ToString("F2");
            mainheroExtraDamageStatText.text = "Additional Damage: " + (talentScript.TotalAttackValue + (mainHeroScript.MainHeroDPSMaxDamageValue * artifactScript.TrippleSwordTotalValue));
        }

        // Update is called once per frame
        void Update()
        {
            if (statsActive.activeInHierarchy)
            {
                UpdateText();
            }

        }
    }
}