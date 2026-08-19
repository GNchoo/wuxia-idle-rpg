using System.Collections;
using System.Collections.Generic;
using SAMPLETEXT.Talent.Manager;
using UnityEngine;

namespace SAMPLETEXT.SkillsAnimation.Manager
{
    public class IceSpikeAnimationScript : MonoBehaviour
    {
        [SerializeField]
        TalentsManagerScript MainTalent;
        // Start is called before the first frame update
        void Start()
        {
            MainTalent.SkillValueManualUpdate(12);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void HideThisGameObject()
        {
            this.gameObject.SetActive(false);
        }
    }
}

