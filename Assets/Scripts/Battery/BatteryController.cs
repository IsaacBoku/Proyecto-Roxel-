using UnityEngine;

public class BatteryController : MonoBehaviour
{
    [Header("Battery Energy")]
    public int maxBatteryPoints = 4;
    public int batteryPoints = 4;
    public bool isPositivePolarity = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log($"Batería colisionó con: {other.name}");
    }

    // Método para consumir puntos de batería
    public bool ConsumeBatteryPoints(int points)
    {
        if (batteryPoints >= points)
        {
            batteryPoints -= points;
            batteryPoints = Mathf.Clamp(batteryPoints, 0, maxBatteryPoints);
            return true; // Consumo exitoso
        }
        return false; // No hay suficientes puntos
    }

    public void RechargeBatteryPoints(int points)
    {
        batteryPoints += points;
        batteryPoints = Mathf.Clamp(batteryPoints, 0, maxBatteryPoints);
    }
}
