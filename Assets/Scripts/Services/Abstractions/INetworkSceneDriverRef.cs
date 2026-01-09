namespace Test.Services.Abstractions
{
    using Test.Network.Managers;

    public interface INetworkSceneDriverRef
    {
        NetworkSceneDriver Current { get; }
        void Set(NetworkSceneDriver driver);
        void Clear(NetworkSceneDriver driver);
    }
}