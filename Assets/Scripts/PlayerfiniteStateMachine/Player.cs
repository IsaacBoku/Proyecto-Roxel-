using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
    public PlayerMagneticState MagneticState {  get; private set; }
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
    public Animator Anim {  get; private set; }
    public PlayerInputHadler InputHadler { get; private set; }
    public Rigidbody2D rb {  get; private set; }

    #endregion
    #region Check Transforms
    [SerializeField]
    private Transform groundCheck;

    [Header("Battery")]
    public GameObject battery;
    public bool isSeparated = false;
    public float maxTimeWithoutBattery = 10f;
    private float currentTime;
    private bool isTimerPaused;
    private float lastBoostTime;

    [Header("Boost Effects")]
    [SerializeField] private ParticleSystem boostEffect;

    [SerializeField]
    public Transform pushCheck;

    [SerializeField]
    public Transform InteractionCheck;

    [SerializeField]
    public Transform ObjectPosition;
    [SerializeField]
    public GameObject ObjectInteraction;


    [SerializeField]
    public Transform playerCheck;
    

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

        IdleState = new PlayerIdleState(this,StateMachine,playerData,"Idle");
        MoveState = new PlayerMoveState(this, StateMachine, playerData,"Move");
        JumpState = new PlayerJumpState(this, StateMachine, playerData,"Air");
        AirState = new PlayerAirState(this, StateMachine, playerData,"Air");
        LandState = new PlayerLandState(this, StateMachine, playerData,"Land");
        PushState = new PlayerPushState(this, StateMachine, playerData, "Push");
        InteractionState = new PlayerInteractionState(this, StateMachine, playerData, "Interaction");
        MagneticState = new PlayerMagneticState(this, StateMachine, playerData, "Magnetic");
        DeadState = new PlayerDeadState(this, StateMachine, playerData, "Dead");
        SeparatedState = new PlayerSeparatedState(this, StateMachine, playerData, "Separated");
        ThrowState = new PlayerThrowState(this, StateMachine, playerData, "Throw");
        AimBatteryState = new PlayerAimBatteryState(this, StateMachine, playerData, "Aim");
        BoostState = new PlayerBoostState(this, StateMachine, playerData, "Boost");

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
        currentTime = maxTimeWithoutBattery;

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
        else if (InputHadler.SeparateInput && isSeparated)
        {
            ReuniteBattery();
        }

        if (InputHadler.InteractInput && CheckInteraction())
        {
            StateMachine.ChangeState(InteractionState);
        }

        if (InputHadler.MagneticInput) // Cambia al estado magnético
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
        if (InputHadler.BoostInput && !isSeparated && Time.time >= lastBoostTime + playerData.boostCooldown)
        {
            StateMachine.ChangeState(BoostState);
            lastBoostTime = Time.time;
            if (boostEffect != null) boostEffect.Play();
        }
        if (isSeparated && !isTimerPaused)
        {
            currentTime += Time.deltaTime;
            Debug.Log($"Tiempo sin batería: {currentTime}/{maxTimeWithoutBattery}");
            if (currentTime >= maxTimeWithoutBattery)
            {
                StateMachine.ChangeState(DeadState);
            }
        }
    }
    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();

    }
    #endregion
    #region Set Functions
    public void ResetTimer()
    {
        currentTime = 0f;
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
        return  Physics2D.Raycast(transform.position, Vector2.right * transform.transform.localScale.x, playerData.distance, playerData.boxMask);
    }
    public void CheckIfShouldFlip(int xInput)
    {
        if(xInput !=0 && xInput != FacingDirection)
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
        battery.transform.parent = null;
        battery.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        isSeparated = true;
        StateMachine.ChangeState(SeparatedState);
        InputHadler.UseSeparateInput();
        ResetTimer();
    }
    private void ReuniteBattery()
    {
        battery.transform.parent = transform;
        battery.transform.localPosition = Vector2.zero; // Ajusta según tu diseño
        battery.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        isSeparated = false;
        StateMachine.ChangeState(IdleState);
        InputHadler.UseSeparateInput();
    }

    #endregion
    #region Other Functions

    private void AnimationTrigger()=> StateMachine.CurrentState.AnimationTrigger();
    private void AnimationFinishTrigger()=> StateMachine.CurrentState.AnimationFinishTrigger();
    private void Flip()
    {
        FacingDirection *= -1;
        transform.Rotate(0.0f,180.0f,0.0f);
        pushCheck.Rotate(0.0f, 180.0f, 0.0f);
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
        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(pushCheck.transform.position, (Vector2)pushCheck.transform.position + Vector2.right * FacingDirection * playerData.distance);
    }
    #endregion
}
