using UnityEngine;

public class BatteryCharger : MonoBehaviour
{
    [SerializeField]
    private float chargeRate = 20f;

    [SerializeField]
    private float maxEnergy = 100f;

    [SerializeField]
    private float fixedChargeAmount = 30f; 

    [SerializeField]
    private bool isReusable = true;

    [SerializeField]
    private ParticleSystem chargeEffect;

    [SerializeField]
    private AudioSource chargeSound;

    [SerializeField]
    private ParticleSystem completeEffect;

    private bool isCharging = false;
    private BatteryController battery;
    private float chargedAmount = 0f;
    private bool hasCharged = false;
    private bool isDisabled = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.GetComponent<Player>().battery != null)
        {
            battery = other.GetComponent<Player>().battery.GetComponent<BatteryController>();
            Debug.Log("Batería detectada en zona de carga");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            battery = null;
            isCharging = false;
            chargedAmount = 0f;
            if (isReusable)
            {
                hasCharged = false;
            }
            StopEffects();
            Debug.Log("Batería salió de la zona de carga");
        }
    }

    public void StartCharging()
    {
        if (battery != null && battery.energyAmounts < maxEnergy && !isDisabled && !hasCharged)
        {
            isCharging = true;
            chargedAmount = 0f;
            PlayEffects();
            Debug.Log("Comenzando a recargar batería");
        }
        else if (isDisabled)
        {
            Debug.Log("Este cargador no es reutilizable y ya ha sido usado.");
        }
        else if (hasCharged)
        {
            Debug.Log("Este cargador ya ha sido usado en esta entrada.");
        }
    }

    public void StopCharging()
    {
        isCharging = false;
        chargedAmount = 0f;
        StopEffects();
    }

    void Update()
    {
        if (isCharging && battery != null)
        {
            float energyToAdd = chargeRate * Time.deltaTime;
            chargedAmount += energyToAdd;

            battery.energyAmounts += energyToAdd;
            battery.energyAmounts = Mathf.Clamp(battery.energyAmounts, 0f, maxEnergy);

            Debug.Log($"Recargando batería: {battery.energyAmounts}/{maxEnergy} (Recargado: {chargedAmount}/{fixedChargeAmount})");

            if (chargedAmount >= fixedChargeAmount || battery.energyAmounts >= maxEnergy)
            {
                isCharging = false;
                hasCharged = true;
                if (!isReusable)
                {
                    isDisabled = true;
                }
                StopEffects();
                if (completeEffect != null) completeEffect.Play();
                Debug.Log("Recarga completada: se alcanzó la cantidad fija o el máximo de energía");
            }
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
