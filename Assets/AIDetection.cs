using UnityEngine;
using System.Collections;

public class AIDetection : MonoBehaviour
{
    public float detectionRange = 10f; // Range within which the AI detects the player
    private AIPatrol aiPatrol; // Reference to the AIPatrol script
    private Transform player; // Reference to the player's transform

    void Start()
    {
        aiPatrol = GetComponent<AIPatrol>(); // Get the AIPatrol component
        player = GameObject.FindWithTag("Player").transform; // Find the player object by tag
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        //Debug.Log(distanceToPlayer.ToString());

        if (distanceToPlayer <= detectionRange && aiPatrol.IsPlayerInPatrolRange(player.position))
        {
            aiPatrol.StartChasing(player.position); // Start chasing the player if within range
        }
        else
        {
            aiPatrol.StopChasing(); // Stop chasing and resume patrolling
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (!playerController.IsStunned())
            {
                playerController.Stun(3f); // Stun the player on contact if not already stunned
            }
        }
    }
}
