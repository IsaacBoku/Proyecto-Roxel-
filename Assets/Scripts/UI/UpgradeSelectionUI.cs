using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button maxTimeButton;
    [SerializeField] private TextMeshProUGUI maxTimeDescription;
    [SerializeField] private Button maxEnergyButton;
    [SerializeField] private TextMeshProUGUI maxEnergyDescription;
    [SerializeField] private Button magneticRangeButton;
    [SerializeField] private TextMeshProUGUI magneticRangeDescription;
    [SerializeField] private Button cancelButton; // Botón para cancelar
    private Player player;

    void Start()
    {
        player = FindAnyObjectByType<Player>();
        upgradePanel.SetActive(false);

        maxTimeButton.onClick.AddListener(() => SelectUpgrade(UpgradeType.MaxTimeWithoutBattery));
        maxEnergyButton.onClick.AddListener(() => SelectUpgrade(UpgradeType.MaxEnergy));
        magneticRangeButton.onClick.AddListener(() => SelectUpgrade(UpgradeType.MagneticRange));
        cancelButton.onClick.AddListener(CancelSelection);

        maxTimeDescription.text = "Aumenta el tiempo que puedes estar sin la batería en 2 segundos.";
        maxEnergyDescription.text = "Aumenta la capacidad máxima de energía de la batería en 20 unidades.";
        magneticRangeDescription.text = "Aumenta el rango de la habilidad magnética en 1 unidad.";
    }

    public void ShowUpgradeSelection()
    {
        Time.timeScale = 0f;
        upgradePanel.SetActive(true);
    }

    private void SelectUpgrade(UpgradeType upgrade)
    {
        player.ApplyUpgrade(upgrade);
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void CancelSelection()
    {
        player.CancelUpgradeSelection();
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
