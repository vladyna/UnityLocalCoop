namespace Test.Network.Abstractions
{
    public interface INetworkSessionLifecycle
    {
        void EndSessionAsHost(string reason = "Host left");
        void Kick(ulong targetClientId);
        void LeaveOnApplicationQuit();
    }
}