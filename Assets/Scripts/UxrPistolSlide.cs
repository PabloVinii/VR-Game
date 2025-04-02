using UltimateXR.Audio;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Haptics;
using UltimateXR.Manipulation;
using UnityEngine;
using UltimateXR.Mechanics.Weapons;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Exemplo de script para gerenciar o "ferrolho" (slide) de uma pistola em VR
    ///     usando a lógica inspirada em UxrShotgunPump. 
    ///     - Requer um UxrFirearmWeapon (com ShotCycle adequado, p.ex: SemiAuto).
    ///     - O slide deve ser um UxrGrabbableObject.
    /// </summary>
    [RequireComponent(typeof(UxrFirearmWeapon))]
    public class UxrPistolSlide : UxrComponent
    {
        #region Inspector Fields

        [Header("Pistol Slide Settings")]
        [SerializeField] private int                _triggerIndex                = 0;
        [SerializeField] private UxrGrabbableObject _slide                      = null;
        [Tooltip("Direção local em que o slide se move, em relação ao transform local do Slide.")]
        [SerializeField] private Vector3            _localSlideDirection        = Vector3.back;
        [Tooltip("Offset em relação à posição local inicial do Slide, que define a 'distância' de movimento.")]
        [SerializeField] private Vector3            _localSlideOffset           = Vector3.back * 0.2f;
        [Range(0, 1)]
        [SerializeField] private float              _slideThreshold             = 0.7f;

        [Header("Audio Samples")]
        [SerializeField] private UxrAudioSample     _audioSlidePull             = new UxrAudioSample();
        [SerializeField] private UxrAudioSample     _audioSlideForward          = new UxrAudioSample();
        [SerializeField] private UxrAudioSample     _audioSlideAlreadyLoaded    = new UxrAudioSample();

        [Header("Haptic Feedback")]
        [SerializeField] private UxrHapticClip      _hapticSlidePull            = new UxrHapticClip(null, UxrHapticClipType.Slide);
        [SerializeField] private UxrHapticClip      _hapticSlideForward         = new UxrHapticClip(null, UxrHapticClipType.Slide);
        [SerializeField] private UxrHapticClip      _hapticSlideAlreadyLoaded   = new UxrHapticClip(null, UxrHapticClipType.Slide);

        #endregion

        #region Private Fields

        private UxrFirearmWeapon _firearm;
        private Vector3          _localStart;
        private State            _state;
        
        #endregion

        #region Enums

        private enum State
        {
            WaitSlideBack,     // Esperando puxar o slide para trás
            WaitSlideForward   // Esperando o slide retornar
        }

        #endregion

        #region Unity

        protected override void Awake()
        {
            base.Awake();

            _firearm    = GetComponent<UxrFirearmWeapon>();
            _localStart = _slide != null ? _slide.transform.localPosition : Vector3.zero;

            if (_localSlideOffset == Vector3.zero)
            {
                Debug.LogWarning("[PistolSlide] ⚠️ _localSlideOffset está em (0,0,0). Isso pode causar erro de divisão!");
            }

            if (_localSlideDirection == Vector3.zero)
            {
                Debug.LogWarning("[PistolSlide] ⚠️ _localSlideDirection está em (0,0,0). Isso impedirá o cálculo do slide.");
            }

            _state = State.WaitSlideBack;
        }


        private void Update()
        {
            if (_slide == null)
            {
                return;
            }

            // 👉 Começamos medindo os vetores
            Vector3 slideDisplacement = _slide.transform.localPosition - _localStart;
            Vector3 scaledDisplacement = Vector3.Scale(slideDisplacement, _localSlideDirection);
            float offsetMagnitude = _localSlideOffset.magnitude;

            float currentSlide = scaledDisplacement.magnitude / Mathf.Max(offsetMagnitude, Mathf.Epsilon);

            switch (_state)
            {
                case State.WaitSlideBack:
                    // Se puxou além do threshold, toca som/haptic
                    if (currentSlide > _slideThreshold)
                    {
                        PlayPullAudioHaptics(_firearm.IsLoaded(_triggerIndex));
                        // Passa para o estado de "aguardar o retorno do slide"
                        _state = State.WaitSlideForward;
                    }
                    break;

                case State.WaitSlideForward:
                    // Se voltamos perto da posição inicial
                    if (currentSlide < _slideThreshold * 0.2f)
                    {
                        PlayForwardAudioHaptics(_firearm.IsLoaded(_triggerIndex));

                        // Carrega arma caso não esteja carregada:
                        if (!_firearm.IsLoaded(_triggerIndex))
                        {
                            _firearm.Reload(_triggerIndex);
                        }

                        // Independente disso, volta a aguardar uma próxima puxada
                        _state = State.WaitSlideBack;
                    }
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void PlayPullAudioHaptics(bool alreadyLoaded)
        {
            // Som
            if (alreadyLoaded)
            {
                _audioSlideAlreadyLoaded.Play(_slide.transform.position);
            }
            else
            {
                _audioSlidePull.Play(_slide.transform.position);
            }

            // Haptics
            if (UxrGrabManager.Instance.GetGrabbingHand(_slide, 0, out UxrHandSide handSide))
            {
                if (alreadyLoaded)
                {
                    UxrAvatar.LocalAvatarInput.SendHapticFeedback(handSide, _hapticSlideAlreadyLoaded);
                }
                else
                {
                    UxrAvatar.LocalAvatarInput.SendHapticFeedback(handSide, _hapticSlidePull);
                }
            }
        }

        private void PlayForwardAudioHaptics(bool alreadyLoaded)
        {
            _audioSlideForward.Play(_slide.transform.position);

            if (UxrGrabManager.Instance.GetGrabbingHand(_slide, 0, out UxrHandSide handSide))
            {
                UxrAvatar.LocalAvatarInput.SendHapticFeedback(handSide, _hapticSlideForward);
            }
        }

        #endregion
    }
}
