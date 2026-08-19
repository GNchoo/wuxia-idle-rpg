using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

namespace SAMPLETEXT
{


    public class Settings : MonoBehaviour
    {
        [Header("Music Button")]
        public AudioSource audioSource1;
        public AudioSource audioSource2;
        public Button musicButton;
        public bool isMuted = false;

        [Header("Sounds Button")]
        public AudioSource[] soundsAudioSources;
        public Button soundsButton;
        public bool isMutedSounds = false;

        void Start()
        {
            LoadMuteState(); // Load the mute state when the AudioManager starts
            ChangeColour(); // Change the button color based on the loaded state
            ChangeColourSound();
        }
        /*  private void Update()
         {
            ColorBlock colors = musicButton.colors;
             if (isMuted)
             {
                 colors.normalColor = Color.red; // Set normal color to red when muted
             }
             else if (!isMuted)
             {
                 colors.normalColor = Color.white; // Set normal color to green when not muted
             }
             musicButton.colors = colors; // Update the button colors
    }*/

        public void ToggleMute()
        {
            isMuted = !isMuted;
            audioSource1.enabled = !isMuted;
            audioSource2.enabled = !isMuted;

            SaveMuteState(); // Save the mute state when it changes
            ChangeColour(); // Call the function to update the button color
        }

        public void ToggleMuteSounds()
        {
            isMutedSounds = !isMutedSounds;
            if (isMutedSounds)
            {
                for (int i = 0; i < soundsAudioSources.Length; i++)
                {
                    soundsAudioSources[i].enabled = false;
                }
            }
            else
            {
                for (int i = 0; i < soundsAudioSources.Length; i++)
                {
                    soundsAudioSources[i].enabled = true;
                }
            }
            SaveMuteState(); // Save the mute state when it changes
            ChangeColourSound(); // Call the function to update the button color

        }

        public void ChangeColour()
        {
            ColorBlock colors = musicButton.colors;
            if (isMuted)
            {
                colors.normalColor = Color.red; // Set normal color to red when muted
            }
            else if (!isMuted)
            {
                colors.normalColor = Color.white; // Set normal color to green when not muted
            }
            // Set selected color to be the same as normal color
            colors.highlightedColor = colors.normalColor;
            colors.pressedColor = colors.normalColor;
            colors.selectedColor = colors.normalColor;


            musicButton.colors = colors; // Update the button colors

        }

        public void ChangeColourSound()
        {
            ColorBlock colors = soundsButton.colors;
            if (isMutedSounds)
            {
                colors.normalColor = Color.red; // Set normal color to red when muted
            }
            else if (!isMutedSounds)
            {
                colors.normalColor = Color.white; // Set normal color to green when not muted
            }
            // Set selected color to be the same as normal color
            colors.highlightedColor = colors.normalColor;
            colors.pressedColor = colors.normalColor;
            colors.selectedColor = colors.normalColor;


            soundsButton.colors = colors; // Update the button colors
        }

        void SaveMuteState()
        {
            PlayerPrefs.SetInt("IsMutedSounds", isMutedSounds ? 1 : 0); // Save the mute state to PlayerPrefs
            PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0); // Save the mute state to PlayerPrefs
            PlayerPrefs.Save(); // Make sure to save PlayerPrefs
        }

        void LoadMuteState()
        {
            isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1; // Load the mute state from PlayerPrefs
            isMutedSounds = PlayerPrefs.GetInt("IsMutedSounds", 0) == 1; // Load the mute state from PlayerPrefs
        }

        public void SetLanguage(string localeCode)
        {
            // Change the active locale
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        }

        public void PlayPurchaseSound()
        {
            soundsAudioSources[5].Play();
        }

        // Function to open the Google Play Store page within the Google Play Store app
        public void OpenGooglePlayStore()
        {
            // This is the package name for your app
            string packageName = "com.CompanyName.ProductName"; //provide your com.info here so the link redirects to your application (find in build settings, other settings)

            // Intent URI to open the app inside Google Play Store app
            string uri = "market://details?id=" + packageName;

            // Fallback URL if the Google Play Store app is not installed
            string fallbackUrl = "https://play.google.com/store/apps/details?id=" + packageName;

            // Try opening the app page in the Google Play Store app
            try
            {
                Application.OpenURL(uri);
            }
            catch (System.Exception)
            {
                // If an exception occurs, it means the device does not have the Google Play Store app installed
                // Fall back to opening the URL in a web browser
                Application.OpenURL(fallbackUrl);
            }
        }

        // Function to open a Discord invite link
        public void OpenDiscordInvite()
        {
            // This is the URL to your Discord invitation
            string discordInviteURL = "https://discord.gg/"; //Add your discord invintation link or any support link preffered
            Application.OpenURL(discordInviteURL);
        }
    }

}