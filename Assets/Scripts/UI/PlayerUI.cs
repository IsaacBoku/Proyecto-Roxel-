using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI crystalText;
    [SerializeField] private TextMeshProUGUI upgradeNotificationText;
    [SerializeField] private float notificationDuration = 3f;
    private Player player;
    private int crystalsPerUpgrade;

    void Start()
    {
        player = FindAnyObjectByType<Player>();
        crystalsPerUpgrade = 5;
        crystalText.text = $"Cristales: 0/{crystalsPerUpgrade}";
        upgradeNotificationText.text = "";
    }

    public void UpdateCrystalUI(int collectedCrystals)
    {
        crystalText.text = $"Cristales: {collectedCrystals}/{crystalsPerUpgrade}";
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
