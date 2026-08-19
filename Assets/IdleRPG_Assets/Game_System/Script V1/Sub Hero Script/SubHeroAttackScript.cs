using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAMPLETEXT.Gameplay.Manager.EnemyHitEffect;
using SAMPLETEXT.FieldAttack;

namespace SAMPLETEXT.SubHero.Attack
{
    public class SubHeroAttackScript : MonoBehaviour
    {
        GameplayEnemyHitEffectScript MainEnemyHitEffect;
        FieldAttackScript[] FieldAttackCollection;
        [SerializeField]
        float moveSpeed;
        GameObject EnemyTarget;
        public int SubHeroAttackID;
        public int EnemyHitEffectReferenceID;
        // Start is called before the first frame update
        void Start()
        {
            FieldAttackCollection = GameObject.FindObjectsOfType<FieldAttackScript>();
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
                for (int i = 0; i < FieldAttackCollection.Length; i++)
                {
                    if (FieldAttackCollection[i].FieldID == SubHeroAttackID)
                    {
                        FieldAttackCollection[i].EnemyDamage();
                        Destroy(gameObject);
                    }
                }

                MainEnemyHitEffect.EnemyHitEffectID = EnemyHitEffectReferenceID;
                MainEnemyHitEffect.SpawnEnemyHitEffect();

            }
        }

        //private void OnTriggerStay2D(Collider2D col)
        //{
        //    if (col.gameObject.tag == "Enemy Target")
        //    {
        //        for (int i = 0; i < FieldAttackCollection.Length; i++)
        //        {
        //            if (FieldAttackCollection[i].FieldID == SubHeroAttackID)
        //            {
        //                FieldAttackCollection[i].EnemyDamage();
        //                Destroy(gameObject);
        //            }
        //        }

        //    }
        //}
    }
}

