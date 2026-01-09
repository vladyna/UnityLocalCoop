using UnityEngine;
using UnityEngine.UI;
namespace Test.UI
{
    public class LoadingUI : MonoBehaviour
    {
        public GameObject loadingPanel;
        public Slider progressBar;

        private void Awake()
        {
            Hide();
        }

        public void Show()
        {
            loadingPanel.SetActive(true);
        }

        public void Hide()
        {
            loadingPanel.SetActive(false);
        }

        public void SetProgress(float value)
        {
            progressBar.value = value;
        }
    }
}
