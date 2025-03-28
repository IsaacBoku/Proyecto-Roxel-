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
        rb.AddForce(conveyorForce, ForceMode2D.Force);

        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            player.isOnConveyorBelt = true;
            player.SetConveyorDirection(isReversed ? -1f : 1f); // Le pasamos la direcci�n de la cinta
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            player.isOnConveyorBelt = false;
            player.SetConveyorDirection(0f); // Resetear la direcci�n de la cinta al salir
        }
    }
}
