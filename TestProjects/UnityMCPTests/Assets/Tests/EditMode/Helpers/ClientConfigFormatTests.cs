using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Models;
using MCPForUnity.Editor.Constants;
using EditorConfigCache = MCPForUnity.Editor.Services.EditorConfigurationCache;

namespace MCPForUnityTests.Editor.Helpers
{
    // Per-client MCP config-format coverage.
    //
    // This fork ships only ClaudeCodeConfigurator, so the Kilo Code and Cline format tests that
    // used to live here went with their configurators. What remains is the generic-client case:
    // a client with no type override must still receive plain type:"http".
    public class ClientConfigFormatTests
    {
        private const string UseHttpTransportPrefKey = EditorPrefKeys.UseHttpTransport;

        private bool _hadHttpTransport;
        private bool _originalHttpTransport;

        [SetUp]
        public void SetUp()
        {
            _hadHttpTransport = EditorPrefs.HasKey(UseHttpTransportPrefKey);
            _originalHttpTransport = EditorPrefs.GetBool(UseHttpTransportPrefKey, true);

            // Force HTTP transport so the remote/streamableHttp branch is exercised.
            EditorPrefs.SetBool(UseHttpTransportPrefKey, true);
            EditorConfigCache.Instance.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            if (_hadHttpTransport)
                EditorPrefs.SetBool(UseHttpTransportPrefKey, _originalHttpTransport);
            else
                EditorPrefs.DeleteKey(UseHttpTransportPrefKey);
            EditorConfigCache.Instance.Refresh();
        }

        [Test]
        public void BuildManualConfigJson_ForGenericHttpClient_UsesPlainHttpType()
        {
            // A client without a type override must continue to receive the generic type:http.
            var client = new McpClient { name = "Cursor" };

            var root = JObject.Parse(ConfigJsonBuilder.BuildManualConfigJson(uvPath: null, client));
            var unity = (JObject)root.SelectToken("mcpServers.unityMCP");

            Assert.NotNull(unity, "Expected mcpServers.unityMCP node");
            Assert.AreEqual("http", (string)unity["type"],
                "Clients without HttpTypeValue should keep the generic type:http");
        }
    }
}
