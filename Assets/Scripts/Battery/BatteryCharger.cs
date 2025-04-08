using UnityEngine;

public class BatteryCharger : MonoBehaviour
{
    [SerializeField]
    private float chargeRate = 20f; // Energ�a recargada por segundo

    [SerializeField]
    private float maxEnergy = 100f; // Energ�a m�xima de la bater�a

    [SerializeField]
    private ParticleSystem chargeEffect;

    [SerializeField]
    private AudioSource chargeSound; 

    private bool isCharging = false;
    private BatteryController battery;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.GetComponent<Player>().battery != null)
        {
            battery = other.GetComponent<Player>().battery.GetComponent<BatteryController>();
            Debug.Log("Bater�a detectada en zona de carga");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            battery = null;
            isCharging = false;
            StopEffects();
            Debug.Log("Bater�a sali� de la zona de carga");
        }
    }

    public void StartCharging()
    {
        if (battery != null && battery.energyAmounts < maxEnergy)
        {
            isCharging = true;
            PlayEffects();
            Debug.Log("Comenzando a recargar bater�a");
        }
    }

    public void StopCharging()
    {
        isCharging = false;
        StopEffects();
    }

    void Update()
    {
        if (isCharging && battery != null)
        {
            battery.energyAmounts += chargeRate * Time.deltaTime;
            battery.energyAmounts = Mathf.Clamp(battery.energyAmounts, 0f, maxEnergy);
            Debug.Log($"Recargando bater�a: {battery.energyAmounts}/{maxEnergy}");
        }
    }

    private void PlayEffects()
    {
        if (chargeEffect != null && !chargeEffect.isPlaying)
        {
            chargeEffect.Play();
        }
        if (chargeSound != null && !chargeSound.isPlaying)
        {
            chargeSound.Play();
        }
    }

    private void StopEffects()
    {
        if (chargeEffect != null && chargeEffect.isPlaying)
        {
            chargeEffect.Stop();
        }
        if (chargeSound != null && chargeSound.isPlaying)
        {
            chargeSound.Stop();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
