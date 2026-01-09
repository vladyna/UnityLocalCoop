using Test.Objects.Abstractions;
using Test.Objects.Models;
using Test.Player.Models;
using UnityEngine;

namespace Test.Objects
{
    public class LocalObjectSimulationDriver : IObjectSimulationDriver
    {
        private readonly IGrabbableController _controller;

        public IGrabbableController Controller => _controller;

        public LocalObjectSimulationDriver(IGrabbableController controller)
        {
            _controller = controller;
        }

        public void Tick(ObjectInput input)
        {
            _controller.Simulate(input, Time.deltaTime);
        }

        public void ReceiveAuthoritativeState(EntityState state) { }
    }
}
