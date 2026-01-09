using Test.Objects.Models;
using Test.Player.Models;

namespace Test.Objects.Abstractions
{
    public interface IObjectSimulationDriver
    {
        IGrabbableController Controller { get; }
        void Tick(ObjectInput input);
        void ReceiveAuthoritativeState(EntityState state);
    }
}
