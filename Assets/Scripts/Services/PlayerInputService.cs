namespace Test.Services
{
    public class PlayerInputService
    {
        private PlayerInputActions _input;

        public PlayerInputActions Input => _input;

        public PlayerInputService()
        {
            _input = new PlayerInputActions();
            _input.Enable();
        }

        public void Enable()
        {
            _input.Enable();
        }

        public void Disable()
        {
            _input.Disable();
        }

    }
}
