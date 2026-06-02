using UnityEngine;

public class BookHelper : MonoBehaviour
{
    private BookSocket currentSocket;

    public void SetCurrentSocket(BookSocket socket)
    {
        currentSocket = socket;
    }

    public void ClearCurrentSocket(BookSocket socket)
    {
        if (currentSocket == socket)
        {
            currentSocket = null;
        }
    }

    /// <summary>
    /// Should be called when the player releases/drops the book.
    /// This can be wired to Meta's Grabbable 'WhenSelectExited' Unity Event in the inspector.
    /// </summary>
    public void OnRelease()
    {
        if (currentSocket != null)
        {
            currentSocket.SnapBook(gameObject);
        }
    }
}
