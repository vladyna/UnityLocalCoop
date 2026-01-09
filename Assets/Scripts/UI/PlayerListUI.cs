using System.Collections.Generic;
using Test.Services;
using Test.Services.Models;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Test.UI
{
    public class PlayerListUI : MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private PlayerEntryUI _playerItemPrefab;
        [SerializeField] private float _pingRefreshInterval = 1f;

        [Inject] private LobbyManager _lobbyManager;
        [Inject] private PingProvider _pingProvider;

        private readonly Dictionary<ulong, PlayerEntryUI> _entries = new Dictionary<ulong, PlayerEntryUI>();
        private float _refreshTimer;

        private void OnEnable()
        {
            if (_lobbyManager != null)
                _lobbyManager.OnRosterChanged += OnRosterChanged;

            _refreshTimer = 0f;
            EnsurePlayerList(true);
        }

        private void OnDisable()
        {
            if (_lobbyManager != null)
                _lobbyManager.OnRosterChanged -= OnRosterChanged;
        }

        private void Update()
        {
            if (_pingRefreshInterval <= 0f)
                return;

            _refreshTimer -= Time.unscaledDeltaTime;
            if (_refreshTimer > 0f)
                return;

            _refreshTimer = _pingRefreshInterval;
            UpdatePingsOnly();
        }

        private void OnRosterChanged(IReadOnlyList<PlayerInfo> _)
        {
            EnsurePlayerList(true);
        }

        public void EnsurePlayerList(bool forceRebuild = false)
        {
            if (_content == null || _playerItemPrefab == null || _lobbyManager == null)
                return;

            if (!forceRebuild && _entries.Count == _lobbyManager.Players.Count)
            {
                foreach (var p in _lobbyManager.Players)
                {
                    if (_entries.ContainsKey(p.ClientId))
                        continue;

                    CreateEntry(p);
                }

                return;
            }

            foreach (Transform t in _content)
            {
                Destroy(t.gameObject);
            }

            _entries.Clear();

            foreach (var p in _lobbyManager.Players)
            {
                CreateEntry(p);
            }
        }

        private void CreateEntry(PlayerInfo player)
        {
            var item = Instantiate(_playerItemPrefab, _content);
            _entries[player.ClientId] = item;

            var ping = _pingProvider.GetPingForClient(player.ClientId);
            item.Set(player.Name, ping);

            var nm = NetworkManager.Singleton;
            var hostId = _lobbyManager.HostClientId;
            var localId = nm != null ? NetworkManager.ServerClientId : 0ul;

            var canKick = (nm != null && nm.IsHost) && (localId == hostId) && (player.ClientId != hostId);
            var id = player.ClientId;
            item.Initialize(canKick, () => _lobbyManager.Kick(id));
        }

        private void UpdatePingsOnly()
        {
            if (_lobbyManager == null)
                return;

            foreach (var p in _lobbyManager.Players)
            {
                var ping = _pingProvider.GetPingForClient(p.ClientId);
                if (_entries.TryGetValue(p.ClientId, out var entry) && entry != null)
                {
                    entry.Set(p.Name, ping);
                }
            }
        }
    }
}