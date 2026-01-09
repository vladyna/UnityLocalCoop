using UnityEngine;
using UnityEngine.UI;
namespace Test.UI
{
    public class LoadingUI : MonoBehaviour
    {
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private Slider _progressBar;

        private void Awake()
        {
            Hide();
        }

        public void Show()
        {
            if (_loadingPanel != null)
                _loadingPanel.SetActive(true);
        }

        public void Hide()
        {
            if (_loadingPanel != null)
                _loadingPanel.SetActive(false);
        }

        public void SetProgress(float value)
        {
            if (_progressBar != null)
                _progressBar.value = value;
        }
    }
}
