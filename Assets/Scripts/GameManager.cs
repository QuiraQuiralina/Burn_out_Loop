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

    [Header("VR Hand HUD Setup")]
    [Tooltip("The canvas UI panel to mount onto the player hand.")]
    public RectTransform wristHUDCanvas;

    [Tooltip("The target hand transform to anchor/parent the wrist HUD to.")]
    public Transform targetHandAnchor;

    [Tooltip("Local position offset relative to target hand anchor.")]
    public Vector3 localPositionOffset = Vector3.zero;

    [Tooltip("Local rotation offset relative to target hand anchor.")]
    public Vector3 localRotationOffset = Vector3.zero;

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
        InitializeWristHUD();

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

    private void InitializeWristHUD()
    {
        if (wristHUDCanvas != null && targetHandAnchor != null)
        {
            Debug.Log("[GameManager] Initializing Hand-Parented Quest UI Panel.");
            wristHUDCanvas.SetParent(targetHandAnchor, false);
            wristHUDCanvas.localPosition = localPositionOffset;
            wristHUDCanvas.localRotation = Quaternion.Euler(localRotationOffset);
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
