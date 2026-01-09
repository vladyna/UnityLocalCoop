using System.Collections.Generic;
using Test.Objects.Models;
using Unity.Netcode;
using UnityEngine;

namespace Test.Objects.Network
{
    [RequireComponent(typeof(ObjectRoot))]
    [RequireComponent(typeof(ObjectNetwork))]
    public class ObjectNetworkReplicator : NetworkBehaviour
    {
        private ObjectRoot _root;
        private ObjectNetwork _net;
        private Rigidbody _rb;

        private int _nextSequence;
        private float _sendTimer;
        private const float SendInterval = 0.01f;
        private readonly List<ObjectInput> _pendingInputs = new List<ObjectInput>(64);

        private void Awake()
        {
            _root = GetComponent<ObjectRoot>();
            _net = GetComponent<ObjectNetwork>();
            _rb = GetComponent<Rigidbody>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner && _net != null)
                _net.OnServerProcessedSequence += OnServerProcessedSequence;
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner && _net != null)
                _net.OnServerProcessedSequence -= OnServerProcessedSequence;
            base.OnNetworkDespawn();
        }

        public void SendInput(ObjectInput input)
        {
            if (!IsSpawned)
                return;

            if (!IsClient || _net == null || _root == null || !_net.IsOwner)
                return;


            if (IsServer)
            {
                input.Sequence = _nextSequence++;
                input.DeltaTime = Time.deltaTime;
                input.Timestamp = Time.time;

                _root.Simulate(input, input.DeltaTime);

                var pos = _root.transform.position;
                var rot = _root.transform.rotation.eulerAngles;
                _net.ApplyServerTransform(pos, rot, input.Sequence);
                return;
            }

            var dt = Time.deltaTime;
            _sendTimer += dt;
            if (_sendTimer < SendInterval)
                return;

            _sendTimer = 0f;

            input.Sequence = _nextSequence++;
            input.DeltaTime = dt;
            input.Timestamp = Time.time;

            _pendingInputs.Add(input);
            SubmitObjectInputServerRpc(input);
        }

        private void OnServerProcessedSequence(int seq)
        {
            if (!IsOwner || _net == null)
                return;

            if (IsServer)
                return;

            _pendingInputs.RemoveAll(i => i.Sequence <= seq);

            var authPos = _net.AuthoritativePosition;
            var authRot = Quaternion.Euler(_net.AuthoritativeRotationEuler);

            if (_rb != null)
            {
                _rb.position = authPos;
                _rb.rotation = authRot;
            }
            else
            {
                transform.position = authPos;
                transform.rotation = authRot;
            }

            foreach (var pending in _pendingInputs)
            {
                var dt = pending.DeltaTime > 0f ? pending.DeltaTime : SendInterval;
                _root.Simulate(pending, dt);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void SubmitObjectInputServerRpc(ObjectInput input, RpcParams rpcParams = default)
        {
            if (!IsServer)
                return;

            var sender = rpcParams.Receive.SenderClientId;
            if (_net != null && _net.OwnerClientId != sender)
                return;

            var dt = input.DeltaTime > 0f ? input.DeltaTime : SendInterval;
            _root.Simulate(input, dt);

            var pos = _root.transform.position;
            var rot = _root.transform.rotation.eulerAngles;

            _net.ApplyServerTransform(pos, rot, input.Sequence);
        }
    }
}
