using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float speed = 2f;
    private bool isActive = false;
    private bool isReturning = false;
    private int currentWaypointIndex = 0;
    private Vector2 lastPosition;
    private Vector2 initialPosition;
    private List<Rigidbody2D> objectsOnPlatform = new List<Rigidbody2D>();

    private void Start()
    {
        lastPosition = transform.position;
        if (waypoints.Length > 0)
        {
            initialPosition = waypoints[0].position;
            transform.position = initialPosition; // Asegurar que empieza en el primer waypoint
        }
        else
        {
            initialPosition = transform.position;
            Debug.LogWarning($"MovingPlatform '{gameObject.name}': No se asignaron waypoints. Usando posición inicial.");
        }
    }

    public void Activate()
    {
        isActive = true;
        isReturning = false; // Cancelar retorno si se reactiva
        Debug.Log($"MovingPlatform '{gameObject.name}': Activada.");
    }

    public void Deactivate()
    {
        isActive = false;
        if (waypoints.Length > 0)
        {
            isReturning = true; // Iniciar retorno al primer waypoint
            currentWaypointIndex = 0; // Resetear para el próximo ciclo
            Debug.Log($"MovingPlatform '{gameObject.name}': Desactivada. Regresando al primer waypoint.");
        }
    }

    private void Update()
    {
        if (isReturning)
        {
            // Mover hacia el primer waypoint
            transform.position = Vector2.MoveTowards(transform.position, initialPosition, speed * Time.deltaTime);

            // Transportar objetos
            Vector2 deltaPosition = (Vector2)transform.position - lastPosition;
            foreach (Rigidbody2D rb in objectsOnPlatform)
            {
                if (rb != null)
                {
                    rb.position += deltaPosition;
                }
            }

            // Verificar si llegó al primer waypoint
            if (Vector2.Distance(transform.position, initialPosition) < 0.1f)
            {
                isReturning = false;
                Debug.Log($"MovingPlatform '{gameObject.name}': Regresó al primer waypoint.");
            }
        }
        else if (isActive && waypoints.Length > 0)
        {
            // Movimiento normal entre waypoints
            Vector2 target = waypoints[currentWaypointIndex].position;
            transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

            // Transportar objetos
            Vector2 deltaPosition = (Vector2)transform.position - lastPosition;
            foreach (Rigidbody2D rb in objectsOnPlatform)
            {
                if (rb != null)
                {
                    rb.position += deltaPosition;
                }
            }

            // Cambiar al siguiente waypoint
            if (Vector2.Distance(transform.position, target) < 0.1f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
        }

        lastPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            ContactPoint2D contact = collision.GetContact(0);
            if (contact.normal.y < -0.5f) // Objeto encima
            {
                if (!objectsOnPlatform.Contains(rb))
                {
                    objectsOnPlatform.Add(rb);
                    Debug.Log($"MovingPlatform '{gameObject.name}': Objeto {rb.gameObject.name} añadido a la plataforma.");
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            objectsOnPlatform.Remove(rb);
            Debug.Log($"MovingPlatform '{gameObject.name}': Objeto {rb.gameObject.name} removido de la plataforma.");
        }
    }
}
