using Unity.Netcode;
using UnityEngine;

namespace Test.Objects.Network
{
    public class ObjectNetwork : NetworkBehaviour
    {
        private readonly NetworkVariable<bool> _isGrabbed = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<ulong> _grabberClientId = new NetworkVariable<ulong>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<Vector3> _netPosition = new NetworkVariable<Vector3>(
            Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<Vector3> _netRotationEuler = new NetworkVariable<Vector3>(
            Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<Vector3> _netVelocity = new NetworkVariable<Vector3>(
            Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<Vector3> _netAngularVelocity = new NetworkVariable<Vector3>(
            Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int> _lastProcessedSequence = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public bool IsGrabbed => _isGrabbed.Value;
        public ulong GrabberClientId => _grabberClientId.Value;

        public event System.Action<int> OnServerProcessedSequence;
        public event System.Action OnBecameOwner;

        public Vector3 AuthoritativePosition => _netPosition.Value;
        public Vector3 AuthoritativeRotationEuler => _netRotationEuler.Value;
        public Vector3 AuthoritativeVelocity => _netVelocity.Value;
        public Vector3 AuthoritativeAngularVelocity => _netAngularVelocity.Value;

        private Vector3 _targetPos;
        private Quaternion _targetRot;
        private Vector3 _targetVel;
        private Vector3 _targetAngVel;
        private const float InterpSpeed = 15f;
        private Rigidbody _rb;

        private float _serverSyncTimer;
        private const float PhysicsSyncInterval = 0.05f;
        private const float PhysicsSyncIntervalSleeping = 0.25f;

        private void OnNetPositionChanged(Vector3 previous, Vector3 current)
        {
            _targetPos = current;
        }

        private void OnNetRotationChanged(Vector3 previous, Vector3 currentEuler)
        {
            _targetRot = Quaternion.Euler(currentEuler);
        }

        private void OnNetVelocityChanged(Vector3 previous, Vector3 current)
        {
            _targetVel = current;
        }

        private void OnNetAngularVelocityChanged(Vector3 previous, Vector3 current)
        {
            _targetAngVel = current;
        }

        private void OnGrabbedChanged(bool previous, bool current)
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();

            if (_rb == null)
                return;

            _rb.isKinematic = current;

            if (current)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _rb = GetComponent<Rigidbody>();

            _lastProcessedSequence.OnValueChanged += (prev, cur) => OnServerProcessedSequence?.Invoke(cur);

            _isGrabbed.OnValueChanged += OnGrabbedChanged;

            _netPosition.OnValueChanged += OnNetPositionChanged;
            _netRotationEuler.OnValueChanged += OnNetRotationChanged;
            _netVelocity.OnValueChanged += OnNetVelocityChanged;
            _netAngularVelocity.OnValueChanged += OnNetAngularVelocityChanged;

            OnGrabbedChanged(false, IsGrabbed);

            _targetPos = _netPosition.Value != Vector3.zero ? _netPosition.Value : transform.position;
            _targetRot = _netRotationEuler.Value != Vector3.zero ? Quaternion.Euler(_netRotationEuler.Value) : transform.rotation;
        }

        public override void OnNetworkDespawn()
        {
            _isGrabbed.OnValueChanged -=  OnGrabbedChanged;

            _netPosition.OnValueChanged -= OnNetPositionChanged;
            _netRotationEuler.OnValueChanged -= OnNetRotationChanged;
            _netVelocity.OnValueChanged -= OnNetVelocityChanged;
            _netAngularVelocity.OnValueChanged -= OnNetAngularVelocityChanged;

            base.OnNetworkDespawn();
        }

        protected override void OnOwnershipChanged(ulong previousOwner, ulong newOwner)
        {
            base.OnOwnershipChanged(previousOwner, newOwner);
            if (IsOwner)
                OnBecameOwner?.Invoke();
        }

        public void ApplyServerTransform(Vector3 pos, Vector3 rotEuler, int lastProcessedSequence = 0)
        {
            if (!IsServer)
                return;

            _netPosition.Value = pos;
            _netRotationEuler.Value = rotEuler;
            _lastProcessedSequence.Value = lastProcessedSequence;

            transform.position = pos;
            transform.rotation = Quaternion.Euler(rotEuler);
            _targetPos = pos;
            _targetRot = Quaternion.Euler(rotEuler);
        }

        private void Update()
        {
            if (!IsSpawned)
                return;

            if (IsServer && _rb != null && !IsGrabbed)
            {
                var interval = _rb.IsSleeping() ? PhysicsSyncIntervalSleeping : PhysicsSyncInterval;
                _serverSyncTimer += Time.deltaTime;
                if (_serverSyncTimer >= interval)
                {
                    _serverSyncTimer = 0f;

                    _netPosition.Value = _rb.position;
                    _netRotationEuler.Value = _rb.rotation.eulerAngles;
                    _netVelocity.Value = _rb.linearVelocity;
                    _netAngularVelocity.Value = _rb.angularVelocity;
                }
            }

            if (IsOwner)
                return;

            var t = Mathf.Clamp01(Time.deltaTime * InterpSpeed);

            if (_rb != null && !_rb.isKinematic)
            {
                _rb.MovePosition(Vector3.Lerp(_rb.position, _targetPos, t));
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, _targetRot, t));

                if (!IsGrabbed)
                {
                    _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, _targetVel, t);
                    _rb.angularVelocity = Vector3.Lerp(_rb.angularVelocity, _targetAngVel, t);
                }
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, _targetPos, t);
                transform.rotation = Quaternion.Slerp(transform.rotation, _targetRot, t);
            }
        }

        [Rpc(SendTo.Server)]
        public void SetGrabbedServerRpc(bool grabbed, ulong grabberClientId, RpcParams rpcParams = default)
        {
            if (!IsServer)
                return;

            var previous = _isGrabbed.Value;

            _isGrabbed.Value = grabbed;
            _grabberClientId.Value = grabbed ? grabberClientId : 0;
            OnGrabbedChanged(previous, grabbed);
        }
    }
}
