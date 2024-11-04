using UnityEngine;

public class Mushroom : MonoBehaviour
{
    [SerializeField] private Vector2 force = new(0, 30f);
    [SerializeField] private bool factorMomentum = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Player player))
            HitPlayer(player);
    }

    private void HitPlayer(Player player)
    {
        if (player.Controller.BoxCollider.bounds.min.y < transform.position.y)
            return;

        if (factorMomentum)
        {
            Vector2 augmentableVector = player.Velocity;

            if (augmentableVector.Equals(Vector2.zero))
            {
                // Apply rotation to the force vector as if it was augmentableVector
                Vector2 adjustedForce = RotateVector(force, transform.eulerAngles.z);
                player.Bounce(adjustedForce);
                return;
            }

            augmentableVector = RotateVector(augmentableVector, -transform.eulerAngles.z);
            augmentableVector = augmentableVector.magnitude < force.magnitude ? augmentableVector * (force.magnitude / augmentableVector.magnitude) : augmentableVector;
            augmentableVector = new Vector2(augmentableVector.x, Mathf.Abs(augmentableVector.y));
            augmentableVector = RotateVector(augmentableVector, transform.eulerAngles.z);
            player.Bounce(augmentableVector);
        }

        else
        {
            Vector2 adjustedForce = RotateVector(force, transform.eulerAngles.z);
            player.Bounce(adjustedForce);
        }
    }

    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;

        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);

        return v;
    }

    public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
    }
}