using UnityEngine;
using UltimateXR.Core;
using UltimateXR.Locomotion;
using UltimateXR.Devices;

[RequireComponent(typeof(CharacterController))]
public class UxrDynamicHeightLocomotion : UxrLocomotion
{
    [Header("Capsule Settings")]
    [Tooltip("Altura mínima do capsule para evitar ficar menor que o \"joelho\" do personagem.")]
    public float minHeight = 0.5f;

    [Tooltip("Altura máxima do capsule, por exemplo a altura de uma pessoa em pé.")]
    public float maxHeight = 2.0f;

    [Header("Movement Settings")]
    [Tooltip("Velocidade de locomoção (m/s)")]
    public float moveSpeed = 2.0f;

    [Tooltip("Velocidade de \"sprint\" (m/s) opcional")]
    public float sprintSpeed = 4.0f;

    [Tooltip("Gravidade aplicada quando não estiver no chão.")]
    public float gravity = -9.81f;

    [Header("Rotation Settings")]
    [Tooltip("Graus por segundo na rotação suave (smooth).")]
    public float rotationSpeed = 120f;

    [Header("Control Setup")]
    [Tooltip("De qual mão queremos ler o joystick de movimentação.")]
    public UxrHandSide movementHand = UxrHandSide.Left;

    [Tooltip("De qual mão queremos ler o joystick de rotação.")]
    public UxrHandSide rotationHand = UxrHandSide.Right;

    [Tooltip("Qual botão ou axis usar para \"sprint\".")]
    public UxrInputButtons sprintButton = UxrInputButtons.Joystick;

    // Referências
    private CharacterController _charController;
    private float               _verticalVelocity;

    public override bool IsSmoothLocomotion => true;

    protected override void Awake()
    {
        base.Awake();
        _charController = GetComponent<CharacterController>();

        // Caso não queira que a física faça rodar ou tombar o character
        _charController.center = Vector3.up * 1.0f; // Posição inicial
        _charController.height = 1.8f;              // Altura inicial aproximada
    }

    protected override void UpdateLocomotion()
    {
        if (Avatar == null)
        {
            Debug.LogWarning("Avatar é nulo! Certifique-se de que este Locomotion está em um objeto que possui UxrAvatar.");
            return;
        }
        if (Avatar == null || Avatar.CameraComponent == null) return;

        UpdateCapsuleHeight();

        // 2. Ler entradas de joystick (esquerdo para movimento, direito para rotação)
        Vector2 inputMove = Avatar.ControllerInput.GetInput2D(movementHand, UxrInput2D.Joystick);
        Vector2 inputTurn = Avatar.ControllerInput.GetInput2D(rotationHand, UxrInput2D.Joystick);

        Debug.Log($"Move input = {inputMove}, Turn input = {inputTurn}");

        // 3. Verificar sprint (botão pressionado?)
        bool isSprinting = Avatar.ControllerInput.GetButtonsPress(movementHand, sprintButton);
        Debug.Log($"Sprint Pressed = {isSprinting}");
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // 4. Calcular direção de movimento no plano horizontal
        Vector3 forward = Vector3.ProjectOnPlane(Avatar.CameraComponent.transform.forward, Vector3.up).normalized;
        Vector3 right   = Vector3.ProjectOnPlane(Avatar.CameraComponent.transform.right,   Vector3.up).normalized;

        Vector3 moveDir = (forward * inputMove.y + right * inputMove.x) * currentSpeed;

        // 5. Lidar com gravidade manualmente
        if (_charController.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = 0f;
        }
        else
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }

        // 6. Combinar movimento horizontal com gravidade
        moveDir.y = _verticalVelocity;

        // 7. Usar CharacterController.Move() para respeitar colisões
        _charController.Move(moveDir * Time.deltaTime);

        // 8. Rotação suave
        float turnInput = inputTurn.x;
        if (Mathf.Abs(turnInput) > 0.01f)
        {
            float turnAmount = turnInput * rotationSpeed * Time.deltaTime;
            UxrManager.Instance.RotateAvatar(Avatar, turnAmount);
        }
    }

    /// <summary>
    /// Ajusta a altura do CharacterController dinamicamente com base na posição (altura) do HMD em relação ao chão.
    /// </summary>
    private void UpdateCapsuleHeight()
    {
        // Posição real do HMD no mundo
        Vector3 headPosition = Avatar.CameraTransform.position;

        // Precisamos saber onde é o "chão" do avatar. 
        // No UltimateXR, há algumas propriedades úteis como CameraFloorPosition, mas vamos fazer algo simples:
        Vector3 floorPosition = Avatar.CameraFloorPosition;

        // Distância vertical da cabeça até o chão
        float userHeight = headPosition.y - floorPosition.y;

        // Clampa para evitar valores absurdos:
        float clampedHeight = Mathf.Clamp(userHeight, minHeight, maxHeight);

        // Ajusta a altura do CharacterController
        _charController.height = clampedHeight;

        // O CharacterController exige que o "center.y" seja metade da altura para ficar "de pé".
        // Se quiser, pode adicionar offsets se o pivot não estiver no chão
        Vector3 newCenter = _charController.center;
        newCenter.y = _charController.height * 0.5f;
        _charController.center = newCenter;
    }
}
