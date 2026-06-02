using UnityEngine;
using UnityEngine.UI;

public class HungerBarManager : MonoBehaviour
{
    public static HungerBarManager Instance { get; private set; }

    [Header("UI Setup")]
    [Tooltip("The parent UI panel containing the hunger bar slider.")]
    public GameObject hungerPanel;

    [Tooltip("The Slider component for displaying hunger percentage.")]
    public Slider hungerSlider;

    private float hungerPercentage = 0f;

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
        SetPanelActive(false);
        UpdateSlider();
    }

    /// <summary>
    /// Gets the current hunger level. (0 to 100).
    /// </summary>
    public float GetHunger()
    {
        return hungerPercentage;
    }

    /// <summary>
    /// Sets the panel visibility status.
    /// </summary>
    public void SetPanelActive(bool active)
    {
        if (hungerPanel != null)
        {
            hungerPanel.SetActive(active);
        }
    }

    /// <summary>
    /// Adds a value to the hunger bar (e.g. 10 for 10%).
    /// </summary>
    public void AddHunger(float amount)
    {
        hungerPercentage = Mathf.Clamp(hungerPercentage + amount, 0f, 100f);
        UpdateSlider();
        Debug.Log($"[HungerBar] Food consumed. Hunger: {hungerPercentage}/100%.");
    }

    /// <summary>
    /// Resets the hunger bar values.
    /// </summary>
    public void ResetHunger()
    {
        hungerPercentage = 0f;
        UpdateSlider();
    }

    private void UpdateSlider()
    {
        if (hungerSlider != null)
        {
            hungerSlider.value = hungerPercentage / 100f;
        }
    }
}
