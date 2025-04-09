using TMPro;
using UnityEngine;

public class UI_BatteryPRuebas : MonoBehaviour
{
    public TextMeshProUGUI batteryEnergy;
    public BatteryController battery;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UI_EnergyBattery();
    }
    public void UI_EnergyBattery()
    {
        string text_energy = battery.energyAmounts.ToString();
        batteryEnergy.text = text_energy;
    }
}
