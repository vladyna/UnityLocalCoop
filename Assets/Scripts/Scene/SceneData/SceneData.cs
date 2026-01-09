using UnityEngine;
namespace Test.Scene.Data
{
    [CreateAssetMenu(fileName = "SceneData", menuName = "Test/Scene/Scene Data")]
    public class SceneData : ScriptableObject
    {
        [SerializeField] private string _sceneName;
        public string SceneName => _sceneName;
    }
}

