using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch_Mechanic : InteractableBase
{
    public enum TargetType
    {
        Platform,
        Door,
        Laser
    }

    [System.Serializable]
    public class TargetEntry
    {
        public TargetType type;
        public GameObject targetObject;
        public bool activateOnTrigger; // true: activar, false: desactivar
        [HideInInspector] public IActivable activable;
        [HideInInspector] public MovingPlatform platform;
    }

    [Header("Switch Settings")]
    [SerializeField] private List<TargetEntry> targets = new List<TargetEntry>();
    [SerializeField] private bool isToggleSwitch = false; // Reemplaza 'sticks'
    [SerializeField] private ParticleSystem activateEffect;
    [SerializeField] private AudioSource activateSound;
    [SerializeField] private AudioSource deactivateSound;

    private Animator ani;
    private SpriteRenderer sr;
    private bool isActivated = false;

    protected override void Start()
    {
        base.Start();
        ani = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // Indicador visual para polaridad
        if (requiresSpecificPolarity && sr != null)
        {
            sr.color = requiredPolarityIsPositive ? Color.red : Color.blue;
        }

        // Inicializar objetivos
        foreach (var target in targets)
        {
            if (target.targetObject == null)
            {
                Debug.LogWarning($"Switch_Mechanic '{gameObject.name}': Un objetivo en la lista 'Targets' no tiene GameObject asignado.");
                continue;
            }

            switch (target.type)
            {
                case TargetType.Platform:
                    target.platform = target.targetObject.GetComponent<MovingPlatform>();
                    if (target.platform == null)
                    {
                        Debug.LogWarning($"Switch_Mechanic '{gameObject.name}': El objetivo '{target.targetObject.name}' no tiene un componente MovingPlatform.");
                    }
                    break;
                case TargetType.Door:
                case TargetType.Laser:
                    target.activable = target.targetObject.GetComponent<IActivable>();
                    if (target.activable == null)
                    {
                        Debug.LogWarning($"Switch_Mechanic '{gameObject.name}': El objetivo '{target.targetObject.name}' no tiene un componente IActivable.");
                    }
                    break;
            }
        }
    }

    public override void Interact()
    {
        if (isActivated && !isToggleSwitch) return;

        var player = FindFirstObjectByType<Player>();
        if (requiresSpecificPolarity && player != null)
        {
            var batteryController = player.battery.GetComponent<BatteryController>();
            if (batteryController.isPositivePolarity != requiredPolarityIsPositive)
            {
                Debug.Log($"Switch_Mechanic '{gameObject.name}': Polaridad incorrecta. Se requiere {(requiredPolarityIsPositive ? "positiva" : "negativa")}.");
                if (deactivateSound != null) deactivateSound.Play();
                return;
            }
        }

        ToggleSwitch(!isActivated);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isActivated && isToggleSwitch) return;

        if (requiresSpecificPolarity)
        {
            BatteryController batteryController = null;
            if (other.CompareTag("Player"))
            {
                var player = other.GetComponent<Player>();
                batteryController = player?.battery.GetComponent<BatteryController>();
            }
            else if (other.CompareTag("Battery"))
            {
                batteryController = other.GetComponent<BatteryController>();
            }

            if (batteryController != null && batteryController.isPositivePolarity != requiredPolarityIsPositive)
            {
                Debug.Log($"Switch_Mechanic '{gameObject.name}': Polaridad incorrecta. Se requiere {(requiredPolarityIsPositive ? "positiva" : "negativa")}.");
                if (deactivateSound != null) deactivateSound.Play();
                return;
            }
        }

        if (!isActivated)
        {
            ToggleSwitch(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isToggleSwitch) return;

        if (isActivated)
        {
            ToggleSwitch(false);
        }
    }

    private void ToggleSwitch(bool activate)
    {
        isActivated = activate;
        isActive = activate;

        if (ani != null)
        {
            ani.SetBool("goDown", activate);
        }

        if (activate)
        {
            if (activateEffect != null) activateEffect.Play();
            if (activateSound != null) activateSound.Play();
        }
        else
        {
            if (deactivateSound != null) deactivateSound.Play();
        }

        foreach (var target in targets)
        {
            bool targetState = activate ? target.activateOnTrigger : !target.activateOnTrigger;
            if (target.type == TargetType.Platform && target.platform != null)
            {
                if (targetState) target.platform.Activate();
                else target.platform.Deactivate();
            }
            else if ((target.type == TargetType.Door || target.type == TargetType.Laser) && target.activable != null)
            {
                target.activable.Toggle(targetState);
                target.activable.SetIgnoreTrigger(!targetState);
            }
        }

        Debug.Log($"Switch_Mechanic '{gameObject.name}': {(activate ? "Activado" : "Desactivado")}");
    }

    private new void OnDrawGizmos()
    {
        foreach (var target in targets)
        {
            if (target.targetObject == null) continue;
            Gizmos.color = target.activateOnTrigger ? Color.cyan : Color.yellow;
            Gizmos.DrawLine(transform.position, target.targetObject.transform.position);
        }
    }
}
