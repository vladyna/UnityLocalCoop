using System.Collections.Generic;
using Test.Services;
using Test.Services.Models;
using UnityEngine;      
using UnityEngine.UI;
using Zenject;
namespace Test.UI
{
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _backToMenuButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Test.UI.PlayerListUI _playerListUi;

        [Inject] private LobbyManager _lobbyManager;
        [Inject] private GameManager _gamaManager;
        [Inject] private PlayerInputService _inputService;

        private bool _isOpen;

        private void Start()
        {
            if (_panel != null)
                _panel.SetActive(false);

            if (_backToMenuButton != null)
                _backToMenuButton.onClick.AddListener(OnBackToMenuClicked);

            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);

            if (_lobbyManager != null)
                _lobbyManager.OnRosterChanged += OnRosterChanged;
        }

        private void OnDestroy()
        {
            if (_lobbyManager != null)
                _lobbyManager.OnRosterChanged -= OnRosterChanged;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Update()
        {
            if (_inputService.Input.UI.Menu.triggered)
                Toggle();
        }

        public void Toggle()
        {
            if (_isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public void Open()
        {
            if (_panel != null)
                _panel.SetActive(true);

            _isOpen = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            DisableLocalPlayer(true);

            _playerListUi?.EnsurePlayerList(true);
        }

        public void Close()
        {
            if (_panel != null)
                _panel.SetActive(false);

            _isOpen = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            DisableLocalPlayer(false);
        }

        private void OnBackToMenuClicked()
        {
            if (_panel != null)
                _panel.SetActive(false);

            _lobbyManager?.LeaveLobby();
            _gamaManager.ForceReturnToMainMenu();
        }

        private void DisableLocalPlayer(bool disable)
        {
            if (disable)
            {
                _inputService?.Disable();
            }
            else
            {
                _inputService?.Enable();
            }
        }

        private void OnRosterChanged(IReadOnlyList<PlayerInfo> _)
        {
            _playerListUi?.EnsurePlayerList(true);
        }
    }
}
