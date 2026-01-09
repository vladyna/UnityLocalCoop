using Test.Zenject.Factories;
using UnityEngine;
using Zenject;
namespace Test.Zenject.Installers
{
    public class PrefabFactoryInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindFactory<GameObject, GameObject, PrefabFactory>().FromFactory<PrefabFactory>();
        }
    }
}