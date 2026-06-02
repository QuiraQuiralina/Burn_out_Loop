using UnityEngine;
using UnityEngine.Events;

public class ClutterSpawner : MonoBehaviour
{
    [Header("Room Settings")]
    [Tooltip("The local RoomClutterManager for the room this spawner belongs to. If null, it will search in parent.")]
    public RoomClutterManager localClutterManager;

    [Header("Spawning Settings")]
    [Tooltip("The prefab to spawn (drag the prefab from your Project window here).")]
    public GameObject trashPrefab; 

    [Tooltip("How far apart the new objects spawn from spawn points.")]
    public float spawnRadius = 0.5f;

    [Tooltip("Height offset to prevent spawning inside the floor.")]
    public float heightOffset = 0.1f;

    [Header("Interaction Events")]
    [Tooltip("Unity Event that can be triggered by Meta Interaction SDK or other scripts to collect this clutter.")]
    public UnityEvent OnInteractedWith;

    private bool isCollected = false;

    private void Start()
    {
        if (localClutterManager == null)
        {
            localClutterManager = GetComponentInParent<RoomClutterManager>();
        }

        if (localClutterManager != null)
        {
            localClutterManager.RegisterClutter(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that touched us is the Player's hand
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    /// <summary>
    /// Collects the clutter item, unregisters it, spawns 10 items in other rooms, and destroys itself.
    /// </summary>
    public void Collect()
    {
        if (isCollected) return;
        isCollected = true;

        Debug.Log($"[ClutterSpawner] Clutter collected: {gameObject.name}");

        // Unregister from local room
        if (localClutterManager != null)
        {
            localClutterManager.UnregisterClutter(this);
        }

        // Trigger events
        OnInteractedWith?.Invoke();

        // Spawn 10 new clutter instances randomly in other rooms
        SpawnClutterInOtherRooms();

        // Disappear
        Destroy(gameObject);
    }

    private void SpawnClutterInOtherRooms()
    {
        if (trashPrefab == null)
        {
            Debug.LogWarning("[ClutterSpawner] trashPrefab is not assigned. Cannot spawn more clutter.");
            return;
        }

        for (int i = 0; i < 10; i++)
        {
            RoomClutterManager otherRoom = RoomClutterManager.GetRandomOtherManager(localClutterManager);
            if (otherRoom != null)
            {
                Transform spawnPt = otherRoom.GetRandomSpawnPoint();
                Vector3 spawnPos = transform.position; // Fallback

                if (spawnPt != null)
                {
                    Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                    spawnPos = spawnPt.position + new Vector3(randomCircle.x, heightOffset, randomCircle.y);
                }
                else
                {
                    // Skip if no spawn points found in target room
                    continue;
                }

                GameObject newTrash = Instantiate(trashPrefab, spawnPos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                ClutterSpawner newSpawner = newTrash.GetComponent<ClutterSpawner>();
                if (newSpawner != null)
                {
                    newSpawner.localClutterManager = otherRoom;
                }
            }
        }
    }
}