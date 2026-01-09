using System.Collections.Generic;
using Test.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
namespace Test.UI
{
    public class TutorialUI : MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private TutorialEntryUI _entryPrefab;

        [Inject] private PlayerInputService _inputService;

        private readonly List<TutorialEntryUI> _entries = new List<TutorialEntryUI>();

        private void OnEnable()
        {
            Rebuild();
        }

        private void OnDisable()
        {
            Clear();
        }

        public void Rebuild()
        {
            Clear();

            if (_content == null || _entryPrefab == null || _inputService == null)
                return;

            var input = _inputService.Input;
            var player = input.Player;

            var actions = new[]
            {
            player.Move,
            player.Look,
            player.Interact,
            player.Release,
            player.Throw,
            player.Delete,
            player.Spawn,
            player.Color
        };

            foreach (var action in actions)
            {
                if (action == null)
                    continue;

                var display = GetActionBindingDisplay(action);

                var entry = Instantiate(_entryPrefab, _content);
                entry.Set(action.name, display);
                _entries.Add(entry);
            }
        }

        private void Clear()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i] != null)
                    Destroy(_entries[i].gameObject);
            }

            _entries.Clear();
        }

        private static string GetActionBindingDisplay(InputAction action)
        {
            var display = action.GetBindingDisplayString();

            if (string.IsNullOrWhiteSpace(display))
                display = "-";

            return display;
        }
    }
}
