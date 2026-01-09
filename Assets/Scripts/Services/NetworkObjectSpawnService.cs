using Test.Objects.Models;
using Test.Services.Abstractions;
using Test.Zenject.Factories;
using Unity.Netcode;
using UnityEngine;
using Zenject;
namespace Test.Services
{
    public class NetworkObjectSpawnService : NetworkBehaviour, IObjectSpawnService
    {
        [SerializeField] private GameObject _objectPrefab;

        [Inject] private PrefabFactory _prefabFactory;

        public void SpawnObject(Vector3 position, Quaternion rotation)
        {
            RequestSpawn(position, rotation);
        }

        private void RequestSpawn(Vector3 pos, Quaternion rot)
        {
            if (IsServer)
                Spawn(pos, rot);
            else
                RequestSpawnServerRpc(new ObjectSpawnRequest
                {
                    Position = pos,
                    Rotation = rot
                });
        }



        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestSpawnServerRpc(ObjectSpawnRequest request)
        {
            Spawn(request.Position, request.Rotation);
        }

        private void Spawn(Vector3 pos, Quaternion rot)
        {
            var obj = _prefabFactory.CreateNetworkObject(_objectPrefab, pos, rot);
            obj.GetComponent<NetworkObject>().Spawn();
        }
    }
}
