using Test.Objects.Abstractions;
using Test.Objects.Models;
using Test.Objects.Network;
using Test.Player.Models;
using UnityEngine;
namespace Test.Objects
{
    public class ObjectRoot : MonoBehaviour
    {
        private IObjectSimulationDriver _driver;
        private GrabbableControllerCore _core;
        private ObjectNetwork _network;
        private Camera _camera;
        private bool _grabPending;
        private Vector3 _pendingTarget;
        private Camera _pendingCamera;

        public bool IsGrabbed => _network != null ? _network.IsGrabbed : false;
        public ulong GrabberClientId => _network != null ? _network.GrabberClientId : 0;

        private void Awake()
        {
            var rb = GetComponent<Rigidbody>();
            _core = new GrabbableControllerCore(rb);

            _network = GetComponent<ObjectNetwork>();
            if (_network == null)
            {
                _driver = new LocalObjectSimulationDriver(_core);
            }
            else
            {
                var replicator = GetComponent<ObjectNetworkReplicator>();
                _driver = new ObjectNetworkSimulationDriver(_core, replicator);
                _network.OnBecameOwner += HandleBecameOwner;
            }
        }

        private void OnDestroy()
        {
            if (_network != null)
                _network.OnBecameOwner -= HandleBecameOwner;
        }

        public void Grab(Vector3 target, Camera camera)
        {
            if (_network == null)
            {
                BeginGrab(target, camera);
            }
            else if (_network.IsOwner)
            {
                BeginGrab(target, camera);
            }
            else
            {
                _grabPending = true;
                _pendingTarget = target;
                _pendingCamera = camera;
            }
        }

        public void Release()
        {
            if (_network != null && !_network.IsOwner)
                return;

            _grabPending = false;
            _pendingCamera = null;
            _camera = null;
            Tick(Vector3.zero, true);
        }

        private void HandleBecameOwner()
        {
            if (!_grabPending)
                return;

            if (_pendingCamera == null)
            {
                _grabPending = false;
                return;
            }

            BeginGrab(_pendingTarget, _pendingCamera);
            _grabPending = false;
            _pendingCamera = null;
        }

        private void BeginGrab(Vector3 target, Camera camera)
        {
            _camera = camera;
            Tick(target, false);
        }

        private void Tick(Vector3 target, bool release)
        {
            var input = new ObjectInput
            {
                TargetPosition = target,
                Release = release,
                Timestamp = Time.time
            };

            _driver.Tick(input);
        }

        private void Update()
        {
            if (_network != null)
            {
                if (!_network.IsGrabbed || !_network.IsOwner)
                    return;
            }

            if (_camera == null)
                return;

            Vector3 target = _camera.transform.position +
                              _camera.transform.forward * 2f;

            Tick(target, false);
        }

        public EntityState Simulate(ObjectInput input, float dt)
        {
            _driver.Controller.Simulate(input, dt);
            return _core.CaptureState();
        }
    }
}
