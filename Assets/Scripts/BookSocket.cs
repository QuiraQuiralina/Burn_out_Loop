using System.Collections;
using UnityEngine;

public class BookSocket : MonoBehaviour
{
    [Tooltip("The parent shelf container that tracks total snapped books.")]
    public ShelfContainer shelfContainer;

    [Tooltip("Lerp speed for snapping transition.")]
    public float snapSpeed = 8f;

    public bool IsOccupied { get; private set; } = false;

    private GameObject currentBookInRange;
    private Coroutine snapCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (IsOccupied || currentBookInRange != null) return;

        // Check if the object entering has a BookHelper component
        BookHelper helper = other.GetComponent<BookHelper>();
        if (helper != null)
        {
            currentBookInRange = other.gameObject;
            helper.SetCurrentSocket(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BookHelper helper = other.GetComponent<BookHelper>();
        if (helper != null && currentBookInRange == other.gameObject)
        {
            helper.ClearCurrentSocket(this);
            currentBookInRange = null;
        }
    }

    /// <summary>
    /// Snaps the book to this socket's local center position and rotation.
    /// </summary>
    public void SnapBook(GameObject book)
    {
        if (IsOccupied) return;

        IsOccupied = true;
        currentBookInRange = null;

        if (snapCoroutine != null)
        {
            StopCoroutine(snapCoroutine);
        }
        snapCoroutine = StartCoroutine(SnapLerpRoutine(book));
    }

    private IEnumerator SnapLerpRoutine(GameObject book)
    {
        // Disable Rigidbody physics
        Rigidbody rb = book.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable colliders to avoid collision clipping issues on shelf
        Collider col = book.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Parent the book to this slot
        book.transform.SetParent(transform);

        Vector3 startPos = book.transform.localPosition;
        Quaternion startRot = book.transform.localRotation;

        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            book.transform.localPosition = Vector3.Lerp(startPos, Vector3.zero, t);
            book.transform.localRotation = Quaternion.Slerp(startRot, Quaternion.identity, t);
            yield return null;
        }

        book.transform.localPosition = Vector3.zero;
        book.transform.localRotation = Quaternion.identity;

        if (shelfContainer != null)
        {
            shelfContainer.RegisterSnappedBook();
        }
    }
}
