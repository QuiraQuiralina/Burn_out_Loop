using UnityEngine;

public class Quest1CleaningState : QuestBase
{
    [Header("Quest Settings")]
    [Tooltip("Number of rooms required to be fully clean to pass this quest.")]
    public int requiredCleanRooms = 2;

    private int cleanRoomsCount = 0;

    public override void BeginQuest()
    {
        base.BeginQuest();
        cleanRoomsCount = 0;

        // Subscribe to all RoomClutterManagers
        foreach (var manager in RoomClutterManager.AllManagers)
        {
            manager.OnRoomCleaned += OnRoomCleanedHandler;
            if (manager.IsCleaned)
            {
                cleanRoomsCount++;
            }
        }

        CheckCompletion();
    }

    public override void EndQuest()
    {
        base.EndQuest();

        // Unsubscribe
        foreach (var manager in RoomClutterManager.AllManagers)
        {
            if (manager != null)
            {
                manager.OnRoomCleaned -= OnRoomCleanedHandler;
            }
        }
    }

    private void OnRoomCleanedHandler()
    {
        cleanRoomsCount = 0;
        foreach (var manager in RoomClutterManager.AllManagers)
        {
            if (manager.IsCleaned)
            {
                cleanRoomsCount++;
            }
        }

        Debug.Log($"[Quest1CleaningState] Room cleaned. Total clean: {cleanRoomsCount}/{requiredCleanRooms}");
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        if (cleanRoomsCount >= requiredCleanRooms && !IsCompleted)
        {
            IsCompleted = true;
            Debug.Log($"[QuestManager] Quest 1 complete ({cleanRoomsCount} rooms clean). Transitioning to next quest.");
            if (GameManager.Instance != null && GameManager.Instance.questStateController != null)
            {
                GameManager.Instance.questStateController.TransitionToNextQuest();
            }
        }
    }
}
