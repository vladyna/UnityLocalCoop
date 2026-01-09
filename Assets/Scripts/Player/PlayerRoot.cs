using Test.Player.Abstractions;
using Test.Player.Models;
using Test.Player.Network;
using Test.Player.Utils;
using Test.Services;
using Test.Services.Abstractions;
using UnityEngine;
using Zenject;

namespace Test.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerRoot : MonoBehaviour
    {
        [SerializeField] private FirstPersonCamera _cameraController;
        [SerializeField] private Camera _camera;

        [SerializeField] private IObjectSpawnService _spawnService;
        [SerializeField] private IObjectGrabService _objectGrabService;

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
            _inputService.Enable();

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
        }

        public EntityState SimulateMovement(MovementInput input, float dt)
        {
            _cameraController.ApplyLook(input.Look);
            _controllerCore.Simulate(input, dt);
            return _controllerCore.CaptureState();
        }

        private void Update()
        {
            if (_playerNetwork != null && !_playerNetwork.IsOwner)
            {
                return;
            }

            HandleMovement();
            HandleSpawning();
            HandleGrabbing();
            HandleColor();
        }

        private void HandleMovement()
        {
            var input = new MovementInput
            {
                Move = _inputService.Input.Player.Move.ReadValue<Vector2>(),
                Look = _inputService.Input.Player.Look.ReadValue<Vector2>(),
                Jump = false,
                Timestamp = Time.time
            };
            _cameraController.ApplyLook(input.Look);
            _driver.Tick(input);
        }

        private void HandleSpawning()
        {
            if (_inputService.Input.Player.Spawn.triggered)
            {
                _spawnService.SpawnObject(
                    transform.position + transform.forward * 2f,
                    Quaternion.identity);
            }
        }

        private void HandleGrabbing()
        {
            if (_inputService.Input.Player.Interact.triggered)
            {
                _objectGrabService.TryGrabObject(
                    _camera.transform.position,
                    _camera.transform.forward,
                    _camera,
                    3f);
            }

            if (_inputService.Input.Player.Release.triggered)
            {
                _objectGrabService.ReleaseGrabbedObject();
            }

            if (_inputService.Input.Player.Throw.triggered)
            {
                _objectGrabService.ThrowGrabbedObject(_camera.transform.forward * 3f);
            }

            if (_inputService.Input.Player.Delete.triggered)
            {
                _objectGrabService.DeleteGrabbedObject();
            }
        }

        private void HandleColor()
        {
            if (_inputService.Input.Player.Color.triggered)
            {
                if (_playerNetwork == null)
                    _playerNetwork = GetComponent<PlayerNetwork>();

                _playerNetwork?.CycleColor();
            }
        }
    }
}
