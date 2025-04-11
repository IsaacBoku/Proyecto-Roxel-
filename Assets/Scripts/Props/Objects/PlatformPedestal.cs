using UnityEngine;

public class PlatformPedestal : MonoBehaviour
{
    [SerializeField] private MovingPlatform platform;
    [SerializeField] private ParticleSystem activateEffect;
    [SerializeField] private AudioSource activateSound;
    private bool hasBattery = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Battery"))
        {
            hasBattery = true;
            platform.Activate();
            if (activateEffect != null) activateEffect.Play();
            if (activateSound != null) activateSound.Play();
            Debug.Log("Plataforma activada");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Battery"))
        {
            hasBattery = false;
            platform.Deactivate();
            Debug.Log("Plataforma desactivada");
        }
    }

    public bool HasBattery => hasBattery;
}
