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

    // Dictionary to keep the original sizes of BoxColliders
    private Dictionary<Transform, Vector3> originalBoxSizes = new Dictionary<Transform, Vector3>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
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
        Vector3 endPosition = transform.position + direction * dashDistance;

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(transform.position, endPosition);
        while (Vector3.Distance(transform.position, endPosition) > 0.05f)
        {
            float distCovered = (Time.time - startTime) * moveSpeed * dashSpeed;
            float fractionOfJourney = distCovered / journeyLength;
            rb.MovePosition(Vector3.Lerp(transform.position, endPosition, fractionOfJourney));

            if (mainChar != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                mainChar.rotation = Quaternion.Lerp(mainChar.rotation, targetRotation, Time.deltaTime * 10f);
            }

            yield return null;
        }

        isDashing = false;
        rb.useGravity = true;
    }

    void TryPickUp()
    {
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

}
