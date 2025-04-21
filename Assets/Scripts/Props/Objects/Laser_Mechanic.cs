using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser_Mechanic : MonoBehaviour, IActivable
{
    [SerializeField] private Animator ani;
    [SerializeField] private int damage = 1;
    [SerializeField] private BoxCollider2D colliderTriggers;
    [SerializeField] private BoxCollider2D colliders;
    public bool ignoreTrigger;

    private void Start()
    {
        // Intentar obtener Animator si no está asignado
        if (ani == null)
        {
            ani = GetComponent<Animator>();
            if (ani == null)
                ani = GetComponentInChildren<Animator>();
            if (ani == null)
                Debug.LogWarning($"Laser_Mechanic '{gameObject.name}': No se encontró Animator en el GameObject o sus hijos.");
        }

        // Intentar obtener colliderTriggers si no está asignado
        if (colliderTriggers == null)
        {
            colliderTriggers = GetComponent<BoxCollider2D>();
            if (colliderTriggers == null)
                Debug.LogWarning($"Laser_Mechanic '{gameObject.name}': No se encontró BoxCollider2D para colliderTriggers.");
        }

        // Advertencia si colliders no está asignado
        if (colliders == null)
        {
            Debug.LogWarning($"Laser_Mechanic '{gameObject.name}': El campo 'colliders' no está asignado en el Inspector.");
        }

        LaserClosed();
    }

    public void LaserOpen()
    {
        if (ani != null)
            ani.SetBool("LaserOpen", true);
        else
            Debug.LogWarning($"Laser_Mechanic '{gameObject.name}': Intento de abrir láser, pero Animator es null.");
    }

    public void LaserClosed()
    {
        if (ani != null)
            ani.SetBool("LaserOpen", false);
        else
            Debug.LogWarning($"Laser_Mechanic '{gameObject.name}': Intento de cerrar láser, pero Animator es null.");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (ignoreTrigger)
            return;

        if (collision.CompareTag("Player"))
        {
            var healthSystem = collision.GetComponent<PlayerHealthSystem>();
            if (healthSystem != null)
                healthSystem.TakeDamage(damage);
            else
                Debug.LogWarning($"Laser_Mechanic '{gameObject.name}': El jugador no tiene PlayerHealthSystem.");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (ignoreTrigger)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            var healthSystem = collision.gameObject.GetComponent<PlayerHealthSystem>();
            if (healthSystem != null)
                healthSystem.TakeDamage(damage);
            else
                Debug.LogWarning($"Laser_Mechanic '{gameObject.name}': El jugador no tiene PlayerHealthSystem.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (ignoreTrigger)
            return;

        if (collision.CompareTag("Player"))
        {
            LaserClosed();
        }
    }

    private void TriggerEnable()
    {
        if (colliderTriggers != null)
            colliderTriggers.enabled = true;
        else
            Debug.LogWarning($"Laser_Mechanic '{gameObject.name}': No se puede habilitar colliderTriggers, es null.");

        if (colliders != null)
            colliders.enabled = true;
        else
            Debug.LogWarning($"Laser_Mechanic '{gameObject.name}': No se puede habilitar colliders, es null.");
    }

    private void TriggerDisable()
    {
        if (colliderTriggers != null)
            colliderTriggers.enabled = false;
        else
            Debug.LogWarning($"Laser_Mechanic '{gameObject.name}': No se puede deshabilitar colliderTriggers, es null.");

        if (colliders != null)
            colliders.enabled = false;
        else
            Debug.LogWarning($"Laser_Mechanic '{gameObject.name}': No se puede deshabilitar colliders, es null.");
    }

    public void Toggle(bool state)
    {
        if (state)
            LaserOpen();
        else
            LaserClosed();
    }

    public void SetIgnoreTrigger(bool ignore)
    {
        ignoreTrigger = ignore;
    }

    private void OnDrawGizmos()
    {
        if (!ignoreTrigger && colliderTriggers != null)
        {
            Gizmos.DrawWireCube(transform.position, new Vector2(colliderTriggers.size.x, colliderTriggers.size.y));
        }
    }

}
