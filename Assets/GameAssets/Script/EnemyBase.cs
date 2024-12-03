using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;
    [Header("Health Settings")]
    public float currentHealth;

    [Header("Damage Settings")]
    public float damageAmount;
    public GameObject sword;

    [Header("Ragdoll corps")]
    public GameObject ragdoll;

    [Space]
    [Header("Material Settings")]
    public Material newMaterial;
    public Material originalMaterial;

    [Space]
    [Header("Particle System")]
    public ParticleSystem takeHitVfx;

    [Space]
    [Header("Particle System")]
    public Transform player;

    public Animator animator;

    void Awake()
    {
        ActiveTarget(false);
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player")){
            playerManager.TakeDamage(damageAmount);
        }
    }

    public void HitVFX()
    {
        if (gameObject != null)
        {
            takeHitVfx.Play();
            StartCoroutine(StopHitVFX());
        }
    }

    IEnumerator StopHitVFX()
    {
        yield return new WaitForSeconds(0.8f);
        takeHitVfx.Stop();
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        animator.SetTrigger("Taking Damage");
        
        if (currentHealth  <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Instantiate(ragdoll, transform.position, transform.rotation);
        Destroy(this.gameObject, .11f);
    }

    public void ActiveTarget(bool bool_)
    {
        Renderer renderer = GetComponentInChildren<Renderer>();

        if(bool_)
        {
            renderer.material = newMaterial;
        }
        else
        {
            renderer.material = originalMaterial;
        }
    }


}
