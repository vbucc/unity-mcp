using System;
using System.IO;
using System.Linq;
using MCPForUnity.Editor.Clients;
using MCPForUnity.Editor.Constants;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Models;
using MCPForUnity.Editor.Services;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace MCPForUnity.Editor.Migrations
{
    /// <summary>
    /// Rewrites MCP client configs when the package version changes.
    ///
    /// Two jobs: keep configs pointing at the current package version, and migrate
    /// clients still carrying a stdio registration (a <c>command</c>/<c>args</c> entry
    /// with no <c>url</c>) from before stdio was removed. Without this, upgrading users
    /// keep a config that launches a transport the server no longer speaks.
    /// </summary>
    [InitializeOnLoad]
    internal static class LegacyClientConfigMigration
    {
        private const string LastUpgradeKey = EditorPrefKeys.LastClientConfigMigrationVersion;

        static LegacyClientConfigMigration()
        {
            if (Application.isBatchMode)
                return;

            EditorApplication.delayCall += RunMigrationIfNeeded;
        }

        private static void RunMigrationIfNeeded()
        {
            EditorApplication.delayCall -= RunMigrationIfNeeded;

            string currentVersion = AssetPathUtility.GetPackageVersion();
            if (string.IsNullOrEmpty(currentVersion) || string.Equals(currentVersion, "unknown", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string lastUpgradeVersion = string.Empty;
            try { lastUpgradeVersion = EditorPrefs.GetString(LastUpgradeKey, string.Empty); } catch { }

            if (string.Equals(lastUpgradeVersion, currentVersion, StringComparison.OrdinalIgnoreCase))
            {
                return; // Already refreshed for this package version
            }

            bool hadFailures = false;
            bool touchedAny = false;

            var configurators = McpClientRegistry.All.OfType<McpClientConfiguratorBase>().ToList();
            foreach (var configurator in configurators)
            {
                try
                {
                    if (!configurator.SupportsAutoConfigure)
                        continue;

                    // Handle CLI-based configurators (e.g., Claude Code CLI)
                    // CheckStatus with attemptAutoRewrite=true re-registers a stale entry
                    if (configurator is ClaudeCliMcpConfigurator)
                    {
                        var previousStatus = configurator.Status;
                        configurator.CheckStatus(attemptAutoRewrite: true);
                        if (configurator.Status != previousStatus)
                        {
                            touchedAny = true;
                        }
                        continue;
                    }

                    // Handle JSON file-based configurators
                    if (!JsonConfigIsLegacyStdio(configurator.Client))
                        continue;

                    MCPServiceLocator.Client.ConfigureClient(configurator);
                    touchedAny = true;
                }
                catch (Exception ex)
                {
                    hadFailures = true;
                    McpLog.Warn($"Failed to refresh MCP config for {configurator.DisplayName}: {ex.Message}");
                }
            }

            if (!touchedAny)
            {
                // Nothing needed refreshing; still record version so we don't rerun every launch
                try { EditorPrefs.SetString(LastUpgradeKey, currentVersion); } catch { }
                return;
            }

            if (hadFailures)
            {
                McpLog.Warn("MCP client config upgrade encountered errors; will retry next session.");
                return;
            }

            try
            {
                EditorPrefs.SetString(LastUpgradeKey, currentVersion);
            }
            catch { }

            McpLog.Info($"Updated MCP client configs to package version {currentVersion}.");
        }

        /// <summary>
        /// True when the client's JSON config still carries a stdio-style entry
        /// (a <c>command</c> field) instead of the HTTP <c>url</c>.
        /// </summary>
        private static bool JsonConfigIsLegacyStdio(McpClient client)
        {
            string configPath = McpConfigurationHelper.GetClientConfigPath(client);
            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                return false;
            }

            try
            {
                var root = JObject.Parse(File.ReadAllText(configPath));

                JToken unityNode = null;
                if (client.IsVsCodeLayout)
                {
                    unityNode = root.SelectToken("servers.unityMCP")
                               ?? root.SelectToken("mcp.servers.unityMCP");
                }
                else
                {
                    unityNode = root.SelectToken("mcpServers.unityMCP");
                }

                if (unityNode == null) return false;

                return unityNode["command"] != null;
            }
            catch
            {
                return false;
            }
        }

    }
}
