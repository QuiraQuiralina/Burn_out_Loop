using UnityEngine;

public abstract class QuestBase : MonoBehaviour
{
    [Header("Quest Info")]
    [Tooltip("The description of the quest displayed on the player's wrist UI.")]
    public string questDescription;

    public bool IsCompleted { get; protected set; }

    /// <summary>
    /// Called when this quest is activated.
    /// </summary>
    public virtual void BeginQuest()
    {
        IsCompleted = false;
        Debug.Log($"[QuestBase] BeginQuest: {questDescription}");
    }

    /// <summary>
    /// Called in Update while this quest is active.
    /// </summary>
    public virtual void UpdateQuest()
    {
    }

    /// <summary>
    /// Called when the quest is finished.
    /// </summary>
    public virtual void EndQuest()
    {
        Debug.Log($"[QuestBase] EndQuest: {questDescription}");
    }
}
