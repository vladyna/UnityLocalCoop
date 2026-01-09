using System;
using Test.Network.Abstractions;
using Test.Services;
using Test.Services.Abstractions;
using Test.UI;
using Unity.Netcode;
using Zenject;

namespace Test.Network.Managers
{
    public class NetworkSceneDriver : NetworkBehaviour, IPlayerRosterNetworkBridge
    {
        public event Action<int> OnClientReady
        {
            add => _sceneFlow.OnClientReady += value;
            remove => _sceneFlow.OnClientReady -= value;
        }

        public event Action OnAllClientsReady
        {
            add => _sceneFlow.OnAllClientsReady += value;
            remove => _sceneFlow.OnAllClientsReady -= value;
        }

        private LobbyManager _lobbyManager;

        private INetworkSceneDriverRef _driverRef;

        private NetworkSessionLifecycle _session;
        private NetworkSceneFlow _sceneFlow;
        private NetworkRosterService _roster;

        [Inject]
        public void Construct(LobbyManager lobbyManager, INetworkSceneDriverRef driverRef)
        {
            _lobbyManager = lobbyManager;
            _lobbyManager.NetworkBridge = this;
            _driverRef = driverRef;
        }

        private void Awake()
        {
            _session = GetComponent<NetworkSessionLifecycle>();
            _sceneFlow = GetComponent<NetworkSceneFlow>();
            _roster = GetComponent<NetworkRosterService>();

            if (_session == null || _sceneFlow == null || _roster == null)
            {
                enabled = false;
            }
        }

        private void Start()
        {
            if (_driverRef?.Current != null)
            {
                Destroy(gameObject);
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
                if (_driverRef?.Current != null)
                {
                    Destroy(_driverRef.Current.gameObject);
                }
            }

            DontDestroyOnLoad(gameObject);
            _driverRef?.Set(this);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        }

        public void OnDestroy()
        {
            _driverRef?.Clear(this);
        }

        private void OnApplicationQuit()
        {
            _session?.LeaveOnApplicationQuit();
        }

   
        public void CommandClientsLoadGame()
        {
            _sceneFlow?.CommandClientsLoadGame();
        }

        public void Kick(ulong targetClientId)
        {
            _session?.Kick(targetClientId);
        }

        public void BroadcastRoster()
        {
            _roster?.BroadcastRoster();
        }
    }
}