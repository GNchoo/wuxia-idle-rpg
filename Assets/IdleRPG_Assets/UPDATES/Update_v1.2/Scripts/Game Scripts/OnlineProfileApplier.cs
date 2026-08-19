using UnityEngine;
using SAMPLETEXT.Account.Manager.Profile;
using SAMPLETEXT.Wallet.Manager;

namespace SAMPLETEXT 
{
    public class OnlineProfileApplier : MonoBehaviour
    {
        [SerializeField] private ProfileManagerScript profileManager;
        [SerializeField] private OnlineRankingUploader rankingUploader;

        private const string PlayerPrefsUserIdKey = "OnlineUserId";
        private const string PlayerPrefsDisplayNameKey = "OnlineDisplayName";
        private const string PlayerPrefsEmailKey = "OnlineEmail";

        private void Awake()
        {
            if (profileManager == null)
                profileManager = FindObjectOfType<ProfileManagerScript>();

            if (profileManager == null)
            {
                Debug.LogWarning("[OnlineProfileApplier] ProfileManagerScript not found on scene.");
                enabled = false;
                return;
            }

            if (rankingUploader == null)
                rankingUploader = FindObjectOfType<OnlineRankingUploader>();

            int onlineUserId = PlayerPrefs.GetInt(PlayerPrefsUserIdKey, -1);
            string onlineName = PlayerPrefs.GetString(PlayerPrefsDisplayNameKey, string.Empty);
            string onlineEmail = PlayerPrefs.GetString(PlayerPrefsEmailKey, string.Empty);

            // No online data - let's behave like offline
            if (onlineUserId == -1 && string.IsNullOrEmpty(onlineName) && string.IsNullOrEmpty(onlineEmail))
            {
                Debug.Log("[OnlineProfileApplier] No online user data, using local profile.");
                return;
            }

            // set game profile
            if (onlineUserId != -1)
                profileManager.GUIDValue = "UID: " + onlineUserId;

            if (!string.IsNullOrEmpty(onlineName))
                profileManager.PlayerNameStringValue = onlineName;

            Debug.Log($"[OnlineProfileApplier] Applied online profile: {onlineName} / UID: {onlineUserId}");

            // Ustawiamy ranking uploader
            if (rankingUploader != null && !string.IsNullOrEmpty(onlineEmail))
            {
                string nameForRanking = string.IsNullOrEmpty(onlineName)
                    ? profileManager.PlayerNameStringValue
                    : onlineName;

                rankingUploader.SetEmail(onlineEmail);
                rankingUploader.SetDisplayName(nameForRanking);

                Debug.Log($"[OnlineProfileApplier] Applied ranking email: {onlineEmail}, name: {nameForRanking}");
            }
        }
    }


}
