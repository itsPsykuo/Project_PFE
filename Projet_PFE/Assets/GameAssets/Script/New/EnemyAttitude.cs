using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public enum AlertStage
{
    Patrol,
    Intrigued
}


public class EnemyAttitude : MonoBehaviour
{
    //Scripts 
    private CombatScript playerCombat;

    [Header("State Machnie Settings :")]
    public AlertStage alertStage;

    [Range(0, 100)] public float alertLevel;
    [Range(0, 360)] public float fovAngle;

    public float fov;

    [Header("Navigation Settings :")]

    public List<Transform> waypoints;

    private NavMeshAgent agent;
    public float InputMagnitude = 0;
    public static bool isIntegred;
    public bool isReady;

    private int currentWaypoinIndex = 0;

    [Header("Raycast Settings :")]

    private GameObject player;
    public float maxDistance;
    public LayerMask layerMask;
    private Vector3 originRay;


    private Animator animator;


    [Header("FOV :")]
    public Vector3 Offset = new Vector3(0f, 1f, 0f);
    

    private void Awake()
    {
        playerCombat = FindAnyObjectByType<CombatScript>();

        player = GameObject.FindWithTag("Player");

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        alertStage = AlertStage.Intrigued;

        alertLevel = 0;
    }


    private void Update()
    {
        playerNotHiding();

        EnemyMove();

        if (playerCombat.isAttackingEnemy)
        {
            isReady = true;
            this.enabled = false;
        }

        bool playerInFOV = false;
        Collider[] targetsInFOV = Physics.OverlapSphere(transform.position + Offset, fov);
        foreach (Collider c in targetsInFOV)
        {
            if (c.CompareTag("Player"))
            {
                float singedAngle = Vector3.Angle(transform.forward, c.transform.position + Offset - transform.position);
                if (Mathf.Abs(singedAngle) < fovAngle / 2)
                    playerInFOV = true;
                break;
            }
        }

        updateAlertState(playerInFOV);
    }

    void EnemyMove()
    {
        if (CheckMovement())
        {
            animator.SetFloat("InputMagnitude", InputMagnitude);

            InputMagnitude += Time.deltaTime;
                
           if(InputMagnitude >= 0.25f)
            InputMagnitude = 0.25f;
        }

        else
        {
            animator.SetFloat("InputMagnitude", InputMagnitude);
            InputMagnitude -= Time.deltaTime;
                if(InputMagnitude <= 0)
                InputMagnitude = 0;
        }
            
    }

    private void updateAlertState(bool playerInFOV)
    {
        switch (alertStage)
        {
            case AlertStage.Patrol:
                agent.speed = 3f;
                agent.stoppingDistance = 2f;
                Patrolling();
                if (playerInFOV && playerNotHiding())
                    alertStage = AlertStage.Intrigued;
                break;

            case AlertStage.Intrigued:
                agent.ResetPath();
                if (playerInFOV && playerNotHiding())
                {
                    transform.DOLookAt(playerCombat.transform.position, .5f);
                    alertLevel += 0.75f;
                    if (alertLevel >= 100)
                    {
                        isIntegred = true;
                        if (isIntegred)
                            agent.ResetPath();
                            isReady = true;
                            this.enabled = false;
                    }
                }
                else
                {
                    alertLevel -= 0.5f;
                    if (alertLevel <= 0)
                    {
                        agent.SetDestination(waypoints[currentWaypoinIndex].position);
                        alertStage = AlertStage.Patrol;
                    }
                }

                break;
        }
    }

    bool CheckMovement()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            return false;
        else
            return true;
    }


    void Patrolling()
    {
        if (waypoints.Count == 0)
            return;

        
        float distanceToWaypoint = Vector3.Distance(waypoints[currentWaypoinIndex].position, transform.position);

        if (distanceToWaypoint <= 3)
        {
            currentWaypoinIndex = (currentWaypoinIndex + 1) % waypoints.Count;
            agent.SetDestination(waypoints[currentWaypoinIndex].position);
        }
        
    }

    bool playerNotHiding()
    {
        originRay = transform.position + Offset;

        Vector3 directionRay = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z) - transform.position; 

        if (Physics.Raycast(originRay, directionRay, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.UseGlobal))
        {
            if (hit.transform == player.transform)
            {
                Debug.DrawRay(originRay, directionRay, Color.green);
                return true;
            }

            else
            {
                Debug.DrawRay(originRay, directionRay, Color.red);
                return false;
            }
        }
        else
        {
            Debug.DrawRay(originRay, directionRay * 50f, Color.cyan);
            return false;
        }
    }
}

