using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAMPLETEXT
{

    public class TabManagerAudio : MonoBehaviour
    {
        public GameObject TeamTab;
        public GameObject GameplayTab;
        public AudioSource audioSource1; // Attach VillageTab AudioSource
        public AudioSource audioSource2; // Attach TeamTab AudioSource
        public Settings settingsScript;

        public float fadeTime = 1.0f; // Duration of the fade in seconds

        private bool isFading = false; // Flag to prevent overlapping fades

        private void Update()
        {
            if (settingsScript.isMuted == true)
            {
                audioSource1.enabled = false;
                audioSource2.enabled = false;
            }
            else
            {
                audioSource1.enabled = true;
                audioSource2.enabled = true;
            }
            // Check if TeamTab is active and manage audio transitions
            if ((TeamTab.activeSelf || GameplayTab.activeSelf) && !audioSource2.isPlaying && audioSource2.enabled == true)
            {
                if (!isFading)
                {
                    StartCoroutine(FadeAudio(audioSource1, audioSource2, fadeTime));
                }
            }
            else if ((!TeamTab.activeSelf && !GameplayTab.activeSelf) && !audioSource1.isPlaying && audioSource1.enabled == true)
            {
                if (!isFading)
                {
                    StartCoroutine(FadeAudio(audioSource2, audioSource1, fadeTime));
                }
            }
        }

        private IEnumerator FadeAudio(AudioSource fadeOutAudio, AudioSource fadeInAudio, float duration)
        {

            isFading = true;
            float currentTime = 0;
            float startVolume = fadeOutAudio.volume;

            // Start playing the audio that needs to be faded in
            fadeInAudio.Play();
            fadeInAudio.volume = 0;

            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                fadeOutAudio.volume = Mathf.Lerp(startVolume, 0, currentTime / duration);
                fadeInAudio.volume = Mathf.Lerp(0, 1, currentTime / duration);
                yield return null;
            }

            // Ensure final volumes are set correctly
            fadeOutAudio.Pause();
            fadeInAudio.volume = 1;
            isFading = false;


        }
    }

}