using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SAMPLETEXT.Gameplay.Optimization
{
    public class SystemControlScript : MonoBehaviour
    {
        [SerializeField]
        int CPUMaxSpeed;
        [SerializeField]
        GameObject DisplayFrameRateObj;
        [SerializeField]
        TextMeshProUGUI FrameRateDisplayText;
        [SerializeField]
        bool DisplayFrameRate;
        // Start is called before the first frame update
        void Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = CPUMaxSpeed;
        }

        // Update is called once per frame
        void Update()
        {
            if (Application.targetFrameRate != CPUMaxSpeed)
            {
                Application.targetFrameRate = CPUMaxSpeed;
            }

            if (DisplayFrameRate == true)
            {
                DisplayFrameRateObj.SetActive(true);
                FrameRateDisplayText.text = "FPS:" + CPUMaxSpeed.ToString();
            }

            if (DisplayFrameRate == false)
            {
                DisplayFrameRateObj.SetActive(false);
               
            }

        }
    }
}

