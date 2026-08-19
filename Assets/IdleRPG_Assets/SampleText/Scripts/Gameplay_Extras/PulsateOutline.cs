using UnityEngine;

namespace SAMPLETEXT
{

    public class PulsateOutline : MonoBehaviour
    {
        public SpriteRenderer outlinedSpriteRenderer; // Assign this in the inspector
        public float pulseSpeed = 1.0f;
        private float startTime;
        public bool stopOutline;

        void Start()
        {
            if (outlinedSpriteRenderer == null)
            {
                Debug.LogError("OutlinedSpriteRenderer is not assigned!");
                return;
            }

            startTime = Time.time;
        }

        void Update()
        {
            if (stopOutline == false)
            {
                // Calculate the alpha value in a pulsating pattern
                float alpha = (Mathf.Sin((Time.time - startTime) * pulseSpeed) + 1) * 0.5f;

                // Update the color with the new alpha value
                Color newColor = outlinedSpriteRenderer.color;
                newColor.a = alpha;
                outlinedSpriteRenderer.color = newColor;
            }

        }
    }
}