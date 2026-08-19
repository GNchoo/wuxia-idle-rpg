using IdleMvp.Boot;
using IdleMvp.UI.Maple;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IdleMvp.UI
{
    /// <summary>
    /// Meta scene entry — EventSystem/Camera + landscape MapleMainHud.
    /// </summary>
    public class MetaSceneEntry : MonoBehaviour
    {
        void Awake()
        {
            GrowGameBootstrap.EnsureRoot();
            EnsureCamera();
            EnsureEventSystem();
            if (FindObjectOfType<MapleMainHud>() == null && FindObjectOfType<GrowAppShell>() == null)
                gameObject.AddComponent<MapleMainHud>();
        }

        static void EnsureCamera()
        {
            if (Camera.main != null) return;
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = GrowTheme.Sky;
            cam.orthographic = true;
            cam.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();
        }

        static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}
