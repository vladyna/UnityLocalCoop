using System.Collections.Generic;
using Test.Player.Abstractions;
using Test.Player.Models;
using Unity.Netcode;
using UnityEngine;
namespace Test.Player.Network
{
    public class PlayerNetworkReplicator : NetworkBehaviour, INetworkReplicator
    {
        private ICharacterController _controller;
        private CharacterController _characterController;
        private PlayerNetwork _playerNetwork;
        private Vector2 _accumulatedLook;
        private int _nextSequence;
        private float _sendTimer;
        private const float SendInterval = 0.01f;
        private readonly List<MovementInput> _pendingInputs = new();
        public void Initialize(ICharacterController controller)
        {
            _controller = controller;
            _playerNetwork = GetComponent<PlayerNetwork>();
            var cc = GetComponent<CharacterController>();
            _characterController = cc;
        }

        public void SendInput(MovementInput input)
        {
            _accumulatedLook += input.Look;
            var grounded = _characterController != null ? _characterController.isGrounded : false;

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
            {
                _sendTimer += Time.deltaTime;
                if (_sendTimer >= SendInterval)
                {
                    _sendTimer = 0f;
                    var sendInput = new MovementInput
                    {
                        Sequence = _nextSequence++,
                        Move = input.Move,
                        Look = _accumulatedLook,
                        DeltaTime = Time.deltaTime,
                        Jump = input.Jump,
                        Timestamp = Time.time
                    };
                    _pendingInputs.Add(sendInput);
                    SubmitMovementServerRpc(sendInput);
                    _accumulatedLook = Vector2.zero;
                }
            }
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                if (NetworkManager.Singleton != null)
                {
                    if (_playerNetwork == null)
                        _playerNetwork = GetComponent<PlayerNetwork>();

                    var pos = transform.position;
                    var rot = transform.rotation.eulerAngles;
                    _playerNetwork?.ApplyServerTransform(pos, rot);
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner && _playerNetwork != null)
            {
                _playerNetwork.OnServerProcessedSequence += OnServerProcessedSequence;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner && _playerNetwork != null)
            {
                _playerNetwork.OnServerProcessedSequence -= OnServerProcessedSequence;
            }
        }

        private void OnServerProcessedSequence(int seq)
        {
            if (!IsOwner)
                return;

            _pendingInputs.RemoveAll(i => i.Sequence <= seq);

            var authPos = _playerNetwork.GetAuthoritativePosition();
            var authRot = _playerNetwork.GetAuthoritativeRotationEuler();

            var state = new EntityState { Position = authPos, Velocity = Vector3.zero, Timestamp = Time.time };
            _controller.ApplyState(state);

            foreach (var pending in _pendingInputs)
            {
                _controller.Simulate(pending, pending.DeltaTime > 0f ? pending.DeltaTime : SendInterval);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void SubmitMovementServerRpc(MovementInput input, RpcParams rpcParams = default)
        {
            if (!IsServer) return;

            var clientId = rpcParams.Receive.SenderClientId;

            if (clientId == NetworkManager.ServerClientId)
            {
                return;
            }

            if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var clientData))
                return;

            var playerObj = clientData.PlayerObject;
            if (playerObj == null)
                return;

            var playerNetwork = playerObj.GetComponentInChildren<PlayerNetwork>();
            if (playerNetwork == null)
                return;

            var controller = playerObj.GetComponentInChildren<PlayerRoot>();
            if (controller == null)
                return;
            var go = controller.gameObject;

            var simDt = input.DeltaTime > 0f ? input.DeltaTime : SendInterval;
            var state = controller.SimulateMovement(input, simDt);
            var pos = state.Position;
            var rotEuler = controller.transform.rotation.eulerAngles;
            playerNetwork.ApplyServerTransform(pos, rotEuler, input.Sequence);
        }
    }
}