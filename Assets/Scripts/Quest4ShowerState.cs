using UnityEngine;

public class Quest4ShowerState : QuestBase
{
    public static Quest4ShowerState Instance { get; private set; }

    [Header("Shower Objects")]
    [Tooltip("The shower trigger zone object to enable on start.")]
    public GameObject showerSystem;

    private void Awake()
    {
        Instance = this;
    }

    public override void BeginQuest()
    {
        base.BeginQuest();

        if (showerSystem != null)
        {
            showerSystem.SetActive(true);
        }
    }

    /// <summary>
    /// Transitions game to completion once player recovers in the pool/bed.
    /// </summary>
    public void CompleteQuest()
    {
        if (!IsCompleted)
        {
            IsCompleted = true;
            Debug.Log("[QuestManager] Quest 4 complete. Roll credits.");
            if (GameManager.Instance != null && GameManager.Instance.questStateController != null)
            {
                GameManager.Instance.questStateController.TransitionToNextQuest();
            }
        }
    }

    public override void EndQuest()
    {
        base.EndQuest();
    }
}
