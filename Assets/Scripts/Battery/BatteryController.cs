using UnityEngine;

public class BatteryController : MonoBehaviour
{
    public float energyAmounts = 100f;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Bater�a colision� con: {other.name}");
    }
}
