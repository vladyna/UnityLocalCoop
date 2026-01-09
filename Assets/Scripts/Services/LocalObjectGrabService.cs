using Test.Services.Abstractions;
using Test.Objects;
using UnityEngine;
namespace Test.Services
{
    public class LocalObjectGrabService : IObjectGrabService
    {
        private ObjectRoot _grabbedObject;

        public void TryGrabObject(Vector3 origin, Vector3 direction, Camera camera, float maxDistance)
        {
            if (_grabbedObject != null)
                return;
            if (Physics.Raycast(origin, direction, out var hit, 3f))
            {
                var grabbable = hit.collider.GetComponentInParent<ObjectRoot>();
                if (grabbable == null || grabbable.IsGrabbed)
                    return;

                _grabbedObject = grabbable;

                _grabbedObject.Grab(hit.point, camera);
            }
        }

        public void ReleaseGrabbedObject()
        {
            if (_grabbedObject != null)
            {
                _grabbedObject.Release();
                _grabbedObject = null;
            }
        }

        public void ThrowGrabbedObject(Vector3 throwForce)
        {
            if (_grabbedObject != null)
            {
                if (_grabbedObject.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.isKinematic = false;
                    rb.AddForce(throwForce.normalized * 8f, ForceMode.VelocityChange);
                }
                _grabbedObject.Release();
                _grabbedObject = null;
            }
        }

        public void DeleteGrabbedObject()
        {
            if (_grabbedObject != null)
            {
                UnityEngine.Object.Destroy(_grabbedObject);
                _grabbedObject = null;
            }
        }
    }
}
