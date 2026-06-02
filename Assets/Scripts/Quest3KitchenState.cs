using UnityEngine;

public class Quest3KitchenState : QuestBase
{
    [Header("Cooking Objects")]
    [Tooltip("The cooking interactive system to activate (optional).")]
    public GameObject kitchenCookingSystem;

    public override void BeginQuest()
    {
        base.BeginQuest();

        if (kitchenCookingSystem != null)
        {
            kitchenCookingSystem.SetActive(true);
        }

        if (HungerBarManager.Instance != null)
        {
            HungerBarManager.Instance.ResetHunger();
            HungerBarManager.Instance.SetPanelActive(true);
        }
        else
        {
            Debug.LogError("[Quest3KitchenState] HungerBarManager is missing!");
        }
    }

    public override void UpdateQuest()
    {
        base.UpdateQuest();

        if (HungerBarManager.Instance != null && HungerBarManager.Instance.GetHunger() >= 100f && !IsCompleted)
        {
            IsCompleted = true;
            Debug.Log("[QuestManager] Quest 3 complete (hunger at 100%). Transitioning to next quest.");
            
            if (HungerBarManager.Instance != null)
            {
                HungerBarManager.Instance.SetPanelActive(false);
            }

            if (GameManager.Instance != null && GameManager.Instance.questStateController != null)
            {
                GameManager.Instance.questStateController.TransitionToNextQuest();
            }
        }
    }

    public override void EndQuest()
    {
        base.EndQuest();

        if (kitchenCookingSystem != null)
        {
            kitchenCookingSystem.SetActive(false);
        }

        if (HungerBarManager.Instance != null)
        {
            HungerBarManager.Instance.SetPanelActive(false);
        }
    }
}
