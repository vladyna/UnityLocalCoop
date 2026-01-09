using UnityEngine;

namespace Test.Player.Utils
{
    public class FirstPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float sensitivity = 1.5f;

        private float _pitch;

        private void Start()
        {
            Cursor.visible = false;
        }

        private void OnDestroy()
        {
            Cursor.visible = true;
        }

        public void ApplyLook(Vector2 look)
        {
            _pitch -= look.y * sensitivity;
            _pitch = Mathf.Clamp(_pitch, -85f, 85f);
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0, 0);
        }
    }
}