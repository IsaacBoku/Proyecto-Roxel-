using UnityEngine;

public class BatteryController : MonoBehaviour
{
    public float energyAmounts = 100f;
    [SerializeField] private Vector2 launchDir;
    [SerializeField] private float batteryGravity;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Bater�a colision� con: {other.name}");
    }
}
