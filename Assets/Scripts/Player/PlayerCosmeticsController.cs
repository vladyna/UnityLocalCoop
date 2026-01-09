using Test.Player.Network;
using Test.Services;
using UnityEngine;

namespace Test.Player
{
    public class PlayerCosmeticsController : MonoBehaviour
    {
        private PlayerInputService _inputService;
        private PlayerNetwork _playerNetwork;

        public void Initialize(PlayerInputService inputService, PlayerNetwork playerNetwork)
        {
            _inputService = inputService;
            _playerNetwork = playerNetwork;
        }

        public void Tick()
        {
            if (_inputService == null || _inputService.Input == null)
                return;

            if (_inputService.Input.Player.Color.triggered)
            {
                if (_playerNetwork == null)
                    _playerNetwork = GetComponent<PlayerNetwork>();

                _playerNetwork?.CycleColor();
            }
        }
    }
}
