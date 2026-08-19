using UnityEngine;
using UnityEngine.SceneManagement;

namespace IdleMvp.Boot
{
    /// <summary>
    /// Legacy template GameScene bootstrap — DISABLED for grow-type path.
    /// Template remains reference-only; use Boot/Meta/Battle under _Project.
    /// </summary>
    public static class MvpBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnSceneLoaded()
        {
            // Intentionally empty: do not spawn template MVP overlays anymore.
            var scene = SceneManager.GetActiveScene();
            if (scene.name != null &&
                scene.name.IndexOf("GameScene", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Debug.LogWarning(
                    "[IdleMvp] Template GameScene detected. Grow MVP uses Assets/_Project/Scenes/Boot → Meta. " +
                    "Open Boot scene (Build Settings index 0).");
            }
        }
    }
}
