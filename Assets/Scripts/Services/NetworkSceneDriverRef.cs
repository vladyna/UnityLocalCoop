using Test.Network.Managers;
using Test.Services.Abstractions;

namespace Test.Services
{
    public sealed class NetworkSceneDriverRef : INetworkSceneDriverRef
    {
        public NetworkSceneDriver Current { get; private set; }

        public void Set(NetworkSceneDriver driver)
        {
            if (driver == null)
                return;

            Current = driver;
        }

        public void Clear(NetworkSceneDriver driver)
        {
            if (driver == null)
                return;

            if (ReferenceEquals(Current, driver))
                Current = null;
        }
    }
}