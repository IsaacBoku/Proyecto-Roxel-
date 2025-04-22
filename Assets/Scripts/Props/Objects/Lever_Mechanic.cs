using System.Collections.Generic;
using UnityEngine;

public class Lever_Mechanic : InteractableBase
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
    private ConveyorBelt_Mechanic conveyorBelt;

    [SerializeField]
    private bool toggleOnInteract = true; 

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

        foreach (var target in targets)
        {
            if (target.activable != null)
            {
                target.activable.Toggle(isActive);
            }
        }

        if (conveyorBelt != null)
        {
            conveyorBelt.ToggleDirection(isActive);
        }

        UpdateVisuals(isActive);

        Debug.Log($"Botón {gameObject.name} interactuado. Estado: {(isActive ? "Activo" : "Inactivo")}.");
    }
}
