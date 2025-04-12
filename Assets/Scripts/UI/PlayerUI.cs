using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI crystalText;
    [SerializeField] private TextMeshProUGUI upgradeNotificationText;
    [SerializeField] private TextMeshProUGUI availableUpgradesText;
    [SerializeField] private Slider energyBar; // Barra de energ�a
    [SerializeField] private Slider timerBar; // Barra del temporizador
    [SerializeField] private Image energyBarFill; // Componente Image del Fill del EnergyBar
    [SerializeField] private Image timerBarFill;  // Componente Image del Fill del TimerBar
    [SerializeField] private TextMeshProUGUI energyBarText; // Texto dentro de la barra de energ�a
    [SerializeField] private TextMeshProUGUI timerBarText;  // Texto dentro de la barra del temporizador
    [SerializeField] private float notificationDuration = 3f;
    [SerializeField] private float blinkSpeed = 2f;
    [SerializeField] private float blinkThreshold = 0.8f; // Parpadea cuando el temporizador est� al 80%
    private Player player;
    private BatteryController batteryController; // Para acceder a la energ�a de la bater�a
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

        // Configurar las barras de energ�a y temporizador
        if (player.battery != null)
        {
            batteryController = player.battery.GetComponent<BatteryController>();
            energyBar.maxValue = batteryController.maxEnergy;
            energyBar.value = batteryController.energyAmounts;
        }
        else
        {
            Debug.LogWarning("No se encontr� la bater�a en el jugador. Aseg�rate de que est� asignada.");
        }

        timerBar.maxValue = player.maxTimeWithoutBattery;
        timerBar.value = player.currentTime;
    }

    void Update()
    {
        // Actualizar la barra de energ�a
        if (batteryController != null)
        {
            // Actualizar el valor m�ximo por si ha cambiado (por ejemplo, tras una mejora)
            energyBar.maxValue = batteryController.maxEnergy;
            energyBar.value = batteryController.energyAmounts;

            // Mostrar el texto de la energ�a (por ejemplo, "75/100")
            energyBarText.text = $"{Mathf.RoundToInt(energyBar.value)}/{Mathf.RoundToInt(energyBar.maxValue)}";

            // Cambiar el color de la barra de energ�a seg�n su porcentaje
            float energyPercentage = energyBar.value / energyBar.maxValue;
            if (energyPercentage <= 0.3f)
            {
                energyBarFill.color = Color.red; // Rojo cuando est� baja (30% o menos)
            }
            else if (energyPercentage <= 0.6f)
            {
                energyBarFill.color = Color.Lerp(Color.red, Color.yellow, (energyPercentage - 0.3f) / 0.3f); // Transici�n de rojo a amarillo
            }
            else
            {
                energyBarFill.color = Color.Lerp(Color.yellow, Color.green, (energyPercentage - 0.6f) / 0.4f); // Transici�n de amarillo a verde
            }
        }

        // Actualizar la barra del temporizador
        timerBar.maxValue = player.maxTimeWithoutBattery; // Actualizar el valor m�ximo por si ha cambiado
        timerBar.value = player.currentTime;
        timerBarText.text = $"{Mathf.RoundToInt(timerBar.value)}/{Mathf.RoundToInt(timerBar.maxValue)}";

        // Calcular el porcentaje del temporizador
        float timerPercentage = timerBar.value / timerBar.maxValue;

        // Cambiar el color de la barra del temporizador seg�n su porcentaje
        if (timerPercentage <= 0.3f)
        {
            timerBarFill.color = Color.green; // Verde cuando est� baja (30% o menos)
        }
        else if (timerPercentage <= 0.6f)
        {
            timerBarFill.color = Color.Lerp(Color.green, Color.yellow, (timerPercentage - 0.3f) / 0.3f); // Transici�n de verde a amarillo
        }
        else
        {
            timerBarFill.color = Color.Lerp(Color.yellow, Color.red, (timerPercentage - 0.6f) / 0.4f); // Transici�n de amarillo a rojo
        }

        // Efecto de parpadeo cuando el temporizador est� cerca de agotarse
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