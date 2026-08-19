#if UNITY_EDITOR
using MCPForUnity.Editor.Services;
using MCPForUnity.Editor.Services.Transport.Transports;
using UnityEditor;
using UnityEngine;

namespace IdleMvp.EditorTools
{
    /// <summary>
    /// Forces the MCP-for-Unity stdio bridge to run so external tools (Claude Code)
    /// can drive this editor without manual window clicks.
    /// ponytail: idempotent on every domain reload; delete this file to opt out.
    /// </summary>
    [InitializeOnLoad]
    static class McpBridgeAutoStart
    {
        static McpBridgeAutoStart()
        {
            EditorApplication.delayCall += EnsureBridge;
        }

        static void EnsureBridge()
        {
            try
            {
                if (EditorPrefs.GetBool("MCPForUnity.UseHttpTransport", true))
                {
                    EditorPrefs.SetBool("MCPForUnity.UseHttpTransport", false);
                    EditorConfigurationCache.Instance.Refresh();
                }

                if (!StdioBridgeHost.IsRunning)
                {
                    StdioBridgeHost.StartAutoConnect();
                    Debug.Log($"[McpBridgeAutoStart] stdio bridge started on port {StdioBridgeHost.GetCurrentPort()}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[McpBridgeAutoStart] " + e.Message);
            }
        }
    }
}
#endif
