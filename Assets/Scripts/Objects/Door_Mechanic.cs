using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_Mechanic : MonoBehaviour
{
    Animator ani;
    private BoxCollider2D colliderTrigger;
    public new BoxCollider2D collider;
    public bool ignoreTrigger;

    private void Start()
    {
        ani = GetComponent<Animator>();
        colliderTrigger = GetComponent<BoxCollider2D>();
        DoorClosed();
    }
    public void DoorOpen()
    {
        ani.SetBool("Door_open", true);
    }
    public void DoorClosed()
    {
        ani.SetBool("Door_open", false);
    } 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (ignoreTrigger)
            return;

        if (collision.tag == "Player")
        {
            DoorOpen();
        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (ignoreTrigger)
            return;

        if (collision.tag == "Player")
        {
            DoorClosed();
        }
    }

    void TiggerEnable()
    {
        colliderTrigger.enabled = true;
        collider.enabled = true;
    }

    void TriggerDisable()
    {
        colliderTrigger.enabled = false;
        collider.enabled = false;
    }
    public void Toggle(bool State)
    {
        if (State)
            DoorOpen();
        else
            DoorClosed();
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
