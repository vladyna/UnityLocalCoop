using Test.Player.Abstractions;
using Test.Player.Models;
using UnityEngine;
namespace Test.Player.Network
{
    public class NetworkSimulationDriver : ISimulationDriver
    {
        private readonly ICharacterController _controller;

        private readonly INetworkReplicator _replicator;
        public NetworkSimulationDriver(ICharacterController controller, INetworkReplicator replicator)
        {
            _controller = controller;
            _replicator = replicator;
            _replicator.Initialize(_controller);
        }

        public void Tick(MovementInput input)
        {
            _controller.Simulate(input, Time.deltaTime);
            _replicator.SendInput(input);
        }

        public void Tick(MovementInput input, float dt)
        {
            _controller.Simulate(input, dt);
        }

        public void ReceiveAuthoritativeState(EntityState state)
        {
            var local = _controller.CaptureState();
            if (Vector3.Distance(local.Position, state.Position) > 0.3f)
            {
                _controller.ApplyState(state);
            }
        }
    }
}
