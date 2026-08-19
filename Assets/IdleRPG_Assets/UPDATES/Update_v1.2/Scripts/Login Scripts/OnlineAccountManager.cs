using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
namespace SAMPLETEXT
{
    public class OnlineAccountManager : MonoBehaviour
    {
        [Header("Server Settings")]
        [SerializeField] private string baseUrl = "https://your-domain.com/idle_api/"; // remember about  '/' at the end

        [Header("UI References")]
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private TMP_InputField nicknameInput;   // only for register
        [SerializeField] private TextMeshProUGUI statusText;     // status / error

        private const string PlayerPrefsUserIdKey = "OnlineUserId";
        private const string PlayerPrefsDisplayNameKey = "OnlineDisplayName";
        private const string PlayerPrefsEmailKey = "OnlineEmail";

        private void Start()
        {
            if (statusText != null)
                statusText.text = "";
        }

        // ====== BUTTON HOOKS ======

        public void OnRegisterButton()
        {
            Debug.Log("[OnlineAccountManager] Register button clicked");
            StartCoroutine(RegisterCoroutine());
        }

        public void OnLoginButton()
        {
            Debug.Log("[OnlineAccountManager] Login button clicked");
            StartCoroutine(LoginCoroutine());
        }

        public void OnGuestButton()
        {
            Debug.Log("[OnlineAccountManager] Guest button clicked");
            SceneManager.LoadScene("LoadingScene V1"); // Change Your Loading SceneName
        }

        // ====== REGISTER ======

        private IEnumerator RegisterCoroutine()
        {
            SetStatus("Registering...");

            if (!ValidateRegisterInputs())
                yield break;

            WWWForm form = new WWWForm();
            form.AddField("email", emailInput.text);
            form.AddField("password", passwordInput.text);
            form.AddField("display_name", nicknameInput.text);

            using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + "register.php", form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    SetStatus("Network error.");
                    yield break;
                }

                string json = www.downloadHandler.text;
                Debug.Log("[OnlineAccountManager] Register response: " + json);

                RegisterResponse resp = JsonUtility.FromJson<RegisterResponse>(json);

                if (resp == null || !resp.success)
                {
                    SetStatus("Error: " + (resp != null ? resp.error : "Invalid response"));
                    yield break;
                }

                SaveLoginData(resp.user_id, resp.display_name, emailInput.text);
                SetStatus("Registered!");
                SceneManager.LoadScene("LoadingScene V1"); // Change Your Loading SceneName
            }
        }

        // ====== LOGIN ======

        private IEnumerator LoginCoroutine()
        {
            SetStatus("Logging in...");

            if (!ValidateLoginInputs())
                yield break;

            WWWForm form = new WWWForm();
            form.AddField("email", emailInput.text);
            form.AddField("password", passwordInput.text);

            using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + "login.php", form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    SetStatus("Network error.");
                    yield break;
                }

                string json = www.downloadHandler.text;
                Debug.Log("[OnlineAccountManager] Login response: " + json);

                LoginResponse resp = JsonUtility.FromJson<LoginResponse>(json);

                if (resp == null || !resp.success)
                {
                    SetStatus("Error: " + (resp != null ? resp.error : "Invalid response"));
                    yield break;
                }

                SaveLoginData(resp.user_id, resp.display_name, emailInput.text);
                SetStatus("Logged in!");
                SceneManager.LoadScene("LoadingScene V1"); // Change Your Loading SceneName
            }
        }

        // ====== HELPERS ======

        private bool ValidateRegisterInputs()
        {
            if (emailInput == null || passwordInput == null || nicknameInput == null)
            {
                SetStatus("Missing UI references.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(emailInput.text) ||
                string.IsNullOrWhiteSpace(passwordInput.text) ||
                string.IsNullOrWhiteSpace(nicknameInput.text))
            {
                SetStatus("All 3 fields required for registration.");
                return false;
            }

            return true;
        }

        private bool ValidateLoginInputs()
        {
            if (emailInput == null || passwordInput == null)
            {
                SetStatus("Missing UI references.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(emailInput.text) ||
                string.IsNullOrWhiteSpace(passwordInput.text))
            {
                SetStatus("Email + Password required.");
                return false;
            }

            return true;
        }

        private void SetStatus(string msg)
        {
            Debug.Log("[OnlineAccountManager] " + msg);
            if (statusText != null)
                statusText.text = msg;
        }

        private void SaveLoginData(int userId, string displayName, string email)
        {
            PlayerPrefs.SetInt(PlayerPrefsUserIdKey, userId);
            PlayerPrefs.SetString(PlayerPrefsDisplayNameKey, displayName);
            PlayerPrefs.SetString(PlayerPrefsEmailKey, email);
            PlayerPrefs.Save();
        }

        [System.Serializable]
        private class RegisterResponse
        {
            public bool success;
            public string error;
            public int user_id;
            public string display_name;
        }

        [System.Serializable]
        private class LoginResponse
        {
            public bool success;
            public string error;
            public int user_id;
            public string display_name;
        }
    }
}