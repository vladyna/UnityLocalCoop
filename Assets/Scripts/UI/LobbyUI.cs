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
            if (_ipInput != null)
            {
                _ipInput.text = _lobbyManager.LastKnownHostLanIp;
            }

            _hostButton.onClick.AddListener(OnHostClicked);
            _connectButton.onClick.AddListener(OnConnectClicked);
            _startGameButton.onClick.AddListener(OnStartGameClicked);
            _lobbyManager.OnNetworkStartFailed += OnNetworkStartFailed;
        }

        private void OnDestroy()
        {
            _lobbyManager.OnNetworkStartFailed -= OnNetworkStartFailed;
        }

        private void OnNetworkStartFailed(string obj)
        {
            _connectionStatus.text = obj;
            _hostButton.interactable = true;
            _connectButton.interactable = true;
            _startGameButton.interactable = true;
        }  

        private void OnHostClicked()
        {
            _lobbyManager.CreateLobby(_portInput.text);
            if (_ipInput != null)
            {
                _ipInput.text = _lobbyManager.LastKnownHostLanIp;
                _ipInput.interactable = false;
            }
        }

        private void OnConnectClicked()
        {
            var ip = _ipInput != null ? _ipInput.text : null;
            _lobbyManager.JoinLobby(ip, _portInput.text);
        }

        private void OnStartGameClicked()
        {
            _gameManager.RequestStartGame();
        }
    }
}
