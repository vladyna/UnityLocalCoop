using Unity.Netcode;

namespace Test.Player.Models
{
    public struct GrabRequest : INetworkSerializable
    {
        public ulong ObjectId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            serializer.SerializeValue(ref ObjectId);
        }
    }
}