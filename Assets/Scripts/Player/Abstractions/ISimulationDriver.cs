using Test.Player.Models;

namespace Test.Player.Abstractions
{
    public interface ISimulationDriver
    {
        void Tick(MovementInput input);
        void ReceiveAuthoritativeState(EntityState state);
    }
}
