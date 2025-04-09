using UnityEngine;

public class MetallicObject : MonoBehaviour
{
    [SerializeField] private ParticleSystem magneticEffect;
    [SerializeField] private AudioSource magneticSound;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMagneticForceApplied(bool isAttracted)
    {
        if (magneticEffect != null)
        {
            magneticEffect.Play();
        }
        if (magneticSound != null)
        {
            magneticSound.Play();
        }
        Debug.Log($"{gameObject.name} está siendo {(isAttracted ? "atraído" : "repelido")}");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
