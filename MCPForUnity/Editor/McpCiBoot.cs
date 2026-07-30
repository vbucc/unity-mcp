using System.Threading.Tasks;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Services;

namespace MCPForUnity.Editor
{
    /// <summary>
    /// Entry point for headless/CI Editors driven by tools/local_harness.py.
    /// </summary>
    public static class McpCiBoot
    {
        /// <summary>
        /// Connect this Editor to the harness's HTTP server.
        ///
        /// Deliberately writes no EditorPrefs: those are shared machine-wide per Unity
        /// version, so an ephemeral CI Editor that persisted its endpoint would repoint
        /// the developer's own Editor. The harness passes its URL via the
        /// UNITY_MCP_HTTP_URL environment variable instead, which HttpEndpointUtility
        /// reads ahead of the pref.
        ///
        /// This only opens the outbound WebSocket. It never starts or stops a local
        /// server process, so the harness can never terminate whatever owns the
        /// developer's usual port.
        /// </summary>
        public static void StartHttpForCi()
        {
            string url = HttpEndpointUtility.GetBaseUrl();
            McpLog.Info($"[CI Boot] Connecting to MCP server at {url}");

            // Fire-and-forget on purpose. -executeMethod runs on the main thread, and
            // blocking it (GetAwaiter().GetResult()) stops EditorApplication.update —
            // which is what dispatches incoming commands. The WebSocket would register
            // but every command would then time out. Returning lets the batch-mode
            // Editor pump normally; the harness polls until commands are answered.
            _ = MCPServiceLocator.Bridge.StartAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    McpLog.Error($"[CI Boot] Error connecting to {url}: {t.Exception?.GetBaseException().Message}");
                }
                else if (!t.Result)
                {
                    McpLog.Error($"[CI Boot] Failed to connect to MCP server at {url}");
                }
                else
                {
                    McpLog.Info($"[CI Boot] Connected to {url}");
                }
            }, TaskScheduler.Default);
        }
    }
}
