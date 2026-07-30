using System;
using System.Threading.Tasks;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Services.Transport.Transports;

namespace MCPForUnity.Editor.Services.Transport
{
    /// <summary>
    /// Coordinates the active transport client and exposes lifecycle helpers.
    /// </summary>
    public class TransportManager
    {
        private IMcpTransportClient _client;
        private TransportState _state = TransportState.Disconnected("http");
        private Func<IMcpTransportClient> _clientFactory;
        private Task<bool> _startTask;

        public TransportManager()
        {
            Configure(() => new WebSocketTransportClient(MCPServiceLocator.ToolDiscovery));
        }

        public void Configure(Func<IMcpTransportClient> clientFactory)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        private IMcpTransportClient GetOrCreateClient() => _client ??= _clientFactory();

        public Task<bool> StartAsync()
        {
            // Editor-main-thread only (no locking needed). Coalesce concurrent starts:
            // manual Connect, reload-resume, and auto-start can otherwise race, and
            // WebSocketTransportClient.StartAsync tears down a live connection first — two
            // interleaved starts bounce each other's session.
            if (_startTask != null && !_startTask.IsCompleted)
            {
                return _startTask;
            }

            _startTask = StartCoreAsync();
            return _startTask;
        }

        private async Task<bool> StartCoreAsync()
        {
            IMcpTransportClient client = GetOrCreateClient();

            bool started = await client.StartAsync();
            if (!started)
            {
                try
                {
                    await client.StopAsync();
                }
                catch (Exception ex)
                {
                    McpLog.Warn($"Error while stopping transport {client.TransportName}: {ex.Message}");
                }
                _state = TransportState.Disconnected(client.TransportName, client.State?.Error ?? "Failed to start");
                return false;
            }

            _state = client.State ?? TransportState.Connected(client.TransportName);
            return true;
        }

        public async Task StopAsync()
        {
            if (_client == null) return;
            try { await _client.StopAsync(); }
            catch (Exception ex) { McpLog.Warn($"Error while stopping transport {_client.TransportName}: {ex.Message}"); }
            finally { _state = TransportState.Disconnected(_client.TransportName); }
        }

        public async Task<bool> VerifyAsync()
        {
            if (_client == null)
            {
                return false;
            }

            bool ok = await _client.VerifyAsync();
            _state = _client.State ?? TransportState.Disconnected(_client.TransportName, "No state reported");
            return ok;
        }

        public TransportState GetState() => _state;

        public bool IsRunning() => _state.IsConnected;

        /// <summary>
        /// Synchronous teardown for shutdown/reload hooks where async awaits are not possible.
        /// </summary>
        public void ForceStop()
        {
            string transportName = _client?.TransportName ?? "http";

            if (_client == null)
            {
                _state = TransportState.Disconnected(transportName);
                return;
            }

            try
            {
                if (_client is WebSocketTransportClient wsClient)
                {
                    wsClient.ForceStop();
                }
                else
                {
                    _client.StopAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                McpLog.Warn($"Error while force-stopping transport {transportName}: {ex.Message}");
            }
            finally
            {
                _state = TransportState.Disconnected(transportName);
            }
        }

        /// <summary>
        /// Gets the active transport client.
        /// Returns null if the client hasn't been created yet.
        /// </summary>
        public IMcpTransportClient GetClient() => _client;
    }
}
