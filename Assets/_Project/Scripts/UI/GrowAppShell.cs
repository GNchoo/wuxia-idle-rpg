using IdleMvp.UI.Maple;
using UnityEngine;

namespace IdleMvp.UI
{
    /// <summary>
    /// Legacy entry — delegates to landscape MapleMainHud.
    /// </summary>
    public class GrowAppShell : MonoBehaviour
    {
        void Awake()
        {
            if (GetComponent<MapleMainHud>() == null)
                gameObject.AddComponent<MapleMainHud>();
        }
    }
}
