using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RoomClutterManager : MonoBehaviour
{
    public static List<RoomClutterManager> AllManagers { get; private set; } = new List<RoomClutterManager>();

    [Header("Room Settings")]
    public string roomName;

    [Header("Post Processing Volumes")]
    [Tooltip("The local clean URP volume profile for this room (fades to 1.0 when clean).")]
    public Volume cleanVolume;
    [Tooltip("The local noir URP volume profile for this room (fades to 0.0 when clean).")]
    public Volume noirVolume;

    [Header("Spawn Settings")]
    [Tooltip("Spawn points in this room where clutter spawned from other rooms can land.")]
    public Transform[] spawnPoints;

    public event Action OnRoomCleaned;

    private HashSet<ClutterSpawner> activeClutter = new HashSet<ClutterSpawner>();
    public bool IsCleaned { get; private set; }

    private void Awake()
    {
        AllManagers.Add(this);
    }

    private void OnDestroy()
    {
        AllManagers.Remove(this);
    }

    /// <summary>
    /// Registers a clutter spawner as active in this room.
    /// </summary>
    public void RegisterClutter(ClutterSpawner clutter)
    {
        if (clutter != null && activeClutter.Add(clutter))
        {
            IsCleaned = false;
        }
    }

    /// <summary>
    /// Unregisters a clutter spawner (called when it is collected/destroyed).
    /// </summary>
    public void UnregisterClutter(ClutterSpawner clutter)
    {
        if (clutter != null && activeClutter.Remove(clutter))
        {
            CheckCleanliness();
        }
    }

    /// <summary>
    /// Checks if there is any active clutter remaining in the room.
    /// </summary>
    public void CheckCleanliness()
    {
        if (activeClutter.Count == 0 && !IsCleaned)
        {
            IsCleaned = true;
            Debug.Log($"[{gameObject.name}] {roomName} fully clean! Fading room volume to pastel CleanLUT.");
            
            if (PostProcessingManager.Instance != null)
            {
                if (cleanVolume != null)
                {
                    PostProcessingManager.Instance.BlendVolume(cleanVolume, 1.0f, 2.0f);
                }
                if (noirVolume != null)
                {
                    PostProcessingManager.Instance.BlendVolume(noirVolume, 0.0f, 2.0f);
                }
            }

            OnRoomCleaned?.Invoke();
        }
    }

    /// <summary>
    /// Returns a random spawn point within this room.
    /// </summary>
    public Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;
        return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
    }

    /// <summary>
    /// Helper to find a random RoomClutterManager other than the specified one.
    /// </summary>
    public static RoomClutterManager GetRandomOtherManager(RoomClutterManager exclude)
    {
        List<RoomClutterManager> others = new List<RoomClutterManager>();
        foreach (var manager in AllManagers)
        {
            if (manager != exclude)
            {
                others.Add(manager);
            }
        }
        if (others.Count == 0) return null;
        return others[UnityEngine.Random.Range(0, others.Count)];
    }
}
