using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum AlertStage
{
    Patrol,
    Peaceful,
    Intrigued,
    Alerted
}

public class GuardNavigatingStatment : MonoBehaviour
{
    [Header("State Machnie Settings :")]

    public AlertStage alertStage;

    [Range(0, 100)] public float alertLevel;
    [Range(0, 360)] public float fovAngle;

    public float fov;

    [Header("Navigation Settings :")]

    public List<Transform> waypoints;
    public GameObject player;
    public float fullTime = 5f;
    private float waitingTime;

    NavMeshAgent agent;

    private int currentWaypoinIndex = 0;

    [Header("Raycast Settings :")]

    public float maxDistance;
    public LayerMask layerMask;
    public Transform playerPosition;

    private Vector3 originRay;
    private Vector3 directionRay;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        alertStage = AlertStage.Patrol;
        alertLevel = 0;
        waitingTime = 1f;
    }

    private void Update()
    {
        bool playerInFOV = false;
        Collider[] targetsInFOV = Physics.OverlapSphere(transform.position, fov);
        foreach (Collider c in targetsInFOV)
        {
            if (c.CompareTag("Player"))
            {
                float singedAngle = Vector3.Angle(transform.forward, c.transform.position - transform.position);
                if (Mathf.Abs(singedAngle) < fovAngle / 2)
                    playerInFOV = true;
                break;
            }
        }
        updateAlertState(playerInFOV);
    }

    private void updateAlertState(bool playerInFOV)
    {
        switch (alertStage)
        {
            case AlertStage.Patrol:
                agent.speed = 4f;
                Patrolling();
                if (playerInFOV && playerNotHiding())
                    alertStage = AlertStage.Peaceful;
                break;

            case AlertStage.Peaceful:
                if (!playerInFOV)
                {
                    waitingTime = fullTime;
                    agent.SetDestination(waypoints[currentWaypoinIndex].position);
                    alertStage = AlertStage.Patrol;
                }
                else
                {
                    agent.ResetPath();
                    agent.speed = 1f;
                    alertStage = AlertStage.Intrigued;
                }
                break;

            case AlertStage.Intrigued:
                if (playerInFOV & playerNotHiding())
                {
                    agent.SetDestination(player.transform.position);
                    agent.speed = Mathf.Lerp(agent.speed, 7f, 0.001f);
                    alertLevel += 0.1f;
                    if (alertLevel >= 100)
                    {
                        alertStage = AlertStage.Alerted;
                    }
                }
                else
                {
                    alertLevel -= 0.1f;
                    if (alertLevel <= 0)
                    {
                        agent.SetDestination(waypoints[currentWaypoinIndex].position);
                        alertStage = AlertStage.Patrol;
                    }
                }
                break;

            case AlertStage.Alerted:
                agent.speed = 5f;
                agent.SetDestination(player.transform.position);
                if (!playerInFOV)
                    alertStage = AlertStage.Intrigued;
                break;
        }
    }

    private void Patrolling()
    {
        waitingTime -= Time.deltaTime;
        Debug.Log(waitingTime);
        if (waitingTime <= 0)
        {
            if (waypoints.Count == 0)
            {
                return;
            }

            float distanceToWaypoint = Vector3.Distance(waypoints[currentWaypoinIndex].position, transform.position);
            waitingTime = fullTime;
            if (distanceToWaypoint <= 3)
            {
                currentWaypoinIndex = (currentWaypoinIndex + 1) % waypoints.Count;
                agent.SetDestination(waypoints[currentWaypoinIndex].position);
            }
        }
    }

    private bool playerNotHiding()
    {
        originRay = transform.position;
        directionRay = playerPosition.position - transform.position;

        if (Physics.Raycast(originRay, directionRay, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.UseGlobal))
        {
            if (hit.transform == playerPosition)
            {
                Debug.Log("Player detected!");
                Debug.DrawRay(originRay, directionRay, Color.green);
                return true;
            }

            else
            {
                Debug.Log("Obstacle between enemy and player: " + hit.transform.name);
                Debug.DrawRay(originRay, directionRay, Color.red);
                return false;
            }
        }
        else
        {
            Debug.DrawRay(originRay, directionRay * 50f, Color.cyan);
            Debug.Log("Nothing happend!");
            return false;
        }
    }
}

