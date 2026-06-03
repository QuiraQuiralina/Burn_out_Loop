using UnityEngine;
using UnityEngine.SceneManagement;

public class BedRecoveryTrigger : MonoBehaviour
{
    [Header("Credits Settings")]
    [Tooltip("The credits canvas in World Space to activate when player reaches the bed.")]
    public GameObject creditsCanvas;

    private bool isRecovered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isRecovered) return;

        if (other.CompareTag("Player"))
        {
            RecoverPlayer();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isRecovered) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            RecoverPlayer();
        }
    }

    private void RecoverPlayer()
    {
        if (isRecovered) return;
        isRecovered = true;

        Debug.Log("[BedRecovery] Bed collision detected! Player safe.");

        // Stop timer
        if (ShowerTrigger.Instance != null)
        {
            ShowerTrigger.Instance.StopTimer();
        }

        // Hide wrist HUD if assigned
        if (GameManager.Instance != null && GameManager.Instance.wristHUDCanvas != null)
        {
            GameManager.Instance.wristHUDCanvas.gameObject.SetActive(false);
        }

        // Check playthrough loop count
        if (GameManager.playthroughCount < 3)
        {
            GameManager.playthroughCount++;
            Debug.Log($"[BedRecovery] Playthrough loop completed. Loading playthrough {GameManager.playthroughCount}/3...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            Debug.Log("[BedRecovery] Final playthrough (3/3) completed. Rolling credits!");
            
            // Reset playthrough count for future runs
            GameManager.playthroughCount = 1;

            // Show Credits Canvas
            if (creditsCanvas != null)
            {
                creditsCanvas.SetActive(true);
            }
            else
            {
                Debug.LogError("[BedRecovery] creditsCanvas is not assigned!");
            }

            // Complete Quest 4
            if (Quest4ShowerState.Instance != null)
            {
                Quest4ShowerState.Instance.CompleteQuest();
            }
        }
    }
}
