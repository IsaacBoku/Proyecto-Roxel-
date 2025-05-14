using System.Collections;
using UnityEngine;

public class BatteryCharger : InteractableBase
{
    [SerializeField]
    private int pointsToRecharge = 1; // Puntos de batería a recargar por uso

    [SerializeField]
    private bool isReusable = true; // ¿Es reutilizable?

    [SerializeField]
    private ParticleSystem chargeEffect; // Efecto de partículas durante la recarga

    [SerializeField]
    private AudioSource chargeSound; // Sonido de recarga

    [SerializeField]
    private ParticleSystem completeEffect; // Efecto al completar la recarga

    [SerializeField]
    private SpriteRenderer spriteRenderer; // Sprite del cargador

    private bool isCharging = false;
    private BatteryController battery;
    private bool hasCharged = false;
    private bool isDisabled = false;

    public bool IsReusable => isReusable;
    public bool IsDisabled => isDisabled;


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.GetComponent<Player>().battery != null)
        {
            battery = other.GetComponent<Player>().battery.GetComponent<BatteryController>();
            StartCharging();
            Debug.Log($"BatteryCharger '{gameObject.name}': Batería detectada en zona de carga");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            battery = null;
            isCharging = false;
            if (isReusable)
            {
                hasCharged = false; // Reiniciar para permitir recarga en la próxima entrada
            }
            StopEffects();
            Debug.Log($"BatteryCharger '{gameObject.name}': Batería salió de la zona de carga");
        }
    }

    public override void Interact()
    {
        //StartCharging();
    }

    public void StartCharging()
    {
        if (battery != null && !isDisabled && !hasCharged)
        {
            // Intentar recargar la batería
            battery.RechargeBatteryPoints(pointsToRecharge);
            isCharging = true;
            hasCharged = true; // Marcar como usado para esta entrada
            if (!isReusable)
            {
                isDisabled = true; // Desactivar si no es reutilizable
            }

            // Reproducir efectos
            PlayEffects();
            if (completeEffect != null) completeEffect.Play();

            // Iniciar coroutine para detener efectos después de una breve animación
            StartCoroutine(StopChargingAfterDelay(0.5f)); // Ajusta la duración según necesites

            Debug.Log($"BatteryCharger '{gameObject.name}': Batería recargada con {pointsToRecharge} puntos. Puntos actuales: {battery.batteryPoints}/{battery.maxBatteryPoints}");
        }
        else if (isDisabled)
        {
            Debug.Log($"BatteryCharger '{gameObject.name}': Este cargador no es reutilizable y ya ha sido usado.");
        }
        else if (hasCharged)
        {
            Debug.Log($"BatteryCharger '{gameObject.name}': Este cargador ya ha sido usado en esta entrada.");
        }
    }

    private IEnumerator StopChargingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isCharging = false;
        StopEffects();
    }
    protected override void Update()
    {
        base.Update();
        UpdateSpriteColor();
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

    new void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    protected override void Start()
    {
        base.Start();

        // Inicializar SpriteRenderer
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"BatteryCharger '{gameObject.name}': No se encontró SpriteRenderer. Añade uno al GameObject.");
            }
        }

        // Establecer color inicial
        UpdateSpriteColor();
    }
    private void UpdateSpriteColor()
    {
        if (spriteRenderer == null) return;

        if (isDisabled)
        {
            spriteRenderer.color = Color.gray; // #808080
        }
        else if (isCharging)
        {
            spriteRenderer.color = Color.green; // #00FF00
        }
        else if (requiresSpecificPolarity)
        {
            spriteRenderer.color = requiredPolarityIsPositive ? Color.red : Color.blue; // #FF0000 o #0000FF
        }
    }
}
