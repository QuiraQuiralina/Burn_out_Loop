using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ShowerTrigger : MonoBehaviour
{
    public static ShowerTrigger Instance { get; private set; }

    [Header("Particle Systems")]
    [Tooltip("The water particle system.")]
    public ParticleSystem waterParticles;
    [Tooltip("The fire particle system.")]
    public ParticleSystem fireParticles;

    [Header("Post Processing Volume")]
    [Tooltip("The global URP volume with the monochrome/high-contrast Fire LUT profile.")]
    public Volume fireVolume;
    [Tooltip("Fading duration for the Fire LUT transition.")]
    public float fireLutFadeDuration = 3f;

    [Header("Bed Materials")]
    [Tooltip("The Renderer of the bed object.")]
    public Renderer bedRenderer;
    [Tooltip("The blue water/pool material to swap to.")]
    public Material waterPoolMaterial;

    [Header("Timer HUD UI")]
    [Tooltip("The parent UI panel for the timer/health countdown on the wrist HUD.")]
    public GameObject timerPanel;
    [Tooltip("Slider displaying remaining health/time.")]
    public Slider timerSlider;
    [Tooltip("Text displaying remaining time.")]
    public TextMeshProUGUI timerText;

    [Header("Timer Settings")]
    [Tooltip("Countdown duration in seconds.")]
    public float countdownDuration = 30f;

    private float timeRemaining;
    private bool isTimerRunning = false;
    private bool isTriggered = false;

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
        // Initial setup: turn off systems
        if (waterParticles != null) waterParticles.Stop();
        if (fireParticles != null) fireParticles.Stop();
        if (timerPanel != null) timerPanel.SetActive(false);
        if (fireVolume != null) fireVolume.weight = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            TriggerShower();
        }
    }

    /// <summary>
    /// Activates the shower sequence.
    /// Can be wired directly to selection triggers in the editor.
    /// </summary>
    public void TriggerShower()
    {
        if (isTriggered) return;
        isTriggered = true;

        Debug.Log("[ShowerTrigger] Shower activated. Water turning to fire! Health countdown running on wrist.");
        StartCoroutine(ShowerSequenceRoutine());
    }

    private IEnumerator ShowerSequenceRoutine()
    {
        // 1. Spill water particle system
        if (waterParticles != null)
        {
            waterParticles.Play();
        }

        // 2. Wait 3 seconds
        yield return new WaitForSeconds(3.0f);

        // Replace water particles with fire/flames
        if (waterParticles != null)
        {
            waterParticles.Stop();
        }
        if (fireParticles != null)
        {
            fireParticles.Play();
        }

        Debug.Log("[ShowerTrigger] Water turned to fire! Fading in FireLUT and swapping bed material.");

        // Trigger Fire LUT transition
        if (fireVolume != null && PostProcessingManager.Instance != null)
        {
            PostProcessingManager.Instance.BlendVolume(fireVolume, 1.0f, fireLutFadeDuration);
        }

        // Swap bed material to blue water/pool
        if (bedRenderer != null && waterPoolMaterial != null)
        {
            bedRenderer.material = waterPoolMaterial;
        }

        // Start countdown timer
        StartCountdown();
    }

    private void StartCountdown()
    {
        if (timerPanel != null)
        {
            timerPanel.SetActive(true);
        }

        timeRemaining = countdownDuration;
        isTimerRunning = true;
    }

    private void Update()
    {
        if (!isTimerRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timerSlider != null)
        {
            timerSlider.value = Mathf.Clamp01(timeRemaining / countdownDuration);
        }

        if (timerText != null)
        {
            timerText.text = $"{Mathf.Max(0f, timeRemaining):F1}s";
        }

        if (timeRemaining <= 0f)
        {
            isTimerRunning = false;
            OnTimerExpired();
        }
    }

    /// <summary>
    /// Stops the timer and hides the HUD timer panel. Called upon successful bed recovery.
    /// </summary>
    public void StopTimer()
    {
        if (!isTimerRunning) return;
        isTimerRunning = false;
        
        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
        }
        Debug.Log("[ShowerTrigger] Timer stopped.");
    }

    private void OnTimerExpired()
    {
        Debug.LogWarning("[ShowerTrigger] Player ran out of time!");
    }
}
