using Test.Player.Models;

namespace Test.Player.Abstractions
{
    public interface ICharacterController
    {
        void Simulate(MovementInput input, float deltaTime);
        EntityState CaptureState();
        void ApplyState(EntityState state);
    }
}
