using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType
{
    None,
    MaxTimeWithoutBattery,
    MaxEnergy,
    MagneticRange
}

public class Player : MonoBehaviour
{
    #region Constants 
    private const int CrystalsPerUpgrade = 5; 
    private const float MaxLifeProgressDefault = 10f;
    #endregion
    #region State Variables
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerAirState AirState { get; private set; }
    public PlayerLandState LandState { get; private set; }
    public PlayerInteractionState InteractionState { get; private set; }
    public PlayerMagneticState MagneticState { get; private set; }
    public PlayerDeadState DeadState { get; private set; }
    public PlayerSeparatedState SeparatedState { get; private set; }
    public PlayerThrowState ThrowState { get; private set; }
    public PlayerAimBatteryState AimBatteryState { get; private set; }
    #endregion
    #region Components
    [SerializeField] private PlayerData playerData;
    private PlayerHealthSystem healthSystem;
    public Animator Anim { get; private set; }
    public PlayerInputHadler InputHandler { get; private set; }
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    #endregion
    #region Battery
    [Header("Battery")]
    public GameObject battery;
    public bool isSeparated = false;
    public float maxLifeProgress = 10f;
    public float currentLifeProgress { get; private set; }
    private bool isLifeProgressPaused;
    private bool isTimerResetting;
    private SpriteRenderer batterySpriteRenderer;
    private Color batteryOriginalColor;

    [Header("Battery Movement")]
    [SerializeField] private float batterySpeed = 5f;
    [SerializeField] private float batteryAmplitude = 0.1f;
    [SerializeField] private float batteryFrequency = 2f;
    [SerializeField] private float bounceAmplitude = 0.05f;
    [SerializeField] private Transform batteryPosition;
    private Vector2 targetBatteryPosition;
    private float floatTimer;
    #endregion
    #region Progression
    [Header("Progression Settings")]
    [SerializeField] private ParticleSystem upgradeEffect;
    private int collectedCrystals;
    private float originalMaxLifeProgress;
    private float originalMaxEnergy;
    private PlayerUI playerUI;
    private UpgradeSelectionUI upgradeSelectionUI;
    private bool isUpgradeSelectionActive;
    #endregion
    #region Audio Settings
    [Header("Audio Settings")]
    [SerializeField, Tooltip("Nombre del sonido al aplicar una mejora en el AudioManager")]
    private string upgradeSoundName = "UpgradeUnlock";
    [SerializeField, Tooltip("Nombre del sonido al separar la batería en el AudioManager")]
    private string separateBatterySoundName = "BatterySeparate";
    [SerializeField, Tooltip("Nombre del sonido al reunir la batería en el AudioManager")]
    private string reuniteBatterySoundName = "BatteryReunite";
    [SerializeField, Tooltip("Nombre del sonido al cambiar la polaridad de la batería en el AudioManager")]
    private string switchPolaritySoundName = "PolaritySwitch";
    [SerializeField, Tooltip("Nombre del sonido al perder vida por tiempo sin batería en el AudioManager")]
    private string lifeProgressLossSoundName = "LifeProgressLoss";
    #endregion
    #region Interaction
    [Header("Interactable Indicator")]
    [SerializeField] public Transform interactionCheck;
    [SerializeField] private InteractableIndicator globalIndicator;
    private GameObject lastInteractable;
    #endregion
    #region Movement
    [Header("Movement")]
    [SerializeField] private Transform groundCheck;
    public Vector2 CurrentVelocity { get; private set; }
    public int FacingDirection { get; private set; }
    public bool IsOnConveyorBelt { get; set; }
    private float conveyorDirection;
    public float OriginalSpeed { get; private set; }
    private Vector2 workspace;

    [Header("Configuración del Conveyor")]
    [SerializeField] private float conveyorSpeedModifier = 0.5f; // Menos control en el conveyor
    private List<ConveyorInfluence> activeConveyors = new List<ConveyorInfluence>();
    private struct ConveyorInfluence
    {
        public float Direction;
        public float Speed;
        public ConveyorBelt_Mechanic Conveyor;
    }
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
        InitializeStateMachine();
        sr = GetComponent<SpriteRenderer>();
        GenerateAimDots();
    }

    private void Start()
    {
        Anim = GetComponent<Animator>();
        InputHandler = GetComponent<PlayerInputHadler>();
        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<PlayerHealthSystem>();

        InitializeMovement();
        InitializeBattery();
        InitializeUI();
        StateMachine.Intialize(IdleState);
    }

    private void Update()
    {
        CurrentVelocity = rb.linearVelocity;
        StateMachine.CurrentState.LogicUpdate();
        HandleInput();
        UpdateBatteryLifeProgress();
        UpdateBatteryPosition();
        CheckInteraction();
        UpdateInteractableIndicator();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }
    #endregion
    #region Initialization
    private void InitializeStateMachine()
    {
        StateMachine = new PlayerStateMachine();
        IdleState = new PlayerIdleState(this, StateMachine, playerData, "Idle");
        MoveState = new PlayerMoveState(this, StateMachine, playerData, "Move");
        JumpState = new PlayerJumpState(this, StateMachine, playerData, "Air");
        AirState = new PlayerAirState(this, StateMachine, playerData, "Air");
        LandState = new PlayerLandState(this, StateMachine, playerData, "Land");
        InteractionState = new PlayerInteractionState(this, StateMachine, playerData, "Interaction");
        MagneticState = new PlayerMagneticState(this, StateMachine, playerData, "Magnetic");
        DeadState = new PlayerDeadState(this, StateMachine, playerData, "Dead");
        SeparatedState = new PlayerSeparatedState(this, StateMachine, playerData, "Separated");
        ThrowState = new PlayerThrowState(this, StateMachine, playerData, "Throw");
        AimBatteryState = new PlayerAimBatteryState(this, StateMachine, playerData, "Aim");
    }

    private void InitializeMovement()
    {
        playerData.movementVeclocity = 8f;
        OriginalSpeed = playerData.movementVeclocity;
        FacingDirection = 1;
    }

    private void InitializeBattery()
    {
        currentLifeProgress = maxLifeProgress;
        originalMaxLifeProgress = maxLifeProgress;
        playerData.maxTimeWithoutBattery = maxLifeProgress;

        if (battery != null)
        {
            batterySpriteRenderer = battery.GetComponent<SpriteRenderer>();
            batteryOriginalColor = batterySpriteRenderer.color;
            var batteryController = battery.GetComponent<BatteryController>();
            batteryController.batteryPoints = batteryController.maxBatteryPoints;
        }
    }

    private void InitializeUI()
    {
        playerUI = FindAnyObjectByType<PlayerUI>();
        upgradeSelectionUI = FindAnyObjectByType<UpgradeSelectionUI>();

        if (playerUI == null)
        {
            Debug.LogWarning("PlayerUI no encontrado en la escena. Asegúrate de que esté presente.");
        }

        if (upgradeSelectionUI == null)
        {
            Debug.LogWarning("UpgradeSelectionUI no encontrado en la escena. Asegúrate de que esté presente.");
        }

        if (globalIndicator != null)
        {
            globalIndicator.Hide();
        }
        else
        {
            Debug.LogWarning("GlobalIndicator no está asignado en el script Player.");
        }
    }
    #endregion
    #region Input Handling
    private void HandleInput()
    {
        if (InputHandler.SeparateInput && !isSeparated)
        {
            SeparateBattery();
        }
        else if (InputHandler.SeparateInput && isSeparated && StateMachine.CurrentState != AimBatteryState)
        {
            ReuniteBattery();
        }

        if (InputHandler.InteractInput && lastInteractable != null)
        {
            StateMachine.ChangeState(InteractionState);
        }

        if (InputHandler.MagneticInput)
        {
            StateMachine.ChangeState(MagneticState);
        }

        if (InputHandler.SwitchPolarityInput)
        {
            SwitchBatteryPolarity();
        }

        if (InputHandler.ThrowInput && !isSeparated)
        {
            StateMachine.ChangeState(AimBatteryState);
        }
    }

    private void SwitchBatteryPolarity()
    {
        var batteryController = battery.GetComponent<BatteryController>();
        batteryController.isPositivePolarity = !batteryController.isPositivePolarity;
        InputHandler.UseSwitchPolarityInput();

        AudioManager.instance.PlaySFX(switchPolaritySoundName);

        if (batterySpriteRenderer != null)
        {
            StopCoroutine(FlashBatteryColor());
            StartCoroutine(FlashBatteryColor());
        }
    }
    #endregion
    #region Battery Management
    private void UpdateBatteryLifeProgress()
    {
        if (!isSeparated || isLifeProgressPaused) return;

        float distanceToBattery = Vector2.Distance(transform.position, battery.transform.position);
        if (distanceToBattery <= playerData.safeRange) return;

        currentLifeProgress -= Time.deltaTime;
        if (currentLifeProgress > 0) return;

        healthSystem.LoseLife();
        AudioManager.instance.PlaySFX(lifeProgressLossSoundName);
        if (healthSystem.currentLives > 0)
        {
            currentLifeProgress = maxLifeProgress;
            Debug.Log($"Vida perdida. Vidas restantes: {healthSystem.currentLives}. Progreso reiniciado a: {currentLifeProgress}");
        }
        else
        {
            Debug.Log("Todas las vidas agotadas. El jugador muere.");
            StateMachine.ChangeState(DeadState);
        }
    }

    private void UpdateBatteryPosition()
    {
        if (isSeparated) return;

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

    private void SeparateBattery()
    {
        float headY = sr != null ? sr.bounds.extents.y : 0.1f;
        Vector2 headPosition = (Vector2)transform.position + new Vector2(0f, headY - 1f);

        battery.transform.parent = null;
        battery.transform.position = batteryPosition.transform.position;

        var rb = battery.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 1f;

        isSeparated = true;
        isLifeProgressPaused = false;
        StateMachine.ChangeState(SeparatedState);
        InputHandler.UseSeparateInput();
        floatTimer = 0f;

        AudioManager.instance.PlaySFX(separateBatterySoundName);
    }

    private void ReuniteBattery()
    {
        StartCoroutine(MoveBatteryToPlayer());
    }

    private IEnumerator MoveBatteryToPlayer()
    {
        var rb = battery.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.Sleep();

        var collider = battery.GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        float elapsedTime = 0f;
        const float moveDuration = 0.5f;
        Vector2 startPos = battery.transform.position;
        battery.transform.parent = null;

        float headY = sr != null ? sr.bounds.extents.y : 0.5f;
        targetBatteryPosition = new Vector2(0f, headY - 1f);
        Vector2 targetPos = (Vector2)transform.position + targetBatteryPosition;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            battery.transform.position = Vector2.Lerp(startPos, batteryPosition.transform.position, elapsedTime / moveDuration);
            yield return null;
        }

        battery.transform.position = batteryPosition.transform.position;
        battery.transform.rotation = Quaternion.identity;
        if (collider != null) collider.enabled = true;

        isSeparated = false;
        isLifeProgressPaused = true;
        currentLifeProgress = maxLifeProgress;
        InputHandler.UseSeparateInput();
        Debug.Log($"Batería recogida. Progreso de vida reiniciado a: {currentLifeProgress}");

        AudioManager.instance.PlaySFX(reuniteBatterySoundName);
    }

    public IEnumerator FlashBatteryColor()
    {
        var batteryController = battery.GetComponent<BatteryController>();
        Color flashColor = batteryController.isPositivePolarity ? Color.red : Color.green;
        batterySpriteRenderer.color = flashColor;
        yield return new WaitForSeconds(0.3f);
        batterySpriteRenderer.color = batteryOriginalColor;
    }

    public bool IsLifeProgressPaused
    {
        get => isLifeProgressPaused;
        set => isLifeProgressPaused = value;
    }
    #endregion
    #region Movement
    public void SetVelocityX(float velocity)
    {
        float finalVelocity = velocity;

        if (IsOnConveyorBelt)
        {
            float conveyorVelocity = 0f;
            foreach (var conveyor in activeConveyors)
            {
                conveyorVelocity += conveyor.Speed * conveyor.Direction;
            }

            // Aplicar influencia del conveyor
            finalVelocity = finalVelocity * conveyorSpeedModifier + conveyorVelocity;
        }

        workspace.Set(finalVelocity, CurrentVelocity.y);
        rb.linearVelocity = workspace;
        CurrentVelocity = workspace;
    }

    public void SetVelocityY(float velocity)
    {
        workspace.Set(CurrentVelocity.x, velocity);
        rb.linearVelocity = workspace;
        CurrentVelocity = workspace;
    }

    public void SetConveyorDirection(float direction, ConveyorBelt_Mechanic conveyor = null, float speed = 0f)
    {
        if (direction == 0f)
        {
            activeConveyors.RemoveAll(c => c.Conveyor == conveyor);
        }
        else
        {
            var influence = new ConveyorInfluence { Direction = direction, Speed = speed, Conveyor = conveyor };
            activeConveyors.RemoveAll(c => c.Conveyor == conveyor);
            activeConveyors.Add(influence);
        }

        IsOnConveyorBelt = activeConveyors.Count > 0;
    }

    public bool CheckIfGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, playerData.groundCheckRadius, playerData.whatIsGround);
    }

    public void CheckIfShouldFlip(int xInput)
    {
        if (xInput != 0 && xInput != FacingDirection)
        {
            Flip();
        }
    }

    private void Flip()
    {
        FacingDirection *= -1;
        transform.Rotate(0f, 180f, 0f);
    }
    #endregion
    #region Interactions
    public bool CheckInteraction()
    {
        if (interactionCheck == null)
        {
            Debug.LogError("InteractionCheck es null. No se puede detectar interacción.");
            return false;
        }

        var detectedObject = Physics2D.OverlapCircle(interactionCheck.position, playerData.interactionRadius, playerData.whatIsInteractable);
        lastInteractable = detectedObject != null ? detectedObject.gameObject : null;
        return lastInteractable != null;
    }

    private void UpdateInteractableIndicator()
    {
        if (globalIndicator == null) return;

        if (lastInteractable == null)
        {
            globalIndicator.Hide();
            return;
        }

        if (IsInteractableDisabled())
        {
            globalIndicator.Hide();
        }
        else
        {
            globalIndicator.Show(lastInteractable.transform.position);
        }
    }

    private bool IsInteractableDisabled()
    {
        var chargeable = lastInteractable.GetComponent<ChargeableObject>();
        if (chargeable != null && chargeable.isActive) return true;

        var charger = lastInteractable.GetComponent<BatteryCharger>();
        if (charger != null && !charger.IsReusable && charger.IsDisabled) return true;

        var pedestal = lastInteractable.GetComponent<PlatformPedestal>();
        return pedestal != null && pedestal.HasBattery;
    }
    #endregion
    #region Aim Dots
    private void GenerateAimDots()
    {
        aimDots = new GameObject[numberOfDots];
        for (int i = 0; i < numberOfDots; i++)
        {
            aimDots[i] = Instantiate(dotPrefab, transform.position, Quaternion.identity, dotsParent);
            aimDots[i].SetActive(false);
        }
    }
    #endregion
    #region Animation
    private void AnimationTrigger() => StateMachine.CurrentState.AnimationTrigger();
    private void AnimationFinishTrigger() => StateMachine.CurrentState.AnimationFinishTrigger();
    #endregion
    #region Upgrades
    public void AddCrystal(int crystalValue)
    {
        collectedCrystals += crystalValue;

        if (playerUI != null)
        {
            playerUI.UpdateCrystalUI(collectedCrystals % CrystalsPerUpgrade, collectedCrystals);
        }
        else
        {
            Debug.LogWarning("PlayerUI es null. No se puede actualizar la UI de cristales.");
        }

        if (collectedCrystals >= CrystalsPerUpgrade)
        {
            if (playerUI != null)
            {
                playerUI.ShowUpgradeNotification("¡Has recolectado 5 cristales! Puedes mejorar");
                Invoke(nameof(ShowUpgradeSelectionAfterDelay), playerUI.NotificationDuration);
            }
            else
            {
                Debug.LogWarning("PlayerUI es null. No se puede mostrar la notificación.");
                ShowUpgradeSelectionAfterDelay();
            }
        }
    }

    private void ShowUpgradeSelectionAfterDelay()
    {
        if (upgradeSelectionUI != null)
        {
            upgradeSelectionUI.ShowUpgradeSelection();
        }
        else
        {
            Debug.LogWarning("UpgradeSelectionUI es null. No se puede seleccionar una mejora.");
        }
    }

    public void ApplyUpgrade(UpgradeType upgrade)
    {
        if (collectedCrystals < CrystalsPerUpgrade)
        {
            Debug.LogWarning("No hay suficientes cristales para aplicar una mejora.");
            return;
        }

        switch (upgrade)
        {
            case UpgradeType.MaxTimeWithoutBattery:
                maxLifeProgress += 2f;
                playerData.maxTimeWithoutBattery = maxLifeProgress;
                Debug.Log($"Mejora desbloqueada: +2 segundos sin batería. Nuevo valor: {maxLifeProgress}");
                break;

            case UpgradeType.MaxEnergy:
                if (battery != null)
                {
                    var batteryController = battery.GetComponent<BatteryController>();
                    batteryController.maxBatteryPoints += 1; // Aumenta en 1 el máximo de puntos
                    batteryController.batteryPoints = Mathf.Clamp(
                        batteryController.batteryPoints,
                        0,
                        batteryController.maxBatteryPoints
                    );
                    Debug.Log($"Mejora desbloqueada: +1 punto máximo de batería. Nuevo valor: {batteryController.maxBatteryPoints}");
                }
                break;

            case UpgradeType.MagneticRange:
                Debug.Log("Mejora desbloqueada: +1 al rango magnético");
                break;

            default:
                Debug.LogWarning("No se seleccionó ninguna mejora válida");
                return;
        }

        collectedCrystals -= CrystalsPerUpgrade;
        isUpgradeSelectionActive = false;

        if (collectedCrystals >= CrystalsPerUpgrade && upgradeSelectionUI != null)
        {
            isUpgradeSelectionActive = true;
            upgradeSelectionUI.ShowUpgradeSelection();
        }

        if (upgradeEffect != null) upgradeEffect.Play();
        AudioManager.instance.PlaySFX(upgradeSoundName);

        ShowUpgradeNotification(upgrade);
        UpdateCrystalUI();
    }

    public void CancelUpgradeSelection()
    {
        isUpgradeSelectionActive = false;

        if (collectedCrystals >= CrystalsPerUpgrade && upgradeSelectionUI != null)
        {
            isUpgradeSelectionActive = true;
            upgradeSelectionUI.ShowUpgradeSelection();
        }

        UpdateCrystalUI();
    }

    public int GetAvailableUpgrades() => collectedCrystals / CrystalsPerUpgrade;
    public int GetDisplayedCrystals() => collectedCrystals % CrystalsPerUpgrade;

    private void UpdateCrystalUI()
    {
        if (playerUI != null)
        {
            playerUI.UpdateCrystalUI(GetDisplayedCrystals(), collectedCrystals);
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
    #region Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, playerData.safeRange);

        if (interactionCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(interactionCheck.position, playerData.interactionRadius);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, playerData.groundCheckRadius);
    }
    #endregion
}