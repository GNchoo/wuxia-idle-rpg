using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SAMPLETEXT.Wallet.Manager;
using SAMPLETEXT.Account.Manager.Profile;
namespace SAMPLETEXT
{
    public class OnlineRankingUploader : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private WalletManagerScript wallet;
        [SerializeField] private ProfileManagerScript profile;

        [Header("Server")]
        [SerializeField] private string updateUrl = "https://your-server.com/idle_api/update_score.php";
        [SerializeField] private float autosendInterval = 60f;
        [SerializeField] private bool autoSend = true;

        private string userEmail;
        private string userDisplayName;

        private void Start()
        {
            if (autoSend)
                StartCoroutine(AutoLoop());
        }

        public void SetEmail(string email)
        {
            userEmail = email;
        }

        public void SetDisplayName(string name)
        {
            userDisplayName = name;
        }

        public void SendNow()
        {
            if (string.IsNullOrEmpty(userEmail) || wallet == null)
            {
                Debug.LogWarning("[Ranking] Missing email or wallet ref, not sending.");
                return;
            }

            StartCoroutine(SendCoroutine());
        }

        private IEnumerator AutoLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(autosendInterval);
                SendNow();
            }
        }

        private IEnumerator SendCoroutine()
        {
            WWWForm form = new WWWForm();
            form.AddField("email", userEmail);
            form.AddField("display_name", string.IsNullOrEmpty(userDisplayName) && profile != null
                                            ? profile.PlayerNameStringValue
                                            : userDisplayName);
            form.AddField("max_gold", wallet.GoldWalletValue.ToString());
            form.AddField("max_gems", wallet.GemWalletValue.ToString());
            form.AddField("max_dps", wallet.DPSWalletValue.ToString());

            using (UnityWebRequest www = UnityWebRequest.Post(updateUrl, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("[Ranking] send error: " + www.error);
                }
                else
                {
                    Debug.Log("[Ranking] score sent: " + www.downloadHandler.text);
                }
            }
        }
    }

}