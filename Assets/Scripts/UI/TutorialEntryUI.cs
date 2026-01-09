using TMPro;
using UnityEngine;

namespace Test.UI
{
    public class TutorialEntryUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _actionNameText;
        [SerializeField] private TextMeshProUGUI _bindingText;

        public void Set(string actionName, string binding)
        {
            if (_actionNameText != null)
                _actionNameText.text = actionName;

            if (_bindingText != null)
                _bindingText.text = binding;
        }
    }
}