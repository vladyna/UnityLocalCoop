using Test.Objects.Models;
using Test.Player.Models;

namespace Test.Objects.Abstractions
{
    public interface IGrabbableController
    {
        void Simulate(ObjectInput input, float deltaTime);
        EntityState CaptureState();
        void ApplyState(EntityState state);
        void SetKinematic(bool value);
    }
}
