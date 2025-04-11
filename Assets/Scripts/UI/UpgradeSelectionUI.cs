using UnityEngine;
using UnityEngine.UI;

public class UpgradeSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject upgradePanel; // Panel que contiene la UI de selección
    [SerializeField] private Button maxTimeButton; // Botón para MaxTimeWithoutBattery
    [SerializeField] private Button maxEnergyButton; // Botón para MaxEnergy
    [SerializeField] private Button magneticRangeButton; // Botón para MagneticRange
    private Player player;

    void Start()
    {
        player = FindAnyObjectByType<Player>();
        upgradePanel.SetActive(false); // Oculta el panel al inicio

        // Asigna las funciones a los botones
        maxTimeButton.onClick.AddListener(() => SelectUpgrade(UpgradeType.MaxTimeWithoutBattery));
        maxEnergyButton.onClick.AddListener(() => SelectUpgrade(UpgradeType.MaxEnergy));
        magneticRangeButton.onClick.AddListener(() => SelectUpgrade(UpgradeType.MagneticRange));
    }

    public void ShowUpgradeSelection()
    {
        // Pausa el juego
        Time.timeScale = 0f;
        upgradePanel.SetActive(true);
    }

    private void SelectUpgrade(UpgradeType upgrade)
    {
        // Aplica la mejora seleccionada
        player.ApplyUpgrade(upgrade);

        // Oculta el panel y reanuda el juego
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
