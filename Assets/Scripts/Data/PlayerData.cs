using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="newPlayerData",menuName = "Data/Player Data/Base Data")]
public class PlayerData : ScriptableObject
{
    [Header("Health State")]
    public int MaxHealth;

    [Header("Damage State")]
    public int damage;

    [Header("Move State")]
    public float movementVeclocity = 10f;

    [Header("Jump State")]
    public float jumpVelocity = 15f;
    public int amountOfJumps = 1;

    [Header("Air State")]
    public float coyoteTime = 0.2f;
    public float variableJumpHeightMultiplier = 0.5f;

    [Header("Push State")]
    public float distance = 1f;
    public LayerMask boxMask;

    [Header("Check Variables")]
    public float groundCheckRadius = 0.3f;
    public LayerMask whatIsGround;

    [Header("Check Interaction")]
    public float interactionRadius = 0.5f;
    public LayerMask whatIsInteractable;

    [Header("Check Magnetic")]
    public float magneticRadius = 0.5f;
    public LayerMask whatIsMagentic;

    [Header("Check ThrowBattery")]
    public float throwForce = 5f;
    public float throwDistance = 5f;

    [SerializeField] public float lightMass = 0.5f;
    [SerializeField] public float normalMass = 1f;
    [SerializeField] public float magneticForce = 10f;
    [SerializeField] public float magneticRange = 5f;
    [SerializeField] public LayerMask whatIsMetallic;


}
