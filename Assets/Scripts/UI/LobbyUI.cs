using Test.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Test.UI
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _connectButton;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private TMP_InputField _ipInput;
        [SerializeField] private TMP_InputField _portInput;
        [SerializeField] private TextMeshProUGUI _connectionStatus;

        [Inject] private GameManager _gameManager;
        [Inject] private LobbyManager _lobbyManager;


        private void Start()
        {
            if (_lobbyManager == null || _gameManager == null)
                return;

            if (_ipInput != null)
            {
                _ipInput.text = _lobbyManager.LastKnownHostLanIp;
            }

            if (_hostButton != null)
                _hostButton.onClick.AddListener(OnHostClicked);

            if (_connectButton != null)
                _connectButton.onClick.AddListener(OnConnectClicked);

            if (_startGameButton != null)
                _startGameButton.onClick.AddListener(OnStartGameClicked);

            _lobbyManager.OnNetworkStartFailed += OnNetworkStartFailed;
            _lobbyManager.OnNetworkStatus += OnNetworkStatus;
        }

        private void OnDestroy()
        {
            if (_lobbyManager != null)
            {
                _lobbyManager.OnNetworkStartFailed -= OnNetworkStartFailed;
                _lobbyManager.OnNetworkStatus -= OnNetworkStatus;
            }
        }

        private void OnNetworkStatus(string message)
        {
            if (_connectionStatus != null)
                _connectionStatus.text = message;
        }

        private void OnNetworkStartFailed(string message)
        {
            if (_connectionStatus != null)
                _connectionStatus.text = message;

            if (_hostButton != null)
                _hostButton.interactable = true;

            if (_connectButton != null)
                _connectButton.interactable = true;

            if (_startGameButton != null)
                _startGameButton.interactable = true;
        }  

        private void OnHostClicked()
        {
            SetButtonsInteractable(false);

            var port = _portInput != null ? _portInput.text : null;
            _lobbyManager.CreateLobby(port);
            if (_ipInput != null)
            {
                _ipInput.text = _lobbyManager.LastKnownHostLanIp;
                _ipInput.interactable = false;
            }
        }

        private void OnConnectClicked()
        {
            SetButtonsInteractable(false);

            var ip = _ipInput != null ? _ipInput.text : null;
            var port = _portInput != null ? _portInput.text : null;
            _lobbyManager.JoinLobby(ip, port);
        }

        private void OnStartGameClicked()
        {
            _gameManager.RequestStartGame();
        }

        private void SetButtonsInteractable(bool value)
        {
            if (_hostButton != null)
                _hostButton.interactable = value;

            if (_connectButton != null)
                _connectButton.interactable = value;
        }
    }
}
