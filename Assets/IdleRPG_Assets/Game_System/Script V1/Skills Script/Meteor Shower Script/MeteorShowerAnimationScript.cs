using SAMPLETEXT.Gameplay.Manager.Enemy;
using System.Collections;
using System.Collections.Generic;
using SAMPLETEXT.Talent.Manager;
using UnityEngine;

namespace SAMPLETEXT.SkillsAnimation.Manager
{
    public class MeteorShowerAnimationScript : MonoBehaviour
    {
        public float MeteorDamageValue;
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
            MainTalent.SkillValueManualUpdate(11);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void HideThisGameObject()
        {
            MainEnemy.EnemyHealthValue -= MeteorDamageValue;
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

