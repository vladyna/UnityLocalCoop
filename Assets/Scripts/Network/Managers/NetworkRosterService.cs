using System.Collections.Generic;
using Test.Services;
using Test.Services.Models;
using Unity.Collections;
using Unity.Netcode;
using Zenject;

namespace Test.Network.Managers
{
    public sealed class NetworkRosterService : NetworkBehaviour
    {
        private LobbyManager _lobbyManager;

        [Inject]
        public void Construct(LobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsServer)
                return;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }

            BroadcastRoster();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer)
                return;

            BroadcastRoster();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer)
                return;

            BroadcastRoster();
        }

        public void BroadcastRoster()
        {
            if (!IsServer)
                return;

            var clients = NetworkManager.Singleton.ConnectedClientsList;
            var ids = new ulong[clients.Count];
            var names = new FixedString32Bytes[clients.Count];

            for (int i = 0; i < clients.Count; i++)
            {
                ids[i] = clients[i].ClientId;
                names[i] = new FixedString32Bytes($"Player_{clients[i].ClientId}");
            }

            ApplyRosterClientRpc(ids, names);
        }

        [Rpc(SendTo.NotServer)]
        private void ApplyRosterClientRpc(ulong[] clientIds, FixedString32Bytes[] names)
        {
            if (_lobbyManager == null)
                return;

            var snapshot = new List<PlayerInfo>(clientIds.Length);
            for (int i = 0; i < clientIds.Length; i++)
            {
                snapshot.Add(new PlayerInfo
                {
                    ClientId = clientIds[i],
                    Name = i < names.Length ? names[i].ToString() : $"Player_{clientIds[i]}"
                });
            }

            _lobbyManager.ApplyRosterSnapshot(snapshot);
        }
    }
}