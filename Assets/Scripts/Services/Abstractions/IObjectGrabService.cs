using UnityEngine;
namespace Test.Services.Abstractions
{
    public interface IObjectGrabService
    {
        void TryGrabObject(Vector3 origin, Vector3 direction, Camera camera, float maxDistance);
        void ReleaseGrabbedObject();
        void ThrowGrabbedObject(Vector3 throwForce);
        void DeleteGrabbedObject();
    }
}

