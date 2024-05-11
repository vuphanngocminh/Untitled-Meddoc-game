using UnityEngine;
using System.Collections.Generic;

public class Container : MonoBehaviour
{   
    [SerializeField]
    private bool isTypeAConsumedInitially = false;
    [SerializeField]
    private bool isTypeBConsumedInitially = false;
    [SerializeField]
    private bool isTypeCConsumedInitially = false;

    private List<Item.ItemType> consumedItemTypes = new List<Item.ItemType>();
    private bool isPlayerNearby = false; // Flag to check if player is nearby

    [SerializeField]
    private Item itemPrefabA; // Prefab for instantiating items of TypeA
    [SerializeField]
    private Item itemPrefabB; // Prefab for instantiating items of TypeB
    [SerializeField]
    private Item itemPrefabC; // Prefab for instantiating items of TypeC

    public LogicScript logic;

    private void Start()
    {
        InitializeConsumedItems();
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && isPlayerNearby)
        {
            DestroyContainerAndSpawnItems();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            isPlayerNearby = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController != null && playerController.IsHoldingItem())
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Item item = playerController.GetHeldItem();
                ConsumeItem(playerController, item);
            }
        }
    }

    private void ConsumeItem(PlayerController playerController, Item item)
    {
        if (!consumedItemTypes.Contains(item.itemType))
        {
            consumedItemTypes.Add(item.itemType);
            Debug.Log("Consumed item of type: " + item.itemType);
            Destroy(item.gameObject); // Destroy the item game object
            playerController.DropObject();

            CheckForAllItemsConsumed(); // Check if all item types are consumed
        }
        else
        {
            Debug.Log("An item of this type has already been consumed: " + item.itemType);
        }
    }

    private void InitializeConsumedItems()
    {
        if (isTypeAConsumedInitially)
            consumedItemTypes.Add(Item.ItemType.TypeA);
        if (isTypeBConsumedInitially)
            consumedItemTypes.Add(Item.ItemType.TypeB);
        if (isTypeCConsumedInitially)
            consumedItemTypes.Add(Item.ItemType.TypeC);
    }

    private void DestroyContainerAndSpawnItems()
    {
        foreach (var itemType in consumedItemTypes)
        {
            InstantiateNewItem(itemType);
        }

        Destroy(gameObject); // Destroy the container itself
    }

    private void InstantiateNewItem(Item.ItemType itemType)
    {
        Item prefab = GetPrefabByType(itemType);
        if (prefab != null)
        {
            Item newItem = Instantiate(prefab, transform.position, Quaternion.identity);
            newItem.itemType = itemType;
            newItem.gameObject.SetActive(true); // Newly instantiated items are active
        }
    }

    private Item GetPrefabByType(Item.ItemType itemType)
    {
        switch (itemType)
        {
            case Item.ItemType.TypeA:
                return itemPrefabA;
            case Item.ItemType.TypeB:
                return itemPrefabB;
            case Item.ItemType.TypeC:
                return itemPrefabC;
            default:
                return null;
        }
    }

    // New method to check if all item types are consumed
    private void CheckForAllItemsConsumed()
    {
        if (consumedItemTypes.Contains(Item.ItemType.TypeA) &&
            consumedItemTypes.Contains(Item.ItemType.TypeB) &&
            consumedItemTypes.Contains(Item.ItemType.TypeC))
        {
            PerformSpecialAction(); // Perform some special action
        }
    }

    // Define the special action to perform when all types are consumed
    private void PerformSpecialAction()
    {   
        logic.addScore();
        Debug.Log("All item types have been consumed! Special action performed.");
        Destroy(gameObject);
        // Add additional logic here for the special action
    }
}
