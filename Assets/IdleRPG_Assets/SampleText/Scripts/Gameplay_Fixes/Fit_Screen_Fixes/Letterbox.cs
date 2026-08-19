using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAMPLETEXT
{


    public class Letterbox : MonoBehaviour
    {
        [SerializeField] private Camera[] targetCameras; // Assign all cameras here
        private float targetAspect = 1080f / 2340f; // 0.46

        void Start()
        {
            UpdateLetterbox();
        }

        void Update()
        {
            // Handle runtime resolution/orientation changes
            UpdateLetterbox();
        }

        private void UpdateLetterbox()
        {
            float windowAspect = (float)Screen.width / Screen.height;
            float scaleHeight = windowAspect / targetAspect;

            Rect newRect;

            if (scaleHeight < 1f)
            {
                // Letterbox: black bars top and bottom
                newRect = new Rect(0, (1f - scaleHeight) / 2f, 1f, scaleHeight);
            }
            else
            {
                // Pillarbox: black bars left and right
                float scaleWidth = 1f / scaleHeight;
                newRect = new Rect((1f - scaleWidth) / 2f, 0, scaleWidth, 1f);
            }

            foreach (Camera cam in targetCameras)
            {
                if (cam == null)
                    continue;

                // RenderTexture cameras must stay full viewport; letterboxing them
                // squeezes the battle into a thin strip inside the RT.
                if (cam.targetTexture != null)
                {
                    cam.rect = new Rect(0f, 0f, 1f, 1f);
                    continue;
                }

                cam.rect = newRect;
            }
        }
    }
}