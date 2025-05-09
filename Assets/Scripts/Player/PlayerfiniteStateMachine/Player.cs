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
    private Vector2 targetBatteryPosition;
    [SerializeField] private Transform positionBattery;
    private float floatTimer;

    [Header("Progression Settings")]
    [SerializeField] private int crystalsPerUpgrade = 5;
    [SerializeField] private ParticleSystem upgradeEffect;
    [SerializeField] private AudioSource upgradeSound;
    private int collectedCrystals = 0;
    private float originalMaxLifeProgress;
    private float originalMaxEnergy;
    private PlayerUI playerUI;
    private UpgradeSelectionUI upgradeSelectionUI;
    private bool isUpgradeSelectionActive = false;

    [Header("Timer Settings")]
    [SerializeField] private float timerResetDuration = 2f;

    [Header("Interactable Indicator")]
    [SerializeField]
    public Transform InteractionCheck;
    [SerializeField] private InteractableIndicator globalIndicator;
    private GameObject lastInteractable;
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
        InteractionState = new PlayerInteractionState(this, StateMachine, playerData, "Interaction");
        MagneticState = new PlayerMagneticState(this, StateMachine, playerData, "Magnetic");
        DeadState = new PlayerDeadState(this, StateMachine, playerData, "Dead");
        SeparatedState = new PlayerSeparatedState(this, StateMachine, playerData, "Separated");
        ThrowState = new PlayerThrowState(this, StateMachine, playerData, "Throw");
        AimBatteryState = new PlayerAimBatteryState(this, StateMachine, playerData, "Aim");

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
        currentLifeProgress = maxLifeProgress;

        originalMaxLifeProgress = maxLifeProgress;
        playerData.maxTimeWithoutBattery = maxLifeProgress;
        if (battery != null)
        {
            originalMaxEnergy = battery.GetComponent<BatteryController>().maxEnergy;
            batterySpriteRenderer = battery.GetComponent<SpriteRenderer>();
            batteryOriginalColor = batterySpriteRenderer.color;
        }

        playerUI = FindAnyObjectByType<PlayerUI>();
        upgradeSelectionUI = FindAnyObjectByType<UpgradeSelectionUI>();

        StateMachine.Intialize(IdleState);

        if (globalIndicator != null)
        {
            globalIndicator.Hide();
        }
        else
        {
            Debug.LogWarning("GlobalIndicator no está asignado en el script Player.");
        }

        if (playerUI == null)
        {
            Debug.LogWarning("PlayerUI no encontrado en la escena. Asegúrate de que esté presente.");
        }

        if (upgradeSelectionUI == null)
        {
            upgradeSelectionUI = FindAnyObjectByType<UpgradeSelectionUI>();
            if (upgradeSelectionUI == null)
            {
                Debug.LogWarning("UpgradeSelectionUI no encontrado en la escena. Asegúrate de que esté presente.");
            }
        }
    }

    private void Update()
    {
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

        bool canInteract = CheckInteraction();
        if (InputHadler.InteractInput && canInteract)
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

            // Efecto de cambio de color
            if (batterySpriteRenderer != null)
            {
                StopCoroutine(FlashBatteryColor());
                StartCoroutine(FlashBatteryColor());
            }
        }

        if (InputHadler.ThrowInput && !isSeparated)
        {
            StateMachine.ChangeState(AimBatteryState);
        }

        if (isSeparated && !isLifeProgressPaused)
        {
            float distanceToBattery = Vector2.Distance(transform.position, battery.transform.position);
            if (distanceToBattery > playerData.safeRange)
            {
                currentLifeProgress -= Time.deltaTime;
                if (currentLifeProgress <= 0)
                {
                    healthSystem.LoseLife();
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
            }
        }

        if (!isSeparated)
        {
            isLifeProgressPaused = true;

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

        // Actualizar el indicador de interacción
        UpdateInteractableIndicator();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }
    #endregion

    #region Set Functions
    private IEnumerator FlashBatteryColor()
    {
        Color flashColor = battery.GetComponent<BatteryController>().isPositivePolarity ? Color.red : Color.green;
        batterySpriteRenderer.color = flashColor;
        yield return new WaitForSeconds(0.3f);
        batterySpriteRenderer.color = batteryOriginalColor;
    }

    public bool IsLifeProgressPaused
    {
        get => isLifeProgressPaused;
        set => isLifeProgressPaused = value;
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

    public void CheckIfShouldFlip(int xInput)
    {
        if (xInput != 0 && xInput != FacingDirection)
        {
            Flip();
        }
    }

    public bool CheckInteraction()
    {
        if (InteractionCheck == null)
        {
            Debug.LogError("InteractionCheck es null. No se puede detectar interacción.");
            return false;
        }

        Collider2D detectedObject = Physics2D.OverlapCircle(InteractionCheck.position, playerData.interactionRadius, playerData.whatIsInteractable);

        if (detectedObject != null)
        {
            lastInteractable = detectedObject.gameObject;
            Debug.Log($"Objeto interactuable detectado: {lastInteractable.name} en posición {lastInteractable.transform.position}");
            return true;
        }

        lastInteractable = null;
        Debug.Log("Ningún objeto interactuable detectado.");
        return false;
    }

    private void UpdateInteractableIndicator()
    {
        if (lastInteractable != null)
        {
            if (globalIndicator == null)
            {
                Debug.LogWarning("GlobalIndicator es null. Asegúrate de asignarlo en el Inspector.");
                return;
            }

            // Verificar si es un ChargeableObject activo
            ChargeableObject chargeable = lastInteractable.GetComponent<ChargeableObject>();
            if (chargeable != null && chargeable.isActive)
            {
                Debug.Log($"Indicador ocultado: {lastInteractable.name} es un ChargeableObject con IsActive == true.");
                globalIndicator.Hide();
                return;
            }

            // Verificar si es un BatteryCharger de un solo uso y está deshabilitado
            BatteryCharger charger = lastInteractable.GetComponent<BatteryCharger>();
            if (charger != null && !charger.IsReusable && charger.IsDisabled)
            {
                Debug.Log($"Indicador ocultado: {lastInteractable.name} es un BatteryCharger de un solo uso con IsDisabled == true.");
                globalIndicator.Hide();
                return;
            }

            // Verificar si es un PlatformPedestal con batería
            PlatformPedestal pedestal = lastInteractable.GetComponent<PlatformPedestal>();
            if (pedestal != null && pedestal.HasBattery)
            {
                Debug.Log($"Indicador ocultado: {lastInteractable.name} es un PlatformPedestal con HasBattery == true.");
                globalIndicator.Hide();
                return;
            }

            // Mostrar indicador para todos los demás casos
            Debug.Log($"Indicador mostrado para: {lastInteractable.name} en posición {lastInteractable.transform.position}");
            globalIndicator.Show(lastInteractable.transform.position);
        }
        else
        {
            if (globalIndicator != null)
            {
                Debug.Log("Indicador ocultado: No hay objeto interactuable.");
                globalIndicator.Hide();
            }
        }
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
        battery.transform.position = positionBattery.transform.position;

        Rigidbody2D rb = battery.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 1f;

        isSeparated = true;
        isLifeProgressPaused = false;
        StateMachine.ChangeState(SeparatedState);
        InputHadler.UseSeparateInput();

        floatTimer = 0f;
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
            battery.transform.position = Vector2.Lerp(startPos, positionBattery.transform.position, elapsedTime / moveDuration);
            yield return null;
        }

        battery.transform.position = positionBattery.transform.position;
        battery.transform.rotation = Quaternion.identity;
        if (collider != null) collider.enabled = true;

        isSeparated = false;
        isLifeProgressPaused = true;
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

        if (InteractionCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(InteractionCheck.position, playerData.interactionRadius);
        }


        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, playerData.groundCheckRadius);
    }
    #endregion

    #region Upgrade Functions
    public void AddCrystal(int crystalValue)
    {
        collectedCrystals += crystalValue;
        Debug.Log($"Cristales recolectados: {collectedCrystals}/{crystalsPerUpgrade}");

        if (playerUI != null)
        {
            playerUI.UpdateCrystalUI(collectedCrystals % crystalsPerUpgrade, collectedCrystals);
        }
        else
        {
            Debug.LogWarning("PlayerUI es null. No se puede actualizar la UI de cristales.");
        }

        if (collectedCrystals >= crystalsPerUpgrade)
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
        if (collectedCrystals < crystalsPerUpgrade)
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

        collectedCrystals -= crystalsPerUpgrade;
        isUpgradeSelectionActive = false;

        if (collectedCrystals >= crystalsPerUpgrade)
        {
            if (upgradeSelectionUI != null)
            {
                isUpgradeSelectionActive = true;
                upgradeSelectionUI.ShowUpgradeSelection();
            }
        }

        if (upgradeEffect != null) upgradeEffect.Play();
        if (upgradeSound != null) upgradeSound.Play();

        ShowUpgradeNotification(upgrade);
        UpdateCrystalUI();
    }

    public void CancelUpgradeSelection()
    {
        isUpgradeSelectionActive = false;

        if (collectedCrystals >= crystalsPerUpgrade)
        {
            if (upgradeSelectionUI != null)
            {
                isUpgradeSelectionActive = true;
                upgradeSelectionUI.ShowUpgradeSelection();
            }
        }

        UpdateCrystalUI();
    }

    public int GetAvailableUpgrades()
    {
        return collectedCrystals / crystalsPerUpgrade;
    }

    public int GetDisplayedCrystals()
    {
        return collectedCrystals % crystalsPerUpgrade;
    }

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
}