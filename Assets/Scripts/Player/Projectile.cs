using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 20f;
    [Range(1f, 10f)][SerializeField] private float margin = 3f;

    private BoxCollider2D boxCollider;
    private Vector2 velocity;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        CheckOffScreen();
    }

    private void FixedUpdate()
    {
        transform.Translate(velocity * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((GameManager.Instance.GroundLayer & (1 << collision.gameObject.layer)) != 0)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                Vector2 normal = contact.normal;

                if (IsWall(normal))
                {
                    if (IsFacingLeft(normal))
                    {
                        Vector3 leftPosition = new(boxCollider.bounds.min.x, boxCollider.bounds.center.y, boxCollider.bounds.center.z);
                        var obj = GameManager.Instance.MakePlatform(leftPosition);
                        obj.transform.Rotate(0f, 180f, 0f);
                        break;
                    }

                    else if (IsFacingRight(normal))
                    {
                        Vector3 rightPosition = new(boxCollider.bounds.max.x, boxCollider.bounds.center.y, boxCollider.bounds.center.z);
                        GameManager.Instance.MakePlatform(rightPosition);
                        break;
                    }
                }
            }

            ObjectPool.ReturnGameObject(gameObject);
        }
    }

    public void Initialize()
    {
        SetVelocity();
    }

    private void SetVelocity()
    {
        // maybe determine by the direction the player's holding (like holding down and left at the same time for example)
        var player = GameManager.Instance.PlayerReference;
        velocity.x = maxSpeed * player.Controller.FaceDirection;
    }

    private void CheckOffScreen()
    {
        Vector2 max = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));
        Vector2 min = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));

        if (transform.position.x > max.x + margin || transform.position.x < min.x - margin || transform.position.y > max.y + margin || transform.position.y < min.y - margin)
            ObjectPool.ReturnGameObject(gameObject);
    }

    private bool IsWall(Vector2 normal)
    {
        return Mathf.Abs(normal.y) < .1f;
    }

    private bool IsFacingLeft(Vector2 normal)
    {
        return Mathf.Approximately(normal.x, -1f);
    }

    private bool IsFacingRight(Vector2 normal)
    {
        return Mathf.Approximately(normal.x, 1f);
    }
}