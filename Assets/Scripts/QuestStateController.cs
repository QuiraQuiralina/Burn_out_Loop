using System;
using UnityEngine;

public class QuestStateController : MonoBehaviour
{
    [Header("Quests Config")]
    [Tooltip("Chronological list of all quests in the game.")]
    public QuestBase[] quests;

    private int currentQuestIndex = -1;

    public QuestBase CurrentQuest => (currentQuestIndex >= 0 && currentQuestIndex < quests.Length) ? quests[currentQuestIndex] : null;

    public event Action<QuestBase> OnQuestTransitioned;
    public event Action OnAllQuestsCompleted;

    /// <summary>
    /// Starts the quest loop from the beginning.
    /// </summary>
    public void StartQuests()
    {
        if (quests == null || quests.Length == 0)
        {
            quests = GetComponents<QuestBase>();
            Debug.Log($"[QuestStateController] Automatically populated quests from components. Count: {quests.Length}");
        }

        currentQuestIndex = -1;
        TransitionToNextQuest();
    }

    private void Update()
    {
        if (CurrentQuest != null && !CurrentQuest.IsCompleted)
        {
            CurrentQuest.UpdateQuest();
        }
    }

    /// <summary>
    /// Transitions to the next quest in the array.
    /// </summary>
    public void TransitionToNextQuest()
    {
        if (CurrentQuest != null)
        {
            CurrentQuest.EndQuest();
        }

        currentQuestIndex++;

        if (currentQuestIndex < quests.Length)
        {
            QuestBase nextQuest = quests[currentQuestIndex];
            Debug.Log($"[QuestStateController] Transitioning to quest: {nextQuest.questDescription}");
            nextQuest.BeginQuest();
            OnQuestTransitioned?.Invoke(nextQuest);
        }
        else
        {
            Debug.Log("[QuestStateController] All quests completed!");
            OnAllQuestsCompleted?.Invoke();
        }
    }
}
