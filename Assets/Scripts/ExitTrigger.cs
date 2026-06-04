using UnityEngine;
using UnityEngine.Events;

public class ExitTrigger : MonoBehaviour
{
    [Header("Exit Event")]
    [Tooltip("Invoked when the player walks into this trigger.")]
    public UnityEvent OnExitReached;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        // Verify the object entering is the player
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            Debug.Log("[ExitTrigger] Player reached the exit! Triggering exit event.");

            // Complete Quest 4 state machine
            if (Quest4ShowerState.Instance != null)
            {
                Quest4ShowerState.Instance.CompleteQuest();
            }

            OnExitReached?.Invoke();
        }
    }
}
