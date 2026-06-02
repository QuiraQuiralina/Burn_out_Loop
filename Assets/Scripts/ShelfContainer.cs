using System;
using UnityEngine;

public class ShelfContainer : MonoBehaviour
{
    [Header("Shelf Settings")]
    [Tooltip("Total slots available on the shelf. This matches the number of trigger sockets.")]
    public int totalSlots = 10;

    private int snappedCount = 0;

    public event Action OnShelfFull;

    /// <summary>
    /// Called by BookSocket when a book successfully snaps into place.
    /// </summary>
    public void RegisterSnappedBook()
    {
        snappedCount++;
        Debug.Log($"[ShelfContainer] Book snapped into Slot. Count: {snappedCount}/{totalSlots}.");

        if (snappedCount >= totalSlots)
        {
            Debug.Log("[ShelfContainer] All book slots occupied!");
            OnShelfFull?.Invoke();
        }
    }
}
