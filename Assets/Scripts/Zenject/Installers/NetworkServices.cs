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

            if (_lobbyManager != null && _lobbyManager.IsInLobby)
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