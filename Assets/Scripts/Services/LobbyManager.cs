using System;
using System.Collections.Generic;
using Test.Network.Abstractions;
using Test.Services.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace Test.Services
{
    public class LobbyManager
    {
        private const ushort DefaultPort = 7777;

        private readonly List<PlayerInfo> _players = new();
        public IReadOnlyList<PlayerInfo> Players => _players;
        public ulong HostClientId { get; private set; }
        public bool IsInLobby
        {
            get
            {
                var nm = NetworkManager.Singleton;
                return nm != null && (nm.IsClient || nm.IsHost);
            }
        }

        public event Action OnHostLeft;
        public event Action<ulong> OnPlayerJoined;
        public event Action<ulong> OnPlayerLeft;
        public event Action<IReadOnlyList<PlayerInfo>> OnRosterChanged;

        public event Action<string> OnNetworkStartFailed;
        public event Action<string> OnNetworkStatus;

        private readonly LanIpService _lanIpService;

        public IPlayerRosterNetworkBridge NetworkBridge { get; set; }

        public string LastKnownHostLanIp { get; private set; } = "127.0.0.1";

        public LobbyManager(LanIpService lanIpProvider)
        {
            _lanIpService = lanIpProvider;
        }

        public void CreateLobby(string port)
        {
            SubscribeToEvents();

            LastKnownHostLanIp = _lanIpService.GetBestLanIpv4Address();
            var parsedPort = ushort.TryParse(port, out var p) ? p : DefaultPort;
            ConfigureTransport(LastKnownHostLanIp, parsedPort);

            var nm = NetworkManager.Singleton;
            if (nm == null)
            {
                OnNetworkStartFailed?.Invoke("NetworkManager is missing.");
                return;
            }

            var ok = nm.StartHost();
            if (!ok)
            {
                OnNetworkStartFailed?.Invoke("Failed to start client; Port is busy\"");
                return;
            }

            OnNetworkStatus?.Invoke($"Host starting on {LastKnownHostLanIp}:{parsedPort}...");
        }

        public void JoinLobby(string port)
        {
            JoinLobby(LastKnownHostLanIp, port);
        }

        public void JoinLobby(string hostIp, string port)
        {
            SubscribeToEvents();

            if (string.IsNullOrWhiteSpace(hostIp))
                hostIp = "127.0.0.1";

            hostIp = hostIp.Trim();
            var parsedPort = ushort.TryParse(port, out var p) ? p : DefaultPort;
            ConfigureTransport(hostIp, parsedPort);

            var nm = NetworkManager.Singleton;
            if (nm == null)
            {
                OnNetworkStartFailed?.Invoke("NetworkManager is missing.");
                return;
            }

            var ok = nm.StartClient();
            if (!ok)
            {
                OnNetworkStartFailed?.Invoke("Failed to start client; Port is busy");
                return;
            }

            OnNetworkStatus?.Invoke($"Client connecting to {hostIp}:{parsedPort}...");
        }

        private void ConfigureTransport(string address, ushort port)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
                return;

            var transport = nm.GetComponent<UnityTransport>();
            if (transport == null)
                return;

            transport.SetConnectionData(address, port);
        }

        private void SubscribeToEvents()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
                return;

            nm.OnTransportFailure -= OnTransportFailure;
            nm.OnTransportFailure += OnTransportFailure;

            nm.OnClientDisconnectCallback -= OnAnyClientDisconnected;
            nm.OnClientDisconnectCallback += OnAnyClientDisconnected;

            nm.OnClientConnectedCallback -= OnAnyClientConnected;
            nm.OnClientConnectedCallback += OnAnyClientConnected;

            nm.OnClientConnectedCallback -= AddPlayer;
            nm.OnClientDisconnectCallback -= RemovePlayer;

            if (nm.IsHost)
            {
                nm.OnClientConnectedCallback += AddPlayer;
                nm.OnClientDisconnectCallback += RemovePlayer;
            }
        }

        private void UnsubscribeFromEvents()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
                return;

            nm.OnClientConnectedCallback -= AddPlayer;
            nm.OnClientDisconnectCallback -= RemovePlayer;

            nm.OnTransportFailure -= OnTransportFailure;
            nm.OnClientDisconnectCallback -= OnAnyClientDisconnected;
            nm.OnClientConnectedCallback -= OnAnyClientConnected;
        }

        private void OnTransportFailure()
        {
            var nm = NetworkManager.Singleton;
            var reason = nm != null ? nm.DisconnectReason : null;
            if (string.IsNullOrWhiteSpace(reason))
                reason = "Transport failure (socket/port/network error).";

            OnNetworkStartFailed?.Invoke(reason);
        }

        private void OnAnyClientConnected(ulong clientId)
        {
            var nm = NetworkManager.Singleton;
            if (nm != null && clientId == nm.LocalClientId)
            {
                OnNetworkStatus?.Invoke("Connected.");
            }
        }

        private void OnAnyClientDisconnected(ulong clientId)
        {
            var nm = NetworkManager.Singleton;
            if (nm != null && clientId == nm.LocalClientId)
            {
                var reason = nm.DisconnectReason;
                if (string.IsNullOrWhiteSpace(reason))
                    reason = "Disconnected.";

                OnNetworkStartFailed?.Invoke(reason);
            }
        }

        private void AddPlayer(ulong clientId)
        {
            _players.Add(new PlayerInfo { ClientId = clientId, Name = $"Player_{clientId}" });
            if (_players.Count == 1)
            {
                HostClientId = clientId;
            }
            OnPlayerJoined?.Invoke(clientId);
            OnRosterChanged?.Invoke(_players);
        }

        private void RemovePlayer(ulong clientId)
        {
            _players.RemoveAll(p => p.ClientId == clientId);
            if (clientId == HostClientId)
            {
                OnHostLeft?.Invoke();
            }
            OnPlayerLeft?.Invoke(clientId);
            OnRosterChanged?.Invoke(_players);
        }

        public void LeaveLobby()
        {
            Shutdown();
        }

        public void Shutdown()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
                return;

            UnsubscribeFromEvents();

            if (nm.IsListening)
            {
                nm.Shutdown();
            }

            _players.Clear();
            OnRosterChanged?.Invoke(_players);
        }

        public void ApplyRosterSnapshot(List<PlayerInfo> snapshot)
        {
            _players.Clear();
            _players.AddRange(snapshot);
            OnRosterChanged?.Invoke(_players);
        }

        public void Kick(ulong targetClientId)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
                return;

            if (!nm.IsHost)
                return;

            NetworkBridge?.Kick(targetClientId);
        }

        public void SetLocalPlayerName(string name)
        {
            var nm = NetworkManager.Singleton;
            var localId = nm != null ? nm.LocalClientId : 0;
            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[i].ClientId != localId)
                    continue;

                var p = _players[i];
                p.Name = name;
                _players[i] = p;
                break;
            }

            OnRosterChanged?.Invoke(_players);
        }
    }
}
