using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthSystem : MonoBehaviour
{
    [SerializeField]
    private PlayerData playerData;

    Player player;

    private int maxHealth;
    public int currentHealth;
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
        Debug.Log(currentHealth);
        animhealth.SetInteger("Vida", 2);
    }
    private void Update()
    {
        Dead();

    }

    private void HealthDefault()
    {
        maxHealth = playerData.MaxHealth;

        currentHealth = maxHealth;
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if(currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Player is dead");
        }
        fx.StartCoroutine("FlashFX");
        Debug.Log(currentHealth);

        if (currentHealth == 2)
        {
            animhealth.SetBool("Vida", true);
        }
        else if (currentHealth == 1)
        {
            animhealth.SetBool("Vida", false);
        }
    }
    public void Button_Damage()
    {
        TakeDamage(playerData.damage);
    }
    public void Dead()
    {
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            player.StateMachine.ChangeState(player.DeadState);
            StartCoroutine(GameOver());
            InputHadler.OnPause();
            Debug.Log("Player is dead");
        }
    }
    private IEnumerator GameOver()
    {
        gameOverUI.SetActive(true);
        yield return new WaitForSeconds(1f);

        aniGameOver.SetBool("GameOver", true);
        Cursor.visible = true;
        yield return new WaitForSeconds(2f);
        //Time.timeScale = 0;
    }
}
