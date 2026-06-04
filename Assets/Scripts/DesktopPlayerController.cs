using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class DesktopPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float gravity = -9.81f;

    [Header("Camera settings")]
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 85.0f;

    [Header("Interaction Settings")]
    public float interactionDistance = 3.0f;
    public Transform holdPoint;

    [Header("Reticle Settings")]
    public Texture2D reticleTexture;
    public float reticleSize = 8f;

    [Header("UI Settings")]
    [Tooltip("The pause panel/menu UI GameObject.")]
    public GameObject pauseUI;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;
    private float verticalVelocity = 0f;
    private bool isPaused = true;

    // Carrying state
    private BookHelper carriedBook;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        // Auto-assign camera if needed
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // Auto-create holdPoint if needed
        if (holdPoint == null && playerCamera != null)
        {
            GameObject hp = new GameObject("HoldPoint");
            hp.transform.SetParent(playerCamera.transform);
            hp.transform.localPosition = new Vector3(0.1f, -0.2f, 0.6f);
            hp.transform.localRotation = Quaternion.identity;
            holdPoint = hp.transform;
        }

        // Set initial paused state
        SetPauseState(true);
    }

    private void Update()
    {
        // Toggle Pause/Resume via Enter and Escape
        if (isPaused)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SetPauseState(false);
            }
            return; // Skip gameplay updates when paused
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetPauseState(true);
                return;
            }
        }

        // 1. Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = moveSpeed * Input.GetAxis("Vertical");
        float curSpeedY = moveSpeed * Input.GetAxis("Horizontal");

        if (characterController.isGrounded)
        {
            verticalVelocity = -0.5f; // keep grounded
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 move = (forward * curSpeedX) + (right * curSpeedY);
        move.y = verticalVelocity;
        characterController.Move(move * Time.deltaTime);

        // 2. Mouse Look
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        // 3. Interaction (E key)
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleInteraction();
        }

        // 4. Drop (G key)
        if (Input.GetKeyDown(KeyCode.G) && carriedBook != null)
        {
            DropBook();
        }
    }

    private void SetPauseState(bool pause)
    {
        isPaused = pause;
        if (pause)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (pauseUI != null) pauseUI.SetActive(true);
            Debug.Log("[DesktopPlayerController] Game Paused (Press Enter to Start/Resume)");
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (pauseUI != null) pauseUI.SetActive(false);
            Debug.Log("[DesktopPlayerController] Game Started/Resumed (Press Escape to Pause)");
        }
    }

    private void HandleInteraction()
    {
        // Raycast from camera center
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            GameObject hitObj = hit.collider.gameObject;

            // Carry logic interaction check
            if (carriedBook != null)
            {
                BookSocket socket = hitObj.GetComponent<BookSocket>();
                if (socket != null && !socket.IsOccupied)
                {
                    // Snap book to shelf
                    BookHelper tempBook = carriedBook;
                    carriedBook = null; // Clear carried book first

                    // SnapBook will handle parenting and physics
                    socket.SnapBook(tempBook.gameObject);
                    Debug.Log($"[DesktopPlayerController] Snapped book to socket: {socket.name}");
                    return;
                }
            }

            // Normal interaction check (when not snapping)
            // Clutter collection
            ClutterSpawner clutter = hitObj.GetComponent<ClutterSpawner>();
            if (clutter == null)
            {
                clutter = hitObj.GetComponentInParent<ClutterSpawner>();
            }
            if (clutter != null)
            {
                clutter.Collect();
                return;
            }

            // Book spawning
            BookSpawner bookSpawner = hitObj.GetComponent<BookSpawner>();
            if (bookSpawner == null)
            {
                bookSpawner = hitObj.GetComponentInParent<BookSpawner>();
            }
            if (bookSpawner != null)
            {
                bookSpawner.TriggerSpawn();
                return;
            }

            // Book pickup
            BookHelper book = hitObj.GetComponent<BookHelper>();
            if (book == null)
            {
                book = hitObj.GetComponentInParent<BookHelper>();
            }
            if (book != null && carriedBook == null)
            {
                PickUpBook(book);
                return;
            }

            // Food Cook (Fridge interaction)
            FoodCook foodCook = hitObj.GetComponent<FoodCook>();
            if (foodCook == null)
            {
                foodCook = hitObj.GetComponentInParent<FoodCook>();
            }
            if (foodCook != null)
            {
                foodCook.SpawnIngredient(); // Simplified fridge snacking
                return;
            }

            // Food consume
            FoodConsume foodConsume = hitObj.GetComponent<FoodConsume>();
            if (foodConsume == null)
            {
                foodConsume = hitObj.GetComponentInParent<FoodConsume>();
            }
            if (foodConsume != null)
            {
                foodConsume.Consume();
                return;
            }
        }

        // If carrying a book and we press E looking at nothing or something non-interactable, drop it
        if (carriedBook != null)
        {
            DropBook();
        }
    }

    private void PickUpBook(BookHelper book)
    {
        carriedBook = book;

        // Disable physics
        Rigidbody rb = book.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable standard collision triggers if necessary, but keep the triggers enabled
        Collider col = book.GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Parent to holdPoint
        book.transform.SetParent(holdPoint);
        book.transform.localPosition = Vector3.zero;
        book.transform.localRotation = Quaternion.identity;

        Debug.Log($"[DesktopPlayerController] Picked up book: {book.name}");
    }

    private void DropBook()
    {
        if (carriedBook == null) return;

        BookHelper bookToDrop = carriedBook;
        carriedBook = null;

        // Unparent
        bookToDrop.transform.SetParent(null);

        // Re-enable physics
        Rigidbody rb = bookToDrop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Re-enable collider
        Collider col = bookToDrop.GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false;
        }

        // Call BookHelper release to allow overlapping trigger snapping
        bookToDrop.OnRelease();

        Debug.Log($"[DesktopPlayerController] Dropped book: {bookToDrop.name}");
    }

    private void OnGUI()
    {
        // Only draw reticle when cursor is locked
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Rect rect = new Rect((Screen.width - reticleSize) / 2f, (Screen.height - reticleSize) / 2f, reticleSize, reticleSize);
            if (reticleTexture != null)
            {
                GUI.DrawTexture(rect, reticleTexture);
            }
            else
            {
                // Fallback: draw a small white square dot
                Texture2D fallback = Texture2D.whiteTexture;
                GUI.color = Color.white;
                GUI.DrawTexture(rect, fallback);
            }
        }
    }
}
