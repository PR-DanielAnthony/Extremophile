using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Controller2D), typeof(Animator))]
public class Player : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Projectile projectile;
    [SerializeField] private AudioClip jumpAudio;
    [SerializeField] private AudioClip bounceAudio;
    [SerializeField] private AudioClip shootAudio;
    [SerializeField] private AudioClip deathAudio;

    private Controller2D controller;
    private Animator animator;
    private AudioSource audioSource;

    private Vector2 input;
    private Vector2 velocity;
    private Vector2 originalBoxOffset;
    private Vector2 originalBoxSize;

    private bool isJumping;
    private bool jumpButtonReleased;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    // life
    private bool isAlive = true;

    public Controller2D Controller => controller;
    public Animator Animator => animator;
    public Vector2 Velocity => velocity;

    private void Awake()
    {
        controller = GetComponent<Controller2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        originalBoxSize = controller.BoxCollider.bounds.size;
        originalBoxOffset = controller.BoxCollider.offset;
    }

    private void Update()
    {
        if (!isAlive)
            return;

        GetComponent<SpriteRenderer>().flipX = controller.FaceDirection != 1;
        input = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (controller.CollisionData.down)
        {
            if (velocity.y == 0)
                isJumping = false;

            coyoteTimeCounter = stats.CoyoteTime;
        }

        else
            coyoteTimeCounter -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = stats.JumpBufferTime;

        else
            jumpBufferCounter -= Time.deltaTime;


        if (coyoteTimeCounter > 0 && jumpBufferCounter > 0 && !isJumping)
            OnJumpInputDown();

        if (!Input.GetKey(KeyCode.Space) && isJumping && !jumpButtonReleased)
            jumpButtonReleased = true;

        if (Input.GetKeyDown(KeyCode.X))
        {
            var proj = ObjectPool.GetObject(projectile.gameObject);
            proj.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
            proj.GetComponent<Projectile>().Initialize();
            audioSource.clip = shootAudio;
            audioSource.Play();
        }

        animator.SetBool("Moving", Mathf.Abs(velocity.x) >= 8f);
    }

    private void FixedUpdate()
    {
        if (!isAlive)
            return;

        UpdateVelocity();
        controller.Move(velocity * Time.fixedDeltaTime, input);

        // if character touches ground or ceiling
        if (controller.CollisionData.up || controller.CollisionData.down)
        {
            animator.SetBool("Jumping", false);
            velocity.y = 0;
        }

        // if character hits a wall (this is so the player doesn't continue moving even after jumping into a wall)
        if (controller.CollisionData.left || controller.CollisionData.right)
            velocity.x = 0;
    }

    #region Jump
    public void OnJumpInputDown()
    {
        isJumping = true;
        jumpButtonReleased = false;
        velocity.y = stats.JumpForce;
        jumpBufferCounter = 0;
        audioSource.clip = jumpAudio;
        audioSource.Play();
        animator.SetBool("Jumping", true);
        animator.SetTrigger("Jump");
    }

    public void Bounce(Vector2 bounce)
    {
        if (!isAlive)
            return;

        isJumping = false;
        jumpButtonReleased = false;
        velocity = bounce;
        audioSource.clip = bounceAudio;
        audioSource.Play();
        jumpBufferCounter = 0;
    }
    #endregion

    #region Velocity
    private void UpdateVelocity()
    {
        HorizontalVelocity();
        VerticalVelocity();
    }

    private void HorizontalVelocity()
    {
        float targetVelocity = input.x * stats.MaxSpeed;

        float acceleration = controller.CollisionData.down ? stats.GroundAcceleration : stats.AirAcceleration;
        float deceleration = controller.CollisionData.down ? stats.GroundDeceleration : stats.AirDeceleration;
        float deltaSpeed = input.x != 0 ? acceleration : deceleration;

        velocity.x = Mathf.MoveTowards(velocity.x, targetVelocity, deltaSpeed * Time.fixedDeltaTime);
    }

    private void VerticalVelocity()
    {
        float inAirGravity = stats.FallAcceleration;

        if (jumpButtonReleased && velocity.y > 0)
            inAirGravity *= stats.JumpEndEarlyGravityModifier;

        if (Mathf.Abs(velocity.y) < stats.JumpPeakThreshold)
            inAirGravity /= stats.JumpPeakTime;

        velocity.y = Mathf.MoveTowards(velocity.y, -stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
    }
    #endregion

    #region Miscellaneous
    private void Sleep(float duration)
    {
        StartCoroutine(PerformSleep(duration));
    }

    private IEnumerator PerformSleep(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }
    #endregion

    public void KillPlayer(ObstacleType obstacleType, Transform newParent, bool newCheckpoint = true)
    {
        if (!isAlive)
            return;

        audioSource.clip = deathAudio;
        audioSource.Play();

        isAlive = false;
        velocity = Vector2.zero;

        transform.parent = newParent;

        if (newCheckpoint)
            GameManager.Instance.ChangeCheckpoint(newParent);

        else
            GameManager.Instance.ChangeCheckpoint(GameManager.Instance.LastMajorCheckpoint);

        switch (obstacleType)
        {
            case ObstacleType.FloorSpike:
                animator.SetTrigger("DieFloor");
                MakePlatform();
                break;
            case ObstacleType.WallSpike:
                animator.SetTrigger("DieWall");

                if (controller.FaceDirection == 1)
                    transform.rotation = Quaternion.Euler(0, 180f, 0);

                else
                    transform.rotation = Quaternion.Euler(0, -180f, 0);

                MakePlatform();
                break;
            case ObstacleType.CeilingSpike:
                animator.SetTrigger("DieCeiling");
                MakePlatform(); // change direction
                break;
            case ObstacleType.Disintigrate:
                animator.SetTrigger("Disintegrate");
                break;
            case ObstacleType.Buzzsaw:
                animator.SetTrigger("DieFloor");
                MakePlatform();
                break;
        }

        GameManager.Instance.Respawn();
        controller.enabled = false;
        enabled = false;
    }

    public void MakePlatform()
    {
        var deadMushrooms = GameManager.Instance.DeadMushrooms;

        while (deadMushrooms.Count >= GameManager.Instance.MaxPlatformCount)
        {
            ObjectPool.ReturnGameObject(deadMushrooms.First());
            deadMushrooms.Remove(deadMushrooms.First());
        }

        if (!GetComponent<Mushroom>())
            gameObject.AddComponent<Mushroom>();

        Controller.BoxCollider.offset = new(0, .5f);
        Controller.BoxCollider.size = new(1, .75f);

        deadMushrooms.Add(gameObject);
    }

    public void Respawn()
    {
        if (isAlive)
            return;

        transform.parent = null;
        velocity = Vector2.zero;
        transform.rotation = Quaternion.identity;
        GetComponent<SpriteRenderer>().flipX = false;
        GetComponent<SpriteRenderer>().flipY = false;
        controller.BoxCollider.size = originalBoxSize;
        controller.BoxCollider.offset = originalBoxOffset;

        if (TryGetComponent(out Mushroom platformComponent))
            Destroy(platformComponent);

        animator.ResetTrigger("DieFloor");
        animator.ResetTrigger("DieWall");
        animator.ResetTrigger("DieCeiling");
        animator.ResetTrigger("Disintegrate");
        isAlive = true;
        controller.enabled = true;
        enabled = true;
    }

    // "Disintegrate" event key
    private void Test()
    {
        ObjectPool.ReturnGameObject(gameObject);
    }
}