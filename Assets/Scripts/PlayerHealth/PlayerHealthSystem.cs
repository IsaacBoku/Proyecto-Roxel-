using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthSystem : MonoBehaviour
{
    [SerializeField]
    private PlayerData playerData;

    private int maxHealth;
    private int currentHealth;

    private void Start()
    {
        HealthDefault();
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
        Debug.Log(currentHealth);
    }
    public void Button_Damage()
    {
        TakeDamage(playerData.damage);
    }
}
