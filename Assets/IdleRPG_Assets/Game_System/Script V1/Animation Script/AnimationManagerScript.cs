using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAMPLETEXT.SubHeroUI.Manager;

namespace SAMPLETEXT.SubHero.Animation
{
    public class AnimationManagerScript : MonoBehaviour
    {
        [SerializeField]
        SubHeroesManagerScript MainSubHero;
        [SerializeField]
        int SlotID;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void CheckSubHeroSlotImage()
        {
            MainSubHero.ResetDefaultImage(SlotID);
        }
    }
}

