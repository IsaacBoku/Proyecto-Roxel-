using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hammer_Mechanic : MonoBehaviour
{
    [Header("Hammer Settings")]
    [SerializeField, Tooltip("Tiempo entre golpes del martillo (en segundos)")]
    private float cooldownTime = 3f;

    [SerializeField, Tooltip("Daño que inflige el martillo al colisionar")]
    private int damage = 1;

    [SerializeField, Tooltip("Capas de los objetos que pueden ser afectados por el martillo")]
    private LayerMask damageableLayerMask;

    [Header("Audio Settings")]
    [SerializeField, Tooltip("Nombre del sonido del martillo en el AudioManager")]
    private string smashSoundName = "HammerSmash";

    [Header("Debug Settings")]
    [SerializeField, Tooltip("Muestra el área de colisión del martillo en el editor")]
    private bool showDebugGizmos = true;

    private Animator animator;
    private BoxCollider2D hammerCollider; // Para visualizar el área de colisión en Gizmos
    private Coroutine hammerCycleCoroutine;
    private bool isActive = true; // Para controlar si el martillo está activo

    private void Awake()
    {
        // Cachear componentes
        animator = GetComponent<Animator>();
        hammerCollider = GetComponent<BoxCollider2D>();

        if (animator == null)
        {
            Debug.LogError($"Hammer_Mechanic '{gameObject.name}': No se encontró Animator en el GameObject. El martillo no funcionará.");
            enabled = false;
            return;
        }

        if (hammerCollider == null)
        {
            Debug.LogWarning($"Hammer_Mechanic '{gameObject.name}': No se encontró BoxCollider2D. No se podrá visualizar el área de colisión en Gizmos.");
        }

        // Validar cooldownTime
        if (cooldownTime <= 0f)
        {
            Debug.LogWarning($"Hammer_Mechanic '{gameObject.name}': cooldownTime debe ser mayor que 0. Ajustando a 0.1.");
            cooldownTime = 0.1f;
        }

        if (damageableLayerMask.value == 0)
        {
            Debug.LogWarning($"Hammer_Mechanic '{gameObject.name}': damageableLayerMask está vacío. El martillo no afectará a ningún objeto.");
        }
    }

    private void Start()
    {
        StartHammerCycle();
    }

    private void StartHammerCycle()
    {
        if (hammerCycleCoroutine != null)
        {
            StopCoroutine(hammerCycleCoroutine);
        }
        hammerCycleCoroutine = StartCoroutine(HammerCycle());
    }

    private IEnumerator HammerCycle()
    {
        while (true)
        {
            if (isActive)
            {
                animator.SetTrigger("Smash");
                AudioManager.instance.PlaySFX(smashSoundName);
            }
            yield return new WaitForSeconds(cooldownTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsLayerInMask(collision.gameObject.layer, damageableLayerMask)) return;

        // Verificar si el objeto tiene un componente de salud genérico
        IHealthSystem healthSystem = collision.gameObject.GetComponent<IHealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(damage);
            Debug.Log($"Hammer_Mechanic '{gameObject.name}': Infligió {damage} de daño a '{collision.gameObject.name}'.");
        }
        else
        {
            Debug.LogWarning($"Hammer_Mechanic '{gameObject.name}': '{collision.gameObject.name}' está en damageableLayerMask pero no tiene IHealthSystem.");
        }
    }

    #region Public Methods
    public void SetActive(bool active)
    {
        isActive = active;
        if (!isActive)
        {
            animator.ResetTrigger("Smash");
            Debug.Log($"Hammer_Mechanic '{gameObject.name}': Martillo desactivado.");
        }
        else
        {
            Debug.Log($"Hammer_Mechanic '{gameObject.name}': Martillo activado.");
        }
    }

    public void SetCooldownTime(float newCooldown)
    {
        if (newCooldown <= 0f)
        {
            Debug.LogWarning($"Hammer_Mechanic '{gameObject.name}': newCooldown debe ser mayor que 0. Ajustando a 0.1.");
            newCooldown = 0.1f;
        }

        cooldownTime = newCooldown;
        StartHammerCycle(); // Reiniciar el ciclo con el nuevo cooldown
        Debug.Log($"Hammer_Mechanic '{gameObject.name}': cooldownTime actualizado a {cooldownTime} segundos.");
    }

    public void SetDamage(int newDamage)
    {
        damage = Mathf.Max(0, newDamage);
        Debug.Log($"Hammer_Mechanic '{gameObject.name}': Daño actualizado a {damage}.");
    }
    #endregion

    #region Helper Methods
    private bool IsLayerInMask(int layer, LayerMask layerMask)
    {
        return ((1 << layer) & layerMask.value) != 0;
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || hammerCollider == null) return;

        Gizmos.color = Color.red;
        Vector3 colliderSize = new Vector3(hammerCollider.size.x, hammerCollider.size.y, 1f);
        Gizmos.DrawWireCube(hammerCollider.bounds.center, colliderSize);
    }
    #endregion
}
