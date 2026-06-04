using UnityEngine;
using UnityEngine.SceneManagement;

public class BedRecoveryTrigger : MonoBehaviour
{
    [Header("WebGL Escape Configuration")]
    [Tooltip("The front door GameObject to disable/open on playthrough 3.")]
    public GameObject frontDoorObject;

    [Tooltip("The ExitTrigger GameObject to enable/activate on playthrough 3.")]
    public GameObject exitTriggerObject;

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

        // Check playthrough loop count
        if (GameManager.playthroughCount < 3)
        {
            GameManager.playthroughCount++;
            Debug.Log($"[BedRecovery] Playthrough loop completed. Loading playthrough {GameManager.playthroughCount}/3...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            Debug.Log("[BedRecovery] Final playthrough (3/3) bed recovery. Unlocking exit door!");

            // Open front door
            if (frontDoorObject != null)
            {
                frontDoorObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[BedRecovery] frontDoorObject is not assigned!");
            }

            // Enable exit trigger
            if (exitTriggerObject != null)
            {
                exitTriggerObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[BedRecovery] exitTriggerObject is not assigned!");
            }

            // Update status text
            if (GameManager.Instance != null && GameManager.Instance.questStatusText != null)
            {
                GameManager.Instance.questStatusText.text = "Escape through the front door!";
            }
        }
    }
}
