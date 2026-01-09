using UnityEngine;
using Unity.Netcode;

namespace Test.Player.Models
{
    public struct PlayerInputData : INetworkSerializable
    {
        public Vector2 Move;
        public Vector2 Look;
        public bool Jump;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Move);
            serializer.SerializeValue(ref Look);
            serializer.SerializeValue(ref Jump);
        }
    }
}

