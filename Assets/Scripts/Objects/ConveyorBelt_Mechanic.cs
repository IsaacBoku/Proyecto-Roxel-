using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt_Mechanic : MonoBehaviour
{
    [SerializeField]
    private float speed = 2f;

    public bool isReversed = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

        if(rb != null)
        {
            float direction = isReversed ? -1f : 1f;
            rb.velocity = new Vector2(speed* direction, rb.velocity.y);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

        if(rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    public void ToggleDirection()
    {
        isReversed = !isReversed;
    }
}
