using Unity.Netcode;
using UnityEngine;

namespace Test.Player.Network
{
    public class PlayerNetwork : NetworkBehaviour
    {
        public string PlayerName { get; private set; }

        [SerializeField] private Renderer _targetRenderer;
        [SerializeField] private Camera _camera;

        private readonly NetworkVariable<Color32> _color = new NetworkVariable<Color32>(
            new Color32(255, 255, 255, 255),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<Vector3> _netPosition = new NetworkVariable<Vector3>(
            Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<Vector3> _netRotationEuler = new NetworkVariable<Vector3>(
            Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int> _lastProcessedSequence = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public event System.Action<int> OnServerProcessedSequence;

        private Vector3 _targetPos;
        private Quaternion _targetRot;
        private const float InterpSpeed = 10f;

        private CharacterController _charController;

        private Material _cachedMaterial;

        private const float OwnerLargeThreshold = 25.0f;
        private const float OwnerMediumThreshold = 0.5f;
        private const float OwnerCorrectionSpeed = 12f;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (_targetRenderer == null)
                _targetRenderer = GetComponentInChildren<Renderer>();

            if (_targetRenderer != null)
                _cachedMaterial = _targetRenderer.material;

            _color.OnValueChanged += OnColorChanged;
            ApplyColor(_color.Value);

            _netPosition.OnValueChanged += OnNetPositionChanged;
            _netRotationEuler.OnValueChanged += OnNetRotationChanged;
            _lastProcessedSequence.OnValueChanged += (prev, cur) => OnServerProcessedSequence?.Invoke(cur);

            _targetPos = transform.position;
            _targetRot = transform.rotation;

            _charController = GetComponent<CharacterController>();
            ApplyOwnershipState();
            if (IsOwner)
                PlayerName = $"Player_{OwnerClientId}";

        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _color.OnValueChanged -= OnColorChanged;
            _netPosition.OnValueChanged -= OnNetPositionChanged;
            _netRotationEuler.OnValueChanged -= OnNetRotationChanged;
        }

        private void ApplyOwnershipState()
        {
            if (_camera != null)
                _camera.enabled = IsOwner;
        }

        public void CycleColor()
        {
            if (!IsOwner)
                return;

            var c = _color.Value;
            Color32 next;

            if (c.r == 255 && c.g == 255 && c.b == 255) next = new Color32(255, 80, 80, 255);
            else if (c.r == 255 && c.g == 80 && c.b == 80) next = new Color32(80, 255, 80, 255);
            else if (c.r == 80 && c.g == 255 && c.b == 80) next = new Color32(80, 80, 255, 255);
            else next = new Color32(255, 255, 255, 255);

            _color.Value = next;
        }

        private void OnNetPositionChanged(Vector3 previous, Vector3 current)
        {
            _targetPos = current;
        }

        private void OnNetRotationChanged(Vector3 previous, Vector3 currentEuler)
        {
            _targetRot = Quaternion.Euler(currentEuler);
        }

        private void Update()
        {
            if (IsOwner)
            {
                var currentPos = _charController != null ? _charController.transform.position : transform.position;
                var delta = _targetPos - currentPos;
                var horizDelta = new Vector3(delta.x, 0f, delta.z);
                var horizDist = horizDelta.magnitude;
                var vertDist = Mathf.Abs(delta.y);

                if (horizDist > OwnerLargeThreshold || vertDist > OwnerLargeThreshold)
                {
                    if (_charController != null)
                    {
                        _charController.enabled = false;
                        _charController.transform.position = _targetPos;
                        _charController.enabled = true;
                    }
                    else
                    {
                        transform.position = _targetPos;
                    }

                    transform.rotation = Quaternion.Euler(_targetRot.eulerAngles);
                }
                else if (horizDist > OwnerMediumThreshold || vertDist > OwnerMediumThreshold)
                {
                    var t = Mathf.Clamp01(Time.deltaTime * OwnerCorrectionSpeed);
                    var correctedPos = Vector3.Lerp(currentPos, _targetPos, t);
                    if (_charController != null)
                    {
                        _charController.enabled = false;
                        _charController.transform.position = correctedPos;
                        _charController.enabled = true;
                    }
                    else
                    {
                        transform.position = correctedPos;
                    }

                    transform.rotation = Quaternion.Slerp(transform.rotation, _targetRot, t);
                }
                return;
            }

            transform.position = Vector3.Lerp(transform.position, _targetPos, Mathf.Clamp01(Time.deltaTime * InterpSpeed));
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRot, Mathf.Clamp01(Time.deltaTime * InterpSpeed));
        }

        public void ApplyServerTransform(Vector3 pos, Vector3 rotEuler, int lastProcessedSequence = 0)
        {
            if (!IsServer) return;

            _netPosition.Value = pos;
            _netRotationEuler.Value = rotEuler;
            _lastProcessedSequence.Value = lastProcessedSequence;

            transform.position = pos;
            transform.rotation = Quaternion.Euler(rotEuler);
            _targetPos = pos;
            _targetRot = Quaternion.Euler(rotEuler);
        }

        public Vector3 GetAuthoritativePosition() => _netPosition.Value;
        public Vector3 GetAuthoritativeRotationEuler() => _netRotationEuler.Value;

        private void OnColorChanged(Color32 previousValue, Color32 newValue) => ApplyColor(newValue);

        private void ApplyColor(Color32 c)
        {
            if (_targetRenderer == null)
                return;

            if (_cachedMaterial != null)
                _cachedMaterial.color = c;
        }
    }
}
