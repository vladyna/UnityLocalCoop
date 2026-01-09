using Test.Player.Abstractions;
using Test.Player.Models;
using Test.Player.Network;
using Test.Services;
using Test.Services.Abstractions;
using UnityEngine;
using Zenject;

namespace Test.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerRoot : MonoBehaviour
    {
        [SerializeField] private PlayerMovementController _movement;
        [SerializeField] private PlayerInteractionController _interaction;
        [SerializeField] private PlayerCosmeticsController _cosmetics;

        private IObjectSpawnService _spawnService;
        private IObjectGrabService _objectGrabService;

        private PlayerInputService _inputService;
        private ISimulationDriver _driver;
        private CharacterControllerCore _controllerCore;

        private PlayerNetwork _playerNetwork;
        private LobbyManager _lobbyManager;

        [Inject]
        public void Construct(PlayerInputService inputService, IObjectSpawnService spawnService, IObjectGrabService objectGrabService, LobbyManager lobbyManager)
        {
            _inputService = inputService ?? _inputService;
            _spawnService = spawnService ?? _spawnService;
            _objectGrabService = objectGrabService ?? _objectGrabService;
            _lobbyManager = lobbyManager ?? _lobbyManager;
        }
        private void Start()
        {
            _inputService?.Enable();

            var cc = GetComponent<CharacterController>();

            var core = new CharacterControllerCore(cc);
            _controllerCore = core;

            if (!_lobbyManager.IsInLobby)
            {
                _driver = new LocalSimulationDriver(core);
            }
            else
            {
                var networkReplicator = GetComponent<INetworkReplicator>();
                _driver = new NetworkSimulationDriver(core, networkReplicator);
            }
            _playerNetwork = GetComponent<PlayerNetwork>();

            if (_movement != null)
                _movement.Initialize(_inputService, _driver);

            if (_interaction != null)
                _interaction.Initialize(_inputService, _spawnService, _objectGrabService);

            if (_cosmetics != null)
                _cosmetics.Initialize(_inputService, _playerNetwork);
        }

        public EntityState SimulateMovement(MovementInput input, float dt)
        {
            _controllerCore.Simulate(input, dt);
            return _controllerCore.CaptureState();
        }

        private void Update()
        {
            if (!CanProcessInput())
                return;

            _movement?.Tick();
            _interaction?.Tick();
            _cosmetics?.Tick();
        }

        private bool CanProcessInput()
        {
            if (_lobbyManager != null && _lobbyManager.IsInLobby)
            {
                if (_playerNetwork != null && !_playerNetwork.IsOwner)
                    return false;
            }

            return true;
        }
    }
}
