using UnityEngine;

public class FoodCook : MonoBehaviour
{
    [Header("Fridge Settings")]
    [Tooltip("The cooked consumable food prefab spawned directly from the fridge.")]
    public GameObject cookedFoodPrefab;

    [Tooltip("Where food spawns when the player interacts with the fridge.")]
    public Transform fridgeSpawnPoint;

    /// <summary>
    /// Spawns the cooked food item directly at the fridge spawn point.
    /// </summary>
    public void SpawnIngredient()
    {
        if (cookedFoodPrefab == null || fridgeSpawnPoint == null)
        {
            Debug.LogError("[FoodCook] Fridge settings (prefab or spawn point) are missing!");
            return;
        }

        Instantiate(cookedFoodPrefab, fridgeSpawnPoint.position, fridgeSpawnPoint.rotation);
        Debug.Log("[FoodCook] Cooked food spawned directly from fridge.");
    }
}
