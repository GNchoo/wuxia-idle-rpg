using UnityEngine;
using UnityEngine.SceneManagement;

namespace IdleMvp.Boot
{
    /// <summary>
    /// Boot scene entry: ensure services, then load Meta hub.
    /// </summary>
    public class GrowBootLoader : MonoBehaviour
    {
        [SerializeField] string metaSceneName = "Meta";
        [SerializeField] float splashSeconds = 0.35f;

        void Start()
        {
            // Idle game: keep simulating when the window loses focus (editor + PC builds).
            Application.runInBackground = true;
            GrowGameBootstrap.EnsureRoot();
            Invoke(nameof(GoMeta), splashSeconds);
        }

        void GoMeta()
        {
            SceneManager.LoadScene(metaSceneName);
        }
    }
}
