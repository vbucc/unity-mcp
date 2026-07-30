using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.External.Tommy;
using MCPForUnity.Editor.Services;
using System.IO;
using MCPForUnity.Editor.Constants;
using UnityEditor;

namespace MCPForUnityTests.Editor.Helpers
{
    public class CodexConfigHelperTests
    {

        /// <summary>
        /// Mock platform service for testing
        /// </summary>
        private class MockPlatformService : IPlatformService
        {
            private readonly bool _isWindows;
            private readonly string _systemRoot;

            public MockPlatformService(bool isWindows, string systemRoot = "C:\\Windows")
            {
                _isWindows = isWindows;
                _systemRoot = systemRoot;
            }

            public bool IsWindows() => _isWindows;
            public string GetSystemRoot() => _isWindows ? _systemRoot : null;
        }

        private bool _hadGitOverride;
        private string _originalGitOverride;
        private bool _hadDevForceRefresh;
        private bool _originalDevForceRefresh;
        private IPlatformService _originalPlatformService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _hadGitOverride = EditorPrefs.HasKey(EditorPrefKeys.GitUrlOverride);
            _originalGitOverride = EditorPrefs.GetString(EditorPrefKeys.GitUrlOverride, string.Empty);
            _hadDevForceRefresh = EditorPrefs.HasKey(EditorPrefKeys.DevModeForceServerRefresh);
            _originalDevForceRefresh = EditorPrefs.GetBool(EditorPrefKeys.DevModeForceServerRefresh, false);
            _originalPlatformService = MCPServiceLocator.Platform;
        }

        [SetUp]
        public void SetUp()
        {
            // Ensure per-test deterministic Git URL (ignore developer overrides)
            EditorPrefs.DeleteKey(EditorPrefKeys.GitUrlOverride);
            // Ensure deterministic uvx args ordering for these tests regardless of editor settings
            // (dev-mode inserts --no-cache/--refresh, which changes the first args).
            EditorPrefs.SetBool(EditorPrefKeys.DevModeForceServerRefresh, false);
            // Refresh the cache so it picks up the test's pref values
            EditorConfigurationCache.Instance.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            // IMPORTANT:
            // These tests can be executed while an MCP session is active (e.g., when running tests via MCP).
            // MCPServiceLocator.Reset() disposes the bridge + transport manager, which can kill the MCP connection
            // mid-run. Instead, restore only what this fixture mutates.
            // To avoid leaking global state to other tests/fixtures, restore the original platform service
            // instance captured before this fixture started running.
            if (_originalPlatformService != null)
            {
                MCPServiceLocator.Register<IPlatformService>(_originalPlatformService);
            }
            else
            {
                MCPServiceLocator.Register<IPlatformService>(new PlatformService());
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (_hadGitOverride)
            {
                EditorPrefs.SetString(EditorPrefKeys.GitUrlOverride, _originalGitOverride);
            }
            else
            {
                EditorPrefs.DeleteKey(EditorPrefKeys.GitUrlOverride);
            }


            if (_hadDevForceRefresh)
            {
                EditorPrefs.SetBool(EditorPrefKeys.DevModeForceServerRefresh, _originalDevForceRefresh);
            }
            else
            {
                EditorPrefs.DeleteKey(EditorPrefKeys.DevModeForceServerRefresh);
            }

        }

        [Test]
        public void TryParseCodexServer_SingleLineArgs_ParsesSuccessfully()
        {
            string toml = string.Join("\n", new[]
            {
                "[mcp_servers.unityMCP]",
                "command = \"uvx --from git+https://github.com/CoplayDev/unity-mcp@v6.3.0#subdirectory=Server\"",
                "args = [\"mcp-for-unity\"]"
            });

            bool result = CodexConfigHelper.TryParseCodexServer(toml, out string command, out string[] args);

            Assert.IsTrue(result, "Parser should detect server definition");
            Assert.AreEqual("uvx --from git+https://github.com/CoplayDev/unity-mcp@v6.3.0#subdirectory=Server", command);
            CollectionAssert.AreEqual(new[] { "mcp-for-unity" }, args);
        }

        [Test]
        public void TryParseCodexServer_MultiLineArgsWithTrailingComma_ParsesSuccessfully()
        {
            string toml = string.Join("\n", new[]
            {
                "[mcp_servers.unityMCP]",
                "command = \"uvx\"",
                "args = [",
                "  \"mcp-for-unity\",",
                "]"
            });

            bool result = CodexConfigHelper.TryParseCodexServer(toml, out string command, out string[] args);

            Assert.IsTrue(result, "Parser should handle multi-line arrays with trailing comma");
            Assert.AreEqual("uvx", command);
            CollectionAssert.AreEqual(new[] { "mcp-for-unity" }, args);
        }

        [Test]
        public void TryParseCodexServer_MultiLineArgsWithComments_IgnoresComments()
        {
            string toml = string.Join("\n", new[]
            {
                "[mcp_servers.unityMCP]",
                "command = \"uvx\"",
                "args = [",
                "  \"mcp-for-unity\", # package name",
                "]"
            });

            bool result = CodexConfigHelper.TryParseCodexServer(toml, out string command, out string[] args);

            Assert.IsTrue(result, "Parser should tolerate comments within the array block");
            Assert.AreEqual("uvx", command);
            CollectionAssert.AreEqual(new[] { "mcp-for-unity" }, args);
        }

        [Test]
        public void TryParseCodexServer_HeaderWithComment_StillDetected()
        {
            string toml = string.Join("\n", new[]
            {
                "[mcp_servers.unityMCP] # annotated header",
                "command = \"uvx\"",
                "args = [\"mcp-for-unity\"]"
            });

            bool result = CodexConfigHelper.TryParseCodexServer(toml, out string command, out string[] args);

            Assert.IsTrue(result, "Parser should recognize section headers even with inline comments");
            Assert.AreEqual("uvx", command);
            CollectionAssert.AreEqual(new[] { "mcp-for-unity" }, args);
        }

        [Test]
        public void TryParseCodexServer_SingleQuotedArgsWithApostrophes_ParsesSuccessfully()
        {
            string toml = string.Join("\n", new[]
            {
                "[mcp_servers.unityMCP]",
                "command = 'uvx'",
                "args = ['mcp-for-unity']"
            });

            bool result = CodexConfigHelper.TryParseCodexServer(toml, out string command, out string[] args);

            Assert.IsTrue(result, "Parser should accept single-quoted arrays with escaped apostrophes");
            Assert.AreEqual("uvx", command);
            CollectionAssert.AreEqual(new[] { "mcp-for-unity" }, args);
        }





        [Test]
        public void BuildCodexServerBlock_HttpMode_GeneratesUrlField()
        {
            // This test verifies HTTP transport mode generates url field instead of command/args

            // Force HTTP mode

            string uvPath = "C:\\Program Files\\uv\\uv.exe";

            string result = CodexConfigHelper.BuildCodexServerBlock(uvPath);

            Assert.IsNotNull(result, "BuildCodexServerBlock should return a valid TOML string");

            // Parse the generated TOML to validate structure
            TomlTable parsed;
            using (var reader = new StringReader(result))
            {
                parsed = TOML.Parse(reader);
            }

            // Verify basic structure
            Assert.IsTrue(parsed.TryGetNode("mcp_servers", out var mcpServersNode), "TOML should contain mcp_servers");
            Assert.IsInstanceOf<TomlTable>(mcpServersNode, "mcp_servers should be a table");

            var mcpServers = mcpServersNode as TomlTable;
            Assert.IsTrue(mcpServers.TryGetNode("unityMCP", out var unityMcpNode), "mcp_servers should contain unityMCP");
            Assert.IsInstanceOf<TomlTable>(unityMcpNode, "unityMCP should be a table");

            var unityMcp = unityMcpNode as TomlTable;

            // Verify features.rmcp_client is enabled for HTTP transport
            Assert.IsTrue(parsed.TryGetNode("features", out var featuresNode), "HTTP mode should include features table");
            Assert.IsInstanceOf<TomlTable>(featuresNode, "features should be a table");
            var features = featuresNode as TomlTable;
            Assert.IsTrue(features.TryGetNode("rmcp_client", out var rmcpNode), "features should include rmcp_client flag");
            Assert.IsInstanceOf<TomlBoolean>(rmcpNode, "rmcp_client should be a boolean");
            Assert.IsTrue((rmcpNode as TomlBoolean).Value, "rmcp_client should be true");
            
            // Verify url field is present
            Assert.IsTrue(unityMcp.TryGetNode("url", out var urlNode), "unityMCP should contain url in HTTP mode");
            Assert.IsInstanceOf<TomlString>(urlNode, "url should be a string");

            var url = (urlNode as TomlString).Value;
            Assert.IsTrue(url.Contains("http"), "URL should be an HTTP endpoint");
            Assert.IsTrue(url.Contains("/mcp"), "URL should contain /mcp path");

            // Verify command and args are NOT present in HTTP mode
            Assert.IsFalse(unityMcp.TryGetNode("command", out _), "HTTP mode should not contain command field");
            Assert.IsFalse(unityMcp.TryGetNode("args", out _), "HTTP mode should not contain args field");
            Assert.IsFalse(unityMcp.TryGetNode("env", out _), "HTTP mode should not contain env field");
        }

        [Test]
        public void TryParseCodexServer_HttpMode_ParsesUrlSuccessfully()
        {
            // This test verifies HTTP mode parsing with url field

            string toml = string.Join("\n", new[]
            {
                "[mcp_servers.unityMCP]",
                "url = \"http://localhost:8080/mcp/v1/rpc\""
            });

            bool result = CodexConfigHelper.TryParseCodexServer(toml, out string command, out string[] args, out string url);

            Assert.IsTrue(result, "Parser should accept HTTP mode with url field");
            Assert.IsNull(command, "Command should be null in HTTP mode");
            Assert.IsNull(args, "Args should be null in HTTP mode");
            Assert.AreEqual("http://localhost:8080/mcp/v1/rpc", url, "URL should be parsed correctly");
        }

        [Test]
        public void UpsertCodexServerBlock_HttpMode_GeneratesUrlField()
        {
            // This test verifies HTTP mode upsert generates url field

            // Force HTTP mode

            string existingToml = string.Join("\n", new[]
            {
                "[other_section]",
                "key = \"value\""
            });

            string uvPath = "C:\\path\\to\\uv.exe";

            string result = CodexConfigHelper.UpsertCodexServerBlock(existingToml, uvPath);

            Assert.IsNotNull(result, "UpsertCodexServerBlock should return a valid TOML string");

            // Parse the generated TOML to validate structure
            TomlTable parsed;
            using (var reader = new StringReader(result))
            {
                parsed = TOML.Parse(reader);
            }

            // Verify existing sections are preserved
            Assert.IsTrue(parsed.TryGetNode("other_section", out _), "TOML should preserve existing sections");

            // Verify mcp_servers structure
            Assert.IsTrue(parsed.TryGetNode("mcp_servers", out var mcpServersNode), "TOML should contain mcp_servers");
            Assert.IsInstanceOf<TomlTable>(mcpServersNode, "mcp_servers should be a table");

            var mcpServers = mcpServersNode as TomlTable;
            Assert.IsTrue(mcpServers.TryGetNode("unityMCP", out var unityMcpNode), "mcp_servers should contain unityMCP");
            Assert.IsInstanceOf<TomlTable>(unityMcpNode, "unityMCP should be a table");

            var unityMcp = unityMcpNode as TomlTable;

            // Verify features.rmcp_client is enabled for HTTP transport
            Assert.IsTrue(parsed.TryGetNode("features", out var featuresNode), "HTTP mode should include features table");
            Assert.IsInstanceOf<TomlTable>(featuresNode, "features should be a table");
            var features = featuresNode as TomlTable;
            Assert.IsTrue(features.TryGetNode("rmcp_client", out var rmcpNode), "features should include rmcp_client flag");
            Assert.IsInstanceOf<TomlBoolean>(rmcpNode, "rmcp_client should be a boolean");
            Assert.IsTrue((rmcpNode as TomlBoolean).Value, "rmcp_client should be true");

            // Verify url field is present
            Assert.IsTrue(unityMcp.TryGetNode("url", out var urlNode), "unityMCP should contain url in HTTP mode");
            Assert.IsInstanceOf<TomlString>(urlNode, "url should be a string");

            var url = (urlNode as TomlString).Value;
            Assert.IsTrue(url.Contains("http"), "URL should be an HTTP endpoint");

            // Verify command and args are NOT present in HTTP mode
            Assert.IsFalse(unityMcp.TryGetNode("command", out _), "HTTP mode should not contain command field");
            Assert.IsFalse(unityMcp.TryGetNode("args", out _), "HTTP mode should not contain args field");
        }
    }
}
