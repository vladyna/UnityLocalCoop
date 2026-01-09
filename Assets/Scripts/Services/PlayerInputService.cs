namespace Test.Services
{
    public class PlayerInputService
    {
        private PlayerInputActions _input;
        private bool _enabled;

        public PlayerInputActions Input => _input;

        public PlayerInputService()
        {
            _input = new PlayerInputActions();
            Enable();
        }

        public void Enable()
        {
            if (_input == null)
                _input = new PlayerInputActions();

            if (_enabled)
                return;

            _input.Enable();
            _enabled = true;
        }

        public void Disable()
        {
            if (_input == null)
                return;

            if (!_enabled)
                return;

            _input.Disable();
            _enabled = false;
        }

    }
}
