using System;

namespace Test.Network.Abstractions
{
    public interface INetworkSceneFlow
    {
        event Action<int> OnClientReady;
        event Action OnAllClientsReady;

        void CommandClientsLoadGame();
    }
}