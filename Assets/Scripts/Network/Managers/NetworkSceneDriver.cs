using System;
using System.Collections.Generic;
using Test.Network.Abstractions;
using Test.Services;
using Test.Services.Abstractions;
using Test.Services.Models;
using Test.UI;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Zenject;

namespace Test.Network.Managers
{
    public class NetworkSceneDriver : NetworkBehaviour, IPlayerRosterNetworkBridge
    {
        public event Action<int> OnClientReady;
        public event Action OnAllClientsReady;

        private LobbyManager _lobbyManager;
        private GameManager _gameManager;

        private bool _serverShuttingDown;
        private INetworkSceneDriverRef _driverRef;
        private LoadingUI _loadingUI;

        [Inject]
        public void Construct(GameManager gameManager, LobbyManager lobbyManager, INetworkSceneDriverRef driverRef, LoadingUI loadingUI)
        {
            _gameManager = gameManager;
            _lobbyManager = lobbyManager;
            _lobbyManager.NetworkBridge = this;
            _driverRef = driverRef;
            _loadingUI = loadingUI;
        }

        private void Start()
        {
            if (_driverRef?.Current != null)
            {
                Destroy(this);
                return;
            }
            _driverRef?.Set(this);
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (_driverRef?.Current != this)
            {
                Destroy(_driverRef?.Current);
            }
            DontDestroyOnLoad(gameObject);
            _driverRef?.Set(this);
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
                BroadcastRoster();
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            }
        }

        public void OnDestroy()
        {
            _driverRef?.Clear(this);
        }

        public void EndSessionAsHost(string reason = "Host left")
        {
            if (!IsServer || _serverShuttingDown)
                return;

            _serverShuttingDown = true;
            HostLeftClientRpc(new FixedString128Bytes(reason));

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.ClientId == NetworkManager.ServerClientId)
                    continue;

                NetworkManager.Singleton.DisconnectClient(client.ClientId);
            }

            NetworkManager.Singleton.Shutdown();
        }

        private void OnApplicationQuit()
        {
            var nm = NetworkManager.Singleton;
            if (nm != null && nm.IsHost)
            {
                _driverRef.Current.EndSessionAsHost("Host application quit");
                return;
            }

            _lobbyManager.LeaveLobby();
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

        public void CommandClientsLoadGame()
        {
            if (!IsServer)
                return;

            _loadingUI?.Show();
            ShowLoadingClientRpc();

            var nm = NetworkManager.Singleton;
            if (nm != null && nm.SceneManager != null)
            {
                nm.SceneManager.LoadScene("Game", LoadSceneMode.Single);
            }
        }

        private void OnLoadEventCompleted(
            string sceneName,
            LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            if (!IsServer)
                return;

            var completedCount = clientsCompleted != null ? clientsCompleted.Count : 0;
            OnClientReady?.Invoke(completedCount);

            if (clientsTimedOut != null)
            {
                for (int i = 0; i < clientsTimedOut.Count; i++)
                {
                    NetworkManager.Singleton?.DisconnectClient(clientsTimedOut[i]);
                }
            }
            _loadingUI?.Hide();
            NotifyAllReadyClientRpc();
            OnAllClientsReady?.Invoke();
        }

        public void Kick(ulong targetClientId)
        {
            if (!IsServer)
                return;

            KickClientServerRpc(targetClientId);
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
        private void HostLeftClientRpc(FixedString128Bytes reason)
        {
            if (IsServer)
                return;

            var nm = NetworkManager.Singleton;
            nm?.Shutdown();

            _lobbyManager?.Shutdown();
            _gameManager.ForceReturnToMainMenu();
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

        [Rpc(SendTo.Server)]
        private void KickClientServerRpc(ulong targetClientId, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != NetworkManager.ServerClientId)
                return;

            if (targetClientId == NetworkManager.ServerClientId)
                return;

            var clientRpcParamsVar = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { targetClientId } }
            };
            KickedClientClientRpc(clientRpcParamsVar);

            NetworkManager.Singleton.DisconnectClient(targetClientId);
        }

        [ClientRpc]
        private void KickedClientClientRpc(ClientRpcParams clientRpcParams = default)
        {
            if (IsServer)
                return;

            _lobbyManager?.Shutdown();
            _gameManager.ForceReturnToMainMenu();
        }

        [Rpc(SendTo.NotServer)]
        private void NotifyAllReadyClientRpc()
        {
            OnAllClientsReady?.Invoke();
            _loadingUI?.Hide();
        }

        [Rpc(SendTo.NotServer)]
        private void ShowLoadingClientRpc()
        {
            _loadingUI?.Show();
        }
    }
}