using UnityEngine;

public class VRPlayerColliders : MonoBehaviour
{
    public static VRPlayerColliders Instance { get; private set; }

    [Tooltip("Collider da mão esquerda")]
    public Collider leftHandCollider;

    [Tooltip("Collider da mão direita")]
    public Collider rightHandCollider;

    [Tooltip("Collider do corpo (capsule ou similar)")]
    public Collider bodyCollider;

    private void Awake()
    {
        // Implementação simples de singleton (assumindo só um player local)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Collider[] GetAllColliders()
    {
        // Retorna o array que contém todos os colliders que compõem o "avatar"
        return new Collider[] { leftHandCollider, rightHandCollider, bodyCollider };
    }
}
