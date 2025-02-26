using UltimateXR.Manipulation;
using UnityEngine;

public class IgnoreCollisionWhenGrabbed : MonoBehaviour
{
    private UxrGrabbableObject _grabbable;
    private Collider[]         _objectColliders;

    private void Awake()
    {
        _grabbable = GetComponent<UxrGrabbableObject>();
        _objectColliders = GetComponentsInChildren<Collider>();

        if (_grabbable)
        {
            _grabbable.Grabbed += OnGrabbed;
            _grabbable.Released += OnReleased;
        }
    }

    private void OnDestroy()
    {
        if (_grabbable)
        {
            _grabbable.Grabbed -= OnGrabbed;
            _grabbable.Released -= OnReleased;
        }
    }

    private void OnGrabbed(object sender, UxrManipulationEventArgs e)
    {
        SetCollisionsIgnored(true);
    }

    private void OnReleased(object sender, UxrManipulationEventArgs e)
    {
        SetCollisionsIgnored(false);
    }

    private void SetCollisionsIgnored(bool ignore)
    {
        // Pegamos as referências do player via o Singleton
        Collider[] avatarColliders = VRPlayerColliders.Instance.GetAllColliders();

        // Iterar pares entre os colliders do objeto e do avatar
        foreach (var objCol in _objectColliders)
        {
            foreach (var avatarCol in avatarColliders)
            {
                if (objCol != avatarCol && objCol && avatarCol)
                {
                    Physics.IgnoreCollision(objCol, avatarCol, ignore);
                }
            }
        }
    }
}
