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
    private bool isDestroyed = false; // Flag to prevent multiple destruction

    [SerializeField] private Item itemPrefabA; // Prefab for instantiating items of TypeA
    [SerializeField] private Item itemPrefabB; // Prefab for instantiating items of TypeB
    [SerializeField] private Item itemPrefabC; // Prefab for instantiating items of TypeC
    [SerializeField] private GameObject aiPrefab; // Prefab for instantiating AI

    [SerializeField] private float destructionDelay = 30f; // Time in seconds after which the container will self-destruct

    private Material containerMaterial; // Local material instance for this container
    private GameObject indicator; // Reference to the indicator child object

    private GameObject stomageIndicator; // Child object for TypeA missing indicator
    private GameObject liverIndicator; // Child object for TypeB missing indicator
    private GameObject heartIndicator; // Child object for TypeC missing indicator

    private Coroutine indicatorCoroutine;
    public LogicScript logic;

    private void Start()
    {
        InitializeConsumedItems();
        CloneMaterial(); // Ensure each container has its own material instance
        StartCoroutine(DestructionCountdown());
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();

        // Find the indicator child objects
        stomageIndicator = transform.Find("StomageIndicator").gameObject;
        liverIndicator = transform.Find("LiverIndicator").gameObject;
        heartIndicator = transform.Find("HeartIndicator").gameObject;

        // Ensure the indicators are initially deactivated
        if (stomageIndicator != null) stomageIndicator.SetActive(false);
        if (liverIndicator != null) liverIndicator.SetActive(false);
        if (heartIndicator != null) heartIndicator.SetActive(false);

        // Find the indicator child object
        indicator = transform.Find("indicator").gameObject;
        if (indicator != null)
        {
            indicator.SetActive(false); // Ensure the indicator is initially deactivated
        }
    }

    private void CloneMaterial()
    {
        // Fetch the MeshRenderer component and clone its material
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        containerMaterial = meshRenderer.material = new Material(meshRenderer.material);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (indicatorCoroutine != null)
            {
                StopCoroutine(indicatorCoroutine);
            }
            indicatorCoroutine = StartCoroutine(DisplayUnconsumedItemTypesForDuration(2f));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            isPlayerNearby = true;
            playerController.AddContainer(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            isPlayerNearby = false;
            playerController.RemoveContainer(this);
        }
    }

    public void InteractWithContainer(PlayerController playerController)
    {
        if (!isDestroyed)
        {
            if (playerController.IsHoldingItem() && Input.GetKeyDown(KeyCode.E))
            {
                Item item = playerController.GetHeldItem();
                ConsumeItem(playerController, item);
            }
            if (Input.GetKeyDown(KeyCode.Q) && gameObject.activeSelf)
            {
                DestroyContainerAndSpawnItems();
            }
        }
    }

    public void OnBecomingClosest()
    {
        // Logic for when the container becomes the closest to the player
        Debug.Log("I am the closest container now!");
        if (indicator != null)
        {
            indicator.SetActive(true);
        }
    }

    public void OnNoLongerClosest()
    {
        // Logic for when the container is no longer the closest to the player
        Debug.Log("I am no longer the closest container.");
        if (indicator != null)
        {
            indicator.SetActive(false);
        }
    }

    private IEnumerator DisplayUnconsumedItemTypesForDuration(float duration)
    {
        // Deactivate all indicators first
        if (stomageIndicator != null) stomageIndicator.SetActive(false);
        if (liverIndicator != null) liverIndicator.SetActive(false);
        if (heartIndicator != null) heartIndicator.SetActive(false);

        // Activate the indicators based on missing item types
        if (!consumedItemTypes.Contains(Item.ItemType.TypeA) && stomageIndicator != null)
        {
            stomageIndicator.SetActive(true);
        }
        if (!consumedItemTypes.Contains(Item.ItemType.TypeB) && liverIndicator != null)
        {
            liverIndicator.SetActive(true);
        }
        if (!consumedItemTypes.Contains(Item.ItemType.TypeC) && heartIndicator != null)
        {
            heartIndicator.SetActive(true);
        }

        // Wait for the specified duration
        yield return new WaitForSeconds(duration);

        // Deactivate all indicators
        if (stomageIndicator != null) stomageIndicator.SetActive(false);
        if (liverIndicator != null) liverIndicator.SetActive(false);
        if (heartIndicator != null) heartIndicator.SetActive(false);
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
        if (gameObject.activeSelf) // Check if the object is still active before calling the method
        {
            DestroyContainerAndSpawnItems();
        }
    }

    private void UpdateContainerColor(float proportionLeft)
    {
        // Change color from white to red as the time decreases
        containerMaterial.color = Color.Lerp(Color.black, Color.red, proportionLeft);
    }

    private void ConsumeItem(PlayerController playerController, Item item)
    {
        if (!isDestroyed && !consumedItemTypes.Contains(item.itemType))
        {
            consumedItemTypes.Add(item.itemType);
            Debug.Log("Consumed item of type: " + item.itemType);
            Destroy(item.gameObject); // Destroy the item game object
            playerController.DropObject();

            if (indicatorCoroutine != null)
            {
                StopCoroutine(indicatorCoroutine);
            }
            indicatorCoroutine = StartCoroutine(DisplayUnconsumedItemTypesForDuration(2f));

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
        isDestroyed = true; // Mark the container as destroyed to prevent multiple calls
        // Stop all potentially conflicting coroutines
        StopAllCoroutines();  // This ensures that no other coroutine can interfere with the color change

        // Set the container color to black immediately
        containerMaterial.color = Color.black;

        foreach (var itemType in consumedItemTypes)
        {
            InstantiateNewItem(itemType);
        }

        StartCoroutine(DelayInstantiateAI(5f)); // 5 seconds delay before instantiating AI
    }

    private IEnumerator DelayInstantiateAI(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        InstantiateAI(); // Instantiate the AI prefab after the delay

        gameObject.SetActive(false); // Now deactivate the container after AI has been instantiated
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

    private void InstantiateAI()
    {
        if (aiPrefab != null)
        {
            Instantiate(aiPrefab, transform.position, Quaternion.identity);
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
        // Add your logic for the special action here
        logic.addScore();
        Debug.Log("All item types have been consumed! Special action performed.");
        logic.checkWin();
        gameObject.SetActive(false); // Destroy this container
    }
}
