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

    [SerializeField] private Material mat;

    void Start()
    {
        if (target != null)
        {
            target.SetActive(isPowered);
            if (mat != null)
            {
                mat.SetFloat("_Progress", isPowered ? 1f : 0f); // Inicializar según el estado
            }
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

        // Activar el target al inicio de la carga
        if (target != null)
        {
            target.SetActive(true);
            if (mat != null)
            {
                mat.SetFloat("_Progress", 0f);
            }
        }

        // Efectos visuales y de sonido
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
            float t = elapsedTime / chargeDuration; // Progreso de 0 a 1
            float energyThisFrame = energyPerSecond * Time.deltaTime;

            if (battery.energyAmounts < energyThisFrame)
            {
                Debug.Log($"{gameObject.name} no puede activarse: Energía insuficiente durante la carga ({battery.energyAmounts}/{energyThisFrame} requerida en este frame).");
                isCharging = false;
                StopEffects();
                if (target != null)
                {
                    target.SetActive(false);
                }
                yield break;
            }

            battery.energyAmounts -= energyThisFrame;
            battery.energyAmounts = Mathf.Clamp(battery.energyAmounts, 0f, battery.maxEnergy);
            Debug.Log($"Consumiendo energía progresivamente: {energyThisFrame}. Energía restante: {battery.energyAmounts}");

            // Cambia el color progresivamente de rojo a verde
            if (sr != null)
            {
                sr.color = Color.Lerp(Color.red, Color.green, t);
            }

            // Actualiza _Progress del material del target
            if (mat != null)
            {
                mat.SetFloat("_Progress", t);
            }

            yield return null;
        }

        // Finalizar la carga
        if (sr != null)
        {
            sr.color = Color.green;
        }
        if (mat != null)
        {
            mat.SetFloat("_Progress", 1f);
        }
        isPowered = true;
        isCharging = false;
        Debug.Log($"{gameObject.name} ha terminado de cargarse.");
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
            if (mat != null)
            {
                mat.SetFloat("_Progress", 0f);
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
