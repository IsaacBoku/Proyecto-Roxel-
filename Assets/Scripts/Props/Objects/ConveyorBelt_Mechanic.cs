using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConveyorBelt_Mechanic : MonoBehaviour
{
    [SerializeField]
    private float speed = 2f;

    public bool isReversed = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        float direction = isReversed ? -1f : 1f;
        Vector2 conveyorForce = new Vector2(speed * direction, 0);
        rb.AddForce(conveyorForce, ForceMode2D.Impulse);

        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            player.IsOnConveyorBelt = true;
            player.SetConveyorDirection(isReversed ? -1f : 1f);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            player.IsOnConveyorBelt = false;
            player.SetConveyorDirection(0f); 
        }
    }
    public void ToggleDirection(bool state)
    {
        isReversed = state;
        Debug.Log($"Cinta transportadora {gameObject.name} dirección: {(isReversed ? "Invertida" : "Normal")}.");
    }
}
