using Test.Scene.Data;
using Test.Services;
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
            Container.Bind<GameManager>()
                .AsSingle()
                .WithArguments(_mainMenuScene, _gameScene)
                .NonLazy();
        }
    }
}