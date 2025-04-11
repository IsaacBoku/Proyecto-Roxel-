using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI crystalText;
    [SerializeField] private TextMeshProUGUI upgradeNotificationText;
    [SerializeField] private TextMeshProUGUI availableUpgradesText; // Nuevo texto para mostrar las mejoras disponibles
    [SerializeField] private float notificationDuration = 3f;
    private Player player;
    private int crystalsPerUpgrade;

    void Start()
    {
        player = FindAnyObjectByType<Player>();
        crystalsPerUpgrade = 5; // Ajusta esto según el valor en Player
        crystalText.text = $"Cristales: 0/{crystalsPerUpgrade}";
        availableUpgradesText.text = "Mejoras disponibles: 0";
        upgradeNotificationText.text = "";
    }

    public void UpdateCrystalUI(int collectedCrystals)
    {
        crystalText.text = $"Cristales: {collectedCrystals}/{crystalsPerUpgrade}";
        int availableUpgrades = player.GetAvailableUpgrades();
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
