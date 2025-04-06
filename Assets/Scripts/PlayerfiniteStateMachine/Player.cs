using System.Collections;
using System.Collections.Generic;
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
    private bool isSeparated = false;
    public float maxTimeWithoutBattery = 10f;
    private float currentTime;

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
    [SerializeField]
    public SpringJoint2D springJoints;
    [SerializeField]
    public LineRenderer lineRenderer;
    
    public GameObject box;
    #endregion
    #region Other Variables

    public Vector2 CurrentVelocity { get; private set; }
    public int FacingDirection { get; private set; }
    public bool isOnConveyorBelt { get; set; } = false;
    private float conveyorDirection = 0f;

    public float originalSpeed;
    private float originalJumpVelocity;
    private Vector2 workspace;
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
    }
    private void Start()
    {
        Anim = GetComponent<Animator>();
        InputHadler = GetComponent<PlayerInputHadler>();
        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<PlayerHealthSystem>();

        playerData.movementVeclocity = 8;

        originalSpeed = playerData.movementVeclocity;
        originalJumpVelocity = playerData.jumpVelocity;

        springJoints = GetComponentInChildren<SpringJoint2D>();
        lineRenderer = GetComponentInChildren<LineRenderer>();

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
        else if (InputHadler.MagneticInputStop)
        {
            StateMachine.ChangeState(IdleState);
            InputHadler.UseMagneticInput();
        }
        if (isSeparated)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                ReuniteBattery();
                StateMachine.ChangeState(DeadState); // O IdleState
            }
        }
        else
        {
            currentTime = maxTimeWithoutBattery;
        }
    }
    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();

    }
    #endregion
    #region Set Functions
    public void SetVelocityX(float velocity)
    {
        float finalVelocity = velocity;
        
        if (isOnConveyorBelt)
        {
            // Si el jugador se mueve en contra de la cinta, reducimos su velocidad
            if (Mathf.Sign(velocity) != Mathf.Sign(conveyorDirection))
            {
                finalVelocity *= 0.5f; // Reducimos la velocidad al 50%
            }
            // Si el jugador se mueve en la misma direcci�n que la cinta, puede mantener o aumentar la velocidad
            else
            {
                finalVelocity *= 1.2f; // Aumentamos un poco la velocidad si va en la misma direcci�n
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
            Debug.Log("Objeto detectado: " + detectedObject.name);
            return true;
        }

        Debug.Log("No se detect� objeto interactivo");
        return false;
    }
    private void SeparateBattery()
    {
        battery.transform.parent = null;
        battery.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        isSeparated = true;
        StateMachine.ChangeState(SeparatedState);
        InputHadler.UseSeparateInput();
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
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(pushCheck.transform.position, (Vector2)pushCheck.transform.position + Vector2.right * FacingDirection * playerData.distance);
    }
    #endregion
}
