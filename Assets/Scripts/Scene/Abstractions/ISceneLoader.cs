using Cysharp.Threading.Tasks;
using Test.Scene.Data;
using UnityEngine.SceneManagement;

namespace Test.Scene.Abstractions
{
    public interface ISceneLoader
    {
        UniTask LoadSceneAsync(SceneData scene, LoadSceneMode mode = LoadSceneMode.Single);
    }
}
