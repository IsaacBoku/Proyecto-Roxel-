using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private int energyValue = 10; // Energía que otorga al recolectar
    [SerializeField] private int crystalValue = 1; // Cantidad de cristales que otorga (por si quieres que algunos cristales valgan más)
    [SerializeField] private ParticleSystem collectEffect; // Efecto visual al recolectar
    [SerializeField] private AudioSource collectSound; // Sonido al recolectar
    private bool isCollected = false; // Para evitar recolectar el mismo cristal más de una vez

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return; // Evita recolectar más de una vez

        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null && player.battery != null)
            {
                isCollected = true; // Marca el cristal como recolectado

                // Aumenta la energía de la batería
                BatteryController battery = player.battery.GetComponent<BatteryController>();
                battery.energyAmounts += energyValue;
                battery.energyAmounts = Mathf.Clamp(battery.energyAmounts, 0f, battery.maxEnergy);

                // Añade el cristal al contador del jugador
                player.AddCrystal(crystalValue);

                // Reproduce efectos visuales y auditivos
                if (collectEffect != null) collectEffect.Play();
                if (collectSound != null) collectSound.Play();

                Debug.Log($"Recolectado cristal de energía: +{energyValue} energía, +{crystalValue} cristal(es)");

                // Destruye el cristal después de un pequeño retraso para que los efectos se reproduzcan
                Destroy(gameObject, 0.5f);
            }
        }
    }
}
