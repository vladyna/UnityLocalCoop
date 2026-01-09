using Test.Player.Abstractions;
using Test.Player.Models;
using Test.Player.Utils;
using Test.Services;
using UnityEngine;

namespace Test.Player
{
    public class PlayerMovementController : MonoBehaviour
    {
        [SerializeField] private FirstPersonCamera _cameraController;

        private PlayerInputService _inputService;
        private ISimulationDriver _driver;

        public void Initialize(PlayerInputService inputService, ISimulationDriver driver)
        {
            _inputService = inputService;
            _driver = driver;
        }

        public void Tick()
        {
            if (_inputService == null || _inputService.Input == null)
                return;

            var input = new MovementInput
            {
                Move = _inputService.Input.Player.Move.ReadValue<Vector2>(),
                Look = _inputService.Input.Player.Look.ReadValue<Vector2>(),
                Jump = false,
                Timestamp = Time.time
            };

            _cameraController?.ApplyLook(input.Look);
            _driver?.Tick(input);
        }
    }
}
