using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SAMPLETEXT.Gameplay.Skills.Manager
{
    public class GameplaySkillsManagerScript : MonoBehaviour
    {
        [Header("Skill Settings")]
        [SerializeField]
        public float[] CurrentTimerCountdownValueCollection;
        [SerializeField]
        float[] MaxTimerCountdownValueCollection;
        [SerializeField]
        TextMeshProUGUI[] TimerCountdownTextCollection;
        [SerializeField]
        Image[] TimerCountdownProgressBarImage;
        [SerializeField]
        Button[] SkillButtonCollection;

        [Header("Lightning Strike Settings")]
        [SerializeField]
        GameObject LightningStrikeAnimationObject;

        [Header("Meteor Shower Settings")]
        [SerializeField]
        GameObject MeteorShowerAnimationObject;

        [Header("Ice Spike Settings")]
        [SerializeField]
        GameObject IceSpikeAnimationObject;

        [Header("Scary Scream Settings")]
        [SerializeField]
        GameObject ScaryScreamAnimationObject;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < TimerCountdownTextCollection.Length; i++)
            {
                TimerCountdownProgressBarImage[i].fillAmount = CurrentTimerCountdownValueCollection[i] / MaxTimerCountdownValueCollection[i];
                TimerCountdownTextCollection[i].text = CurrentTimerCountdownValueCollection[i].ToString("F0") + "(s)";
                
                if (CurrentTimerCountdownValueCollection[i] < 0.0)
                {
                    TimerCountdownProgressBarImage[i].gameObject.SetActive(false);
                    SkillButtonCollection[i].interactable = true;
                }

                if (CurrentTimerCountdownValueCollection[i] >= 0.0)
                {
                    TimerCountdownProgressBarImage[i].gameObject.SetActive(true);
                    CurrentTimerCountdownValueCollection[i] -= Time.deltaTime * 1.3f;
                    SkillButtonCollection[i].interactable = false;
                }
            }
        }

        public void SkillActivate(int SkillID)
        {
            

            if (SkillID == 0)
            {
                CurrentTimerCountdownValueCollection[SkillID] = MaxTimerCountdownValueCollection[SkillID];
                LightningStrikeAnimationObject.gameObject.SetActive(true);
            }

            if (SkillID == 1)
            {
                CurrentTimerCountdownValueCollection[SkillID] = MaxTimerCountdownValueCollection[SkillID];
                MeteorShowerAnimationObject.gameObject.SetActive(true);
            }

            if (SkillID == 2)
            {
                CurrentTimerCountdownValueCollection[SkillID] = MaxTimerCountdownValueCollection[SkillID];
                IceSpikeAnimationObject.gameObject.SetActive(true);
            }

            if (SkillID == 3)
            {
                CurrentTimerCountdownValueCollection[SkillID] = MaxTimerCountdownValueCollection[SkillID];
                ScaryScreamAnimationObject.gameObject.SetActive(true);
            }
        }

        public void SkillActiveButton(GameObject SkillButtonID)
        {
            if (SkillButtonID.activeSelf == false) 
            {
                SkillButtonID.SetActive(true);
            }
            else if (SkillButtonID.activeSelf == true)
            {
                SkillButtonID.SetActive(false);
            }
        }
    }
}

