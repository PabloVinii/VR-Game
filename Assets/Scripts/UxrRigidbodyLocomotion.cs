using UltimateXR.Core;
using UltimateXR.Devices;
using UltimateXR.Locomotion;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class UxrRigidbodyLocomotion : UxrLocomotion
{
    [Header("Movement Settings")]
    public float moveSpeed = 2.0f;
    public float sprintSpeed = 4.0f;
    public bool useEngineGravity = true;

    [Header("Rotation Settings")]
    public float rotationSpeed = 120f;

    [Header("Control Setup")]
    public UxrHandSide movementHand = UxrHandSide.Left;   // Mão do joystick de movimento
    public UxrHandSide rotationHand = UxrHandSide.Right;  // Mão do joystick de rotação
    public UxrInputButtons sprintButton = UxrInputButtons.Joystick;

    [Header("Capsule Settings")]
    public float minHeight = 0.5f;
    public float maxHeight = 2.0f;

    // Se quiser girar em torno de outro ponto (por ex. o chão sob os pés):
    [Header("Turning Pivot (Opcional)")]
    public Transform turnSource;

    private Rigidbody       _rb;
    private CapsuleCollider _capsule;

    public override bool IsSmoothLocomotion => true;

    protected override void Awake()
    {
        base.Awake();

        _rb = GetComponent<Rigidbody>();
        _capsule = GetComponent<CapsuleCollider>();

        // Evitar tombar
        _rb.freezeRotation = true;
        _rb.useGravity = useEngineGravity;
    }

    protected override void UpdateLocomotion()
    {
        // Se não tiver Avatar ou câmera, abortar
        if (Avatar == null || Avatar.CameraComponent == null)
            return;

        // 1) Lê input de movimento
        Vector2 inputMove = Avatar.ControllerInput.GetInput2D(movementHand, UxrInput2D.Joystick);
        bool isSprinting  = Avatar.ControllerInput.GetButtonsPress(movementHand, sprintButton);
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // Movimento baseado na direção da Câmera
        Vector3 camForward = Vector3.ProjectOnPlane(Avatar.CameraComponent.transform.forward, Vector3.up).normalized;
        Vector3 camRight   = Vector3.ProjectOnPlane(Avatar.CameraComponent.transform.right,   Vector3.up).normalized;
        Vector3 moveDir    = (camForward * inputMove.y + camRight * inputMove.x) * currentSpeed;

        Vector3 targetMovePosition = _rb.position + moveDir * Time.deltaTime;

        // 2) Lê input de rotação
        Vector2 inputTurn = Avatar.ControllerInput.GetInput2D(rotationHand, UxrInput2D.Joystick);
        float turnInput   = inputTurn.x;
        float angle       = rotationSpeed * Time.deltaTime * turnInput;

        // 3) Calcula rotação manual (sem RotateAvatar do UXR)
        //    Gira em torno do pivot (turnSource) ou do próprio _rb.position
        Vector3 pivot = turnSource ? turnSource.position : _rb.position;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.up);

        // Rotacionamos a posição resultante em torno do pivot
        Vector3 newPosition = targetMovePosition; 
        if (Mathf.Abs(turnInput) > 0.01f)
        {
            newPosition = q * (targetMovePosition - pivot) + pivot;

            // Aplica rotação ao rigidbody
            Quaternion newRot = _rb.rotation * q;
            _rb.MoveRotation(newRot);

            Avatar.transform.rotation = newRot;
        }

        _rb.MovePosition(newPosition);


    }
}
