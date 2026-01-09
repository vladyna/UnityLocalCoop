using Test.Services;
using Test.UI;
using Unity.Collections;
using Unity.Netcode;
using Zenject;

namespace Test.Network.Managers
{
    public sealed class NetworkSessionLifecycle : NetworkBehaviour
    {
        private LobbyManager _lobbyManager;
        private GameManager _gameManager;

        private bool _serverShuttingDown;

        [Inject]
        public void Construct(LobbyManager lobbyManager, GameManager gameManager)
        {
            _lobbyManager = lobbyManager;
            _gameManager = gameManager;
        }

        public void LeaveOnApplicationQuit()
        {
            var nm = NetworkManager.Singleton;
            if (nm != null && nm.IsHost)
            {
                EndSessionAsHost("Host application quit");
                return;
            }

            _lobbyManager.LeaveLobby();
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

        public void Kick(ulong targetClientId)
        {
            if (!IsServer)
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

        [ClientRpc]
        private void KickedClientClientRpc(ClientRpcParams clientRpcParams = default)
        {
            if (IsServer)
                return;

            _lobbyManager?.Shutdown();
            _gameManager.ForceReturnToMainMenu();
        }
    }
}