
using System;
using System.Threading.Tasks;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Services.Transport;

namespace MCPForUnity.Editor.Services
{
    /// <summary>
    /// Bridges the editor UI to the HTTP transport's WebSocket connection.
    /// </summary>
    public class BridgeControlService : IBridgeControlService
    {
        private readonly TransportManager _transportManager;

        public BridgeControlService()
        {
            _transportManager = MCPServiceLocator.TransportManager;
        }

        private static BridgeVerificationResult BuildVerificationResult(TransportState state, bool pingSucceeded)
        {
            string transportLabel = string.IsNullOrWhiteSpace(state.TransportName)
                ? "http"
                : state.TransportName;
            string detailSuffix = string.IsNullOrWhiteSpace(state.Details) ? string.Empty : $" [{state.Details}]";
            string message = state.Error
                ?? (state.IsConnected ? $"Transport '{transportLabel}' connected{detailSuffix}" : $"Transport '{transportLabel}' disconnected{detailSuffix}");

            return new BridgeVerificationResult
            {
                Success = pingSucceeded,
                HandshakeValid = true,
                PingSucceeded = pingSucceeded,
                Message = message
            };
        }

        public bool IsRunning => _transportManager.IsRunning();

        public async Task<bool> StartAsync()
        {
            try
            {
                bool started = await _transportManager.StartAsync();
                if (!started)
                {
                    McpLog.Warn("Failed to start MCP transport");
                }
                return started;
            }
            catch (Exception ex)
            {
                McpLog.Error($"Error starting MCP transport: {ex.Message}");
                return false;
            }
        }

        public async Task StopAsync()
        {
            try
            {
                await _transportManager.StopAsync();
            }
            catch (Exception ex)
            {
                McpLog.Warn($"Error stopping MCP transport: {ex.Message}");
            }
        }

        public async Task<BridgeVerificationResult> VerifyAsync()
        {
            bool pingSucceeded = await _transportManager.VerifyAsync();
            return BuildVerificationResult(_transportManager.GetState(), pingSucceeded);
        }
    }
}
