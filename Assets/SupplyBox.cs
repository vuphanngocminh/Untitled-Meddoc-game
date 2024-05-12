using UnityEngine;

public class SupplyBox : MonoBehaviour
{
    public GameObject itemPrefab; // Drag your item prefab here in the inspector
    public Transform spawnPoint; // Assign a child GameObject as the spawn point

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && Input.GetKeyDown(KeyCode.E) && !player.IsHoldingItem())
            {
                GameObject newItem = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
                newItem.SetActive(true); // Ensure the instantiated item is active
            }
        }
    }
}
