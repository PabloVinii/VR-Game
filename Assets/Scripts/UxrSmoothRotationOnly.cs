using UltimateXR.Core;
using UltimateXR.Devices;
using UltimateXR.Locomotion;
using UnityEngine;

namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Simplified locomotion component that only applies smooth rotation
    ///     when the user moves the joystick horizontal axis.
    ///     It removes teleport or any other forward/backward movement logic.
    /// </summary>
    public class UxrSmoothRotationOnly : UxrLocomotion
    {
        [Header("Rotation Settings")]
        [SerializeField] private float        _rotationSpeedDegreesPerSecond = 120.0f;
        [SerializeField] private UxrHandSide  _rotationHand                 = UxrHandSide.Right;
        [SerializeField] private UxrInput2D   _rotationJoystick             = UxrInput2D.Joystick;

        /// <summary>
        ///  If you want to mark it as "smooth locomotion" so que
        ///  o sistema possa reconhecê-lo nesse sentido.
        /// </summary>
        public override bool IsSmoothLocomotion => true;

        /// <summary>
        ///  Responsável por atualizar a "lógica de locomoção" (só rotação, nesse caso).
        ///  É chamado automaticamente pelo sistema UltimateXR em cada frame.
        /// </summary>
        protected override void UpdateLocomotion()
        {
            // Certifica de que temos um Avatar válido
            if (Avatar == null)
            {
                return;
            }

            // Lê a posição do joystick no controle configurado
            Vector2 input = Avatar.ControllerInput.GetInput2D(_rotationHand, _rotationJoystick);
            
            // Se a leitura horizontal for significativa, aplicamos a rotação
            float rotationInput = input.x; // Eixo X (esquerda/direita)

            if (Mathf.Abs(rotationInput) > 0.01f)
            {
                // Calcula quanto queremos girar este frame
                float rotationAmount = rotationInput * _rotationSpeedDegreesPerSecond * Time.deltaTime;

                // Aplica rotação ao Avatar
                UxrManager.Instance.RotateAvatar(Avatar, rotationAmount);
            }
        }
    }
}
