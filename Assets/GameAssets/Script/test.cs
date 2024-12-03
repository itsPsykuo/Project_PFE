using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class test : MonoBehaviour
{
    private EnemyAI enemy;
    void Start()
    {
        enemy = FindAnyObjectByType<EnemyAI>();
    }

    // Update is called once per frame
    void Update()
    {
        enemy.enabled = true;
        this.enabled = false;
    }
}
