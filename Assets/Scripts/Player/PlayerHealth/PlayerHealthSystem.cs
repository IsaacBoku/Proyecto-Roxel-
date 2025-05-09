using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthSystem : MonoBehaviour
{
    [SerializeField]
    private PlayerData playerData;

    Player player;

    private int maxLives = 4;
    public int currentLives { get; private set; }
    [Header("Animations")]
    public Animator animhealth;
    public Animator aniGameOver;

    [Header("UI_GameOver")]
    [SerializeField]
    private GameObject gameOverUI;
    [SerializeField] PlayerInputHadler InputHadler;
    public EntityFX fx { get; private set; }

    private void Awake()
    {
        HealthDefault();
        player = GetComponent<Player>();
    }
    private void Start()
    {
        fx = GetComponent<EntityFX>();
        Debug.Log($"Vidas iniciales: {currentLives}");
    }
    private void Update()
    {
        Dead();
    }

    private void HealthDefault()
    {
        currentLives = maxLives;
    }
    public void TakeDamage(int damage)
    {
        currentLives -= damage;

        if (currentLives <= 0)
        {
            currentLives = 0;
            Debug.Log("Jugador muerto por daño.");
        }
        fx.StartCoroutine("FlashFX");
        UpdateHealthAnimation();
        Debug.Log($"Vidas restantes tras daño: {currentLives}");
    }

    public void LoseLife()
    {
        currentLives--;
        if (currentLives <= 0)
        {
            currentLives = 0;
            Debug.Log("Jugador muerto por agotar vidas.");
        }
        fx.StartCoroutine("FlashFX");
        UpdateHealthAnimation();
        Debug.Log($"Vida perdida. Vidas restantes: {currentLives}");
    }
    private void UpdateHealthAnimation()
    {
        if (animhealth == null) return;

        // Ajusta según tu controlador de animaciones
        animhealth.SetInteger("Vidas", currentLives);
    }
    public void Button_Damage()
    {
        TakeDamage(playerData.damage);
    }
    public void Dead()
    {
        if (currentLives <= 0)
        {
            currentLives = 0;
            player.StateMachine.ChangeState(player.DeadState);
            StartCoroutine(GameOver());
            InputHadler.OnPause();
            Debug.Log("Jugador muerto.");
        }
    }
    private IEnumerator GameOver()
    {
        gameOverUI.SetActive(true);
        yield return new WaitForSeconds(1f);

        aniGameOver.SetBool("GameOver", true);
        Cursor.visible = true;
        yield return new WaitForSeconds(2f);
    }
}
