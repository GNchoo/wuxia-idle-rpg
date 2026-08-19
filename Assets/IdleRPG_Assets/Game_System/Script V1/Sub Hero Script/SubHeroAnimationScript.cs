using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SAMPLETEXT.FieldAttack;

namespace SAMPLETEXT.SubHeroInField.Animation
{
    public class SubHeroAnimationScript : MonoBehaviour
    {
        [SerializeField]
        FieldAttackScript MainFieldAttackScript;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SpawnAttack()
        {
            if (MainFieldAttackScript != null)
            {
                MainFieldAttackScript.SubHeroAttackActivate();
            }
        }
    }
}

