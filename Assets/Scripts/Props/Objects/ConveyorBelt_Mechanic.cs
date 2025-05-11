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

    [Header("Interacci�n con Objetos")]
    [SerializeField, Tooltip("Multiplicador de fuerza para objetos gen�ricos (como cajas)")]
    private float genericObjectForceMultiplier = 1f;

    [SerializeField, Tooltip("Multiplicador de fuerza para la bater�a del jugador")]
    private float batteryForceMultiplier = 0.5f;

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

    [SerializeField, Tooltip("Nombre del sonido del conveyor en el AudioManager")]
    private string sfxSoundName = "ConveyorLoop";

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
    private Dictionary<Collider2D, (Rigidbody2D rb, Player player, BatteryController battery, float forceMultiplier)> cachedObjects = new Dictionary<Collider2D, (Rigidbody2D, Player, BatteryController, float)>();
    private float originalSpeed;
    private bool isSoundPlaying = false;
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
        if (rb == null) return; // Solo procesar objetos con Rigidbody2D

        Player player = collision.GetComponent<Player>();
        BatteryController battery = null;
        float forceMultiplier = genericObjectForceMultiplier;

        // Determinar si el objeto es la bater�a del jugador
        if (player != null && player.battery != null && collision.gameObject == player.battery)
        {
            battery = player.battery.GetComponent<BatteryController>();
            forceMultiplier = batteryForceMultiplier;
        }
        else if (player != null)
        {
            forceMultiplier = 1f; // El jugador usa fuerza completa
        }

        cachedObjects[collision] = (rb, player, battery, forceMultiplier);
        OnObjectEnter.Invoke(collision.gameObject, direction);

        if (entryEffect != null)
        {
            entryEffect.Play();
        }

        // Reproducir sonido si no est� sonando y el conveyor no est� detenido
        if (!isSoundPlaying && direction != ConveyorDirection.Stopped)
        {
            AudioManager.instance.PlaySFX(sfxSoundName);
            isSoundPlaying = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!cachedObjects.TryGetValue(collision, out var cached)) return;

        Rigidbody2D rb = cached.rb;
        Player player = cached.player;
        BatteryController battery = cached.battery;
        float forceMultiplier = cached.forceMultiplier;

        // Aplicar fuerza al Rigidbody2D
        if (rb != null)
        {
            rb.AddForce(cachedConveyorForce * forceMultiplier * Time.deltaTime / accelerationTime, forceMode);
        }

        // Manejar l�gica del jugador
        if (player != null)
        {
            player.IsOnConveyorBelt = true;
            player.SetConveyorDirection(cachedConveyorForce.x / speed, this, speed);
        }

        // Manejar l�gica de la bater�a
        if (battery != null)
        {
            // Efecto de energ�a
            if (batteryEffect != 0f)
            {
                battery.energyAmounts += batteryEffect * Time.deltaTime;
                battery.energyAmounts = Mathf.Clamp(battery.energyAmounts, 0f, battery.maxEnergy);
            }

            // Efecto de polaridad
            if (affectBatteryPolarity && player != null)
            {
                bool shouldBePositive = direction == ConveyorDirection.Forward;
                if (battery.isPositivePolarity != shouldBePositive)
                {
                    battery.isPositivePolarity = shouldBePositive;
                    player.StartCoroutine(player.FlashBatteryColor());
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

        // Detener el sonido si no hay m�s objetos en el conveyor
        if (cachedObjects.Count == 0)
        {
            AudioManager.instance.StopAllSFX();
            isSoundPlaying = false;
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

        // Ajustar el sonido seg�n la direcci�n
        if (direction == ConveyorDirection.Stopped && isSoundPlaying)
        {
            AudioManager.instance.StopAllSFX();
            isSoundPlaying = false;
        }
        else if (direction != ConveyorDirection.Stopped && !isSoundPlaying && cachedObjects.Count > 0)
        {
            AudioManager.instance.PlaySFX(sfxSoundName);
            isSoundPlaying = true;
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
