using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SAMPLETEXT.ItemPurchase.Manager.Boost;
using SAMPLETEXT.Wallet.Manager;
using SAMPLETEXT.ItemPurchase.Manager.DailyAds;
using SAMPLETEXT.Gameplay.Manager.Enemy;
using SAMPLETEXT.Artifact.Manager;
using SAMPLETEXT.ItemPurchase.Manager.Item;
using SAMPLETEXT.TabCollection.Manager;


namespace SAMPLETEXT
{

    public class KnobNotificationSystem : MonoBehaviour
    {
        [SerializeField] Image globalKnob;
        [SerializeField] Image[] notificationKnobs;
        // Start is called before the first frame update
        void Start()
        {
            if (notificationKnobs[0].enabled == true || notificationKnobs[1].enabled == true || notificationKnobs[2].enabled == true)
            {
                globalKnob.enabled = true;
            }
            else
            {
                globalKnob.enabled = false;
            }

        }

        // Update is called once per frame
        void Update()
        {
            if (notificationKnobs[0].enabled == true || notificationKnobs[1].enabled == true || notificationKnobs[2].enabled == true)
            {
                globalKnob.enabled = true;
            }
            else
            {
                globalKnob.enabled = false;
            }
        }

    }

}