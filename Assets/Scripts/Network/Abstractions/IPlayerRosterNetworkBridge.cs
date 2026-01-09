namespace Test.Network.Abstractions
{
    public interface IPlayerRosterNetworkBridge
    {
        void Kick(ulong targetClientId);
        void BroadcastRoster();
    }
}