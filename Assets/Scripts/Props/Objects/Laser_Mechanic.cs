using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser_Mechanic : MonoBehaviour, IActivable
{
    [SerializeField] private Animator ani;
    [SerializeField] private int damage = 1;
    [SerializeField] private BoxCollider2D colliderTriggers;
    [SerializeField] private BoxCollider2D colliders;
    public bool ignoreTrigger = false;

    [Header("Audio Settings")]
    [SerializeField, Tooltip("Nombre del sonido al activar el l�ser en el AudioManager")]
    private string openSoundName = "LaserOn";
    [SerializeField, Tooltip("Nombre del sonido al desactivar el l�ser en el AudioManager")]
    private string closeSoundName = "LaserOff";
    [SerializeField, Tooltip("Nombre del sonido al infligir da�o en el AudioManager")]
    private string damageSoundName = "LaserHit";

    private void Start()
    {
        if (ani == null)
        {
            ani = GetComponent<Animator>();
            if (ani == null)
                ani = GetComponentInChildren<Animator>();
            if (ani == null)
                Debug.LogWarning($"Laser_Mechanic '{gameObject.name}': No se encontr� Animator en el GameObject o sus hijos.");
        }

        if (colliderTriggers == null)
        {
            colliderTriggers = GetComponent<BoxCollider2D>();
            if (colliderTriggers == null)
                Debug.LogWarning($"Laser_Mechanic '{gameObject.name}': No se encontr� BoxCollider2D para colliderTriggers.");
        }

        if (colliders == null)
        {
            Debug.LogWarning($"Laser_Mechanic '{gameObject.name}': El campo 'colliders' no est� asignado en el Inspector.");
        }

        LaserClosed();
    }

    public void LaserOpen()
    {
        if (ani != null)
        {
            ani.SetBool("LaserOpen", true);
            AudioManager.instance.PlaySFX(openSoundName);
        }
        else
        {
            Debug.LogWarning($"Laser_Mechanic '{gameObject.name}': Intento de abrir l�ser, pero Animator es null.");
        }
    }

    public void LaserClosed()
    {
        if (ani != null)
        {
            ani.SetBool("LaserOpen", false);
            AudioManager.instance.PlaySFX(closeSoundName);
        }
        else
        {
            Debug.LogWarning($"Laser_Mechanic '{gameObject.name}': Intento de cerrar l�ser, pero Animator es null.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ApplyDamage(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ApplyDamage(collision.gameObject);
        }
    }

    private void ApplyDamage(GameObject target)
    {
        Debug.Log($"Laser_Mechanic '{gameObject.name}': Infligi� da�o al jugador.");
        var healthSystem = target.GetComponent<PlayerHealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(damage);
            AudioManager.instance.PlaySFX(damageSoundName);
        }
        else
        {
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
