using Unity.Netcode;
using UnityEngine;

namespace Test.Network.Utils
{
    internal static class NetworkBootstrap
    {
        private const string NetworkManagerPrefabResourcePath = "NetworkManager";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (NetworkManager.Singleton != null)
                return;

            var prefab = Resources.Load<GameObject>(NetworkManagerPrefabResourcePath);
            if (prefab == null)
            {
                Debug.LogError($"NetworkBootstrap: missing prefab at Resources/{NetworkManagerPrefabResourcePath}.prefab");
                return;
            }

            var instance = Object.Instantiate(prefab);
            instance.name = prefab.name;

            var nm = instance.GetComponent<NetworkManager>();
            if (nm == null)
            {
                Debug.LogError("NetworkBootstrap: prefab does not contain a NetworkManager component.");
                Object.Destroy(instance);
                return;
            }

            Object.DontDestroyOnLoad(instance);
        }
    }
}
