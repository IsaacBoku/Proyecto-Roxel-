using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public enum ConveyorDirection
{
    Forward,  // Hacia adelante
    Reverse,  // Hacia atr�s
    Stopped   // Detenido
}

public class ConveyorBelt_Mechanic : MonoBehaviour
{
    #region Serialized Fields
    [Header("Configuraci�n del Conveyor")]
    [SerializeField, Tooltip("Velocidad del conveyor en unidades por segundo")]
    private float speed = 2f;

    [SerializeField, Tooltip("Direcci�n inicial del movimiento del conveyor")]
    private ConveyorDirection direction = ConveyorDirection.Forward;

    [SerializeField, Tooltip("Modo de aplicaci�n de la fuerza")]
    private ForceMode2D forceMode = ForceMode2D.Force;

    [SerializeField, Tooltip("Tiempo de aceleraci�n para un movimiento suave")]
    private float accelerationTime = 0.2f;

    [Header("Configuraci�n Din�mica")]
    [SerializeField, Tooltip("Habilita cambios din�micos de velocidad y direcci�n")]
    private bool useDynamicBehavior = false;

    [SerializeField, Tooltip("Intervalo en segundos para cambios de direcci�n")]
    private float directionChangeInterval = 5f;

    [SerializeField, Tooltip("Curva para modificar la velocidad din�micamente")]
    private AnimationCurve speedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

    [Header("Interacci�n con Bater�a")]
    [SerializeField, Tooltip("Efecto en la energ�a de la bater�a por segundo (positivo para cargar, negativo para drenar)")]
    private float batteryEffect = 0f;

    [SerializeField, Tooltip("Si es verdadero, el conveyor afecta la polaridad de la bater�a")]
    private bool affectBatteryPolarity = false;

    [Header("Visuales y Retroalimentaci�n")]
    [SerializeField, Tooltip("Animador opcional para los visuales del conveyor")]
    private Animator conveyorAnimator;

    [SerializeField, Tooltip("Nombre del par�metro de animaci�n para la velocidad")]
    private string speedParameter = "Speed";

    [SerializeField, Tooltip("Efecto de part�culas al entrar en el conveyor")]
    private ParticleSystem entryEffect;

    [SerializeField, Tooltip("Efecto de part�culas al salir del conveyor")]
    private ParticleSystem exitEffect;

    [SerializeField, Tooltip("Sonido del conveyor mientras est� activo")]
    private AudioSource conveyorSound;

    [Header("Optimizaci�n")]
    [SerializeField, Tooltip("Capas de los objetos que pueden ser afectados por el conveyor")]
    private LayerMask movableLayerMask;

    [Header("Eventos")]
    public UnityEvent<GameObject, ConveyorDirection> OnObjectEnter;
    public UnityEvent<GameObject> OnObjectExit;

    [Header("Depuraci�n")]
    [SerializeField, Tooltip("Muestra gizmos para depuraci�n (direcci�n y velocidad)")]
    private bool showDebugGizmos = true;
    #endregion

    #region Private Fields
    private Collider2D conveyorCollider;
    private float dynamicTimer;
    private Vector2 cachedConveyorForce;
    private Dictionary<Collider2D, (Rigidbody2D rb, Player player, bool isBattery)> cachedObjects = new Dictionary<Collider2D, (Rigidbody2D, Player, bool)>();
    private float originalSpeed;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // Cachear el Collider2D
        conveyorCollider = GetComponent<Collider2D>();
        if (conveyorCollider == null)
        {
            Debug.LogError($"{gameObject.name} requiere un componente Collider2D.");
            enabled = false;
            return;
        }

        // Asegurarse de que el collider sea un trigger
        if (!conveyorCollider.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}: El Collider2D debe ser un trigger para que el conveyor funcione correctamente.");
            conveyorCollider.isTrigger = true;
        }

        // Cachear la velocidad original
        originalSpeed = speed;
    }

    private void Start()
    {
        UpdateConveyorForce();
        UpdateVisuals();
    }

    private void Update()
    {
        // Comportamiento din�mico
        if (useDynamicBehavior)
        {
            dynamicTimer += Time.deltaTime;
            float curveValue = speedCurve.Evaluate(dynamicTimer % directionChangeInterval / directionChangeInterval);
            speed = Mathf.Lerp(0f, originalSpeed, curveValue);

            if (dynamicTimer >= directionChangeInterval)
            {
                dynamicTimer = 0f;
                SetDirection(direction == ConveyorDirection.Forward ? ConveyorDirection.Reverse : ConveyorDirection.Forward);
            }

            UpdateConveyorForce();
            UpdateVisuals();
        }
    }

    private void OnValidate()
    {
        // Validar LayerMask
        if (movableLayerMask.value == 0)
        {
            Debug.LogWarning($"El LayerMask 'movableLayerMask' en {gameObject.name} est� vac�o. Esto har� que el conveyor no afecte a ning�n objeto.");
        }

        // Validar par�metros
        if (accelerationTime <= 0f)
        {
            Debug.LogWarning($"El 'accelerationTime' en {gameObject.name} debe ser mayor que 0. Se ajustar� a un valor m�nimo.");
            accelerationTime = 0.01f;
        }

        UpdateVisuals();
    }
    #endregion

    #region Trigger Events
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsLayerInMask(collision.gameObject.layer, movableLayerMask)) return;

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        Player player = collision.GetComponent<Player>();
        bool isBattery = collision.gameObject == (player?.battery);

        if (rb != null || player != null)
        {
            cachedObjects[collision] = (rb, player, isBattery);
            OnObjectEnter.Invoke(collision.gameObject, direction);
        }

        if (entryEffect != null)
        {
            entryEffect.Play();
        }

        if (conveyorSound != null && !conveyorSound.isPlaying && direction != ConveyorDirection.Stopped)
        {
            conveyorSound.Play();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!cachedObjects.TryGetValue(collision, out var cached)) return;

        Rigidbody2D rb = cached.rb;
        Player player = cached.player;
        bool isBattery = cached.isBattery;

        // Aplicar fuerza al Rigidbody2D
        if (rb != null)
        {
            float forceMultiplier = isBattery ? 0.5f : 1f; // Reducir fuerza para la bater�a
            rb.AddForce(cachedConveyorForce * forceMultiplier * Time.deltaTime / accelerationTime, forceMode);
        }

        // Manejar l�gica del jugador
        if (player != null)
        {
            player.IsOnConveyorBelt = true;
            player.SetConveyorDirection(cachedConveyorForce.x / speed, this, speed);

            // Interacci�n con la bater�a
            if (player.battery != null)
            {
                var batteryController = player.battery.GetComponent<BatteryController>();
                if (batteryController != null)
                {
                    // Efecto de energ�a
                    if (batteryEffect != 0f)
                    {
                        batteryController.energyAmounts += batteryEffect * Time.deltaTime;
                        batteryController.energyAmounts = Mathf.Clamp(batteryController.energyAmounts, 0f, batteryController.maxEnergy);
                    }

                    // Efecto de polaridad
                    if (affectBatteryPolarity)
                    {
                        bool shouldBePositive = direction == ConveyorDirection.Forward;
                        if (batteryController.isPositivePolarity != shouldBePositive)
                        {
                            batteryController.isPositivePolarity = shouldBePositive;
                            player.StartCoroutine(player.FlashBatteryColor());
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!cachedObjects.Remove(collision, out var cached)) return;

        if (cached.player != null)
        {
            cached.player.IsOnConveyorBelt = false;
            cached.player.SetConveyorDirection(0f, this);
        }

        OnObjectExit.Invoke(collision.gameObject);

        if (exitEffect != null)
        {
            exitEffect.Play();
        }

        if (conveyorSound != null && cachedObjects.Count == 0)
        {
            conveyorSound.Stop();
        }
    }
    #endregion

    #region Public Methods
    public void SetDirection(ConveyorDirection newDirection)
    {
        direction = newDirection;
        UpdateConveyorForce();
        UpdateVisuals();

        // Desactivar el collider si est� detenido
        if (conveyorCollider != null)
        {
            conveyorCollider.enabled = direction != ConveyorDirection.Stopped;
        }

        Debug.Log($"Conveyor {gameObject.name} configurado a: {direction}");
    }

    public void ToggleDirection(bool reverse)
    {
        SetDirection(reverse ? ConveyorDirection.Reverse : ConveyorDirection.Forward);
    }

    public void SetSpeed(float newSpeed)
    {
        originalSpeed = newSpeed;
        speed = newSpeed;
        UpdateConveyorForce();
        UpdateVisuals();
    }
    #endregion

    #region Helper Methods
    private bool IsLayerInMask(int layer, LayerMask layerMask)
    {
        return ((1 << layer) & layerMask.value) != 0;
    }

    private void UpdateConveyorForce()
    {
        float directionMultiplier = direction switch
        {
            ConveyorDirection.Forward => 1f,
            ConveyorDirection.Reverse => -1f,
            ConveyorDirection.Stopped => 0f,
            _ => 1f
        };
        cachedConveyorForce = new Vector2(speed * directionMultiplier, 0f);
    }

    private void UpdateVisuals()
    {
        if (conveyorAnimator != null)
        {
            float animSpeed = direction == ConveyorDirection.Stopped ? 0f : speed * (direction == ConveyorDirection.Reverse ? -1f : 1f);
            conveyorAnimator.SetFloat(speedParameter, animSpeed);
        }

        if (conveyorSound != null)
        {
            float pitch = direction == ConveyorDirection.Stopped ? 0f : 1f * (direction == ConveyorDirection.Reverse ? -0.8f : 1f);
            conveyorSound.pitch = pitch;
        }
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Gizmos.color = direction == ConveyorDirection.Forward ? Color.green : (direction == ConveyorDirection.Reverse ? Color.red : Color.gray);
        Vector3 directionVector = direction == ConveyorDirection.Forward ? Vector3.right : (direction == ConveyorDirection.Reverse ? Vector3.left : Vector3.zero);
        Gizmos.DrawRay(transform.position, directionVector * speed);
    }
    #endregion
}
