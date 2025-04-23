using System.Collections.Generic;
using UnityEngine;

public class PlatformPedestal : InteractableBase
{
    // Enum para tipos de objetivos
    public enum TargetType
    {
        Platform,
        Laser,
        Door
    }

    // Clase para objetivos
    [System.Serializable]
    public class TargetEntry
    {
        public TargetType type;
        public GameObject targetObject;
        [HideInInspector] public IActivable activable; // Para Laser y Door
        [HideInInspector] public MovingPlatform platform; // Para Platform
    }

    [SerializeField]
    private List<TargetEntry> targets = new List<TargetEntry>();

    [SerializeField]
    private ParticleSystem activateEffect;

    [SerializeField]
    private AudioSource activateSound;

    private bool hasBattery = false;

    // Propiedad pública para Player
    public bool HasBattery => hasBattery;

    protected override void Start()
    {
        base.Start();
        // Inicializar objetivos
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
                    target.activable = target.targetObject.GetComponent<IActivable>();
                    if (target.activable == null)
                    {
                        Debug.LogWarning($"PlatformPedestal '{gameObject.name}': El objetivo '{target.targetObject.name}' no tiene un componente IActivable (Laser).");
                    }
                    break;
                case TargetType.Door:
                    target.activable = target.targetObject.GetComponent<IActivable>();
                    if (target.activable == null)
                    {
                        Debug.LogWarning($"PlatformPedestal '{gameObject.name}': El objetivo '{target.targetObject.name}' no tiene un componente IActivable (Door).");
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
            hasBattery = true;
            ActivateTargets();
            if (activateEffect != null) activateEffect.Play();
            if (activateSound != null) activateSound.Play();
            Debug.Log($"PlatformPedestal '{gameObject.name}': Objetivos activados");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Battery"))
        {
            hasBattery = false;
            DeactivateTargets();
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
                target.activable.SetIgnoreTrigger(false);
            }
        }
    }
}
