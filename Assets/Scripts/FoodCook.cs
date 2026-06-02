using System.Collections;
using UnityEngine;

public class FoodCook : MonoBehaviour
{
    [Header("Fridge Settings")]
    [Tooltip("The raw ingredient prefab spawned from the fridge.")]
    public GameObject ingredientPrefab;

    [Tooltip("Where ingredients spawn when the player interacts with the fridge.")]
    public Transform fridgeSpawnPoint;

    [Header("Stove Settings")]
    [Tooltip("The cooked consumable food prefab.")]
    public GameObject cookedFoodPrefab;

    [Tooltip("Where cooked food spawns on the stove.")]
    public Transform stoveSpawnPoint;

    [Tooltip("Cooking particle system.")]
    public ParticleSystem cookingParticles;

    [Tooltip("Cooking sound effect.")]
    public AudioSource cookingAudio;

    [Tooltip("Time in seconds to cook an ingredient.")]
    public float cookDuration = 2.0f;

    private bool isCooking = false;

    /// <summary>
    /// Spawns a raw ingredient at the fridge spawn point.
    /// Can be wired to a fridge door or button selection event.
    /// </summary>
    public void SpawnIngredient()
    {
        if (ingredientPrefab == null || fridgeSpawnPoint == null)
        {
            Debug.LogError("[FoodCook] Fridge settings (prefab or spawn point) are missing!");
            return;
        }

        Instantiate(ingredientPrefab, fridgeSpawnPoint.position, fridgeSpawnPoint.rotation);
        Debug.Log("[FoodCook] Ingredient spawned from fridge.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCooking) return;

        FoodIngredient ingredient = other.GetComponent<FoodIngredient>();
        if (ingredient != null)
        {
            StartCoroutine(CookRoutine(other.gameObject));
        }
    }

    private IEnumerator CookRoutine(GameObject ingredientObj)
    {
        isCooking = true;
        Debug.Log("[FoodCook] Cooking started...");

        // Consume the raw ingredient immediately
        Destroy(ingredientObj);

        // Turn on particles/sfx
        if (cookingParticles != null)
        {
            cookingParticles.Play();
        }
        if (cookingAudio != null)
        {
            cookingAudio.Play();
        }

        yield return new WaitForSeconds(cookDuration);

        // Turn off particles/sfx
        if (cookingParticles != null)
        {
            cookingParticles.Stop();
        }
        if (cookingAudio != null)
        {
            cookingAudio.Stop();
        }

        // Spawn cooked food
        if (cookedFoodPrefab != null && stoveSpawnPoint != null)
        {
            Instantiate(cookedFoodPrefab, stoveSpawnPoint.position, stoveSpawnPoint.rotation);
            Debug.Log("[FoodCook] Cooking complete! Cooked food spawned.");
        }
        else
        {
            Debug.LogError("[FoodCook] Cooked food settings are missing!");
        }

        isCooking = false;
    }
}
