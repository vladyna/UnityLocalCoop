using Test.Services;
using Test.Services.Abstractions;
using UnityEngine;

namespace Test.Player
{
    public class PlayerInteractionController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        private PlayerInputService _inputService;
        private IObjectSpawnService _spawnService;
        private IObjectGrabService _objectGrabService;

        public void Initialize(PlayerInputService inputService, IObjectSpawnService spawnService, IObjectGrabService objectGrabService)
        {
            _inputService = inputService;
            _spawnService = spawnService;
            _objectGrabService = objectGrabService;
        }

        public void Tick()
        {
            if (_inputService == null || _inputService.Input == null)
                return;

            HandleSpawning();
            HandleGrabbing();
        }

        private void HandleSpawning()
        {
            if (_spawnService == null)
                return;

            if (_inputService.Input.Player.Spawn.triggered)
            {
                _spawnService.SpawnObject(
                    transform.position + transform.forward * 2f,
                    Quaternion.identity);
            }
        }

        private void HandleGrabbing()
        {
            if (_objectGrabService == null || _camera == null)
                return;

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
    }
}
