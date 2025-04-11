using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI crystalText;
    [SerializeField] private TextMeshProUGUI upgradeNotificationText;
    [SerializeField] private TextMeshProUGUI availableUpgradesText;
    [SerializeField] private Slider energyBar; // Barra de energía
    [SerializeField] private Slider timerBar; // Barra del temporizador
    [SerializeField] private Image energyBarFill; // Asigna el componente Image del Fill del EnergyBar
    [SerializeField] private Image timerBarFill;  // Asigna el componente Image del Fill del TimerBar
    [SerializeField] private float notificationDuration = 3f;
    [SerializeField] private float blinkSpeed = 2f;
    [SerializeField] private float blinkThreshold = 0.8f; // Parpadea cuando el temporizador está al 80%
    private Player player;
    private BatteryController batteryController; // Para acceder a la energía de la batería
    private int crystalsPerUpgrade;
    private float blinkTimer = 0f;

    void Start()
    {
        player = FindAnyObjectByType<Player>();
        crystalsPerUpgrade = 5;

        // Configurar los valores iniciales de los cristales y mejoras disponibles
        crystalText.text = $"Cristales: 0/{crystalsPerUpgrade}";
        availableUpgradesText.text = "Mejoras disponibles: 0";
        upgradeNotificationText.text = "";

        // Configurar las barras de energía y temporizador
        if (player.battery != null)
        {
            batteryController = player.battery.GetComponent<BatteryController>();
            energyBar.maxValue = batteryController.maxEnergy;
            energyBar.value = batteryController.energyAmounts;
        }
        else
        {
            Debug.LogWarning("No se encontró la batería en el jugador. Asegúrate de que esté asignada.");
        }

        timerBar.maxValue = player.maxTimeWithoutBattery;
        timerBar.value = player.currentTime;
    }

    void Update()
    {
        if (batteryController != null)
        {
            energyBar.value = batteryController.energyAmounts;
        }

        timerBar.value = player.currentTime;
        float timerPercentage = timerBar.value / timerBar.maxValue;

        if (timerPercentage >= blinkThreshold)
        {
            blinkTimer += Time.deltaTime * blinkSpeed;
            float alpha = Mathf.PingPong(blinkTimer, 1f);
            Color fillColor = timerBarFill.color;
            fillColor.a = alpha;
            timerBarFill.color = fillColor;
        }
        else
        {
            Color fillColor = timerBarFill.color;
            fillColor.a = 1f;
            timerBarFill.color = fillColor;
        }
    }

    public void UpdateCrystalUI(int displayedCrystals, int totalCrystals)
    {
        crystalText.text = $"Cristales: {displayedCrystals}/{crystalsPerUpgrade}";
        int availableUpgrades = totalCrystals / crystalsPerUpgrade;
        availableUpgradesText.text = $"Mejoras disponibles: {availableUpgrades}";
    }

    public void ShowUpgradeNotification(string upgradeMessage)
    {
        upgradeNotificationText.text = upgradeMessage;
        Invoke(nameof(HideNotification), notificationDuration);
    }

    private void HideNotification()
    {
        upgradeNotificationText.text = "";
    }
}
