using System.Collections;
using UnityEngine;

public class FoodConsume : MonoBehaviour
{
    [Header("Consume Settings")]
    [Tooltip("The tag identifying the player's face/mouth trigger zone.")]
    public string faceTriggerTag = "PlayerFace";

    [Tooltip("Proximity radius to find and dissolve other nearby food props.")]
    public float dissolveRadius = 2.0f;

    [Tooltip("Hunger points restored by eating this food item.")]
    public float hungerRestoreAmount = 10f;

    private bool isConsumed = false;

    private void OnTriggerEnter(Collider other)
    {
        // Consume if it touches the player's face, hands, or generic player tag
        if (other.CompareTag(faceTriggerTag) || other.CompareTag("Player"))
        {
            Consume();
        }
    }

    /// <summary>
    /// Consumes the food item, updates hunger, dissolves nearby food, and destroys itself.
    /// Can be wired to selection click events in the editor.
    /// </summary>
    public void Consume()
    {
        if (isConsumed) return;
        isConsumed = true;

        Debug.Log($"[FoodConsume] Consumed food item: {gameObject.name}");

        // Update hunger bar
        if (HungerBarManager.Instance != null)
        {
            HungerBarManager.Instance.AddHunger(hungerRestoreAmount);
        }
        else
        {
            Debug.LogWarning("[FoodConsume] HungerBarManager Instance not found.");
        }

        // Dissolve nearby food props
        DissolveNearbyProps();

        // Destroy self
        Destroy(gameObject);
    }

    private void DissolveNearbyProps()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, dissolveRadius);
        foreach (var col in colliders)
        {
            if (col.gameObject == gameObject) continue;

            // Only dissolve other food items
            bool isFoodOrIngredient = col.GetComponent<FoodConsume>() != null;

            if (isFoodOrIngredient)
            {
                if (GameManager.Instance != null)
                {
                    // Run coroutines on GameManager to ensure they survive this object's destruction
                    GameManager.Instance.StartCoroutine(DissolveObjectRoutine(col.gameObject));
                }
                else
                {
                    // Fallback if GameManager is not ready
                    col.gameObject.SetActive(false);
                    Destroy(col.gameObject);
                }
            }
        }
    }

    private IEnumerator DissolveObjectRoutine(GameObject obj)
    {
        if (obj == null) yield break;

        // Disable collider to prevent double-processing
        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Freeze physics
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Vector3 startScale = obj.transform.localScale;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration && obj != null)
        {
            elapsed += Time.deltaTime;
            obj.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
            yield return null;
        }

        if (obj != null)
        {
            Destroy(obj);
        }
    }
}
