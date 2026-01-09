using Test.Objects.Abstractions;
using Test.Objects.Models;
using Test.Player.Models;
using UnityEngine;

namespace Test.Objects
{
    public class GrabbableControllerCore : IGrabbableController
    {
        private readonly Rigidbody _rb;

        public GrabbableControllerCore(Rigidbody rb)
        {
            _rb = rb;
        }

        public void Simulate(ObjectInput input, float dt)
        {
            if (input.Release)
            {
                _rb.isKinematic = false;
                return;
            }

            _rb.isKinematic = true;

            _rb.MovePosition(input.TargetPosition);
        }

        public EntityState CaptureState()
        {
            return new EntityState
            {
                Position = _rb.position,
                Velocity = _rb.linearVelocity,
                Timestamp = Time.time
            };
        }

        public void ApplyState(EntityState state)
        {
            _rb.position = state.Position;
            _rb.linearVelocity = state.Velocity;
        }

        public void SetKinematic(bool value)
        {
            _rb.isKinematic = value;
        }
    }
}
