using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Test.UI
{
    public class PlayerEntryUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _playerPingText;
        [SerializeField] private Button _kick;

        public void Initialize(bool canKick, Action onKick)
        {
            if (_kick == null)
                return;

            _kick.gameObject.SetActive(canKick);
            _kick.onClick.RemoveAllListeners();
            if (canKick && onKick != null)
                _kick.onClick.AddListener(() => onKick());
        }

        public void Set(string playerName, string ping)
        {
            if (_playerNameText != null)
                _playerNameText.text = playerName;
            if (_playerPingText != null)
                _playerPingText.text = ping;
        }
    }
}
