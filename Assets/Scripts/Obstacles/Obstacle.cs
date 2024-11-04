using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private ObstacleType obstacleType;

    private BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Player player))
            HitPlayer(collision, player);
    }

    private void HitPlayer(Collision2D collision, Player player)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 normal = contact.normal;

            switch (obstacleType)
            {
                case ObstacleType.FloorSpike:
                    if (IsFloor(normal) && IsFacingUp(normal) && player.Controller.BoxCollider.bounds.min.y > transform.position.y)
                    {
                        KillAndRepositionPlayer(player);
                        return;
                    }
                    break;

                case ObstacleType.WallSpike:
                    if (IsWall(normal) && (IsFacingLeft(normal) || IsFacingRight(normal)))
                    {
                        KillAndRepositionPlayer(player);
                        return;
                    }
                    break;

                case ObstacleType.CeilingSpike:
                    if (IsFacingDown(normal))
                    {
                        FlipAndKillPlayer(player);
                        return;
                    }
                    break;

                case ObstacleType.Buzzsaw:
                    RotatePlayerToCollisionPoint(collision, player);
                    player.KillPlayer(obstacleType, transform, false);
                    return;

                case ObstacleType.Disintigrate:
                    player.KillPlayer(obstacleType, transform, false);
                    return;
            }
        }
    }

    #region Hit Player Helpers
    private void KillAndRepositionPlayer(Player player)
    {
        player.transform.position = transform.position;
        player.KillPlayer(obstacleType, transform);
    }

    private void FlipAndKillPlayer(Player player)
    {
        player.transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, 180));
        player.GetComponent<SpriteRenderer>().flipY = true;
        player.KillPlayer(obstacleType, transform);
    }

    private void RotatePlayerToCollisionPoint(Collision2D collision, Player player)
    {
        Vector2 collisionPoint = collision.GetContact(0).point;
        Vector2 direction = collisionPoint - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        player.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    private void OnTransformChildrenChanged()
    {
        if (boxCollider)
            boxCollider.enabled = HasActiveChild();
    }

    private bool HasActiveChild()
    {
        foreach (Transform child in transform)
            if (child.gameObject.activeSelf)
                return false;

        return true;
    }
    #endregion

    #region Collision Checkers
    private bool IsWall(Vector2 normal)
    {
        return Mathf.Abs(normal.y) < .1f;
    }

    private bool IsFloor(Vector2 normal)
    {
        return Mathf.Abs(normal.x) < .1f;
    }

    private bool IsFacingUp(Vector2 normal)
    {
        return Mathf.Approximately(normal.y, -1f);
    }

    private bool IsFacingDown(Vector2 normal)
    {
        return Mathf.Approximately(normal.y, 1f);
    }

    private bool IsFacingLeft(Vector2 normal)
    {
        return Mathf.Approximately(normal.x, -1f);
    }

    private bool IsFacingRight(Vector2 normal)
    {
        return Mathf.Approximately(normal.x, 1f);
    }
    #endregion
}

public enum ObstacleType
{
    FloorSpike,
    WallSpike,
    CeilingSpike,
    Disintigrate,
    Buzzsaw
}