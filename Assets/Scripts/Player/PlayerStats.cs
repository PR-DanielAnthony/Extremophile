using UnityEngine;

[CreateAssetMenu(fileName = "Player Stats", menuName = "Assets/Create Player Stats", order = 0)]
public class PlayerStats : ScriptableObject
{
    [Header("Ground Movement")]
    [SerializeField] private float maxSpeed = 14f;
    [SerializeField] private float groundAcceleration = 120f;
    [SerializeField] private float groundDeceleration = 60f;

    [Header("Air Movement")]
    [SerializeField] private float jumpForce = 36f;
    [SerializeField] private float maxFallSpeed = 40f;
    [SerializeField] private float fallAcceleration = 110f;
    [SerializeField] private float airAcceleration = 60f;
    [SerializeField] private float airDeceleration = 30f;

    [Header("Jump Settings")]
    [SerializeField] private float coyoteTime = .15f;
    [SerializeField] private float jumpBufferTime = .2f;

    [SerializeField] private float jumpPeakTime = 3f;
    [SerializeField] private float jumpPeakThreshold = 1f;
    
    [SerializeField] private float jumpEndEarlyGravityModifier = 3f;

    public float MaxSpeed => maxSpeed;
    public float GroundAcceleration => groundAcceleration;
    public float GroundDeceleration => groundDeceleration;

    public float JumpForce => jumpForce;
    public float MaxFallSpeed => maxFallSpeed;
    public float FallAcceleration => fallAcceleration;
    public float AirAcceleration => airAcceleration;
    public float AirDeceleration => airDeceleration;

    public float CoyoteTime => coyoteTime;
    public float JumpBufferTime => jumpBufferTime;

    public float JumpPeakTime => jumpPeakTime;
    public float JumpPeakThreshold => jumpPeakThreshold;

    public float JumpEndEarlyGravityModifier => jumpEndEarlyGravityModifier;
}