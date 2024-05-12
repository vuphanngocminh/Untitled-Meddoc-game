using UnityEngine;
using System.Collections;

public class ItemSpawner : MonoBehaviour
{
    public GameObject itemPrefab; // Drag your item prefab here in the inspector
    public Vector3 center; // Center of the spawn region
    public Vector3 size; // Size of the spawn region (width, height, depth)
    public float spawnInterval = 5f; // Time in seconds between spawns
    public int minItems = 1; // Minimum number of items to spawn each cycle
    public int maxItems = 5; // Maximum number of items to spawn each cycle

    private void Start()
    {
        // Start the spawn cycle coroutine
        StartCoroutine(SpawnCycle());
    }

    private IEnumerator SpawnCycle()
    {
        while (true)
        {
            // Wait for the specified interval
            yield return new WaitForSeconds(spawnInterval);

            // Spawn a random number of items
            int itemsToSpawn = Random.Range(minItems, maxItems + 1);
            for (int i = 0; i < itemsToSpawn; i++)
            {
                SpawnItem();
            }
        }
    }

    private void SpawnItem()
    {
        // Generate a random position within the box
        Vector3 position = center + new Vector3(
            Random.Range(-size.x / 2, size.x / 2),
            Random.Range(-size.y / 2, size.y / 2),
            Random.Range(-size.z / 2, size.z / 2));

        // Instantiate the item at the random position
        GameObject newItem = Instantiate(itemPrefab, position, Quaternion.identity);

        // Access the script component where 'isTypeAConsumedInitially' is declared
        var itemScript = newItem.GetComponent<Container>(); // Replace 'ItemScript' with the actual script class name

        // Check if the script exists and set 'isTypeAConsumedInitially' randomly
        if (itemScript != null)
        {
            itemScript.isTypeAConsumedInitially = Random.value > 0.5f; // 50% chance to be true or false
            itemScript.isTypeBConsumedInitially = Random.value > 0.5f;
            if (!itemScript.isTypeAConsumedInitially && !itemScript.isTypeBConsumedInitially){
                itemScript.isTypeCConsumedInitially = Random.value > 0.5f;
            }
        }
        else
        {
            Debug.LogWarning("ItemScript component not found on the prefab. Please ensure it is attached.");
        }
    }


    private void OnDrawGizmosSelected()
    {
        // Draw a wireframe box in the editor to show the spawn region
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);
    }
}
