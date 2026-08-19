using IdleMvp.Boot;
using IdleMvp.UI.Maple;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IdleMvp.UI
{
    /// <summary>
    /// Optional Battle scene — same landscape Maple HUD.
    /// </summary>
    public class BattleSceneEntry : MonoBehaviour
    {
        void Awake()
        {
            GrowGameBootstrap.EnsureRoot();
            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = GrowTheme.Sky;
                cam.orthographic = true;
                cam.tag = "MainCamera";
            }

            if (FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            if (FindObjectOfType<MapleMainHud>() == null && FindObjectOfType<GrowAppShell>() == null)
                gameObject.AddComponent<MapleMainHud>();
        }
    }
}
