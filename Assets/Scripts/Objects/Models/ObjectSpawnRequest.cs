using UnityEngine;
using Unity.Netcode;

namespace Test.Objects.Models
{
    public struct ObjectSpawnRequest : INetworkSerializable
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);
        }
    }
}
