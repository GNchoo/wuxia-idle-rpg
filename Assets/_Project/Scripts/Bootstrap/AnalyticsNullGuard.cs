using System;
using System.Reflection;
using UnityEngine;

namespace IdleMvp.Bootstrap
{
    /// <summary>
    /// Unity.Services.Analytics AnalyticsContainer calls m_Service on OnApplicationPause
    /// even when Initialize() never ran (m_Service == null). Destroy those orphan containers.
    /// </summary>
    public static class AnalyticsNullGuard
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void HookEarly()
        {
            Application.focusChanged -= OnFocus;
            Application.focusChanged += OnFocus;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void SanitizeOnLoad() => Sanitize();

        static void OnFocus(bool _) => Sanitize();

        static void Sanitize()
        {
            var type = Type.GetType("Unity.Services.Analytics.AnalyticsContainer, Unity.Services.Analytics");
            if (type == null) return;

            var serviceField = type.GetField("m_Service", BindingFlags.Instance | BindingFlags.NonPublic);
            if (serviceField == null) return;

#if UNITY_2023_1_OR_NEWER
            var found = UnityEngine.Object.FindObjectsByType(type, FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var found = Resources.FindObjectsOfTypeAll(type);
#endif
            if (found == null || found.Length == 0) return;

            for (int i = 0; i < found.Length; i++)
            {
                var mb = found[i] as MonoBehaviour;
                if (mb == null) continue;
                object service = null;
                try { service = serviceField.GetValue(mb); }
                catch { /* ignore */ }
                if (service != null) continue;

                // Disabled MB still receives OnApplicationPause — must destroy the GO.
                var go = mb.gameObject;
                if (go != null)
                    UnityEngine.Object.Destroy(go);
            }
        }
    }
}
