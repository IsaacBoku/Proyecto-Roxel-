using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Crystals Settings")]
    [SerializeField] private TextMeshProUGUI crystalText;
    [SerializeField] private TextMeshProUGUI upgradeNotificationText;
    [SerializeField] private TextMeshProUGUI availableUpgradesText;

    [Header("Life Settings")]
    [SerializeField] private Slider[] lifeBars;
    [SerializeField] private Image[] lifeBarFills;

    [Header("Energy Settings")]
    [SerializeField] private Slider energyBar; 
    [SerializeField] private Image energyBarFill;
    [SerializeField] private TextMeshProUGUI energyBarText;

    [Header("UI Settings")]
    [SerializeField] private float notificationDuration = 3f;
    [SerializeField] private float blinkSpeed = 2f;
    [SerializeField] private float blinkThreshold = 0.6f;
    [SerializeField] private float barLerpSpeed = 5f;

    [Header("Constants")]
    private const int maxLifeBars = 4;
    private const int crystalsPerUpgrade = 5;

    [Header("Variables")]
    private Player player;
    private BatteryController batteryController; 
    private float blinkTimer = 0f;
    private float targetEnergyValue;
    private float[] targetLifeValues;

    public float NotificationDuration => notificationDuration;

    void Awake()
    {
        lifeBars = new Slider[4];
        lifeBarFills = new Image[4];
        targetLifeValues = new float[maxLifeBars];
    }

    void Start()
    {
        player = FindAnyObjectByType<Player>();
        if (player == null)
        {
            Debug.LogError("No se encontró el componente Player en la escena.");
            return;
        }

        if (player.battery != null)
        {
            batteryController = player.battery.GetComponent<BatteryController>();
            if (batteryController == null)
            {
                Debug.LogWarning("No se encontró BatteryController en la batería del jugador.");
            }
        }
        else
        {
            Debug.LogWarning("No se encontró la batería en el jugador. Asegúrate de que esté asignada.");
        }

        InitializeEnergyBar();
        InitializeLifeBars();
        InitializeCrystalUI();
    }

    void Update()
    {
        UpdateEnergyBar();
        UpdateLifeBars();
    }
    private void InitializeEnergyBar()
    {
        if (batteryController == null || energyBar == null || energyBarFill == null || energyBarText == null)
        {
            Debug.LogWarning("Componentes de la barra de energía no asignados correctamente.");
            return;
        }

        energyBar.maxValue = batteryController.maxEnergy;
        energyBar.value = batteryController.energyAmounts;
        targetEnergyValue = batteryController.energyAmounts;
        energyBarText.text = "100%";
    }

    private void InitializeLifeBars()
    {
        for (int i = 0; i < maxLifeBars; i++)
        {
            GameObject lifeBarObj = GameObject.Find($"LifeBar{i + 1}");
            if (lifeBarObj == null)
            {
                Debug.LogWarning($"No se encontró LifeBar{i + 1} en la escena.");
                continue;
            }

            lifeBars[i] = lifeBarObj.GetComponent<Slider>();
            lifeBarFills[i] = lifeBarObj.transform.Find("Fill")?.GetComponent<Image>();

            if (lifeBars[i] == null || lifeBarFills[i] == null)
            {
                Debug.LogWarning($"No se encontraron todos los componentes para LifeBar{i + 1}. Verifica la jerarquía.");
                continue;
            }

            lifeBars[i].maxValue = player.maxLifeProgress;
            lifeBars[i].value = player.maxLifeProgress;
            targetLifeValues[i] = player.maxLifeProgress;
        }
    }

    private void InitializeCrystalUI()
    {
        if (crystalText == null || availableUpgradesText == null || upgradeNotificationText == null)
        {
            Debug.LogWarning("Componentes de la UI de cristales no asignados correctamente.");
            return;
        }

        crystalText.text = $"Cristales: 0/{crystalsPerUpgrade}";
        availableUpgradesText.text = "Mejoras disponibles: 0";
        upgradeNotificationText.text = "";
    }

    private void UpdateEnergyBar()
    {
        if (batteryController == null || energyBar == null || energyBarFill == null || energyBarText == null)
        {
            return;
        }

        energyBar.maxValue = batteryController.maxEnergy;
        targetEnergyValue = batteryController.energyAmounts;
        energyBar.value = Mathf.Lerp(energyBar.value, targetEnergyValue, Time.deltaTime * barLerpSpeed);

        float energyPercentage = energyBar.value / energyBar.maxValue;
        energyBarText.text = $"{Mathf.RoundToInt(energyPercentage * 100)}%";

        // Actualizar color de la barra de energía
        UpdateEnergyBarColor(energyPercentage);
    }

    private void UpdateEnergyBarColor(float energyPercentage)
    {
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

    private void UpdateLifeBars()
    {
        int currentLives = player.GetComponent<PlayerHealthSystem>().currentLives;

        for (int i = 0; i < maxLifeBars; i++)
        {
            if (lifeBars[i] == null || lifeBarFills[i] == null)
            {
                continue;
            }

            if (i < currentLives)
            {
                lifeBars[i].gameObject.SetActive(true);
                lifeBars[i].maxValue = player.maxLifeProgress;

                if (i == currentLives - 1 && player.isSeparated && !player.IsLifeProgressPaused)
                {
                    // Actualizar barra de vida actual cuando está separada
                    targetLifeValues[i] = player.currentLifeProgress;
                    lifeBars[i].value = Mathf.Lerp(lifeBars[i].value, targetLifeValues[i], Time.deltaTime * barLerpSpeed);
                    float lifePercentage = lifeBars[i].value / lifeBars[i].maxValue;
                    UpdateLifeBarColorAndBlink(i, lifePercentage);
                }
                else
                {
                    // Mostrar barra llena para vidas no afectadas o cuando no está separada
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

    private void UpdateLifeBarColorAndBlink(int index, float lifePercentage)
    {
        // Actualizar color de la barra de vida
        if (lifePercentage <= 0.3f)
        {
            lifeBarFills[index].color = Color.red;
        }
        else if (lifePercentage <= 0.6f)
        {
            lifeBarFills[index].color = Color.Lerp(Color.red, Color.yellow, (lifePercentage - 0.3f) / 0.3f);
        }
        else
        {
            lifeBarFills[index].color = Color.Lerp(Color.yellow, Color.green, (lifePercentage - 0.6f) / 0.4f);
        }

        // Aplicar efecto de parpadeo si el porcentaje es bajo
        if (lifePercentage >= blinkThreshold)
        {
            blinkTimer += Time.deltaTime * blinkSpeed;
            float alpha = Mathf.PingPong(blinkTimer, 1f);
            Color fillColor = lifeBarFills[index].color;
            fillColor.a = alpha;
            lifeBarFills[index].color = fillColor;
        }
        else
        {
            Color fillColor = lifeBarFills[index].color;
            fillColor.a = 1f;
            lifeBarFills[index].color = fillColor;
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