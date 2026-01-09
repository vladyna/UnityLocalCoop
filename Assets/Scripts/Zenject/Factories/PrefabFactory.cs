using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Test.Zenject.Factories
{
    public class PrefabFactory : PlaceholderFactory<GameObject, GameObject>
    {
        private readonly DiContainer _container;

        public PrefabFactory(DiContainer container)
        {
            _container = container;
        }

        public override GameObject Create(GameObject prefab)
        {
            return _container.InstantiatePrefab(prefab, Vector3.zero, Quaternion.identity, null);
        }

        public GameObject CreateNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var go = Object.Instantiate(prefab, position, rotation, null);

            _container.InjectGameObject(go);

            if (go.GetComponent<NetworkObject>() == null)
            {
                Debug.LogWarning($"PrefabFactory.CreateNetworkObject: '{prefab.name}' has no NetworkObject component.", go);
            }

            return go;
        }
    }
}
