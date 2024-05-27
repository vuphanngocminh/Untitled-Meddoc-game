using UnityEngine;
using UnityEngine.AI;

public class AIPatrol : MonoBehaviour
{
    public int numberOfPatrolPoints = 4; // Number of patrol points to generate
    public float patrolRange = 20f; // Range within which the AI patrols
    private Vector3[] patrolPoints; // Array of patrol points
    private int currentPatrolIndex; // Index of the current patrol point
    private NavMeshAgent agent; // Reference to the NavMeshAgent component
    private bool chasingPlayer; // Flag to indicate if the AI is chasing the player
    private Vector3 initialPosition; // Initial position of the AI

    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component missing from this game object");
        }
        agent.autoBraking = false; // Disable auto-braking to allow smooth transitions
        initialPosition = transform.position; // Set the initial position
        GeneratePatrolPoints(); // Generate random patrol points
        GotoNextPatrolPoint();
    }

    void GeneratePatrolPoints()
    {
        patrolPoints = new Vector3[numberOfPatrolPoints];
        for (int i = 0; i < numberOfPatrolPoints; i++)
        {
            Vector3 randomPoint = initialPosition + Random.insideUnitSphere * patrolRange;
            randomPoint.y = initialPosition.y; // Keep the patrol points at the same height
            patrolPoints[i] = randomPoint;
        }
    }

    void GotoNextPatrolPoint()
    {
        if (patrolPoints.Length == 0)
            return;

        agent.destination = patrolPoints[currentPatrolIndex]; // Set the agent's destination
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length; // Move to the next patrol point
    }

    void Update()
    {
        if (!chasingPlayer && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GotoNextPatrolPoint(); // Go to the next patrol point if close to the current one
        }
    }

    public void StartChasing(Vector3 playerPosition)
    {
        chasingPlayer = true;
        agent.destination = playerPosition; // Set the agent's destination to the player's position
    }

    public void StopChasing()
    {
        chasingPlayer = false;
        GotoNextPatrolPoint(); // Resume patrolling
    }

    public bool IsPlayerInPatrolRange(Vector3 playerPosition)
    {
        return Vector3.Distance(initialPosition, playerPosition) <= patrolRange; // Check if the player is within the patrol range
    }
}
