using UnityEngine;

public class Quest2StudyState : QuestBase
{
    [Header("Components")]
    [Tooltip("The book spawner helper to activate when quest starts.")]
    public BookSpawner bookSpawner;

    [Tooltip("The shelf container that tracks snapped books.")]
    public ShelfContainer shelfContainer;

    public override void BeginQuest()
    {
        base.BeginQuest();

        if (bookSpawner != null)
        {
            bookSpawner.gameObject.SetActive(true);
        }

        if (shelfContainer != null)
        {
            shelfContainer.OnShelfFull += OnShelfFullHandler;
        }
        else
        {
            Debug.LogError("[Quest2StudyState] shelfContainer is null!");
        }
    }

    public override void EndQuest()
    {
        base.EndQuest();

        if (shelfContainer != null)
        {
            shelfContainer.OnShelfFull -= OnShelfFullHandler;
        }
    }

    private void OnShelfFullHandler()
    {
        if (!IsCompleted)
        {
            IsCompleted = true;
            Debug.Log("[QuestManager] Quest 2 complete. Transitioning to next quest.");
            if (GameManager.Instance != null && GameManager.Instance.questStateController != null)
            {
                GameManager.Instance.questStateController.TransitionToNextQuest();
            }
        }
    }
}
