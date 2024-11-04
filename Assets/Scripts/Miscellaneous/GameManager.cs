using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private Transform checkpointOrigin;
    [SerializeField] private LayerMask groundLayer;

    private Player playerReference;
    private Transform currentCheckpoint;
    private Transform lastMajorCheckpoint;

    private bool respawning = true;
    private float currentTimeInLoad = 0;

    private readonly List<GameObject> deadMushrooms = new();
    private readonly float respawnTime = .25f;

    private const int MAX_PLATFORM_COUNT = 5;

    public static GameManager Instance => instance;
    public LayerMask GroundLayer => groundLayer;
    public Player PlayerReference => playerReference;
    public Transform LastMajorCheckpoint => lastMajorCheckpoint;
    public List<GameObject> DeadMushrooms => deadMushrooms;
    public int MaxPlatformCount => MAX_PLATFORM_COUNT;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        currentCheckpoint = checkpointOrigin;
        lastMajorCheckpoint = currentCheckpoint;
    }

    private void FixedUpdate()
    {
        currentTimeInLoad += Time.fixedDeltaTime;

        if (respawning && currentTimeInLoad > respawnTime && currentCheckpoint && playerPrefab)
        {
            playerReference = ObjectPool.GetObject(playerPrefab).GetComponent<Player>();
            FindAnyObjectByType<CameraFollow>().Target = playerReference.Controller;
            playerReference.Respawn();
            playerReference.transform.SetPositionAndRotation(currentCheckpoint.position + (Vector3.up * 2), Quaternion.identity);
            playerReference.Animator.SetBool("Jumping", true);
            playerReference.Animator.SetTrigger("Jump");
            playerReference.Bounce(Vector2.up * 10f);
            respawning = false;
        }
    }

    public void Respawn()
    {
        respawning = true;
        currentTimeInLoad = 0;
    }

    public void ChangeMajorCheckpoint(Transform newCheckpoint)
    {
        lastMajorCheckpoint = newCheckpoint;
    }

    public void ChangeCheckpoint(Transform newCheckpoint)
    {
        currentCheckpoint = newCheckpoint;
    }

    public GameObject MakePlatform(Vector2 position)
    {
        while (deadMushrooms.Count >= MAX_PLATFORM_COUNT)
        {
            ObjectPool.ReturnGameObject(deadMushrooms.First());
            deadMushrooms.Remove(deadMushrooms.First());
        }

        var platform = ObjectPool.GetObject(platformPrefab);

        if (!platform.TryGetComponent(out Mushroom platformComponent))
            platformComponent = platform.AddComponent<Mushroom>();

        platformComponent.SetPositionAndRotation(position, Quaternion.identity);

        deadMushrooms.Add(platform);
        return platform;
    }
}