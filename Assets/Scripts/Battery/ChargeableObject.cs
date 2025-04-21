using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeableObject : MonoBehaviour
{
    public enum TargetType
    {
        Door,
        Laser
    }
    [System.Serializable]
    public class TargetEntry
    {
        public TargetType type;
        public GameObject targetObject;
        [HideInInspector] public IActivable activable;
    }

    [SerializeField]
    private GameObject target;

    [SerializeField]
    private List<TargetEntry> targets = new List<TargetEntry>();


    [SerializeField]
    private bool isPowered = false;

    [SerializeField]
    private float requiredEnergy = 50f;

    [SerializeField]
    private ParticleSystem chargeEffect;

    [SerializeField]
    private AudioSource chargeSound;

    [SerializeField]
    private float chargeDuration = 2f;

    private bool isCharging = false; 

    [SerializeField] private Material mat;

    void Start()
    {
        foreach (var target in targets)
        {
            if (target.targetObject != null)
            {
                switch (target.type)
                {
                    case TargetType.Door:
                        target.activable = target.targetObject.GetComponent<Door_Mechanic>();
                        break;
                    case TargetType.Laser:
                        target.activable = target.targetObject.GetComponent<Laser_Mechanic>();
                        break;
                }

                if (target.activable != null)
                {
                    target.activable.Toggle(isPowered);
                    target.activable.SetIgnoreTrigger(true);
                }
                else
                {
                    Debug.LogWarning($"El objetivo {target.targetObject.name} no tiene un componente {target.type} válido.");
                }
            }
        }

        if (target != null)
        {
            target.SetActive(isPowered);
            if (mat != null)
            {
                mat.SetFloat("_Progress", isPowered ? 1f : 0f);
            }
        }
    }

    public void StartCharging(BatteryController battery)
    {
        if (isCharging || isPowered) return;
        StartCoroutine(ChargeProgressively(battery));
    }

    private IEnumerator ChargeProgressively(BatteryController battery)
    {
        isCharging = true;
        float elapsedTime = 0f;
        float energyToConsume = requiredEnergy;
        float energyPerSecond = energyToConsume / chargeDuration;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (target != null)
        {
            target.SetActive(true);
            if (mat != null)
            {
                mat.SetFloat("_Progress", 0f);
            }
        }
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
        foreach (var target in targets)
        {
            if (target.activable != null)
            {
                target.activable.Toggle(true);
            }
        }
        if (mat != null)
        {
            mat.SetFloat("_Progress", 0f);
        }

        while (elapsedTime < chargeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / chargeDuration; 
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
                foreach (var target in targets)
                {
                    if (target.activable != null)
                    {
                        target.activable.Toggle(false);
                    }
                }
                yield break;
            }

            battery.energyAmounts -= energyThisFrame;
            battery.energyAmounts = Mathf.Clamp(battery.energyAmounts, 0f, battery.maxEnergy);
            Debug.Log($"Consumiendo energía progresivamente: {energyThisFrame}. Energía restante: {battery.energyAmounts}");

            if (sr != null)
            {
                sr.color = Color.Lerp(Color.red, Color.green, t);
            }

            if (mat != null)
            {
                mat.SetFloat("_Progress", t);
            }

            yield return null;
        }

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
            foreach (var target in targets)
            {
                if (target.activable != null)
                {
                    target.activable.Toggle(false);
                }
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
