using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints; // Lista de puntos por los que pasa la plataforma
    [SerializeField] private float speed = 2f;
    private bool isActive = false;
    private int currentWaypointIndex = 0;
    private Vector2 lastPosition;
    private List<Rigidbody2D> objectsOnPlatform = new List<Rigidbody2D>();

    private void Start()
    {
        lastPosition = transform.position;
    }

    public void Activate()
    {
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
    }

    private void Update()
    {
        if (!isActive) return;

        Vector2 target = waypoints[currentWaypointIndex].position;
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

        Vector2 deltaPosition = (Vector2)transform.position - lastPosition;
        foreach (Rigidbody2D rb in objectsOnPlatform)
        {
            if (rb != null)
            {
                rb.position += deltaPosition;
            }
        }

        lastPosition = transform.position;

        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length; // Cicla entre los waypoints
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            ContactPoint2D contact = collision.GetContact(0);
            if (contact.normal.y < -0.5f)
            {
                if (!objectsOnPlatform.Contains(rb))
                {
                    objectsOnPlatform.Add(rb);
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
        }
    }
}
