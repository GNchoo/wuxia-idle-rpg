using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SAMPLETEXT.Gameplay.Manager.EnemyHitEffect
{
    public class GameplayEnemyHitEffectScript : MonoBehaviour
    {
        [SerializeField]
        GameObject[] EnemyHitEffectCollectionObj;
        [SerializeField]
        Transform[] EnemyHitSpawnPos;
        public int EnemyHitEffectID;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SpawnEnemyHitEffect()
        {
            int a = Random.Range(0, EnemyHitSpawnPos.Length);

            Instantiate(EnemyHitEffectCollectionObj[EnemyHitEffectID], EnemyHitSpawnPos[a].transform.position, EnemyHitSpawnPos[a].rotation);
        }
    }
}

