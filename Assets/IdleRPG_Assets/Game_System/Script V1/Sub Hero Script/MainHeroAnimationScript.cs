using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAMPLETEXT.Gameplay.Manager.MainHero;
using Newtonsoft.Json.Bson;

namespace SAMPLETEXT.MainHeroInField.Animation
{
    public class MainHeroAnimationScript : MonoBehaviour
    {
        [SerializeField]
        GameplayMainHeroManagerScript MainHeroManager;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void MainHeroAttack()
        {
            MainHeroManager.AttackActivate();
        }
    }
}

