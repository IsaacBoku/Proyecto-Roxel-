using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private int energyValue = 10;
    [SerializeField] private int crystalValue = 1;
    [SerializeField] private bool isSpecial = false;
    [SerializeField] private ParticleSystem collectEffect;

    [Header("Audio Settings")]
    [SerializeField, Tooltip("Nombre del sonido al recolectar el coleccionable en el AudioManager")]
    private string collectSoundName = "CollectiblePickup";

    private bool isCollected = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null && player.battery != null)
            {
                isCollected = true;

                int finalEnergyValue = isSpecial ? energyValue * 2 : energyValue;
                int finalCrystalValue = isSpecial ? crystalValue * 3 : crystalValue;

                BatteryController battery = player.battery.GetComponent<BatteryController>();
                battery.energyAmounts += finalEnergyValue;
                battery.energyAmounts = Mathf.Clamp(battery.energyAmounts, 0f, battery.maxEnergy);

                player.AddCrystal(finalCrystalValue);

                if (collectEffect != null) collectEffect.Play();
                AudioManager.instance.PlaySFX(collectSoundName); // Reproducir sonido de recolección

                Debug.Log($"Recolectado cristal de energía: +{finalEnergyValue} energía, +{finalCrystalValue} cristal(es)");

                Destroy(gameObject, 0.2f);
            }
        }
    }
}
