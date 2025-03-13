using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser_Mechanic : MonoBehaviour
{
    Animator ani;

    public int damage = 1;

    private BoxCollider2D colliderTriggers;
    public  BoxCollider2D colliders;

    public bool ignoreTrigger;
    private void Start()
    {
        ani = GetComponent<Animator>();
        colliderTriggers = GetComponent<BoxCollider2D>();

    }
    public void LaserOpen()
    {
        ani.SetBool("LaserOpen", true);
    }
    public void LaserClosed() => ani.SetBool("LaserOpen", false);
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (ignoreTrigger)
            return;

        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerHealthSystem>().TakeDamage(damage);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (ignoreTrigger)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerHealthSystem>().TakeDamage(damage);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (ignoreTrigger)
            return;

        if (collision.tag == "Player")
        {
            LaserClosed();
        }
    }
    void TiggerEnable()
    {
        colliderTriggers.enabled = true;
        colliders.enabled = true;
    }
    void TriggerDisable()
    {
        colliderTriggers.enabled = false;
        colliders.enabled = false;
    }
    public void Toggle(bool State)
    {
        if (State)
            LaserOpen();
        else
            LaserClosed();
    }
    private void OnDrawGizmos()
    {
        if (!ignoreTrigger)
        {
            BoxCollider2D box = GetComponent<BoxCollider2D>();

            Gizmos.DrawWireCube(transform.position, new Vector2(box.size.x, box.size.y));
        }
    }
}
