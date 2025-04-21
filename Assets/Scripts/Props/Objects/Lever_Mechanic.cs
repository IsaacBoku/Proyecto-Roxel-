using System.Collections.Generic;
using UnityEngine;

public class Lever_Mechanic : InteractableBase
{
    // Enum para tipos de objetivos
    public enum TargetType
    {
        Door,
        Laser
    }

    // Clase para objetivos
    [System.Serializable]
    public class TargetEntry
    {
        public TargetType type;
        public GameObject targetObject;
        [HideInInspector] public IActivable activable;
    }

    [SerializeField]
    private List<TargetEntry> targets = new List<TargetEntry>(); // Objetivos

    [SerializeField]
    private ConveyorBelt_Mechanic conveyorBelt; // Cinta transportadora (opcional)

    [SerializeField]
    private bool toggleOnInteract = true; // Alternar o solo activar

    protected override void Start()
    {
        base.Start();
        // Inicializar objetivos
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
            isActive = !isActive; // Alternar estado
        }
        else
        {
            isActive = true; // Solo activar
        }

        // Activar/desactivar objetivos
        foreach (var target in targets)
        {
            if (target.activable != null)
            {
                target.activable.Toggle(isActive);
            }
        }

        // Cambiar dirección de la cinta si está asignada
        if (conveyorBelt != null)
        {
            conveyorBelt.ToggleDirection(isActive);
        }

        // Actualizar efectos
        UpdateVisuals(isActive);

        Debug.Log($"Botón {gameObject.name} interactuado. Estado: {(isActive ? "Activo" : "Inactivo")}.");
    }
}
