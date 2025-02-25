using UltimateXR.Core;
using UltimateXR.Devices;
using UltimateXR.Locomotion;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class UxrRigidbodyLocomotion : UxrLocomotion
{
    [Header("Movement Settings")]
    [Tooltip("Velocidade de locomoção (m/s) normal.")]
    public float moveSpeed = 2.0f;

    [Tooltip("Velocidade de \"sprint\" (m/s).")]
    public float sprintSpeed = 4.0f;

    [Tooltip("Utilizar gravidade interna do Rigidbody (caso contrário, faremos manual).")]
    public bool useEngineGravity = true;

    [Header("Rotation Settings")]
    [Tooltip("Velocidade de rotação (graus por segundo) ao girar pelo joystick.")]
    public float rotationSpeed = 120f;

    [Header("Control Setup")]
    [Tooltip("Qual mão controlará o movimento.")]
    public UxrHandSide movementHand = UxrHandSide.Left;

    [Tooltip("Qual mão controlará a rotação.")]
    public UxrHandSide rotationHand = UxrHandSide.Right;

    [Tooltip("Botão que ativa o 'sprint'.")]
    public UxrInputButtons sprintButton = UxrInputButtons.Joystick;

    [Header("Capsule Settings")]
    [Tooltip("Altura mínima do capsule para evitar ficar menor que o \"joelho\" do personagem.")]
    public float minHeight = 0.5f;

    [Tooltip("Altura máxima do capsule, por exemplo a altura de uma pessoa em pé.")]
    public float maxHeight = 2.0f;

    private Rigidbody _rb;
    private CapsuleCollider _capsule;

    public override bool IsSmoothLocomotion => true;

    protected override void Awake()
    {
        base.Awake();

        _rb = GetComponent<Rigidbody>();
        _capsule = GetComponent<CapsuleCollider>();

        // Configurações típicas de um Rigidbody de “player”:
        // 1. Evitar que tombe (freeze rotation).
        _rb.freezeRotation = true;  

        // 2. Se quisermos usar a gravidade natural do Unity:
        //    (Senão, setar false e aplicar gravidade manual).
        _rb.useGravity = useEngineGravity;  
    }

    protected override void UpdateLocomotion()
    {
        if (Avatar == null || Avatar.CameraComponent == null) return;

        UpdateCapsuleHeight();
        
        // Ler input de movimento (joystick da mão de movimento)
        Vector2 inputMove = Avatar.ControllerInput.GetInput2D(movementHand, UxrInput2D.Joystick);
        // Ler input de rotação (joystick da mão de rotação)
        Vector2 inputTurn = Avatar.ControllerInput.GetInput2D(rotationHand, UxrInput2D.Joystick);

        // Verificar sprint
        bool isSprinting = Avatar.ControllerInput.GetButtonsPress(movementHand, sprintButton);
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // Direção de movimento (no plano horizontal)
        Vector3 forward = Vector3.ProjectOnPlane(Avatar.CameraComponent.transform.forward, Vector3.up).normalized;
        Vector3 right   = Vector3.ProjectOnPlane(Avatar.CameraComponent.transform.right,   Vector3.up).normalized;
        Vector3 moveDir = (forward * inputMove.y + right * inputMove.x) * currentSpeed;

        Vector3 newPosition = _rb.position + moveDir * Time.deltaTime;
        _rb.MovePosition(newPosition);

        float turnAmount = inputTurn.x;
        if (Mathf.Abs(turnAmount) > 0.01f)
        {
            float degrees = turnAmount * rotationSpeed * Time.deltaTime;
            UxrManager.Instance.RotateAvatar(Avatar, degrees);
        }
    }

    void UpdateCapsuleHeight()
    {
        float userHeight = Avatar.CameraTransform.position.y - Avatar.CameraFloorPosition.y;
        float clampedHeight = Mathf.Clamp(userHeight, minHeight, maxHeight);
        _capsule.height = clampedHeight;
        _capsule.center = new Vector3(0, clampedHeight / 2f, 0);
    }

}
