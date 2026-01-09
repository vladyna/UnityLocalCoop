using Test.Player.Abstractions;
using Test.Player.Models;
using UnityEngine;

namespace Test.Player
{
    public class CharacterControllerCore : ICharacterController
    {
        private readonly CharacterController _cc;
        private Vector3 _velocity;
        private float _yaw;
        public CharacterControllerCore(CharacterController cc)
        {
            _cc = cc;
        }

        public void Simulate(MovementInput input, float dt)
        {
            Vector3 localMove = new Vector3(input.Move.x, 0, input.Move.y);

            _yaw += input.Look.x;
            _cc.transform.rotation = Quaternion.Euler(0, _yaw, 0);

            Vector3 move = _cc.transform.TransformDirection(localMove);

            _cc.Move(move * dt * 5f);

            if (_cc.isGrounded)
                _velocity.y = 0;

            if (input.Jump && _cc.isGrounded)
                _velocity.y = 5f;

            _velocity.y += Physics.gravity.y * dt;
            _cc.Move(_velocity * dt);
        }

        public EntityState CaptureState()
        {
            return new EntityState
            {
                Position = _cc.transform.position,
                Velocity = _velocity,
                Timestamp = Time.time
            };
        }

        public void ApplyState(EntityState state)
        {
            _cc.enabled = false;
            _cc.transform.position = state.Position;
            _cc.enabled = true;
            _velocity = state.Velocity;
        }
    }
}
