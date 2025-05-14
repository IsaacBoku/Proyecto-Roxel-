using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IActivable
{
    public bool isActive { get; protected set; } = false;
    [SerializeField] protected bool requiresSpecificPolarity = false;
    [SerializeField] protected bool requiredPolarityIsPositive = true;

    [SerializeField]
    protected ParticleSystem effect;


    protected virtual void Start()
    {
    }
    protected virtual void Update()
    {
    }

    public abstract void Interact();

    public virtual void Toggle(bool state)
    {
        isActive = state;
        UpdateVisuals(state);
        Debug.Log($"{gameObject.name} estado cambiado a: {(state ? "Activo" : "Inactivo")}.");
    }

    public virtual void SetIgnoreTrigger(bool ignore)
    {
    }

    protected virtual void UpdateVisuals(bool state)
    {
        if (effect != null)
        {
            if (state)
            {
                var main = effect.main;
                main.startColor = requiresSpecificPolarity ? (requiredPolarityIsPositive ? Color.red : Color.blue) : Color.white;
                effect.Play();
            }
            else
            {
                effect.Stop();
            }
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
