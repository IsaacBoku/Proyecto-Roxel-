using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hammer_Mechanic : MonoBehaviour
{
    private Animator animator;
    public float cooldownTime = 3f;

    public int damage = 1;
    private void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(HammerCycle());
    }

    private IEnumerator HammerCycle()
    {
        while (true)
        {
            animator.SetTrigger("Smash");
            yield return new WaitForSeconds(cooldownTime); 
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerHealthSystem>().TakeDamage(damage);
            Debug.Log("�El jugador ha sido aplastado!");
        }
    }
}
