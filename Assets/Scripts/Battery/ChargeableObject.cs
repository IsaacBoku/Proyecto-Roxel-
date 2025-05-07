using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargeableObject : InteractableBase
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
    private List<TargetEntry> targets = new List<TargetEntry>();

    [SerializeField]
    private float requiredEnergy = 50f;

    [SerializeField]
    private float chargeDuration = 2f;

    [SerializeField]
    private Material mat;

    public bool isCharging = false;

    [SerializeField]
    private Slider slider_Energy;

    protected override void Start()
    {
        base.Start();
        foreach (var target in targets)
        {
            if (target.targetObject == null)
            {
                Debug.LogWarning($"ChargeableObject '{gameObject.name}': Un objetivo en la lista 'Targets' no tiene GameObject asignado.");
                continue;
            }

            switch (target.type)
            {
                case TargetType.Door:
                    target.activable = target.targetObject.GetComponent<Door_Mechanic>();
                    break;
                case TargetType.Laser:
                    target.activable = target.targetObject.GetComponent<Laser_Mechanic>();
                    break;
            }

            if (target.activable == null)
            {
                Debug.LogWarning($"ChargeableObject '{gameObject.name}': El objetivo '{target.targetObject.name}' no tiene un componente {target.type} válido.");
                continue;
            }

            target.activable.Toggle(isActive);
            target.activable.SetIgnoreTrigger(true);
        }

        /*if (mat != null)
        {
            mat.SetFloat("_Progress", isActive ? 1f : 0f);
        }*/
    }

    public override void Interact()
    {
        Debug.Log($"ChargeableObject '{gameObject.name}': Necesitas una batería para interactuar.");
    }

    public void StartCharging(BatteryController battery)
    {
        if (isCharging || isActive)
        {
            Debug.Log($"ChargeableObject '{gameObject.name}': Ya está cargando o activo.");
            return;
        }

        if (battery == null)
        {
            Debug.Log($"ChargeableObject '{gameObject.name}': No se puede interactuar sin una batería.");
            //return;
        }

        StartCoroutine(ChargeProgressively(battery));
    }

    private IEnumerator ChargeProgressively(BatteryController battery)
    {
        isCharging = true;
        float elapsedTime = 0f;
        float energyToConsume = requiredEnergy;
        float energyPerSecond = energyToConsume / chargeDuration;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // Activar objetivos y efectos
        foreach (var target in targets)
        {
            if (target.activable != null)
            {
                target.activable.Toggle(true);
            }
        }


        UpdateVisuals(true);

        // Ensure the slider is initialized
        if (slider_Energy != null)
        {
            slider_Energy.value = 0f; // Start at 0
            slider_Energy.maxValue = 1f; // Set max value to 1 for progress (0 to 1)
        }

        while (elapsedTime < chargeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / chargeDuration;
            float energyThisFrame = energyPerSecond * Time.deltaTime;

            if (battery.energyAmounts < energyThisFrame)
            {
                Debug.Log($"{gameObject.name} no puede activarse: Energía insuficiente ({battery.energyAmounts}/{energyThisFrame} requerida en este frame).");
                isCharging = false;
                foreach (var target in targets)
                {
                    if (target.activable != null)
                    {
                        target.activable.Toggle(false);
                    }
                }
                UpdateVisuals(false);

                // Reset slider on failure
                if (slider_Energy != null)
                {
                    slider_Energy.value = 0f;
                }


                yield break;
            }

            battery.energyAmounts -= energyThisFrame;
            battery.energyAmounts = Mathf.Clamp(battery.energyAmounts, 0f, battery.maxEnergy);
            //Debug.Log($"Consumiendo energía progresivamente: {energyThisFrame}. Energía restante: {battery.energyAmounts}");

            // Update slider to reflect charging progress
            if (slider_Energy != null)
            {
                slider_Energy.value = t; // Update slider value (0 to 1)
            }
            if (mat != null)
            {
                mat.SetFloat("_Progress", t);
            }

            yield return null;
        }

        isActive = true;
        isCharging = false;
        if (mat != null)
        {
            mat.SetFloat("_Progress", 1f);
        }
        // Set slider to max when charging completes
        if (slider_Energy != null)
        {
            slider_Energy.value = 1f;
        }
        Debug.Log($"{gameObject.name} ha terminado de cargarse.");
    }

    public void Deactivate()
    {
        if (isActive)
        {
            isActive = false;
            foreach (var target in targets)
            {
                if (target.activable != null)
                {
                    target.activable.Toggle(false);
                }
            }
            UpdateVisuals(false);
            if (mat != null)
            {
                mat.SetFloat("_Progress", 0f);
            }
            Debug.Log($"{gameObject.name} ha sido desactivado.");
        }
    }
}
