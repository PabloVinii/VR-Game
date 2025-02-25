using UltimateXR.Manipulation;
using UltimateXR.Avatar;
using UnityEngine;

public class IgnoreCollisionWhenGrabbed : MonoBehaviour
{
    // O collider do Player ou CharacterController
    // Você pode arrastar via Inspector, ou buscar via script
    [SerializeField] private Collider _playerCollider;

    private UxrGrabbableObject _grabbable;
    private Collider           _myCollider;

    private void Awake()
    {
        // Vamos supor que este script está no mesmo objeto que tem UxrGrabbableObject
        _grabbable = GetComponent<UxrGrabbableObject>();
        _myCollider = GetComponent<Collider>();

        // Subscreve nos eventos
        _grabbable.Grabbed += OnGrabbed;
        _grabbable.Released += OnReleased;
    }

    private void OnDestroy()
    {
        // Boas práticas: remover subscrição ao destruir
        if (_grabbable)
        {
            _grabbable.Grabbed -= OnGrabbed;
            _grabbable.Released -= OnReleased;
        }
    }

    private void OnGrabbed(object sender, UxrManipulationEventArgs e)
    {
        if (_playerCollider && _myCollider)
        {
            // Ignora colisão com o jogador
            Physics.IgnoreCollision(_myCollider, _playerCollider, true);
        }
    }

    private void OnReleased(object sender, UxrManipulationEventArgs e)
    {
        if (_playerCollider && _myCollider)
        {
            // Restaura colisão
            Physics.IgnoreCollision(_myCollider, _playerCollider, false);
        }
    }
}
