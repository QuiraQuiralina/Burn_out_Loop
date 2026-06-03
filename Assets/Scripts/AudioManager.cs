using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct QuestAudioClips
{
    [Header("Quest 1 (Cleaning)")]
    [Tooltip("Clip played when cleaning quest starts.")]
    public AudioClip q1Entry;
    [Tooltip("Clip played when cleaning quest ends.")]
    public AudioClip q1End;

    [Header("Quest 2 (Study)")]
    [Tooltip("Clip played when study quest starts.")]
    public AudioClip q2Entry;
    [Tooltip("Clip played when study quest ends.")]
    public AudioClip q2End;

    [Header("Quest 4 (Shower)")]
    [Tooltip("Clip played when shower quest starts.")]
    public AudioClip q4Entry;
    [Tooltip("Clip played when shower quest ends.")]
    public AudioClip q4End;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Setup")]
    [Tooltip("The AudioSource used to play the clips.")]
    public AudioSource audioSource;

    [Header("Playthrough 1 Audio")]
    public QuestAudioClips playthrough1Clips;

    [Header("Playthrough 2 Audio")]
    public QuestAudioClips playthrough2Clips;

    [Header("Playthrough 3 Audio")]
    public QuestAudioClips playthrough3Clips;

    private void Awake()
    {
        // Singleton pattern: ensure only one manager persists across scene loads
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        SubscribeToEvents();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SubscribeToEvents();
    }

    /// <summary>
    /// Finds the QuestStateController in the new scene and subscribes to its start/end events.
    /// </summary>
    private void SubscribeToEvents()
    {
        QuestStateController controller = FindAnyObjectByType<QuestStateController>();
        if (controller != null)
        {
            // Unsubscribe first to avoid double-subscriptions
            controller.OnQuestStarted -= PlayQuestStartAudio;
            controller.OnQuestEnded -= PlayQuestEndAudio;

            // Subscribe
            controller.OnQuestStarted += PlayQuestStartAudio;
            controller.OnQuestEnded += PlayQuestEndAudio;
            Debug.Log("[AudioManager] Subscribed to QuestStateController events.");
        }
    }

    private void PlayQuestStartAudio(QuestBase quest)
    {
        AudioClip clip = GetClipForQuest(quest, true);
        if (clip != null)
        {
            Debug.Log($"[AudioManager] Playing Entry Audio for quest: {quest.questDescription} (Playthrough {GameManager.playthroughCount}/3)");
            PlayClip(clip);
        }
    }

    private void PlayQuestEndAudio(QuestBase quest)
    {
        AudioClip clip = GetClipForQuest(quest, false);
        if (clip != null)
        {
            Debug.Log($"[AudioManager] Playing End Audio for quest: {quest.questDescription} (Playthrough {GameManager.playthroughCount}/3)");
            PlayClip(clip);
        }
    }

    private AudioClip GetClipForQuest(QuestBase quest, bool isEntry)
    {
        int playthrough = GameManager.playthroughCount;
        QuestAudioClips clips;

        // Select the clip set based on the active playthrough number
        if (playthrough == 1)
        {
            clips = playthrough1Clips;
        }
        else if (playthrough == 2)
        {
            clips = playthrough2Clips;
        }
        else
        {
            clips = playthrough3Clips;
        }

        // Identify the quest type and return the correct clip
        if (quest is Quest1CleaningState)
        {
            return isEntry ? clips.q1Entry : clips.q1End;
        }
        else if (quest is Quest2StudyState)
        {
            return isEntry ? clips.q2Entry : clips.q2End;
        }
        else if (quest is Quest4ShowerState)
        {
            return isEntry ? clips.q4Entry : clips.q4End;
        }

        return null;
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
