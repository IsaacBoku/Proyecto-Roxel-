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
            Debug.Log("Player is dead");
        }
    }
}
