using UnityEngine;

public class ClutterSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [Tooltip("The prefab to spawn (drag the prefab from your Project window here).")]
    public GameObject trashPrefab; 

    [Tooltip("How far apart the new objects spawn.")]
    public float spawnRadius = 0.5f;

    [Tooltip("Height offset to prevent spawning inside the floor.")]
    public float heightOffset = 0.1f;

    // This function runs automatically when another object enters this object's trigger zone
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that touched us is the Player's hand
        // We will set up the "Player" tag in the next step
        if (other.CompareTag("Player"))
        {
            SpawnNewTrash();
            Destroy(gameObject); // Object disappears
        }
    }

    void SpawnNewTrash()
    {
        for (int i = 0; i < 2; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, heightOffset, randomCircle.y);

            Instantiate(trashPrefab, spawnPos, Random.rotation);
        }
    }
}