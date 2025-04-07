using UnityEngine;

public class BatteryController : MonoBehaviour
{
    public float energyAmounts = 100f;
    [SerializeField] private Vector2 launchDir;
    [SerializeField] private float batteryGravity;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Batería colisionó con: {other.name}");
    }
}
