using Test.Player.Models;

namespace Test.Player.Abstractions
{
    public interface INetworkReplicator
    {
        void Initialize(ICharacterController controller);
        void SendInput(MovementInput input);
    }
}

