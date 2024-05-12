using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Container : MonoBehaviour
{
    [SerializeField] public bool isTypeAConsumedInitially = false;
    [SerializeField] public bool isTypeBConsumedInitially = false;
    [SerializeField] public bool isTypeCConsumedInitially = false;

    private List<Item.ItemType> consumedItemTypes = new List<Item.ItemType>();
    private bool isPlayerNearby = false;

    [SerializeField] private Item itemPrefabA; // Prefab for instantiating items of TypeA
    [SerializeField] private Item itemPrefabB; // Prefab for instantiating items of TypeB
    [SerializeField] private Item itemPrefabC; // Prefab for instantiating items of TypeC

    [SerializeField] private float destructionDelay = 30f; // Time in seconds after which the container will self-destruct

    private Material containerMaterial; // Local material instance for this container

    public LogicScript logic; // Reference to the logic script handling game rules

    private void Start()
    {
        InitializeConsumedItems();
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
        CloneMaterial(); // Ensure each container has its own material instance
        StartCoroutine(DestructionCountdown());
    }

    private void CloneMaterial()
    {
        // Fetch the MeshRenderer component and clone its material
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        containerMaterial = meshRenderer.material = new Material(meshRenderer.material);
    }

    private void Update()
    {
        if (isPlayerNearby)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                DestroyContainerAndSpawnItems();
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                DisplayUnconsumedItemTypes();
            }
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

    private void DisplayUnconsumedItemTypes()
    {
        string unconsumedTypes = "Unconsumed Item Types: ";
        bool foundUnconsumed = false;

        if (!consumedItemTypes.Contains(Item.ItemType.TypeA))
        {
            unconsumedTypes += "Stomage ";
            foundUnconsumed = true;
        }
        if (!consumedItemTypes.Contains(Item.ItemType.TypeB))
        {
            unconsumedTypes += "Liver ";
            foundUnconsumed = true;
        }
        if (!consumedItemTypes.Contains(Item.ItemType.TypeC))
        {
            unconsumedTypes += "Heart ";
            foundUnconsumed = true;
        }

        logic.missing(unconsumedTypes);
        Debug.Log(unconsumedTypes);
    }

    private IEnumerator DestructionCountdown()
    {
        float timeLeft = destructionDelay;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            UpdateContainerColor(timeLeft / destructionDelay); // Update color based on the proportion of time left
            yield return null;
        }
        gameObject.SetActive(false); // Destroy the container when time runs out
    }

    private void UpdateContainerColor(float proportionLeft)
    {
        // Change color from white to red as the time decreases
        containerMaterial.color = Color.Lerp(Color.black, Color.red, proportionLeft);
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

        gameObject.SetActive(false); // Destroy the container itself
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

    private void CheckForAllItemsConsumed()
    {
        if (consumedItemTypes.Contains(Item.ItemType.TypeA) &&
            consumedItemTypes.Contains(Item.ItemType.TypeB) &&
            consumedItemTypes.Contains(Item.ItemType.TypeC))
        {
            PerformSpecialAction(); // Perform some special action
        }
    }

    private void PerformSpecialAction()
    {
        logic.addScore();
        logic.checkWin();
        Debug.Log("All item types have been consumed! Special action performed.");
        gameObject.SetActive(false); // Destroy this container
    }
}
