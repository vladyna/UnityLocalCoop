using Test.Services;
using Test.Services.Abstractions;
using Zenject;
namespace Test.Zenject.Installers
{
    public class NetworkServices : MonoInstaller
    {
        [Inject] private LobbyManager _lobbyManager;
        public override void InstallBindings()
        {

            var isNetwork = _lobbyManager != null && _lobbyManager.IsInLobby;
            if (isNetwork)
            {
                Container.Bind<IObjectSpawnService>()
                    .To<NetworkObjectSpawnService>()
                    .FromComponentInHierarchy()
                    .AsSingle();

                Container.Bind<IObjectGrabService>()
                    .To<NetworkObjectGrabService>()
                    .FromComponentInHierarchy()
                    .AsSingle();
            }
            else
            {
                Container.Bind<IObjectSpawnService>()
                    .To<LocalObjectSpawnService>()
                    .FromComponentInHierarchy()
                    .AsSingle();

                Container.Bind<IObjectGrabService>()
                    .To<LocalObjectGrabService>()
                    .FromNew()
                    .AsSingle();
            }
        }
    }
}