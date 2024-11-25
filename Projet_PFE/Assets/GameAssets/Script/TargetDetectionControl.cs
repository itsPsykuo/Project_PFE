using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TargetDetectionControl : MonoBehaviour
{
    public static TargetDetectionControl instance;

    [SerializeField] private EnemyAI currentTarget;

    [Header("Components")]
    public PlayerControl playerControl;

    [Header("Scene")]
    public List<Transform> allTargetsInScene = new List<Transform>();

    [Space]
    [Header("Target Detection")]
    public LayerMask whatIsEnemy;
    public bool canChangeTarget = true;

    [Tooltip("Detection Range: \n Player range for detecting potential targets.")]
    [Range(0f, 15f)] public float detectionRange = 10f;

    [Tooltip("Dot Product Threshold \nHigher Values: More strict alignment required \nLower Values: Allows for broader targeting")]
    [Range(0f, 1f)] public float dotProductThreshold = 0.15f;

    [SerializeField] Vector3 inputDirection;

    [Space]
    [Header("Debug")]
    public bool debug;
    
    public Transform checkPos;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        PopulateTargetInScene();
    }

    public void PopulateTargetInScene()
    {
        // Find all active GameObject in the scene
        EnemyBase[] allGameObjects = FindObjectsOfType<EnemyBase>();

        // Convert the array to a list
        List<EnemyBase> gameObjectList = new List<EnemyBase>(allGameObjects);

        // Output the number of GameObjects found
        if (debug)
            Debug.Log("Number of targets found: " + gameObjectList.Count);

        // Optionally, iterate over the list and do something with each GameObject
        foreach (EnemyBase obj in gameObjectList)
        {
            allTargetsInScene.Add(obj.transform);
        }
    }

    public EnemyAI CurrentTarget()
    {
        return currentTarget;
    }

    public void SetCurrentTarget(EnemyAI target)
    {
        currentTarget = target;
    }

    public void RemoveEnemy(){
        for (int i = allTargetsInScene.Count - 1; i >= 0; i--) // Iterate backward to safely remove items
        {
            EnemyBase enemyBase = allTargetsInScene[i].GetComponent<EnemyBase>();
            if (enemyBase != null && enemyBase.currentHealth <= 0)
            {
                Debug.Log("Removing enemy: " + allTargetsInScene[i].name);
                allTargetsInScene.RemoveAt(i);
            }
        }
    }


    public void GetEnemyInInputDirection()
    {
        Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;

        if (inputDirection != Vector3.zero)
        {
            inputDirection = Camera.main.transform.TransformDirection(inputDirection);
            inputDirection.y = 0;
            inputDirection.Normalize();

            Transform closestEnemy = GetClosestEnemyInDirection(inputDirection);

            if (closestEnemy != null && (Vector3.Distance(transform.position, closestEnemy.position)) <= detectionRange)
            {
                playerControl.ChangeTarget(closestEnemy);
                Debug.Log("Closest enemy in direction: " + closestEnemy.name);
            }
        }

        Transform GetClosestEnemyInDirection(Vector3 inputDirection)
        {
            Transform closestEnemy = null;
            float maxDotProduct = dotProductThreshold; // Start with the threshold value

            foreach (Transform enemy in allTargetsInScene)
            {
                Vector3 enemyDirection = (enemy.position - transform.position).normalized;
                float dotProduct = Vector3.Dot(inputDirection, enemyDirection);

                if (dotProduct > maxDotProduct)
                {
                    maxDotProduct = dotProduct;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }
    }

        public float InputMagnitude()
    {
        return inputDirection.magnitude;
    }

    void Update()
    {
        GetEnemyInInputDirection();
        RemoveEnemy();
    }
}
