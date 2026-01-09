using System.Collections.Generic;
using Test.Services;
using Test.Zenject.Factories;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Test.Player
{
    public class PlayerSpawner : MonoBehaviour
    {

        [SerializeField] private GameObject _localPlayerPrefab;
        [SerializeField] private GameObject _networkPlayerPrefab;

        [Inject] private LobbyManager _lobbyManager;
        [Inject] private PrefabFactory _prefabFactory;

        void Awake()
        {
            if (!_lobbyManager.IsInLobby)
            {
                _prefabFactory.Create(_localPlayerPrefab);
            }
            else
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
                return;

            var client = NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var data) ? data : null;
            if (client != null && client.PlayerObject != null)
                return;

            var player = _prefabFactory.CreateNetworkObject(
                _networkPlayerPrefab,
                new Vector3(clientId * 2f, 1f, 0f),
                Quaternion.identity);

            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }

        private void OnSceneLoaded(
            string sceneName,
            LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            if (sceneName != "Game")
                return;

            Debug.Log($"Scene {sceneName} loaded for clients: {string.Join(",", clientsCompleted)}");

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject != null)
                    continue;

                var player = _prefabFactory.CreateNetworkObject(
                    _networkPlayerPrefab,
                    new Vector3(client.ClientId * 2f, 1f, 0f),
                    Quaternion.identity);

                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId, true);
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }
        }
    }
}
