using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAMPLETEXT.Gameplay.Manager.MainHero;
using SAMPLETEXT.Gameplay.Manager.EnemyHitEffect;

namespace SAMPLETEXT.MainHero.Attack
{
    public class MainHeroAttackScript : MonoBehaviour
    {
        GameplayEnemyHitEffectScript MainEnemyHitEffect;
        GameplayMainHeroManagerScript MainHero;
        [SerializeField]
        float moveSpeed;
        GameObject EnemyTarget;
        public int EnemyHitEffectReferenceID;
        // Start is called before the first frame update
        void Start()
        {
            MainHero = GameObject.FindObjectOfType<GameplayMainHeroManagerScript>();
            MainEnemyHitEffect = GameObject.FindObjectOfType<GameplayEnemyHitEffectScript>();
            EnemyTarget = GameObject.FindGameObjectWithTag("Enemy Target");
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = Vector2.MoveTowards(transform.position, EnemyTarget.transform.position, moveSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.tag == "Enemy Target")
            {
                MainHero.EnemyDamage();
                Destroy(gameObject);

                MainEnemyHitEffect.EnemyHitEffectID = EnemyHitEffectReferenceID;
                MainEnemyHitEffect.SpawnEnemyHitEffect();
            }
        }
    }
}

