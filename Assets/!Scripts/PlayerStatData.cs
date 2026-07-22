using AOT;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementData", menuName = "Scriptable Objects/PlayerMovementData")]
public class PlayerStatData : ScriptableObject
{
    [Header("Locomotion-Based")]
    [Min(1f)] public float MaxWalkSpeed;
    [Min(1f)] public float MaxJogSpeed;
    [Min(1f)]public float MaxRunSpeed;
    [Min(1f)]public float AccelarationToWalk;
    [Min(1f)]public float AccelarationToJog;
    [Min(1f)]public float AccelarationToRun;
    [Min(0.1f)]public float DeacclarationForWalk;
    [Min(0.1f)]public float DeacclarationForJog;
    [Min(0.1f)]public float DeacclarationForRun;
    [Min(0.1f)] public float finalVelocityReachingSpeed;
    [Min(1f)]public float bodyTurningSpeed;
    [Min(0f)] public float animationTurnLerpSpeed;

    [Header("Shoot Module Based")]
    public float health;
    public float damageInflict;
    [Min(0.25f)]public float fireRate;
}