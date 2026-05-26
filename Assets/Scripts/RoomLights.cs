using UnityEngine;

public class RoomLights : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Drag all the Light components you want to toggle here.")]
    public Light[] lightsToControl;

    private void OnTriggerEnter(Collider other)
    {
        // Checks if the object entering is the Player (Hands or Body)
        if (other.CompareTag("Player"))
        {
            ToggleLights(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ToggleLights(false);
        }
    }

    void ToggleLights(bool status)
    {
        foreach (Light light in lightsToControl)
        {
            if (light != null)
            {
                light.enabled = status;
            }
        }
    }
}