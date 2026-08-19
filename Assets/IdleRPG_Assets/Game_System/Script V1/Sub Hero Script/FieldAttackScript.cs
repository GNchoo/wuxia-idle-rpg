using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAMPLETEXT.Gameplay.Manager.Enemy;
using SAMPLETEXT.SubHeroUI.Manager;
using SAMPLETEXT.SubHero.Attack;
using SAMPLETEXT.Artifact.Manager;
using SAMPLETEXT.Talent.Manager;

namespace SAMPLETEXT.FieldAttack
{
    public class FieldAttackScript : MonoBehaviour
    {

        [Header("Miss System")]
        [SerializeField]
        public float subHeroDamageMiss;
        [SerializeField]
        public float subHeroDamageChanceCurrent = 95f;
        [SerializeField]
        public float subHeroDamageChanceMax = 100f;
        [SerializeField] public bool missedAttack = false;

        [Header("Critical System")]
        [SerializeField] public float criticalDamageDefault = 0;
        [SerializeField]
        public float subHerocriticalChance;
        [SerializeField]
        public float subHerocriticalDamage;
        [SerializeField]
        public bool subHerocriticalAttacked = false;
        [SerializeField]
        public float subHerocriticalAttackChance;
        [SerializeField]
         ArtifactManagerScript artifactScript;
        [SerializeField]
        TalentsManagerScript talentScript;

        [Header("Sub Hero Settings")]
        [SerializeField]
        GameplayEnemyManagerScript MainEnemyStats;
        [SerializeField]
        SubHeroesManagerScript MainSubHeroes;
        public float SubHeroDamageValue;
        [SerializeField]
        GameObject[] SubHeroAttackObj;
        [SerializeField]
        int[] SubHeroAttackID;
        [SerializeField]
        Transform SubHeroAttackSpawnPos;
        public int FieldID;
        public float AttackDamage;
        public int SubHeroID;

        [Header("Attack Damage Text Spawn Settings")]
        [SerializeField]
        GameObject SubHeroDamageTextObj;
        [SerializeField]
        Transform SubHeroDamageSpawnPos;

        [Header("For Testing Only")]
        [SerializeField]
        float MaxTimer;
        float Timer;
        bool TimerControl;
        // Start is called before the first frame update
        void Start()
        {

        }

        void TimerCountDownTempUpdate()
        {
            //if (TimerControl == false)
            //{
            //    Timer = MaxTimer;
            //    TimerControl = true;
            //}

            //Timer -= Time.deltaTime;

            //if (Timer <= 0)
            //{
            //    if (TimerControl == true)
            //    {
            //        GameObject TempAttack = Instantiate(SubHeroAttackObj[SubHeroAttackID[SubHeroID]], SubHeroAttackSpawnPos.transform.position, SubHeroAttackSpawnPos.rotation) as GameObject;

            //        TempAttack.GetComponent<SubHeroAttackScript>().SubHeroAttackID = FieldID;


            //        TimerControl = false;
            //    }
            //}
        }

        // Update is called once per frame
        void Update()
        {
            //TimerCountDownTempUpdate();
        }

        public void ResetTalentCriticalDamage()
        {
            AttackDamage = MainSubHeroes.SubHeroActiveAttack[FieldID] * MainSubHeroes.SubHeroAnimatorAttackSpeed[FieldID] + (MainSubHeroes.SubHeroActiveAttack[FieldID] * MainSubHeroes.SubHeroAnimatorAttackSpeed[FieldID]) * artifactScript.TrippleSwordTotalValue;
        }

        public void CheckDamageManualUpdate()
        {
            float tempArtifactAdditional = (MainSubHeroes.SubHeroActiveAttack[FieldID] * MainSubHeroes.SubHeroAnimatorAttackSpeed[FieldID]) * artifactScript.TrippleSwordTotalValue;
            AttackDamage = (MainSubHeroes.SubHeroActiveAttack[FieldID] * MainSubHeroes.SubHeroAnimatorAttackSpeed[FieldID]) + tempArtifactAdditional ;
            subHerocriticalDamage = AttackDamage + ((AttackDamage * 2f) * ((artifactScript.BloodySkullTotalValue + talentScript.TotalCritialDamageSubHeroes) / 100f));
        }

        public void EnemyDamage()
        {
            subHeroDamageMiss = Random.Range(0, subHeroDamageChanceMax);

            subHerocriticalChance = Random.Range(0, subHeroDamageChanceMax);
            subHerocriticalAttackChance = artifactScript.InevitableVictoryTotalValue; //critical artifact chance

            

            if (subHeroDamageMiss >= subHeroDamageChanceCurrent)
            {
                missedAttack = true;
                subHerocriticalAttacked = false;
                Instantiate(SubHeroDamageTextObj, SubHeroDamageSpawnPos.transform.position, SubHeroDamageSpawnPos.rotation);
            }
            else if (subHerocriticalChance > subHerocriticalAttackChance)
            {
                missedAttack = false;
                subHerocriticalAttacked = false;
                AttackDamage = Mathf.Round(AttackDamage);
                MainEnemyStats.EnemyHealthValue -= AttackDamage;
                MainEnemyStats.EnemyHealthCheckReduction();
                Instantiate(SubHeroDamageTextObj, SubHeroDamageSpawnPos.transform.position, SubHeroDamageSpawnPos.rotation);
            }
            else if (subHerocriticalChance <= subHerocriticalAttackChance)
            {
                missedAttack = false;
                
                if(subHerocriticalAttackChance == 0)
                {
                    subHerocriticalAttacked = false;
                    AttackDamage = Mathf.Round(AttackDamage);
                    MainEnemyStats.EnemyHealthValue -= AttackDamage;
                }
                else
                {
                    subHerocriticalAttacked = true;
                    AttackDamage =  subHerocriticalDamage;
                    MainEnemyStats.EnemyHealthValue -= subHerocriticalDamage;
                }
                
                MainEnemyStats.EnemyHealthCheckReduction();
                Instantiate(SubHeroDamageTextObj, SubHeroDamageSpawnPos.transform.position, SubHeroDamageSpawnPos.rotation);
                CheckDamageManualUpdate();
            }



        }

        public void SubHeroAttackActivate()
        {
            GameObject TempAttack = Instantiate(SubHeroAttackObj[SubHeroAttackID[SubHeroID]], SubHeroAttackSpawnPos.transform.position, SubHeroAttackSpawnPos.rotation) as GameObject;

            TempAttack.GetComponent<SubHeroAttackScript>().SubHeroAttackID = FieldID;
        }
    }
}

