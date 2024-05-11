using System.Collections.Generic;
using UnityEngine;

public static class Patient
{
    private static HashSet<string> itemsCollected = new HashSet<string>();

    public static void AddItem(string item)
    {
        itemsCollected.Add(item);
        CheckItems();
    }

    private static void CheckItems()
    {
        if (itemsCollected.Contains("Item1") && itemsCollected.Contains("Item2") && itemsCollected.Contains("Item3"))
        {
            Debug.Log("All different items collected!");
            // Trigger any action or event here
        }
    }
}
