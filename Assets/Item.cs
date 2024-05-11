using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
    public enum ItemType
    {
        TypeA,
        TypeB,
        TypeC
    }

    public ItemType itemType;

    [SerializeField] private float destructionDelay = 10f; // Time in seconds after which the item will self-destruct
    [SerializeField] private Color startColor = Color.white; // Start color of the item
    [SerializeField] private Color endColor = Color.red; // End color of the item when it's about to be destroyed

    private Material itemMaterial; // Local material instance for this item

    private void Start()
    {
        CloneMaterial(); // Ensure each item has its own material instance
        StartCoroutine(ColorShiftAndDestructionCountdown());
    }

    private void CloneMaterial()
    {
        // Get the MeshRenderer component and clone its material to make sure each item has its own instance
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            itemMaterial = meshRenderer.material = new Material(meshRenderer.material);
        }
        else
        {
            Debug.LogError("MeshRenderer component not found on the GameObject.");
        }
    }

    private IEnumerator ColorShiftAndDestructionCountdown()
    {
        float timeLeft = destructionDelay;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            UpdateItemColor(timeLeft / destructionDelay); // Update color based on the proportion of time left
            yield return null;
        }
        Destroy(gameObject); // Destroy the item when time runs out
    }

    private void UpdateItemColor(float proportionLeft)
    {
        // Change color from startColor to endColor as the time decreases
        itemMaterial.color = Color.Lerp(endColor, startColor, proportionLeft);
    }
}
