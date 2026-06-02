using UnityEngine;
using UnityEngine.Events;

public class BookSpawner : MonoBehaviour
{
    [Header("Book Settings")]
    [Tooltip("The book prefab to spawn (must have Rigidbody and optionally grabbables).")]
    public GameObject bookPrefab;

    [Tooltip("Radial spawn distance from the spawner / player.")]
    public float spawnRadius = 1.0f;

    [Tooltip("Height offset relative to player/spawner position.")]
    public float heightOffset = 0.5f;

    [Header("Events")]
    [Tooltip("Triggered when the initial book is interacted with.")]
    public UnityEvent OnBookInteracted;

    private bool hasSpawned = false;

    private void OnTriggerEnter(Collider other)
    {
        // Fallback collider trigger
        if (other.CompareTag("Player"))
        {
            TriggerSpawn();
        }
    }

    /// <summary>
    /// Call this when the player grabs or touches the initial book.
    /// </summary>
    public void TriggerSpawn()
    {
        if (hasSpawned) return;
        hasSpawned = true;

        Debug.Log("[BookSpawner] Initial book touched! Spawning 10 books radially.");

        OnBookInteracted?.Invoke();

        SpawnRadialBooks();

        // Disable this initial spawner/anchor object
        gameObject.SetActive(false);
    }

    private void SpawnRadialBooks()
    {
        if (bookPrefab == null)
        {
            Debug.LogError("[BookSpawner] bookPrefab is not assigned!");
            return;
        }

        Vector3 centerPos = transform.position;
        if (GameManager.Instance != null && GameManager.Instance.targetHandAnchor != null)
        {
            centerPos = GameManager.Instance.targetHandAnchor.position;
        }

        for (int i = 0; i < 10; i++)
        {
            float angle = i * (360f / 10f) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * spawnRadius, heightOffset, Mathf.Sin(angle) * spawnRadius);
            Vector3 spawnPos = centerPos + offset;

            GameObject book = Instantiate(bookPrefab, spawnPos, Quaternion.Euler(0f, i * (360f / 10f), 0f));
            book.name = $"Book_{i + 1}";
        }
    }
}
