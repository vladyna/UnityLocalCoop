using UnityEngine;
using Unity.Netcode;

namespace Test.Player.Models
{
    public struct MovementInput : INetworkSerializable
    {
        public int Sequence;
        public Vector2 Move;
        public Vector2 Look;
        public float DeltaTime;
        public bool Jump;
        public float Timestamp;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            serializer.SerializeValue(ref Sequence);
            serializer.SerializeValue(ref Move);
            serializer.SerializeValue(ref Look);
            serializer.SerializeValue(ref DeltaTime);
            serializer.SerializeValue(ref Jump);
            serializer.SerializeValue(ref Timestamp);
        }
    }
}
