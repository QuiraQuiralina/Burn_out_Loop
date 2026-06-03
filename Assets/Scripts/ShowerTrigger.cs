using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ShowerTrigger : MonoBehaviour
{
    public static ShowerTrigger Instance { get; private set; }

    [Header("Particle Systems")]
    [Tooltip("The water particle systems.")]
    public ParticleSystem[] waterParticles;
    [Tooltip("The fire particle systems.")]
    public ParticleSystem[] fireParticles;

    [Header("Post Processing Volume")]
    [Tooltip("The global URP volume with the monochrome/high-contrast Fire LUT profile.")]
    public Volume fireVolume;
    [Tooltip("Fading duration for the Fire LUT transition.")]
    public float fireLutFadeDuration = 3f;

    [Header("Bed Mesh Swap")]
    [Tooltip("The normal bed GameObject (e.g. MESH_DORMITOR_Plapuma_SNOW).")]
    public GameObject normalBedObject;
    [Tooltip("The water bed GameObject (e.g. MESH_DORMITOR_Plapuma_APA).")]
    public GameObject waterBedObject;

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
    private Coroutine normalWaterCoroutine;

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
        if (waterParticles != null)
        {
            foreach (var ps in waterParticles)
            {
                if (ps != null) ps.Stop();
            }
        }
        if (fireParticles != null)
        {
            foreach (var ps in fireParticles)
            {
                if (ps != null) ps.Stop();
            }
        }
        if (timerPanel != null) timerPanel.SetActive(false);
        if (fireVolume != null) fireVolume.weight = 0f;

        // Ensure proper bed states on start
        if (normalBedObject != null) normalBedObject.SetActive(true);
        if (waterBedObject != null) waterBedObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            // Check if Quest 4 is active
            bool isQuest4Active = false;
            if (GameManager.Instance != null && GameManager.Instance.questStateController != null)
            {
                isQuest4Active = (GameManager.Instance.questStateController.CurrentQuest is Quest4ShowerState);
            }

            if (isQuest4Active)
            {
                // Start climax sequence
                isTriggered = true;
                if (normalWaterCoroutine != null)
                {
                    StopCoroutine(normalWaterCoroutine);
                    normalWaterCoroutine = null;
                }
                Debug.Log("[ShowerTrigger] Quest 4 active. Climax sequence started! Water turning to fire in 15 seconds.");
                StartCoroutine(ShowerSequenceRoutine());
            }
            else
            {
                // Play normal shower water
                StartNormalWater();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isTriggered) return; // If climax has started, don't stop water on exit

        if (other.CompareTag("Player"))
        {
            StopNormalWater();
        }
    }

    private void StartNormalWater()
    {
        if (normalWaterCoroutine != null)
        {
            StopCoroutine(normalWaterCoroutine);
        }
        normalWaterCoroutine = StartCoroutine(NormalWaterRoutine());
    }

    private void StopNormalWater()
    {
        if (normalWaterCoroutine != null)
        {
            StopCoroutine(normalWaterCoroutine);
            normalWaterCoroutine = null;
        }
        if (waterParticles != null)
        {
            foreach (var ps in waterParticles)
            {
                if (ps != null) ps.Stop();
            }
        }
    }

    private IEnumerator NormalWaterRoutine()
    {
        if (waterParticles != null)
        {
            foreach (var ps in waterParticles)
            {
                if (ps != null) ps.Play();
            }
        }
        // Auto-shutoff after 15 seconds
        yield return new WaitForSeconds(15.0f);

        if (waterParticles != null)
        {
            foreach (var ps in waterParticles)
            {
                if (ps != null) ps.Stop();
            }
        }
        normalWaterCoroutine = null;
    }

    private IEnumerator ShowerSequenceRoutine()
    {
        // 1. Spill water particle systems immediately
        if (waterParticles != null)
        {
            foreach (var ps in waterParticles)
            {
                if (ps != null) ps.Play();
            }
        }

        // 2. Wait 15 seconds
        yield return new WaitForSeconds(15.0f);

        // 3. Start fire particles (run alongside water for another 15s)
        if (fireParticles != null)
        {
            foreach (var ps in fireParticles)
            {
                if (ps != null) ps.Play();
            }
        }

        Debug.Log("[ShowerTrigger] Water turning to fire! Fading in FireLUT and swapping bed cover mesh.");

        // Trigger Fire LUT transition
        if (fireVolume != null && PostProcessingManager.Instance != null)
        {
            PostProcessingManager.Instance.BlendVolume(fireVolume, 1.0f, fireLutFadeDuration);
        }

        // Swap bed cover objects
        if (normalBedObject != null)
        {
            normalBedObject.SetActive(false);
        }
        if (waterBedObject != null)
        {
            waterBedObject.SetActive(true);
        }

        // Start countdown timer
        StartCountdown();

        // 4. Run both water and fire together for another 15 seconds
        yield return new WaitForSeconds(15.0f);

        // 5. Stop water particles (only fire remains)
        if (waterParticles != null)
        {
            foreach (var ps in waterParticles)
            {
                if (ps != null) ps.Stop();
            }
        }
        Debug.Log("[ShowerTrigger] Water stopped. Only fire particles remaining.");
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
