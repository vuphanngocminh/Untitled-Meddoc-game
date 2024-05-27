using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float dashDistance = 5f;
    public float dashSpeed = 2f;
    private bool isDashing = false;

    public Transform cameraTransform;
    public Transform mainChar;

    private Rigidbody rb;
    private Transform heldObject = null; // Reference to the held object
    private BoxCollider heldObjectBoxCollider = null; // Reference to the held object's box collider

    private bool isStunned = false; // Now private
    private float stunCooldownTime = 5f; // Cooldown period after being stunned
    private float stunEndTime = 0f; // Time when the stun will end
    private float stunCooldownEndTime = 0f; // Time when the cooldown will end
    private Coroutine stunCoroutine = null; // Reference to the active stun coroutine

    // Dictionary to keep the original sizes of BoxColliders
    private Dictionary<Transform, Vector3> originalBoxSizes = new Dictionary<Transform, Vector3>();
    private List<Container> nearbyContainers = new List<Container>(); // List of nearby containers

    private Container currentClosestContainer = null;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Prevent the player from rotating due to physics
    }

    void Update()
    {
        if (isStunned)
        {
            if (Time.time >= stunEndTime)
            {
                EndStun();
            }
            else
            {
                // Skip Update logic when stunned
                return;
            }
        }

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontalInput, 0f, verticalInput);

        if (!isDashing && movementDirection != Vector3.zero)
        {
            if (mainChar != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                mainChar.rotation = Quaternion.Lerp(mainChar.rotation, targetRotation, Time.deltaTime * 10f);
            }

            rb.MovePosition(transform.position + movementDirection.normalized * moveSpeed * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && movementDirection != Vector3.zero)
        {
            Vector3 dashDirection = movementDirection.normalized;
            StartCoroutine(Dash(dashDirection));
        }

        // Find the closest container and interact with it
        Container closestContainer = GetClosestContainer();
        if (closestContainer != currentClosestContainer)
        {
            if (currentClosestContainer != null)
            {
                currentClosestContainer.OnNoLongerClosest();
            }
            if (closestContainer != null)
            {
                closestContainer.OnBecomingClosest();
            }
            currentClosestContainer = closestContainer;
        }

        if (closestContainer != null)
        {
            closestContainer.InteractWithContainer(this);
        }

        // Toggle pick up or drop the object
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject != null)
            {
                DropObject();
            }
            else
            {
                TryPickUp();
            }
        }
    }

    IEnumerator Dash(Vector3 direction)
    {
        isDashing = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero; // Stop any existing movement
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + direction * dashDistance;

        // Check if there's an obstacle in the dash path
        RaycastHit hit;
        if (Physics.Raycast(startPosition, direction, out hit, dashDistance))
        {
            endPosition = hit.point; // Adjust the end position to the obstacle's point
            // Small offset to prevent getting stuck in the obstacle
            endPosition -= direction * 0.2f;
        }

        float smoothTime = 0.2f; // Smooth time for the transition
        Vector3 velocity = Vector3.zero; // Reference velocity for the SmoothDamp method

        float maxDashTime = 0.8f; // Maximum time allowed for the dash
        float elapsedTime = 0f; // Time elapsed since the start of the dash

        while (Vector3.Distance(transform.position, endPosition) > 0.05f && elapsedTime < maxDashTime)
        {
            transform.position = Vector3.SmoothDamp(transform.position, endPosition, ref velocity, smoothTime, moveSpeed * dashSpeed);
            elapsedTime += Time.deltaTime;

            if (mainChar != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                mainChar.rotation = Quaternion.Lerp(mainChar.rotation, targetRotation, Time.deltaTime * 10f);
            }

            yield return null;
        }

        // Ensure player is not stuck
        transform.position = endPosition;

        isDashing = false;
        rb.useGravity = true;
        rb.velocity = Vector3.zero; // Stop any residual movement after dash
        Debug.Log("Dash complete");
    }

    public void TryPickUp()
    {
        if (isStunned)
        {
            // Prevent picking up items while stunned
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Pickup") && hitCollider.transform != mainChar)
            {
                Item item = hitCollider.GetComponent<Item>();

                if (item != null)
                {
                    heldObject = hitCollider.transform;
                    Rigidbody heldRb = heldObject.GetComponent<Rigidbody>();
                    if (heldRb != null)
                    {
                        heldRb.isKinematic = true; // Disable Rigidbody when picking up
                    }

                    // Find and resize the first BoxCollider
                    heldObjectBoxCollider = heldObject.GetComponent<BoxCollider>();
                    if (heldObjectBoxCollider != null)
                    {
                        // Store original size in the dictionary
                        originalBoxSizes[heldObject] = heldObjectBoxCollider.size;
                        // Set the BoxCollider size to zero
                        heldObjectBoxCollider.size = Vector3.zero;
                    }

                    heldObject.SetParent(mainChar);
                    heldObject.localPosition = Vector3.forward; // Adjust position as needed
                    heldObject.localRotation = Quaternion.Euler(0, 0, 0);

                    // Do something different based on the item type
                    switch (item.itemType)
                    {
                        case Item.ItemType.TypeA:
                            Debug.Log("Picked up Type A item");
                            break;
                        case Item.ItemType.TypeB:
                            Debug.Log("Picked up Type B item");
                            break;
                        case Item.ItemType.TypeC:
                            Debug.Log("Picked up Type C item");
                            break;
                    }

                    break;
                }
            }
        }
    }

    public void DropObject()
    {
        if (heldObject != null)
        {
            // Restore the original size of the box collider if it exists in the dictionary
            if (heldObjectBoxCollider != null && originalBoxSizes.ContainsKey(heldObject))
            {
                heldObjectBoxCollider.size = originalBoxSizes[heldObject];
            }

            heldObject.SetParent(null);
            Rigidbody heldRb = heldObject.GetComponent<Rigidbody>();
            if (heldRb != null)
            {
                heldRb.isKinematic = false; // Enable Rigidbody when dropping
            }

            heldObjectBoxCollider = null; // Clear the reference
            heldObject = null;
        }
    }

    public bool IsHoldingItem()
    {
        return heldObject != null;
    }

    public Item GetHeldItem()
    {
        if (IsHoldingItem())
        {
            return heldObject.GetComponent<Item>();
        }
        return null;
    }

    public void Stun(float duration)
    {
        if (Time.time < stunCooldownEndTime)
        {
            // If the cooldown is not finished, ignore the new stun attempt
            return;
        }

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }

        stunCoroutine = StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        stunEndTime = Time.time + duration; // Set the end time for the stun
        stunCooldownEndTime = stunEndTime + stunCooldownTime; // Set the end time for the cooldown
        Debug.Log("Player is stunned");

        // Disable player movement here
        rb.velocity = Vector3.zero; // Stop player movement immediately
        rb.isKinematic = true; // Make Rigidbody kinematic to disable physics-based movement

        yield return new WaitForSeconds(duration);

        EndStun();
    }

    private void EndStun()
    {
        // Enable player movement here
        rb.isKinematic = false; // Re-enable physics-based movement
        isStunned = false;
        stunCoroutine = null; // Clear the coroutine reference
        Debug.Log("Player is no longer stunned");
    }

    public bool IsStunned()
    {
        return isStunned;
    }

    public void AddContainer(Container container)
    {
        if (!nearbyContainers.Contains(container))
        {
            nearbyContainers.Add(container);
        }
    }

    public void RemoveContainer(Container container)
    {
        if (nearbyContainers.Contains(container))
        {
            nearbyContainers.Remove(container);
        }
    }

    private Container GetClosestContainer()
    {
        Container closestContainer = null;
        float closestDistance = float.MaxValue;

        foreach (Container container in nearbyContainers)
        {
            float distance = Vector3.Distance(transform.position, container.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestContainer = container;
            }
        }

        return closestContainer;
    }
}
