using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    [SerializeField] protected LayerMask layerMask;
    [SerializeField] private float horizontalRayDistance = .25f;
    [SerializeField] private float verticalRayDistance = .25f;

    protected BoxCollider2D bc;
    protected RaycastOrigins raycastOrigins;

    protected int horizontalRayCount;
    protected int verticalRayCount;

    protected float horizontalRaySpacing;
    protected float verticalRaySpacing;

    protected const float SKIN_WIDTH = .015f;

    public BoxCollider2D BoxCollider => bc;

    private void Awake()
    {
        bc = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        CalculateRaySpacing();
    }

    protected void CalculateRaySpacing()
    {
        Bounds bounds = bc.bounds;
        bounds.Expand(SKIN_WIDTH * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        horizontalRayCount = Mathf.RoundToInt(boundsHeight / horizontalRayDistance);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / verticalRayDistance);

        horizontalRaySpacing = boundsHeight / (horizontalRayCount - 1);
        verticalRaySpacing = boundsWidth / (verticalRayCount - 1);
    }

    protected void UpdateRaycastOrigins()
    {
        Bounds bounds = bc.bounds;
        bounds.Expand(SKIN_WIDTH * -2);

        raycastOrigins.topLeft = new(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new(bounds.max.x, bounds.max.y);

        raycastOrigins.bottomLeft = new(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new(bounds.max.x, bounds.min.y);
    }
}

public struct RaycastOrigins
{
    public Vector2 topLeft, topRight;
    public Vector2 bottomLeft, bottomRight;
}