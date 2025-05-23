using System.Collections.Generic;
using UnityEngine;

public class PlatformPedestal : InteractableBase
{
    public enum TargetType
    {
        Platform,
        Laser,
        Door
    }

    [System.Serializable]
    public class TargetEntry
    {
        public TargetType type;
        public GameObject targetObject;
        [HideInInspector] public IActivable activable;
        [HideInInspector] public MovingPlatform platform;
    }

    [SerializeField] private List<TargetEntry> targets = new List<TargetEntry>();
    [SerializeField] private ParticleSystem activateEffect;
    [SerializeField] private Transform batteryConnectionPoint;
    private bool hasBattery = false;
    private SpriteRenderer sr;
    private Animator anim;
    private GameObject connectedBattery;

    [Header("Audio Settings")]
    [SerializeField, Tooltip("Nombre del sonido al activar el pedestal en el AudioManager")]
    private string activateSoundName = "PedestalOn";
    [SerializeField, Tooltip("Nombre del sonido al desactivar el pedestal en el AudioManager")]
    private string deactivateSoundName = "PedestalOff";

    public bool HasBattery => hasBattery;

    protected override void Start()
    {
        base.Start();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        if (requiresSpecificPolarity && sr != null)
        {
            sr.color = requiredPolarityIsPositive ? Color.red : Color.blue;
        }

        foreach (var target in targets)
        {
            if (target.targetObject == null)
            {
                Debug.LogWarning($"PlatformPedestal '{gameObject.name}': Un objetivo en la lista 'Targets' no tiene GameObject asignado.");
                continue;
            }

            switch (target.type)
            {
                case TargetType.Platform:
                    target.platform = target.targetObject.GetComponent<MovingPlatform>();
                    if (target.platform == null)
                    {
                        Debug.LogWarning($"PlatformPedestal '{gameObject.name}': El objetivo '{target.targetObject.name}' no tiene un componente MovingPlatform.");
                    }
                    break;
                case TargetType.Laser:
                case TargetType.Door:
                    target.activable = target.targetObject.GetComponent<IActivable>();
                    if (target.activable == null)
                    {
                        Debug.LogWarning($"PlatformPedestal '{gameObject.name}': El objetivo '{target.targetObject.name}' no tiene un componente IActivable.");
                    }
                    break;
            }
        }
    }

    public override void Interact()
    {
        Debug.Log($"PlatformPedestal '{gameObject.name}': No se puede interactuar directamente. Coloca una batería.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Battery"))
        {
            var batteryController = other.GetComponent<BatteryController>();
            if (requiresSpecificPolarity && batteryController.isPositivePolarity != requiredPolarityIsPositive)
            {
                Debug.Log($"PlatformPedestal '{gameObject.name}': Polaridad incorrecta. Se requiere {(requiredPolarityIsPositive ? "positiva" : "negativa")}.");
                return;
            }

            // Guardar referencia a la batería
            connectedBattery = other.gameObject;
            hasBattery = true;

            // Posicionar la batería en el punto de conexión
            if (batteryConnectionPoint != null)
            {
                connectedBattery.transform.position = batteryConnectionPoint.position;
                connectedBattery.transform.rotation = batteryConnectionPoint.rotation;

                // Desactivar física para que no se mueva
                Rigidbody2D rb = connectedBattery.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }
            }

            ActivateTargets();
            if (anim != null) anim.SetBool("Power", true);
            if (activateEffect != null) activateEffect.Play();
            AudioManager.instance.PlaySFX(activateSoundName);
            Debug.Log($"PlatformPedestal '{gameObject.name}': Objetivos activados");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Battery") && other.gameObject == connectedBattery)
        {
            // Restaurar la física de la batería
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }

            hasBattery = false;
            connectedBattery = null;
            DeactivateTargets();
            if (anim != null) anim.SetBool("Power", false);
            AudioManager.instance.PlaySFX(deactivateSoundName);
            Debug.Log($"PlatformPedestal '{gameObject.name}': Objetivos desactivados");
        }
    }

    private void ActivateTargets()
    {
        foreach (var target in targets)
        {
            if (target.type == TargetType.Platform && target.platform != null)
            {
                target.platform.Activate();
            }
            else if ((target.type == TargetType.Laser || target.type == TargetType.Door) && target.activable != null)
            {
                target.activable.Toggle(true);
                target.activable.SetIgnoreTrigger(true);
            }
        }
    }

    private void DeactivateTargets()
    {
        foreach (var target in targets)
        {
            if (target.type == TargetType.Platform && target.platform != null)
            {
                target.platform.Deactivate();
            }
            else if ((target.type == TargetType.Laser || target.type == TargetType.Door) && target.activable != null)
            {
                target.activable.Toggle(false);
                target.activable.SetIgnoreTrigger(true);
            }
        }
    }
}
