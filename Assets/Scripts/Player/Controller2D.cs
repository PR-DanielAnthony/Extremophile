using UnityEngine;

public class Controller2D : RaycastController
{
    private CollisionData collisionData;
    private Vector2 input;
    private int faceDirection = 1;

    public CollisionData CollisionData => collisionData;
    public Vector2 Input => input;
    public int FaceDirection { get { return faceDirection; } set { faceDirection = value; } }

    public void Move(Vector2 velocity, Vector2 input)
    {
        UpdateRaycastOrigins();
        collisionData.Reset();
        this.input = input;

        if (velocity.x != 0)
            faceDirection = (int)Mathf.Sign(velocity.x);

        HorizontalCollision(ref velocity);

        if (velocity.y != 0)
            VerticalCollision(ref velocity);

        transform.Translate(velocity);
    }

    #region Collision
    private void HorizontalCollision(ref Vector2 velocity)
    {
        int direction = faceDirection;
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;

        if (Mathf.Abs(velocity.x) < SKIN_WIDTH)
            rayLength = 2 * SKIN_WIDTH;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 origin = direction == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            origin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, rayLength, layerMask);
            Debug.DrawRay(origin, Vector2.right * direction, Color.red);

            if (hit)
            {
                if (hit.distance == 0)
                    continue;

                ///CORNER CHECK///
                //bool hitLoopCheck = false;
                //bool upOrDown = velocity.y > 0; // if 0, it's true/up, otherwise it's false/down
                //bool firstOrLastRay = i == 0 || i == horizontalRayCount - 1;
                //float airDirection = upOrDown ? -1 : 1;

                //if (Mathf.Abs(velocity.x) > 0 && firstOrLastRay)
                //{
                //    for (int j = 1; j < horizontalRayCount; j++)
                //    {
                //        var newOrigin = origin;
                //        newOrigin += (horizontalRaySpacing * i) * airDirection * Vector2.up;
                //        RaycastHit2D newHit = Physics2D.Raycast(newOrigin, Vector2.right * direction, rayLength, layerMask);
                //        Debug.DrawRay(newOrigin, Vector2.right * direction, Color.green, 1.5f);
                //        Debug.Log(upOrDown);

                //        if (newHit)
                //        {
                //            Debug.Log("IDK");
                //            hitLoopCheck = true;
                //            break;
                //        }
                //    }

                //    if (!hitLoopCheck)
                //    {
                //        Debug.Log("IDK2");
                //        float remainingSpace = hit.distance - SKIN_WIDTH;

                //        if (remainingSpace < cornerCorrectionDistance)
                //        {
                //            Debug.Log("IDK3");
                //            float nudgeAmount = .2f;
                //            Debug.Log(upOrDown);
                //            // this needs correction
                //            float k = upOrDown ? nudgeAmount : -nudgeAmount;
                //            velocity.y += k;
                //            break;
                //        }
                //    }
                //}

                ///NORMAL COLLISION STUFF///
                velocity.x = (hit.distance - SKIN_WIDTH) * direction;
                rayLength = hit.distance;

                collisionData.left = direction == -1;
                collisionData.right = direction == 1;
            }
        }
    }

    private void VerticalCollision(ref Vector2 velocity)
    {
        float direction = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 origin = direction == -1 ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            origin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up * direction, rayLength, layerMask);
            Debug.DrawRay(origin, Vector2.up * direction, Color.red);

            if (hit)
            {
                ///CORNER CHECK///
                // consider checking half the rays (from starting point left or right depending on the face direction?)
                // if odd number, use the lower number of half (if 3, use 1, if 5, use 2, etc.)
                bool hitLoopCheck = false;
                float firstOrLastRay = faceDirection == 1 ? 0 : verticalRayCount - 1;

                if (direction == 1 && i == firstOrLastRay)
                {
                    for (int j = 1; j < verticalRayCount; j++)
                    {
                        var newOrigin = origin;
                        newOrigin += (verticalRaySpacing * j + velocity.x) * faceDirection * Vector2.right;
                        RaycastHit2D newHit = Physics2D.Raycast(newOrigin, Vector2.up, rayLength, layerMask);
                        Debug.DrawRay(newOrigin, Vector2.up, Color.green, 1.5f);

                        if (newHit)
                        {
                            hitLoopCheck = true;
                            break;
                        }
                    }

                    if (!hitLoopCheck)
                    {
                        float nudgeAmount = .1f;
                        float k = faceDirection == 1 ? nudgeAmount : -nudgeAmount;
                        velocity.x += k;
                        break;
                    }
                }

                ///NORMAL COLLISION STUFF///
                velocity.y = (hit.distance - SKIN_WIDTH) * direction;
                rayLength = hit.distance;

                collisionData.down = direction == -1;
                collisionData.up = direction == 1;
            }
        }
    }
    #endregion
}

[System.Serializable]
public struct CollisionData
{
    public bool up, down;
    public bool left, right;
    public bool fallingThroughPlatform;

    public void Reset()
    {
        up = down = false;
        left = right = false;
    }
}