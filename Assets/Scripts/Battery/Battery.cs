using System.Collections.Generic;
using UnityEngine;

public class Battery : MonoBehaviour
{
    private List<IEnergizable> objetosCercanos = new List<IEnergizable>();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IEnergizable energizable))
        {
            energizable.Energizar(true);
            objetosCercanos.Add(energizable);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out IEnergizable energizable))
        {
            energizable.Energizar(false);
            objetosCercanos.Remove(energizable);
        }
    }
}
