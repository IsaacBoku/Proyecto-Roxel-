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
    public PlayerRopeState RopeState { get; private set; }
    public PlayerInteractionState InteractionState { get; private set; }


    [SerializeField]
    private PlayerData playerData;
    #endregion
    #region Components
    public Animator Anim {  get; private set; }
    public PlayerInputHadler InputHadler { get; private set; }
    public Rigidbody2D rb {  get; private set; }
    #endregion
    #region Check Transforms
    [SerializeField]
    private Transform groundCheck;

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
        RopeState = new PlayerRopeState(this, StateMachine, playerData, "Rope");
        InteractionState = new PlayerInteractionState(this, StateMachine, playerData, "Interaction");
    }
    private void Start()
    {
        Anim = GetComponent<Animator>();
        InputHadler = GetComponent<PlayerInputHadler>();
        rb = GetComponent<Rigidbody2D>();

        springJoints = GetComponentInChildren<SpringJoint2D>();
        lineRenderer = GetComponentInChildren<LineRenderer>();

        FacingDirection = 1;

        StateMachine.Intialize(IdleState);
    }
    private void Update()
    {
        Debug.Log(CheckInteraction());
        CurrentVelocity = rb.velocity;
        StateMachine.CurrentState.LogicUpdate();
    }
    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }
    #endregion
    #region Set Functions
    public void SetVelocityX(float velocity)
    {
        workspace.Set(velocity, CurrentVelocity.y);
        rb.velocity = workspace;
        CurrentVelocity = workspace;
    }
    public void SetVelocityY(float velocity)
    {
        workspace.Set(CurrentVelocity.x, velocity);
        rb.velocity = workspace;
        CurrentVelocity = workspace;
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
    public void PushCheck()
    {
        Physics2D.queriesStartInColliders = false;
        RaycastHit2D hit = Physics2D.Raycast(pushCheck.transform.position, Vector2.right * FacingDirection, playerData.distance, playerData.boxMask);

        if (hit.collider != null && hit.collider.gameObject.tag == "pushable")
        {
            box = hit.collider.gameObject;

            box.GetComponent<FixedJoint2D>().enabled = true;
            box.GetComponent<FixedJoint2D>().connectedBody = this.GetComponent<Rigidbody2D>();
            Debug.Log("Agarro");
        }
        else 
        {
            box.GetComponent<FixedJoint2D>().enabled = false;
        }
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
