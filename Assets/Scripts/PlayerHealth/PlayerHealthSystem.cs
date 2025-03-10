using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthSystem : MonoBehaviour
{
    [SerializeField]
    private PlayerData playerData;

    private int maxHealth;
    private int currentHealth;
    public EntityFX fx { get; private set; }

    private void Start()
    {
        HealthDefault();
        fx = GetComponent<EntityFX>();
        Debug.Log(currentHealth);
    }

    private void HealthDefault()
    {
        maxHealth = playerData.MaxHealth;

        currentHealth = maxHealth;
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        fx.StartCoroutine("FlashFX");
        Debug.Log(currentHealth);
    }
    public void Button_Damage()
    {
        TakeDamage(playerData.damage);
    }
}
