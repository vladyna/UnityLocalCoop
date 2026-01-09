using System.Collections.Generic;
using Test.Objects;
using Test.Objects.Network;
using Test.Player.Models;
using Test.Services.Abstractions;
using Unity.Netcode;
using UnityEngine;
namespace Test.Services
{
    public class NetworkObjectGrabService : NetworkBehaviour, IObjectGrabService
    {
        private readonly Dictionary<ulong, ulong> _grabberByObjectId = new Dictionary<ulong, ulong>();
        private ulong _grabbedObjectId;
        private ObjectRoot _grabbedObject;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer && NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

            base.OnNetworkDespawn();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer || NetworkManager.Singleton == null)
                return;

            var toRelease = new List<ulong>();
            foreach (var kvp in _grabberByObjectId)
            {
                if (kvp.Value == clientId)
                    toRelease.Add(kvp.Key);
            }

            foreach (var objectId in toRelease)
            {
                if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var netObj))
                {
                    _grabberByObjectId.Remove(objectId);
                    continue;
                }

                _grabberByObjectId[objectId] = 0;
                netObj.ChangeOwnership(NetworkManager.ServerClientId);

                var grabbableNet = netObj.GetComponent<ObjectNetwork>();
                if (grabbableNet != null)
                    grabbableNet.SetGrabbedServerRpc(false, 0);
            }
        }


        public void TryGrabObject(Vector3 origin, Vector3 direction, Camera camera, float maxDistance)
        {
            if (_grabbedObject != null)
                return;
            if (Physics.Raycast(origin, direction, out var hit, 3f))
            {   
                var netObj = hit.collider.GetComponentInParent<NetworkObject>();
                if (netObj != null)
                {
                    var grabbable = netObj.GetComponent<ObjectRoot>();
                    if (grabbable == null || grabbable.IsGrabbed)
                        return;

                    _grabbedObjectId = netObj.NetworkObjectId;
                    _grabbedObject = grabbable;
                            
                    RequestGrabServerRpc(new GrabRequest { ObjectId = netObj.NetworkObjectId });
                    _grabbedObject.Grab(hit.point, camera);
                }
            }
        }

        public void ReleaseGrabbedObject()
        {
            if (_grabbedObject != null)
            {
                _grabbedObject.Release();
                _grabbedObject = null;

                RequestReleaseServerRpc(new GrabRequest { ObjectId = _grabbedObjectId });
            }
        }

        public void ThrowGrabbedObject(Vector3 throwForce)
        {
            if (_grabbedObject != null)
            {
                RequestThrowServerRpc(new GrabRequest { ObjectId = _grabbedObjectId }, throwForce);
                _grabbedObject = null;
            }
        }


        public void DeleteGrabbedObject()
        {
            if (_grabbedObject != null)
            {
                RequestDeleteServerRpc(new GrabRequest { ObjectId = _grabbedObjectId });
                _grabbedObject = null;
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void RequestGrabServerRpc(GrabRequest req, RpcParams p = default)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(req.ObjectId, out var netObj))
                return;

            var grabbable = netObj.GetComponent<ObjectRoot>();
            if (grabbable == null)
                return;

            var grabbableNet = netObj.GetComponent<ObjectNetwork>();
            if (grabbableNet == null)
                return;

            var requesterClientId = p.Receive.SenderClientId;

            if (_grabberByObjectId.TryGetValue(req.ObjectId, out var currentGrabber) && currentGrabber != 0)
                return;

            if (netObj.OwnerClientId != NetworkManager.ServerClientId)
                return;

            _grabberByObjectId[req.ObjectId] = requesterClientId;
            netObj.ChangeOwnership(requesterClientId);
            grabbableNet.SetGrabbedServerRpc(true, requesterClientId);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void RequestReleaseServerRpc(GrabRequest req, RpcParams p = default)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(req.ObjectId, out var netObj))
                return;

            var grabbable = netObj.GetComponent<ObjectRoot>();
            if (grabbable == null)
                return;

            var grabbableNet = netObj.GetComponent<ObjectNetwork>();
            if (grabbableNet == null)
                return;

            var requesterClientId = p.Receive.SenderClientId;

            if (_grabberByObjectId.TryGetValue(req.ObjectId, out var currentGrabber) && currentGrabber != requesterClientId)
                return;

            _grabberByObjectId[req.ObjectId] = 0;
            netObj.ChangeOwnership(NetworkManager.ServerClientId);
            grabbableNet.SetGrabbedServerRpc(false, 0);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void RequestThrowServerRpc(GrabRequest req, Vector3 forward, RpcParams p = default)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(req.ObjectId, out var netObj))
                return;

            var grabbable = netObj.GetComponent<ObjectRoot>();
            if (grabbable == null)
                return;

            var grabbableNet = netObj.GetComponent<ObjectNetwork>();
            if (grabbableNet == null)
                return;

            var requesterClientId = p.Receive.SenderClientId;
            if (!_grabberByObjectId.TryGetValue(req.ObjectId, out var currentGrabber) || currentGrabber != requesterClientId)
                return;

            _grabberByObjectId[req.ObjectId] = 0;
            netObj.ChangeOwnership(NetworkManager.ServerClientId);
            grabbableNet.SetGrabbedServerRpc(false, 0);

            if (netObj.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = false;
                rb.AddForce(forward.normalized * 8f, ForceMode.VelocityChange);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void RequestDeleteServerRpc(GrabRequest req, RpcParams p = default)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(req.ObjectId, out var netObj))
                return;

            var grabbable = netObj.GetComponent<ObjectRoot>();
            if (grabbable == null)
                return;

            var grabbableNet = netObj.GetComponent<ObjectNetwork>();
            if (grabbableNet == null)
                return;

            var requesterClientId = p.Receive.SenderClientId;
            if (!_grabberByObjectId.TryGetValue(req.ObjectId, out var currentGrabber) || currentGrabber != requesterClientId)
                return;

            _grabberByObjectId.Remove(req.ObjectId);
            grabbableNet.SetGrabbedServerRpc(false, 0);
            netObj.Despawn(true);
        }

    }
}
