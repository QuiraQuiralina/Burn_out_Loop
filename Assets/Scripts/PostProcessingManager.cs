using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingManager : MonoBehaviour
{
    public static PostProcessingManager Instance { get; private set; }

    private Dictionary<Volume, Coroutine> activeBlends = new Dictionary<Volume, Coroutine>();

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

    /// <summary>
    /// Smoothly transitions a Volume's weight to a target value over a duration.
    /// </summary>
    public void BlendVolume(Volume volume, float targetWeight, float duration)
    {
        if (volume == null) return;

        if (activeBlends.TryGetValue(volume, out Coroutine activeCoroutine))
        {
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
            }
            activeBlends.Remove(volume);
        }

        Coroutine newCoroutine = StartCoroutine(BlendVolumeRoutine(volume, targetWeight, duration));
        activeBlends[volume] = newCoroutine;
    }

    private IEnumerator BlendVolumeRoutine(Volume volume, float targetWeight, float duration)
    {
        float startWeight = volume.weight;
        float elapsed = 0f;

        // Prevent division by zero
        if (duration <= 0f)
        {
            volume.weight = targetWeight;
            activeBlends.Remove(volume);
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            volume.weight = Mathf.Lerp(startWeight, targetWeight, t);
            yield return null;
        }

        volume.weight = targetWeight;
        activeBlends.Remove(volume);
    }
}
