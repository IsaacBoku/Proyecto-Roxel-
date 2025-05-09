using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI crystalText;
    [SerializeField] private TextMeshProUGUI upgradeNotificationText;
    [SerializeField] private TextMeshProUGUI availableUpgradesText;
    [SerializeField] private Slider energyBar; 
    [SerializeField] private Slider[] lifeBars;
    [SerializeField] private Image energyBarFill;
    [SerializeField] private Image[] lifeBarFills;
    [SerializeField] private TextMeshProUGUI energyBarText;
    [SerializeField] private float notificationDuration = 3f;
    [SerializeField] private float blinkSpeed = 2f;
    [SerializeField] private float blinkThreshold = 0.6f;
    [SerializeField] private float barLerpSpeed = 5f;
    private Player player;
    private BatteryController batteryController; 
    private int crystalsPerUpgrade;
    private float blinkTimer = 0f;
    private float energyTargetValue;
    private float[] lifeTargetValues;
    
    public float NotificationDuration => notificationDuration;

    void Awake()
    {
        lifeBars = new Slider[4];
        lifeBarFills = new Image[4];
        lifeTargetValues = new float[4];
    }

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
            energyTargetValue = batteryController.energyAmounts;
        }
        else
        {
            Debug.LogWarning("No se encontró la batería en el jugador. Asegúrate de que esté asignada.");
        }

        for (int i = 0; i < 4; i++)
        {
            GameObject lifeBarObj = GameObject.Find($"LifeBar{i + 1}");
            if (lifeBarObj != null)
            {
                lifeBars[i] = lifeBarObj.GetComponent<Slider>();
                lifeBarFills[i] = lifeBarObj.transform.Find("Fill")?.GetComponent<Image>();
                if (lifeBars[i] == null || lifeBarFills[i] == null)
                {
                    Debug.LogWarning($"No se encontraron todos los componentes para LifeBar{i + 1}. Verifica la jerarquía.");
                }
                else
                {
                    lifeBars[i].maxValue = player.maxLifeProgress;
                    lifeBars[i].value = player.maxLifeProgress;
                    lifeTargetValues[i] = player.maxLifeProgress;
                }
            }
            else
            {
                Debug.LogWarning($"No se encontró LifeBar{i + 1} en la escena.");
            }
        }
    }

    void Update()
    {
        if (batteryController != null)
        {
            energyBar.maxValue = batteryController.maxEnergy;
            energyTargetValue = batteryController.energyAmounts;

            energyBar.value = Mathf.Lerp(energyBar.value, energyTargetValue, Time.deltaTime * barLerpSpeed);

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

        int currentLives = player.GetComponent<PlayerHealthSystem>().currentLives;
        for (int i = 0; i < 4; i++)
        {
            if (lifeBars[i] == null || lifeBarFills[i] == null) continue;

            if (i < currentLives)
            {
                lifeBars[i].gameObject.SetActive(true);
                lifeBars[i].maxValue = player.maxLifeProgress;
                if (i == currentLives - 1 && player.isSeparated && !player.IsLifeProgressPaused)
                {
                    lifeTargetValues[i] = player.currentLifeProgress;
                    lifeBars[i].value = Mathf.Lerp(lifeBars[i].value, lifeTargetValues[i], Time.deltaTime * barLerpSpeed);

                    float lifePercentage = lifeBars[i].value / lifeBars[i].maxValue;
                    if (lifePercentage <= 0.3f)
                    {
                        lifeBarFills[i].color = Color.red;
                    }
                    else if (lifePercentage <= 0.6f)
                    {
                        lifeBarFills[i].color = Color.Lerp(Color.red, Color.yellow, (lifePercentage - 0.3f) / 0.3f);
                    }
                    else
                    {
                        lifeBarFills[i].color = Color.Lerp(Color.yellow, Color.green, (lifePercentage - 0.6f) / 0.4f);
                    }

                    if (lifePercentage >= blinkThreshold)
                    {
                        blinkTimer += Time.deltaTime * blinkSpeed;
                        float alpha = Mathf.PingPong(blinkTimer, 1f);
                        Color fillColor = lifeBarFills[i].color;
                        fillColor.a = alpha;
                        lifeBarFills[i].color = fillColor;
                    }
                    else
                    {
                        Color fillColor = lifeBarFills[i].color;
                        fillColor.a = 1f;
                        lifeBarFills[i].color = fillColor;
                    }
                }
                else
                {
                    lifeBars[i].value = lifeBars[i].maxValue;
                    lifeBarFills[i].color = Color.green;
                }
            }
            else
            {
                lifeBars[i].gameObject.SetActive(false);
            }
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