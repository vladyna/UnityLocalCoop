using Test.Objects.Abstractions;
using Test.Objects.Models;
using Test.Player.Models;
using UnityEngine;
namespace Test.Objects.Network
{
    public class ObjectNetworkSimulationDriver : IObjectSimulationDriver
    {
        private readonly GrabbableControllerCore _core;
        private readonly ObjectNetworkReplicator _replicator;

        public IGrabbableController Controller => _core;

        public ObjectNetworkSimulationDriver(GrabbableControllerCore core, ObjectNetworkReplicator replicator)
        {
            _core = core;
            _replicator = replicator;
        }

        public void Tick(ObjectInput input)
        {
            input.DeltaTime = Time.deltaTime;
            _core.Simulate(input, input.DeltaTime);
            _replicator?.SendInput(input);
        }

        public void ReceiveAuthoritativeState(EntityState state)
        {
            _core.ApplyState(state);
        }
    }
}
