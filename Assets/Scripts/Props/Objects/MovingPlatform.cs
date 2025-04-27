using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public enum PlatformMode
    {
        Continuous,       // Moves continuously between waypoints
        PedestalControlled // Controlled by a pedestal
    }

    [SerializeField] private PlatformMode mode = PlatformMode.PedestalControlled;
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
        InitializePlatform();
        if (mode == PlatformMode.Continuous)
        {
            isActive = true;
            Debug.Log($"MovingPlatform '{gameObject.name}': Started in Continuous mode.");
        }
    }

    public void Activate()
    {
        if (mode == PlatformMode.PedestalControlled)
        {
            isActive = true;
            isReturning = false;
            Debug.Log($"MovingPlatform '{gameObject.name}': Activated by pedestal.");
        }
    }

    public void Deactivate()
    {
        if (mode == PlatformMode.PedestalControlled)
        {
            isActive = false;
            if (waypoints.Length > 0)
            {
                isReturning = true;
                currentWaypointIndex = 0;
                Debug.Log($"MovingPlatform '{gameObject.name}': Deactivated by pedestal. Returning to initial waypoint.");
            }
        }
    }

    private void Update()
    {
        if (!CanMove()) return;

        Vector2 targetPosition = GetTargetPosition();
        MoveToTarget(targetPosition);
        UpdateObjectsOnPlatform();

        if (HasReachedTarget(targetPosition))
        {
            HandleTargetReached();
        }

        lastPosition = transform.position;
    }

    private void InitializePlatform()
    {
        lastPosition = transform.position;
        if (waypoints.Length > 0)
        {
            initialPosition = waypoints[0].position;
            transform.position = initialPosition;
        }
        else
        {
            initialPosition = transform.position;
            Debug.LogWarning($"MovingPlatform '{gameObject.name}': No waypoints assigned. Using initial position.");
        }
    }

    private bool CanMove()
    {
        return (mode == PlatformMode.Continuous || isActive || isReturning) && waypoints.Length > 0;
    }

    private Vector2 GetTargetPosition()
    {
        return isReturning && mode == PlatformMode.PedestalControlled
            ? initialPosition
            : waypoints[currentWaypointIndex].position;
    }

    private void MoveToTarget(Vector2 target)
    {
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    private void UpdateObjectsOnPlatform()
    {
        Vector2 deltaPosition = (Vector2)transform.position - lastPosition;
        foreach (Rigidbody2D rb in objectsOnPlatform)
        {
            if (rb != null)
            {
                rb.position += deltaPosition;
            }
        }
    }

    private bool HasReachedTarget(Vector2 target)
    {
        return Vector2.Distance(transform.position, target) < 0.1f;
    }

    private void HandleTargetReached()
    {
        if (isReturning && mode == PlatformMode.PedestalControlled)
        {
            isReturning = false;
            Debug.Log($"MovingPlatform '{gameObject.name}': Returned to initial waypoint.");
        }
        else
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
        if (rb != null && IsObjectOnTop(collision))
        {
            objectsOnPlatform.Add(rb);
            Debug.Log($"MovingPlatform '{gameObject.name}': Object '{rb.gameObject.name}' added to platform.");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            objectsOnPlatform.Remove(rb);
            Debug.Log($"MovingPlatform '{gameObject.name}': Object '{rb.gameObject.name}' removed from platform.");
        }
    }

    private bool IsObjectOnTop(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);
        return contact.normal.y < -0.5f;
    }
}
