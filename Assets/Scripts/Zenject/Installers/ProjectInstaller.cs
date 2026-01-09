using Test.Network;
using Test.Scene;
using Test.Scene.Abstractions;
using Test.Services;
using Test.Services.Abstractions;
using Test.UI;
using Zenject;

namespace Test.Zenject.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<INetworkSceneDriverRef>()
                .To<NetworkSceneDriverRef>()
                .AsSingle()
                .NonLazy();

            Container.Bind<LanIpService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<PingProvider>()
                .AsSingle()
                .NonLazy();

            Container.Bind<LobbyManager>()
                .AsSingle()
                .NonLazy();

            Container.Bind<LoadingUI>()
                .FromComponentInHierarchy()
                .AsSingle()
                .NonLazy();

            Container.Bind<ISceneLoader>()
                .To<SceneLoader>()
                .AsSingle()
                .NonLazy();

            Container.Bind<PlayerInputService>()
                .AsSingle()
                .NonLazy();
        }
    }
}