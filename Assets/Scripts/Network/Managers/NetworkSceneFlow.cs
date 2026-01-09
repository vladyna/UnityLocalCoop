using System;
using System.Collections.Generic;
using Test.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Zenject;

namespace Test.Network.Managers
{
    public sealed class NetworkSceneFlow : NetworkBehaviour
    {
        private const string GameSceneName = "Game";

        public event Action<int> OnClientReady;
        public event Action OnAllClientsReady;

        private LoadingUI _loadingUI;
        private int _expectedClientsToLoad;

        [Inject]
        public void Construct(LoadingUI loadingUI)
        {
            _loadingUI = loadingUI;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsServer)
                return;

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            }
        }

        public void CommandClientsLoadGame()
        {
            if (!IsServer)
                return;

            _loadingUI?.Show();
            ShowLoadingClientRpc();

            var nm = NetworkManager.Singleton;
            if (nm == null)
                return;

            _expectedClientsToLoad = nm.ConnectedClientsList != null ? nm.ConnectedClientsList.Count : 0;

            if (nm.SceneManager != null)
                nm.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
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

            if (_expectedClientsToLoad <= 0 || completedCount >= _expectedClientsToLoad)
            {
                _loadingUI?.Hide();
                NotifyAllReadyClientRpc();
                OnAllClientsReady?.Invoke();
            }
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