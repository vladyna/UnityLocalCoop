using Test.Scene.Data;
using Test.Services;
using Test.Services.Abstractions;
using UnityEngine;
using Zenject;

namespace Test.Zenject.Installers
{
    public class GameManagerInstaller : MonoInstaller
    {
        [SerializeField] private SceneData _mainMenuScene;
        [SerializeField] private SceneData _gameScene;

        public override void InstallBindings()
        {
            var gameManager = new GameManager(
                Container.Resolve<Test.Scene.Abstractions.ISceneLoader>(),
                Container.Resolve<LobbyManager>(),
                Container.Resolve<INetworkSceneDriverRef>(),
                _mainMenuScene,
                _gameScene);

            Container.Bind<GameManager>()
                .FromInstance(gameManager)
                .AsSingle()
                .NonLazy();
        }
    }
}