using Unity.Netcode;
using Zenject;

namespace Test.Network.Utils
{
    public class ZenjectNetcodeInjector : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            var sceneContext = FindFirstObjectByType<SceneContext>();
            if (sceneContext != null)
            {
                sceneContext.Container.InjectGameObject(gameObject);
                return;
            }

            if (ProjectContext.HasInstance)
            {
                ProjectContext.Instance.Container.InjectGameObject(gameObject);
            }
        }
    }
}