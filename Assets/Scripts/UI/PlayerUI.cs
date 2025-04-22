using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI crystalText;
    [SerializeField] private TextMeshProUGUI upgradeNotificationText;
    [SerializeField] private TextMeshProUGUI availableUpgradesText;
    [SerializeField] private Slider energyBar; 
    [SerializeField] private Slider timerBar;
    [SerializeField] private Image energyBarFill;
    [SerializeField] private Image timerBarFill;
    [SerializeField] private TextMeshProUGUI energyBarText;
    [SerializeField] private TextMeshProUGUI timerBarText;
    [SerializeField] private float notificationDuration = 3f;
    [SerializeField] private float blinkSpeed = 2f;
    [SerializeField] private float blinkThreshold = 0.6f;
    [SerializeField] private float barLerpSpeed = 5f;
    private Player player;
    private BatteryController batteryController; 
    private int crystalsPerUpgrade;
    private float blinkTimer = 0f;
    private float energyTargetValue;
    private float timerTargetValue;

    public float NotificationDuration => notificationDuration;

    void Start()
    {
        player = FindAnyObjectByType<Player>();
        crystalsPerUpgrade = 5;

        crystalText.text = $"Cristales: 0/{crystalsPerUpgrade}";
        availableUpgradesText.text = "Mejoras disponibles: 0";
        upgradeNotificationText.text = "";

        if (player.battery != null)
        {
            batteryController = player.battery.GetComponent<BatteryController>();
            energyBar.maxValue = batteryController.maxEnergy;
            energyBar.value = batteryController.energyAmounts;
            energyTargetValue = energyBar.value;
        }
        else
        {
            Debug.LogWarning("No se encontró la batería en el jugador. Asegúrate de que esté asignada.");
        }

        timerBar.maxValue = player.maxTimeWithoutBattery;
        timerBar.value = player.currentTime;
        timerTargetValue = timerBar.value;
    }

    void Update()
    {
        if (batteryController != null)
        {
            energyBar.maxValue = batteryController.maxEnergy;
            energyTargetValue = batteryController.energyAmounts;

            energyBar.value = Mathf.Lerp(energyBar.value, energyTargetValue, Time.deltaTime * barLerpSpeed);

            energyBarText.text = $"{Mathf.RoundToInt(energyBar.value)}/{Mathf.RoundToInt(energyBar.maxValue)}";

            float energyPercentage = energyBar.value / energyBar.maxValue;
            if (energyPercentage <= 0.3f)
            {
                energyBarFill.color = Color.red;
            }
            else if (energyPercentage <= 0.6f)
            {
                energyBarFill.color = Color.Lerp(Color.red, Color.yellow, (energyPercentage - 0.3f) / 0.3f);
            }
            else
            {
                energyBarFill.color = Color.Lerp(Color.yellow, Color.green, (energyPercentage - 0.6f) / 0.4f);
            }
        }

        timerBar.maxValue = player.maxTimeWithoutBattery;
        timerTargetValue = player.currentTime;

        timerBar.value = Mathf.Lerp(timerBar.value, timerTargetValue, Time.deltaTime * barLerpSpeed);
        timerBarText.text = $"{Mathf.RoundToInt(timerBar.value)}/{Mathf.RoundToInt(timerBar.maxValue)}";

        float timerPercentage = timerBar.value / timerBar.maxValue;

        if (timerPercentage <= 0.3f)
        {
            timerBarFill.color = Color.green;
        }
        else if (timerPercentage <= 0.6f)
        {
            timerBarFill.color = Color.Lerp(Color.green, Color.yellow, (timerPercentage - 0.3f) / 0.3f);
        }
        else
        {
            timerBarFill.color = Color.Lerp(Color.yellow, Color.red, (timerPercentage - 0.6f) / 0.4f);
        }

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