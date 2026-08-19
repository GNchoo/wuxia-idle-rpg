using IdleMvp.Boot;
using IdleMvp.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IdleMvp.Boot
{
    /// <summary>
    /// Wires Boot/Meta/Battle scene entries without relying on scene YAML script GUIDs.
    /// </summary>
    public static class GrowSceneAutoWire
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Hook()
        {
            SceneManager.sceneLoaded -= OnLoaded;
            SceneManager.sceneLoaded += OnLoaded;
            OnLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        static void OnLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!scene.IsValid() || !scene.isLoaded) return;
            string n = scene.name;

            if (n == "Boot")
            {
                GrowGameBootstrap.EnsureRoot();
                var entry = GameObject.Find("SceneEntry");
                if (entry == null)
                {
                    entry = new GameObject("SceneEntry");
                    SceneManager.MoveGameObjectToScene(entry, scene);
                }
                if (entry.GetComponent<GrowBootLoader>() == null)
                    entry.AddComponent<GrowBootLoader>();
                return;
            }

            if (n == "Meta")
            {
                var entry = GameObject.Find("SceneEntry") ?? new GameObject("SceneEntry");
                if (entry.GetComponent<MetaSceneEntry>() == null)
                    entry.AddComponent<MetaSceneEntry>();
                return;
            }

            if (n == "Battle")
            {
                var entry = GameObject.Find("SceneEntry") ?? new GameObject("SceneEntry");
                if (entry.GetComponent<BattleSceneEntry>() == null)
                    entry.AddComponent<BattleSceneEntry>();
            }
        }
    }
}
