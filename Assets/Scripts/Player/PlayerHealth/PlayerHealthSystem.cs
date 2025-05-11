using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthSystem : MonoBehaviour, IHealthSystem
{
    [SerializeField] private PlayerData playerData;
    private Player player;

    [Header("Health Settings")]
    [SerializeField, Tooltip("Número máximo de vidas del jugador")]
    private int maxLives = 4;
    public int currentLives { get; private set; }

    [Header("Animations")]
    [SerializeField, Tooltip("Animator para la barra de salud")]
    private Animator animHealth;
    [SerializeField, Tooltip("Animator para la pantalla de game over")]
    private Animator aniGameOver;

    [Header("UI and Input")]
    [SerializeField, Tooltip("UI de game over")]
    private GameObject gameOverUI;
    [SerializeField] private PlayerInputHadler inputHandler;

    [Header("Audio Settings")]
    [SerializeField, Tooltip("Nombre del sonido al recibir daño en el AudioManager")]
    private string damageSoundName = "PlayerHit";
    [SerializeField, Tooltip("Nombre del sonido al morir en el AudioManager")]
    private string deathSoundName = "PlayerDeath";

    [Header("Invulnerability")]
    [SerializeField, Tooltip("Tiempo de invulnerabilidad tras recibir daño (en segundos)")]
    private float invulnerabilityDuration = 1f;

    private EntityFX fx;
    private bool isInvulnerable = false;
    private bool isDead = false;

    private void Awake()
    {
        // Cachear componentes
        player = GetComponent<Player>();
        fx = GetComponent<EntityFX>();

        // Validaciones
        if (player == null)
        {
            Debug.LogError($"PlayerHealthSystem '{gameObject.name}': No se encontró componente Player.");
            enabled = false;
            return;
        }

        if (animHealth == null)
        {
            Debug.LogWarning($"PlayerHealthSystem '{gameObject.name}': animHealth no asignado. La animación de salud no funcionará.");
        }

        if (aniGameOver == null)
        {
            Debug.LogWarning($"PlayerHealthSystem '{gameObject.name}': aniGameOver no asignado. La animación de game over no funcionará.");
        }

        if (gameOverUI == null)
        {
            Debug.LogWarning($"PlayerHealthSystem '{gameObject.name}': gameOverUI no asignado. La UI de game over no se mostrará.");
        }

        if (inputHandler == null)
        {
            Debug.LogWarning($"PlayerHealthSystem '{gameObject.name}': inputHandler no asignado. No se pausará la entrada.");
        }

        // Inicializar salud
        InitializeHealth();
    }

    private void Start()
    {
        Debug.Log($"Vidas iniciales: {currentLives}");
    }

    private void InitializeHealth()
    {
        currentLives = maxLives > 0 ? maxLives : 4; // Usar valor por defecto si maxLives es inválido
        UpdateHealthAnimation();
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable || isDead || damage <= 0) return;

        currentLives -= damage;
        Debug.Log($"PlayerHealthSystem '{gameObject.name}': Recibió {damage} de daño. Vidas restantes: {currentLives}");

        AudioManager.instance.PlaySFX(damageSoundName); // Reproducir sonido de daño

        if (currentLives <= 0)
        {
            currentLives = 0;
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityCoroutine());
        }

        UpdateHealthAnimation();
        if (fx != null) fx.StartCoroutine("FlashFX");
    }

    public void LoseLife()
    {
        if (isInvulnerable || isDead) return;

        currentLives--;
        Debug.Log($"PlayerHealthSystem '{gameObject.name}': Perdida 1 vida. Vidas restantes: {currentLives}");

        AudioManager.instance.PlaySFX(damageSoundName); // Reproducir sonido de daño

        if (currentLives <= 0)
        {
            currentLives = 0;
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityCoroutine());
        }

        UpdateHealthAnimation();
        if (fx != null) fx.StartCoroutine("FlashFX");
    }

    private void UpdateHealthAnimation()
    {
        if (animHealth != null)
        {
            animHealth.SetInteger("Vidas", currentLives);
        }
    }

    public void Button_Damage()
    {
        if (playerData != null && playerData.damage > 0)
        {
            TakeDamage(playerData.damage);
        }
        else
        {
            Debug.LogWarning($"PlayerHealthSystem '{gameObject.name}': playerData.damage no está configurado correctamente.");
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        AudioManager.instance.PlaySFX(deathSoundName); // Reproducir sonido de muerte

        if (player != null)
        {
            player.StateMachine.ChangeState(player.DeadState);
        }

        StartCoroutine(GameOverCoroutine());
        if (inputHandler != null)
        {
            inputHandler.OnPause();
        }

        Debug.Log($"PlayerHealthSystem '{gameObject.name}': Jugador muerto.");
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    private IEnumerator GameOverCoroutine()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            yield return new WaitForSeconds(1f);
        }

        if (aniGameOver != null)
        {
            aniGameOver.SetBool("GameOver", true);
            Cursor.visible = true;
            yield return new WaitForSeconds(2f);
        }

        // Opcional: Desactivar movimiento o otros sistemas
        if (player != null)
        {
            player.enabled = false; // Desactivar el Player para evitar movimiento
        }
    }

    #region Public Methods
    public void ResetHealth()
    {
        InitializeHealth();
        isDead = false;
        isInvulnerable = false;
        UpdateHealthAnimation();
        Debug.Log($"PlayerHealthSystem '{gameObject.name}': Salud reiniciada a {currentLives} vidas.");
    }

    public bool IsAlive()
    {
        return currentLives > 0 && !isDead;
    }
    #endregion
}

public interface IHealthSystem
{
    void TakeDamage(int damage);
}