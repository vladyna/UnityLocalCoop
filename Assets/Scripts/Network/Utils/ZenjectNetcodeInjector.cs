using Unity.Netcode;
using Zenject;

namespace Test.Network.Utils
{
    public class ZenjectNetcodeInjector : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            var sceneContext = FindFirstObjectByType<SceneContext>();
            sceneContext.Container.InjectGameObject(gameObject);
        }
    }
}