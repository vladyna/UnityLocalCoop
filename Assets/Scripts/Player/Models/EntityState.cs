using Unity.Netcode;
using UnityEngine;

namespace Test.Player.Models
{
    public struct EntityState : INetworkSerializable
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float Timestamp;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
    where T : IReaderWriter
        {
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Velocity);
            serializer.SerializeValue(ref Timestamp);
        }
    }
}

