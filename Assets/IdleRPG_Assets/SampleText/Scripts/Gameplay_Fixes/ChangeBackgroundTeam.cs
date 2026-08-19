using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SAMPLETEXT
{


    public class ChangeBackgroundTeam : MonoBehaviour
    {
        public GameObject gameplayBackground;
        public GameObject teamBackground;

        public void OnEnable()
        {
            teamBackground.GetComponent<Image>().sprite = gameplayBackground.GetComponent<Image>().sprite;
        }
    }

}