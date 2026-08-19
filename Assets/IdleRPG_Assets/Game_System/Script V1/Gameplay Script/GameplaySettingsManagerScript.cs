using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAMPLETEXT.Gameplay.Manager.Settings
{
    public class GameplaySettingsManagerScript : MonoBehaviour
    {
        [SerializeField]
        GameObject[] VillageAchievementsCollection;
        [SerializeField]
        GameObject[] GameplayAchievementsCollection;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void GameplayButtonSettings(GameObject SettingsIDObj)
        {
            if (SettingsIDObj.activeSelf == true)
            {
                SettingsIDObj.gameObject.SetActive(false);
            }

            else if (SettingsIDObj.activeSelf == false)
            {
                SettingsIDObj.gameObject.SetActive(true);
            }
        }

        public void DisableVillageAchivementDisplay()
        {
            foreach(GameObject VAC in VillageAchievementsCollection)
            {
                VAC.gameObject.SetActive(false);
            }
        }

        public void DisableGameplayAchivementDisplay()
        {
            foreach (GameObject GAC in GameplayAchievementsCollection)
            {
                GAC.gameObject.SetActive(false);
            }
        }
    }
}

