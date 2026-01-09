using Test.Player.Abstractions;
using Test.Player.Models;
using UnityEngine;

namespace Test.Player
{
    public class LocalSimulationDriver : ISimulationDriver
    {
        private readonly ICharacterController _controller;
  
  
        public LocalSimulationDriver(ICharacterController controller)
        {
            _controller = controller;

        }

        public void Tick(MovementInput input)
        {
            _controller.Simulate(input, Time.deltaTime);
        }

        public void Tick(MovementInput input, float dt)
        {
            _controller.Simulate(input, dt);
        }

        public void ReceiveAuthoritativeState(EntityState state)
        {
          
        }
    }
}
