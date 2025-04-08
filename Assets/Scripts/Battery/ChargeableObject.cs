using UnityEngine;

public class ChargeableObject : MonoBehaviour
{
    [SerializeField]
    private GameObject target; // El objeto que se activa (plataforma, puerta, etc.)

    [SerializeField]
    private bool isPowered = false; // Estado inicial (desactivado por defecto)

    [SerializeField]
    private float requiredEnergy = 50f; // Energía mínima necesaria para activarse (opcional)

    [SerializeField]
    private ParticleSystem chargeEffect; 

    [SerializeField]
    private AudioSource chargeSound; 

    void Start()
    {
        // Asegura que el objeto objetivo esté desactivado al inicio si no está powered
        if (target != null && !isPowered)
        {
            target.SetActive(false);
        }
    }

    public float ReceiveEnergy(float availableEnergy)
    {
        if (!isPowered && availableEnergy >= requiredEnergy)
        {
            isPowered = true;
            ActivateTarget();
            PlayEffects();
            return requiredEnergy; // Devuelve solo la energía requerida
        }
        return 0f; // No consume energía si no hay suficiente o ya está activado
    }

    private void ActivateTarget()
    {
        if (target != null)
        {
            target.SetActive(true); // Activa la plataforma, puerta, etc.
            Debug.Log($"{gameObject.name} ha sido activado con energía.");
        }
    }

    private void PlayEffects()
    {
        if (chargeEffect != null)
        {
            chargeEffect.Play(); // Reproduce partículas
        }
        if (chargeSound != null)
        {
            chargeSound.Play(); // Reproduce sonido
        }
    }

    // Método opcional para desactivar manualmente (si necesitas resetear)
    public void Deactivate()
    {
        if (isPowered)
        {
            isPowered = false;
            if (target != null)
            {
                target.SetActive(false);
            }
            Debug.Log($"{gameObject.name} ha sido desactivado.");
        }
    }

    // Para depuración en el editor
    void OnDrawGizmos()
    {
        Gizmos.color = isPowered ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f); // Visualiza el objeto en el editor
    }
}
