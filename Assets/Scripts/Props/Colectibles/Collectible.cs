using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private int energyValue = 10;
    [SerializeField] private int crystalValue = 1;
    [SerializeField] private bool isSpecial = false; // Indica si es un cristal especial
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private AudioSource collectSound;
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

                int finalEnergyValue = isSpecial ? energyValue * 2 : energyValue; // Duplica la energía si es especial
                int finalCrystalValue = isSpecial ? crystalValue * 3 : crystalValue; // Triplica los cristales si es especial

                BatteryController battery = player.battery.GetComponent<BatteryController>();
                battery.energyAmounts += finalEnergyValue;
                battery.energyAmounts = Mathf.Clamp(battery.energyAmounts, 0f, battery.maxEnergy);

                player.AddCrystal(finalCrystalValue);

                if (collectEffect != null) collectEffect.Play();
                if (collectSound != null) collectSound.Play();

                Debug.Log($"Recolectado cristal de energía: +{finalEnergyValue} energía, +{finalCrystalValue} cristal(es)");

                Destroy(gameObject, 0.5f);
            }
        }
    }
}
