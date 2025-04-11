using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType
{
    None,
    MaxTimeWithoutBattery, // Aumenta el tiempo máximo sin la batería
    MaxEnergy,            // Aumenta la capacidad máxima de energía de la batería
    MagneticRange         // Aumenta el rango de la habilidad magnética
}

public class Player : MonoBehaviour
{
    #region State Variables
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerAirState AirState { get; private set; }
    public PlayerLandState LandState { get; private set; }
    public PlayerPushState PushState { get; private set; }
    public PlayerInteractionState InteractionState { get; private set; }
    public PlayerMagneticState MagneticState { get; private set; }
    public PlayerDeadState DeadState { get; private set; }
    public PlayerSeparatedState SeparatedState { get; private set; }
    public PlayerThrowState ThrowState { get; private set; }
    public PlayerAimBatteryState AimBatteryState { get; private set; }
    public PlayerBoostState BoostState { get; private set; }

    [SerializeField]
    private PlayerData playerData;

    private PlayerHealthSystem healthSystem;
    #endregion

    #region Components
    public Animator Anim { get; private set; }
    public PlayerInputHadler InputHadler { get; private set; }
    public Rigidbody2D rb { get; private set; }
    private SpriteRenderer sr;
    #endregion

    #region Check Transforms
    [SerializeField]
    private Transform groundCheck;

    [Header("Battery")]
    public GameObject battery;
    public bool isSeparated = false;
    public float maxTimeWithoutBattery = 10f;
    public float currentTime;
    private bool isTimerPaused;
    private bool isTimerResetting;

    [Header("Battery Movement")]
    [SerializeField] private float batterySpeed = 5f;
    [SerializeField] private float batteryAmplitude = 0.1f;
    [SerializeField] private float batteryFrequency = 2f;
    [SerializeField] private float bounceAmplitude = 0.05f;
    private Vector2 targetBatteryPosition;
    private float floatTimer;

    [Header("Progression Settings")]
    [SerializeField] private int crystalsPerUpgrade = 5;
    [SerializeField] private ParticleSystem upgradeEffect;
    [SerializeField] private AudioSource upgradeSound;
    private int collectedCrystals = 0;
    private float originalMaxTimeWithoutBattery;
    private float originalMaxEnergy;
    private PlayerUI playerUI;
    private UpgradeSelectionUI upgradeSelectionUI;

    [Header("Timer Settings")]
    [SerializeField] private float timerResetDuration = 2f;

    [Header("Boost Effects")]
    [SerializeField] private ParticleSystem boostEffect;
    private float lastBoostTime;

    [SerializeField]
    public Transform InteractionCheck;
    #endregion

    #region Other Variables
    public Vector2 CurrentVelocity { get; private set; }
    public int FacingDirection { get; private set; }
    public bool isOnConveyorBelt { get; set; } = false;
    private float conveyorDirection = 0f;

    public float originalSpeed;
    private Vector2 workspace;
    #endregion

    #region Aim Dots
    [Header("Aim Dots")]
    [SerializeField] private int numberOfDots = 10;
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private Transform dotsParent;
    private GameObject[] aimDots;
    public GameObject[] AimDots => aimDots;
    #endregion

    #region Unity Callback Functions
    private void Awake()
    {
        StateMachine = new PlayerStateMachine();

        IdleState = new PlayerIdleState(this, StateMachine, playerData, "Idle");
        MoveState = new PlayerMoveState(this, StateMachine, playerData, "Move");
        JumpState = new PlayerJumpState(this, StateMachine, playerData, "Air");
        AirState = new PlayerAirState(this, StateMachine, playerData, "Air");
        LandState = new PlayerLandState(this, StateMachine, playerData, "Land");
        PushState = new PlayerPushState(this, StateMachine, playerData, "Push");
        InteractionState = new PlayerInteractionState(this, StateMachine, playerData, "Interaction");
        MagneticState = new PlayerMagneticState(this, StateMachine, playerData, "Magnetic");
        DeadState = new PlayerDeadState(this, StateMachine, playerData, "Dead");
        SeparatedState = new PlayerSeparatedState(this, StateMachine, playerData, "Separated");
        ThrowState = new PlayerThrowState(this, StateMachine, playerData, "Throw");
        AimBatteryState = new PlayerAimBatteryState(this, StateMachine, playerData, "Aim");
        BoostState = new PlayerBoostState(this, StateMachine, playerData, "Boost");

        sr = GetComponent<SpriteRenderer>();

        GenerateDots();
    }

    private void Start()
    {
        Anim = GetComponent<Animator>();
        InputHadler = GetComponent<PlayerInputHadler>();
        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<PlayerHealthSystem>();

        playerData.movementVeclocity = 8;
        originalSpeed = playerData.movementVeclocity;

        FacingDirection = 1;
        currentTime = 0f; // Inicializa currentTime en 0

        originalMaxTimeWithoutBattery = maxTimeWithoutBattery;
        if (battery != null)
        {
            originalMaxEnergy = battery.GetComponent<BatteryController>().maxEnergy;
        }

        playerUI = FindAnyObjectByType<PlayerUI>();
        upgradeSelectionUI = FindAnyObjectByType<UpgradeSelectionUI>();

        StateMachine.Intialize(IdleState);
    }

    private void Update()
    {
        Debug.Log(CheckIfGrounded());
        CurrentVelocity = rb.linearVelocity;
        StateMachine.CurrentState.LogicUpdate();

        if (InputHadler.SeparateInput && !isSeparated)
        {
            SeparateBattery();
        }
        else if (InputHadler.SeparateInput && isSeparated && StateMachine.CurrentState != AimBatteryState)
        {
            ReuniteBattery();
        }

        if (InputHadler.InteractInput && CheckInteraction())
        {
            StateMachine.ChangeState(InteractionState);
        }

        if (InputHadler.MagneticInput)
        {
            StateMachine.ChangeState(MagneticState);
        }

        if (InputHadler.SwitchPolarityInput)
        {
            battery.GetComponent<BatteryController>().isPositivePolarity = !battery.GetComponent<BatteryController>().isPositivePolarity;
            InputHadler.UseSwitchPolarityInput();
            Debug.Log("Polaridad cambiada a: " + (battery.GetComponent<BatteryController>().isPositivePolarity ? "Positiva" : "Negativa"));
        }

        if (InputHadler.ThrowInput && !isSeparated)
        {
            StateMachine.ChangeState(AimBatteryState);
        }

        /*if (InputHadler.BoostInput && !isSeparated && Time.time >= lastBoostTime + playerData.boostCooldown)
        {
            StateMachine.ChangeState(BoostState);
            lastBoostTime = Time.time;
            if (boostEffect != null) boostEffect.Play();
        }*/

        // Lógica del temporizador cuando la batería está separada
        if (isSeparated && !isTimerPaused && !isTimerResetting)
        {
            float distanceToBattery = Vector2.Distance(transform.position, battery.transform.position);
            if (distanceToBattery > playerData.safeRange)
            {
                currentTime += Time.deltaTime;
                Debug.Log($"Tiempo sin batería (fuera de rango): {currentTime}/{maxTimeWithoutBattery}. Distancia: {distanceToBattery}");
                if (currentTime >= maxTimeWithoutBattery)
                {
                    Debug.Log("Tiempo sin batería agotado. El jugador muere.");
                    StateMachine.ChangeState(DeadState);
                }
            }
            else
            {
                Debug.Log($"Batería dentro del rango seguro ({distanceToBattery}/{playerData.safeRange}). Temporizador pausado.");
            }
        }

        // Si la batería regresa al jugador, inicia la disminución progresiva del temporizador
        if (!isSeparated && currentTime > 0 && !isTimerResetting)
        {
            StartCoroutine(ResetTimerProgressively());
        }

        if (!isSeparated)
        {
            Vector2 targetPos = (Vector2)transform.position + targetBatteryPosition;
            floatTimer += Time.deltaTime;
            float floatOffset = Mathf.Sin(floatTimer * batteryFrequency) * batteryAmplitude;
            targetPos += new Vector2(0f, floatOffset);

            Vector2 velocityOffset = -CurrentVelocity * bounceAmplitude;
            targetPos += velocityOffset;

            battery.transform.position = Vector2.Lerp(battery.transform.position, targetPos, batterySpeed * Time.deltaTime);

            float rotationAngle = CurrentVelocity.x * 2f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, rotationAngle);
            battery.transform.rotation = Quaternion.Lerp(battery.transform.rotation, targetRotation, batterySpeed * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }
    #endregion

    #region Set Functions
    private IEnumerator ResetTimerProgressively()
    {
        isTimerResetting = true;
        float startTime = currentTime;
        float elapsedTime = 0f;

        while (elapsedTime < timerResetDuration)
        {
            elapsedTime += Time.deltaTime;
            currentTime = Mathf.Lerp(startTime, 0f, elapsedTime / timerResetDuration);
            Debug.Log($"Disminuyendo temporizador progresivamente: {currentTime}/{maxTimeWithoutBattery}");
            yield return null;
        }

        currentTime = 0f;
        isTimerResetting = false;
        Debug.Log("Temporizador reiniciado progresivamente a 0.");
    }

    public void ResetTimer()
    {
        StartCoroutine(ResetTimerProgressively());
    }

    public bool IsTimerPaused
    {
        get => isTimerPaused;
        set => isTimerPaused = value;
    }

    public void SetVelocityX(float velocity)
    {
        float finalVelocity = velocity;

        if (isOnConveyorBelt)
        {
            if (Mathf.Sign(velocity) != Mathf.Sign(conveyorDirection))
            {
                finalVelocity *= 0.5f;
            }
            else
            {
                finalVelocity *= 1.2f;
            }
        }

        workspace.Set(finalVelocity, CurrentVelocity.y);
        rb.linearVelocity = workspace;
        CurrentVelocity = workspace;
    }

    public void SetVelocityY(float velocity)
    {
        float finalVelocity = velocity;

        workspace.Set(CurrentVelocity.x, finalVelocity);
        rb.linearVelocity = workspace;
        CurrentVelocity = workspace;
    }

    public void SetConveyorDirection(float direction)
    {
        conveyorDirection = direction;
    }
    #endregion

    #region Check Functions
    public bool CheckIfGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, playerData.groundCheckRadius, playerData.whatIsGround);
    }

    public bool CheckIfPush()
    {
        return Physics2D.Raycast(transform.position, Vector2.right * transform.transform.localScale.x, playerData.distance, playerData.boxMask);
    }

    public void CheckIfShouldFlip(int xInput)
    {
        if (xInput != 0 && xInput != FacingDirection)
        {
            Flip();
        }
    }

    public bool CheckInteraction()
    {
        Collider2D detectedObject = Physics2D.OverlapCircle(InteractionCheck.position, playerData.interactionRadius, playerData.whatIsInteractable);

        if (detectedObject != null)
        {
            return true;
        }

        return false;
    }

    private void SeparateBattery()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector2 headPosition;
        if (sr != null)
        {
            float headY = sr.bounds.extents.y;
            headPosition = (Vector2)transform.position + new Vector2(0f, headY - 1f);
        }
        else
        {
            headPosition = (Vector2)transform.position + new Vector2(0f, 0.1f);
        }

        battery.transform.parent = null;
        battery.transform.position = headPosition;

        Rigidbody2D rb = battery.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 1f;

        isSeparated = true;
        StateMachine.ChangeState(SeparatedState);
        InputHadler.UseSeparateInput();

        floatTimer = 0f;
        currentTime = 0f; // Reinicia el temporizador al separar la batería
    }

    private void ReuniteBattery()
    {
        StartCoroutine(MoveBatteryToPlayer());
    }

    private IEnumerator MoveBatteryToPlayer()
    {
        Rigidbody2D rb = battery.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.Sleep();

        Collider2D collider = battery.GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        float elapsedTime = 0f;
        float moveDuration = 0.5f;
        Vector2 startPos = battery.transform.position;
        battery.transform.parent = null;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float headY = sr != null ? sr.bounds.extents.y : 0.5f;
        targetBatteryPosition = new Vector2(0f, headY - 1f);
        Vector2 targetPos = (Vector2)transform.position + targetBatteryPosition;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            battery.transform.position = Vector2.Lerp(startPos, targetPos, elapsedTime / moveDuration);
            yield return null;
        }

        battery.transform.position = targetPos;
        battery.transform.rotation = Quaternion.identity;
        if (collider != null) collider.enabled = true;

        isSeparated = false;
        StateMachine.ChangeState(IdleState);
        InputHadler.UseSeparateInput();
        Debug.Log("Batería recogida y colocada encima de la cabeza.");
    }
    #endregion

    #region Other Functions
    private void AnimationTrigger() => StateMachine.CurrentState.AnimationTrigger();
    private void AnimationFinishTrigger() => StateMachine.CurrentState.AnimationFinishTrigger();

    private void Flip()
    {
        FacingDirection *= -1;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    private void GenerateDots()
    {
        aimDots = new GameObject[numberOfDots];
        for (int i = 0; i < numberOfDots; i++)
        {
            aimDots[i] = Instantiate(dotPrefab, transform.position, Quaternion.identity, dotsParent);
            aimDots[i].SetActive(false);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, playerData.safeRange);
    }
    #endregion

    #region Upgrade Functions
    public void AddCrystal(int crystalValue)
    {
        collectedCrystals += crystalValue;
        Debug.Log($"Cristales recolectados: {collectedCrystals}/{crystalsPerUpgrade}");

        while (collectedCrystals >= crystalsPerUpgrade)
        {
            if (upgradeSelectionUI != null)
            {
                upgradeSelectionUI.ShowUpgradeSelection();
            }
            else
            {
                ApplyUpgrade(UpgradeType.MaxTimeWithoutBattery);
            }

            collectedCrystals -= crystalsPerUpgrade;
        }

        UpdateCrystalUI();
    }

    public void ApplyUpgrade(UpgradeType upgrade)
    {
        switch (upgrade)
        {
            case UpgradeType.MaxTimeWithoutBattery:
                maxTimeWithoutBattery += 2f;
                playerData.maxTimeWithoutBattery = maxTimeWithoutBattery; // Sincroniza con playerData
                Debug.Log($"Mejora desbloqueada: +2 segundos sin batería. Nuevo valor: {maxTimeWithoutBattery}");
                break;

            case UpgradeType.MaxEnergy:
                if (battery != null)
                {
                    BatteryController batteryController = battery.GetComponent<BatteryController>();
                    batteryController.maxEnergy += 20f;
                    batteryController.energyAmounts = Mathf.Clamp(batteryController.energyAmounts, 0f, batteryController.maxEnergy);
                    Debug.Log($"Mejora desbloqueada: +20 de capacidad máxima de energía. Nuevo valor: {batteryController.maxEnergy}");
                }
                break;

            case UpgradeType.MagneticRange:
                Debug.Log("Mejora desbloqueada: +1 al rango magnético");
                break;

            default:
                Debug.LogWarning("No se seleccionó ninguna mejora válida");
                break;
        }

        if (upgradeEffect != null) upgradeEffect.Play();
        if (upgradeSound != null) upgradeSound.Play();

        ShowUpgradeNotification(upgrade);
    }

    private void UpdateCrystalUI()
    {
        if (playerUI != null)
        {
            playerUI.UpdateCrystalUI(collectedCrystals);
        }
    }

    private void ShowUpgradeNotification(UpgradeType upgrade)
    {
        if (playerUI != null)
        {
            playerUI.ShowUpgradeNotification($"¡Mejora desbloqueada: {upgrade}!");
        }
    }
    #endregion
}