using UnityEngine;

namespace Test.Services.Abstractions
{
    public interface IObjectSpawnService
    {
        void SpawnObject(Vector3 position, Quaternion rotation);
    }
}

