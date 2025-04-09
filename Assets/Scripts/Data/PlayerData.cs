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

    [Header("Check Ground")]
    public float groundCheckRadius = 0.3f;
    public LayerMask whatIsGround;

    [Header("Check Interaction")]
    public float interactionRadius = 0.5f;
    public LayerMask whatIsInteractable;

    [Header("Check ThrowBattery")]
    public float throwForce = 5f;
    public float throwDistance = 5f;

    [Header("Battery")]
    [SerializeField] public float lightMass = 0.5f;
    [SerializeField] public float normalMass = 1f;

    [Header("Check MetalicObjetcs")]
    [SerializeField] public float magneticForce = 500f;
    [SerializeField] public float magneticRange = 5f;
    [SerializeField] public LayerMask whatIsMetallic;

    [Header("Boost Settings")]
    public float boostCost = 20f; // Costo de energ�a del impulso
    public float boostSpeed = 15f; // Velocidad del impulso
    public float boostDuration = 0.3f; // Duraci�n del impulso (en segundos)
    public float boostCooldown = 1f;

}
