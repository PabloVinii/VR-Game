using UnityEngine;

public class PhysicRig : MonoBehaviour
{
    [SerializeField] private Transform playerHead;
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;

    [SerializeField] private ConfigurableJoint leftHandJoint;
    [SerializeField] private ConfigurableJoint rightHandJoint;

    [SerializeField] private CapsuleCollider bodyCollider;
    
    [SerializeField] private float bodyHeightMin = 0.5f;
    [SerializeField] private float bodyHeightMax = 2;

    void FixedUpdate()
    {
        bodyCollider.height = Mathf.Clamp(playerHead.localPosition.y, bodyHeightMin, bodyHeightMax);
        bodyCollider.center = new Vector3(playerHead.localPosition.x, bodyCollider.height / 2, playerHead.localPosition.z);

        leftHandJoint.targetPosition = leftController.localPosition;
        leftHandJoint.targetRotation = leftController.localRotation;
        
        rightHandJoint.targetPosition = rightController.localPosition;
        rightHandJoint.targetRotation = rightController.localRotation;
    }
}
