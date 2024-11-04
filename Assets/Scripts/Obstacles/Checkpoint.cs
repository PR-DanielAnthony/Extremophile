using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Player _))
        {
            GameManager.Instance.ChangeCheckpoint(transform);
            GameManager.Instance.ChangeMajorCheckpoint(transform);
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}