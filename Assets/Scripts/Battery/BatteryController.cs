using UnityEngine;

public class BatteryController : MonoBehaviour
{
    [Header("Battery Energy")]
    public float energyAmounts = 100f;
    public float maxEnergy = 100f;
    public bool isPositivePolarity = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Batería colisionó con: {other.name}");
    }
}
