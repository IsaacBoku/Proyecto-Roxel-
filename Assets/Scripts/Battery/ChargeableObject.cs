using UnityEngine;
using System.Collections;

public class ChargeableObject : MonoBehaviour
{
    [SerializeField]
    private GameObject target;

    [SerializeField]
    private bool isPowered = false;

    [SerializeField]
    private float requiredEnergy = 50f;

    [SerializeField]
    private ParticleSystem chargeEffect;

    [SerializeField]
    private AudioSource chargeSound;

    [SerializeField]
    private float chargeDuration = 2f; // Duración de la carga progresiva (en segundos)

    private bool isCharging = false; // Indica si el objeto está en proceso de carga

    void Start()
    {
        if (target != null && !isPowered)
        {
            target.SetActive(false);
        }
    }

    public void StartCharging(BatteryController battery)
    {
        if (isCharging || isPowered) return; // No permite nuevas interacciones si ya está cargando o activado
        StartCoroutine(ChargeProgressively(battery));
    }

    private IEnumerator ChargeProgressively(BatteryController battery)
    {
        isCharging = true;
        float elapsedTime = 0f;
        float energyToConsume = requiredEnergy;
        float energyPerSecond = energyToConsume / chargeDuration;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (chargeEffect != null)
        {
            var main = chargeEffect.main;
            main.startColor = Color.green;
            chargeEffect.Play();
        }
        if (chargeSound != null)
        {
            chargeSound.Play();
        }

        while (elapsedTime < chargeDuration)
        {
            elapsedTime += Time.deltaTime;
            float energyThisFrame = energyPerSecond * Time.deltaTime;

            if (battery.energyAmounts < energyThisFrame)
            {
                Debug.Log($"{gameObject.name} no puede activarse: Energía insuficiente durante la carga ({battery.energyAmounts}/{energyThisFrame} requerida en este frame).");
                isCharging = false;
                StopEffects();
                yield break;
            }

            battery.energyAmounts -= energyThisFrame;
            battery.energyAmounts = Mathf.Clamp(battery.energyAmounts, 0f, battery.maxEnergy);
            Debug.Log($"Consumiendo energía progresivamente: {energyThisFrame}. Energía restante: {battery.energyAmounts}");

            // Cambia el color progresivamente de rojo a verde
            if (sr != null)
            {
                float t = elapsedTime / chargeDuration;
                sr.color = Color.Lerp(Color.red, Color.green, t);
            }

            yield return null;
        }

        if (sr != null)
        {
            sr.color = Color.green;
        }
        isPowered = true;
        ActivateTarget();
        isCharging = false;
        Debug.Log($"{gameObject.name} ha terminado de cargarse.");
    }

    private void ActivateTarget()
    {
        if (target != null)
        {
            target.SetActive(true);
            Debug.Log($"{gameObject.name} ha sido activado con {requiredEnergy} de energía.");
        }
    }

    private void StopEffects()
    {
        if (chargeEffect != null)
        {
            chargeEffect.Stop();
        }
        if (chargeSound != null)
        {
            chargeSound.Stop();
        }
    }

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

    void OnDrawGizmos()
    {
        Gizmos.color = isPowered ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
