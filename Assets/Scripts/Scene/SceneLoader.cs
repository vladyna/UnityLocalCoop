using Cysharp.Threading.Tasks;
using Test.Scene.Abstractions;
using Test.Scene.Data;
using Test.Services.Abstractions;
using Test.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Test.Scene
{
    public class SceneLoader : ISceneLoader
    {
        private readonly INetworkSceneDriverRef _driverRef;
        private readonly LoadingUI _loadingUI;

        public SceneLoader(INetworkSceneDriverRef driverRef, LoadingUI loadingUI)
        {
            _driverRef = driverRef;
            _loadingUI = loadingUI;
        }

        public async UniTask LoadSceneAsync(SceneData scene, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (scene == null || string.IsNullOrWhiteSpace(scene.SceneName))
            {
                return;
            }

            _loadingUI.Show();
            await UniTask.DelayFrame(1);

            var sceneName = scene.SceneName;
            var nm = NetworkManager.Singleton;

            if (nm != null && nm.IsServer && nm.SceneManager != null)
            {
                nm.SceneManager.LoadScene(sceneName, mode);
                return;
            }

            bool loaded = false;

            UnityEngine.Events.UnityAction<UnityEngine.SceneManagement.Scene, LoadSceneMode> handler = null;
            handler = (s, lm) =>
            {
                if (s.name != sceneName)
                    return;

                SceneManager.sceneLoaded -= handler;
                loaded = true;
            };

            SceneManager.sceneLoaded += handler;

            var op = SceneManager.LoadSceneAsync(sceneName, mode);
            if (op != null)
            {
                while (!op.isDone)
                {
                    _loadingUI.SetProgress(op.progress);
                    await UniTask.Yield();
                }
            }

            while (!loaded)
            {
                await UniTask.Yield();
            }

            var isNetworkSession = nm != null && (nm.IsClient || nm.IsServer);
            if (!isNetworkSession)
            {
                _loadingUI.Hide();
            }
        }
    }
}
