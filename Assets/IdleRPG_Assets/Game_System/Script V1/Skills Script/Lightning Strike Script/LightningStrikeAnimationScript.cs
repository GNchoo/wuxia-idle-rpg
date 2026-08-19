using SAMPLETEXT.Gameplay.Manager.Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAMPLETEXT.Talent.Manager;

namespace SAMPLETEXT.SkillsAnimation.Manager
{
    public class LightningStrikeAnimationScript : MonoBehaviour
    {
        public float LightningStrikeDamageValue;
        [Header("Spawn Damage Text Settings")]
        [SerializeField]
        GameplayEnemyManagerScript MainEnemy;
        [SerializeField]
        GameObject SpawnDamageTextObj;
        [SerializeField]
        Transform SpawnDamageTextPos;
        [SerializeField]
        TalentsManagerScript MainTalent;
        // Start is called before the first frame update
        void Start()
        {
            MainTalent.SkillValueManualUpdate(10);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void HideThisGameObject()
        {
            LightningStrikeDamageValue = (MainEnemy.EnemyMaxHealthValue / 100f) * (MainTalent.TalentSkillsCollectionValue[10]);
            MainEnemy.EnemyHealthValue = MainEnemy.EnemyHealthValue - LightningStrikeDamageValue;
            MainEnemy.EnemyHealthTextUpdate();
            Instantiate(SpawnDamageTextObj, SpawnDamageTextPos.position, Quaternion.identity);
            StartCoroutine(DelayHide());
        }

        IEnumerator DelayHide()
        {
               yield return new WaitForSeconds(.1f);
            this.gameObject.SetActive(false);
        }
    }
}

