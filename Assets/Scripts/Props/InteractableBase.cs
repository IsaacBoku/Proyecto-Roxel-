using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IActivable
{
    [SerializeField]
    protected bool isActive = false; // Estado del interactivo

    [SerializeField]
    protected ParticleSystem effect; // Efecto de partículas

    [SerializeField]
    protected AudioSource sound; // Sonido

    // Método Start virtual para que las clases hijas lo sobrescriban
    protected virtual void Start()
    {
        // Puede estar vacío o incluir inicializaciones genéricas si es necesario
    }

    // Método abstracto para la interacción
    public abstract void Interact();

    // Implementación de IActivable
    public virtual void Toggle(bool state)
    {
        isActive = state;
        UpdateVisuals(state);
        Debug.Log($"{gameObject.name} estado cambiado a: {(state ? "Activo" : "Inactivo")}.");
    }

    public virtual void SetIgnoreTrigger(bool ignore)
    {
        // Por defecto, no se usa
    }

    // Actualizar efectos visuales y sonoros
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

    // Gizmo para depuración
    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
