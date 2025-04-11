using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private int energyValue = 10; // Energ�a que otorga al recolectar
    [SerializeField] private int crystalValue = 1; // Cantidad de cristales que otorga (por si quieres que algunos cristales valgan m�s)
    [SerializeField] private ParticleSystem collectEffect; // Efecto visual al recolectar
    [SerializeField] private AudioSource collectSound; // Sonido al recolectar
    private bool isCollected = false; // Para evitar recolectar el mismo cristal m�s de una vez

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return; // Evita recolectar m�s de una vez

        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null && player.battery != null)
            {
                isCollected = true; // Marca el cristal como recolectado

                // Aumenta la energ�a de la bater�a
                BatteryController battery = player.battery.GetComponent<BatteryController>();
                battery.energyAmounts += energyValue;
                battery.energyAmounts = Mathf.Clamp(battery.energyAmounts, 0f, battery.maxEnergy);

                // A�ade el cristal al contador del jugador
                player.AddCrystal(crystalValue);

                // Reproduce efectos visuales y auditivos
                if (collectEffect != null) collectEffect.Play();
                if (collectSound != null) collectSound.Play();

                Debug.Log($"Recolectado cristal de energ�a: +{energyValue} energ�a, +{crystalValue} cristal(es)");

                // Destruye el cristal despu�s de un peque�o retraso para que los efectos se reproduzcan
                Destroy(gameObject, 0.5f);
            }
        }
    }
}
