using TMPro;
using UnityEngine;

public class UI_BatteryPRuebas : MonoBehaviour
{
    public TextMeshProUGUI batteryEnergy;
    public TextMeshProUGUI tiembattery;
    public BatteryController battery;
    public Player player;

    void Start()
    {
        
    }

    void Update()
    {
        UI_EnergyBattery();
        UI_VidaBattery();
    }
    public void UI_EnergyBattery()
    {
        //string text_energy = battery.energyAmounts.ToString();
        //batteryEnergy.text = text_energy;
    }
    public void UI_VidaBattery()
    {
        string text_vida = player.currentLifeProgress.ToString();
        tiembattery.text = text_vida;
    }
}
