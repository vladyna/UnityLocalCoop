using Cysharp.Threading.Tasks;
using Test.Scene.Abstractions;
using Test.Scene.Data;
using Test.Services.Abstractions;
using Test.Services.Enums;
using Unity.Netcode;

namespace Test.Services
{
    public class GameManager
    {
        public GameState State { get; private set; } = GameState.MainMenu;

        private readonly ISceneLoader _sceneLoader;
        private readonly LobbyManager _lobbyManager;
        private readonly INetworkSceneDriverRef _driverRef;
        private readonly SceneData _mainMenuScene;
        private readonly SceneData _gameScene;

        public GameManager(
            ISceneLoader sceneLoader,
            LobbyManager lobbyManager,
            INetworkSceneDriverRef driverRef,
            SceneData mainMenuScene,
            SceneData gameScene)
        {
            _sceneLoader = sceneLoader;
            _lobbyManager = lobbyManager;
            _driverRef = driverRef;
            _mainMenuScene = mainMenuScene;
            _gameScene = gameScene;
        }

        public void RequestStartGame()
        {
            if (State != GameState.MainMenu)
                return;

            var nm = NetworkManager.Singleton;
            if (_lobbyManager.IsInLobby)
            {
                if (nm == null || !nm.IsHost)
                    return;

                // Tell non-host clients to show loading UI and start loading.
                _driverRef?.Current?.CommandClientsLoadGame();
            }

            SetState(GameState.Loading);
            LoadGameLocal().Forget();
        }

        public async UniTask LoadGameLocal()
        {
            await _sceneLoader.LoadSceneAsync(_gameScene);
            SetState(GameState.Game);
        }

        public void ForceLoadGame()
        {
            SetState(GameState.Loading);
            ForceLoadGameAsync().Forget();
        }

        private async UniTask ForceLoadGameAsync()
        {
            await _sceneLoader.LoadSceneAsync(_gameScene);
            SetState(GameState.Game);
        }

        public void RequestReturnToMainMenu()
        {
            if (State != GameState.Game)
                return;

            SetState(GameState.Loading);
            ReturnToMainMenu().Forget();
        }

        public void ForceReturnToMainMenu()
        {
            SetState(GameState.Loading);
            ReturnToMainMenu().Forget();
        }

        private async UniTask ReturnToMainMenu()
        {
            await _sceneLoader.LoadSceneAsync(_mainMenuScene);
            SetState(GameState.MainMenu);
        }

        private void SetState(GameState state)
        {
            State = state;
        }
    }
}