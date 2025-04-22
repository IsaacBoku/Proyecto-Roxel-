using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IActivable
{
    public bool isActive { get; protected set; } = false;

    [SerializeField]
    protected ParticleSystem effect;

    [SerializeField]
    protected AudioSource sound;

    protected virtual void Start()
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
                main.startColor = Color.green;
                effect.Play();
            }
            else
            {
                effect.Stop();
            }
        }
        if (sound != null)
        {
            if (state)
                sound.Play();
            else
                sound.Stop();
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
