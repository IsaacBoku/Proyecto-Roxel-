using System.Collections.Generic;
using UnityEngine;

public class Lever_Mechanic : InteractableBase
{
    public enum TargetType
    {
        Door,
        Laser,
        ConveyorBelt
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
    private bool toggleOnInteract = true;

    private Animator anim;

    [Header("Audio Settings")]
    [SerializeField, Tooltip("Nombre del sonido al activar la palanca en el AudioManager")]
    private string activateSoundName = "LeverOn";
    [SerializeField, Tooltip("Nombre del sonido al desactivar la palanca en el AudioManager")]
    private string deactivateSoundName = "LeverOff";

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    protected override void Start()
    {
        base.Start();
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
                    case TargetType.ConveyorBelt:
                        target.activable = target.targetObject.GetComponent<ConveyorBelt_Mechanic>();
                        break;
                }

                if (target.activable != null)
                {
                    target.activable.Toggle(isActive);
                    target.activable.SetIgnoreTrigger(true);
                }
                else
                {
                    Debug.LogWarning($"El objetivo {target.targetObject.name} no tiene un componente {target.type} válido.");
                }
            }
        }
    }

    public override void Interact()
    {
        if (toggleOnInteract)
        {
            isActive = !isActive;
        }
        else
        {
            isActive = true;
        }

        if (isActive)
        {
            anim.SetBool("Open", true);
        }
        else
        {
            anim.SetBool("Open", false);
        }

        foreach (var target in targets)
        {
            if (target.activable != null)
            {
                target.activable.Toggle(isActive);
                if (target.type == TargetType.ConveyorBelt)
                {
                    ConveyorBelt_Mechanic conveyor = target.targetObject.GetComponent<ConveyorBelt_Mechanic>();
                    if (conveyor != null)
                    {
                        conveyor.ToggleDirection(isActive);
                    }
                }
            }
        }

        if (isActive)
        {
            AudioManager.instance.PlaySFX(activateSoundName);
        }
        else
        {
            AudioManager.instance.PlaySFX(deactivateSoundName);
        }

        UpdateVisuals(isActive);

        Debug.Log($"Botón {gameObject.name} interactuado. Estado: {(isActive ? "Activo" : "Inactivo")}.");
    }
}
