using Test.Services.Abstractions;
using Test.Zenject.Factories;
using UnityEngine;
using Zenject;
namespace Test.Services
{
    public class LocalObjectSpawnService : MonoBehaviour, IObjectSpawnService
    {
        [SerializeField] private GameObject _objectPrefab;

        [Inject] private PrefabFactory _prefabFactory;

        public void SpawnObject(Vector3 position, Quaternion rotation)
        {
            var obj = _prefabFactory.Create(_objectPrefab);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
    }
}
