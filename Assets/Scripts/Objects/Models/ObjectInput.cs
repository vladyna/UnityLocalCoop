using Unity.Netcode;
using UnityEngine;

namespace Test.Objects.Models
{
    public struct ObjectInput : INetworkSerializable
    {
        public int Sequence;
        public Vector3 TargetPosition;
        public float DeltaTime;
        public bool Release;
        public float Timestamp;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            serializer.SerializeValue(ref Sequence);
            serializer.SerializeValue(ref TargetPosition);
            serializer.SerializeValue(ref DeltaTime);
            serializer.SerializeValue(ref Release);
            serializer.SerializeValue(ref Timestamp);
        }
    }
}
