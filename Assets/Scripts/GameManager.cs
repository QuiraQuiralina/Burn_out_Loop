using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Playthrough Config")]
    [Tooltip("The current playthrough loop count (1 to 3).")]
    public static int playthroughCount = 1;

    [Header("State Controller")]
    public QuestStateController questStateController;

    [Header("HUD UI Elements")]
    [Tooltip("Text element displaying current quest status.")]
    public TextMeshProUGUI questStatusText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Debug.Log($"[GameManager] Starting Playthrough: {playthroughCount}/3");

        if (questStateController != null)
        {
            questStateController.OnQuestTransitioned += UpdateHUD;
            questStateController.OnAllQuestsCompleted += OnGameCompleted;
            questStateController.StartQuests();
        }
        else
        {
            Debug.LogError("[GameManager] QuestStateController reference is missing!");
        }
    }



    private void UpdateHUD(QuestBase currentQuest)
    {
        if (questStatusText != null && currentQuest != null)
        {
            questStatusText.text = currentQuest.questDescription;
            Debug.Log($"[QuestManager] Active Quest: {currentQuest.questDescription}");
        }
    }

    private void OnGameCompleted()
    {
        if (questStatusText != null)
        {
            questStatusText.text = "Experience Complete";
        }
    }

    private void OnDestroy()
    {
        if (questStateController != null)
        {
            questStateController.OnQuestTransitioned -= UpdateHUD;
            questStateController.OnAllQuestsCompleted -= OnGameCompleted;
        }
    }
}
