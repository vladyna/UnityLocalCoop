using UnityEngine;

namespace Test.Player.Utils
{
    public class FirstPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private float _sensitivity = 1.5f;

        private float _pitch;

        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDestroy()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void ApplyLook(Vector2 look)
        {
            if (_cameraPivot == null)
                return;

            _pitch -= look.y * _sensitivity;
            _pitch = Mathf.Clamp(_pitch, -85f, 85f);
            _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0, 0);
        }
    }
}