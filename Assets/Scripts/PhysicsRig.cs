using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsRig : MonoBehaviour
{
    public Transform leftController;
    public Transform rightController;

    public ConfigurableJoint leftHandJoint;
    public ConfigurableJoint rightHandJoint;

    public CapsuleCollider bodyCollider;

    // Update is called once per frame
    void FixedUpdate()
    {
        leftHandJoint.targetPosition = leftController.localPosition;
        leftHandJoint.targetRotation = leftController.localRotation;

        rightHandJoint.targetPosition = rightController.localPosition;
        rightHandJoint.targetRotation = rightController.localRotation;

    }
}