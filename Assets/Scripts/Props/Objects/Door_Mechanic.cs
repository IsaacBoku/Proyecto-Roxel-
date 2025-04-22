using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_Mechanic : MonoBehaviour, IActivable
{
    [SerializeField] private Animator ani;
    [SerializeField] private BoxCollider2D colliderTrigger;
    [SerializeField] public  new BoxCollider2D collider;
    public bool ignoreTrigger;

    private void Start()
    {
        if (ani == null)
        {
            ani = GetComponent<Animator>();
            if (ani == null)
                ani = GetComponentInChildren<Animator>();
            if (ani == null)
                Debug.LogWarning($"Door_Mechanic '{gameObject.name}': No se encontró Animator en el GameObject o sus hijos.");
        }

        if (colliderTrigger == null)
        {
            colliderTrigger = GetComponent<BoxCollider2D>();
            if (colliderTrigger == null)
                Debug.LogWarning($"Door_Mechanic '{gameObject.name}': No se encontró BoxCollider2D para colliderTrigger.");
        }

        if (collider == null)
        {
            Debug.LogWarning($"Door_Mechanic '{gameObject.name}': El campo 'collider' no está asignado en el Inspector.");
        }

        DoorClosed();
    }

    public void DoorOpen()
    {
        if (ani != null)
            ani.SetBool("Door_open", true);
        else
            Debug.LogWarning($"Door_Mechanic '{gameObject.name}': Intento de abrir puerta, pero Animator es null.");
    }

    public void DoorClosed()
    {
        if (ani != null)
            ani.SetBool("Door_open", false);
        else
            Debug.LogWarning($"Door_Mechanic '{gameObject.name}': Intento de cerrar puerta, pero Animator es null.");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (ignoreTrigger)
            return;

        if (collision.CompareTag("Player"))
        {
            DoorOpen();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (ignoreTrigger)
            return;

        if (collision.CompareTag("Player"))
        {
            DoorClosed();
        }
    }

    private void TriggerEnable()
    {
        if (colliderTrigger != null)
            colliderTrigger.enabled = true;
        else
            Debug.LogWarning($"Door_Mechanic '{gameObject.name}': No se puede habilitar colliderTrigger, es null.");

        if (collider != null)
            collider.enabled = true;
        else
            Debug.LogWarning($"Door_Mechanic '{gameObject.name}': No se puede habilitar collider, es null.");
    }

    private void TriggerDisable()
    {
        if (colliderTrigger != null)
            colliderTrigger.enabled = false;
        else
            Debug.LogWarning($"Door_Mechanic '{gameObject.name}': No se puede deshabilitar colliderTrigger, es null.");

        if (collider != null)
            collider.enabled = false;
        else
            Debug.LogWarning($"Door_Mechanic '{gameObject.name}': No se puede deshabilitar collider, es null.");
    }

    public void Toggle(bool state)
    {
        if (state)
            DoorOpen();
        else
            DoorClosed();
    }

    public void SetIgnoreTrigger(bool ignore)
    {
        ignoreTrigger = ignore;
    }

    private void OnDrawGizmos()
    {
        if (!ignoreTrigger && colliderTrigger != null)
        {
            Gizmos.DrawWireCube(transform.position, new Vector2(colliderTrigger.size.x, colliderTrigger.size.y));
        }
    }

}
