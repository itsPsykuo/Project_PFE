using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Space]
    [Header("Components")]
    [SerializeField] private PlayerControl playerControl;
    [SerializeField] private TargetDetectionControl targetDetectionControl;
    [SerializeField] private ThirdPersonController thirdPersonController;

    public float maxHealth = 100f;
    public float currentHealth;
    public ParticleSystem Healing;
    public Animator animator;


    private void Update()
    {
        selfHeal();
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        animator.SetTrigger("Taking Damage");
        
        if (currentHealth  <= 0)
        {
            playerControl.enabled = false;
            targetDetectionControl.enabled = false;
            thirdPersonController.enabled = false;
            animator.SetTrigger("Die");
        }
    }

    public void selfHeal()
    {
        if (currentHealth < maxHealth)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                currentHealth += 10f;
                if (currentHealth > maxHealth)
                {
                    currentHealth = maxHealth;
                }

                Healing.Play();
                StartCoroutine(StopHealingVFX());
            }
        }
    }

    IEnumerator StopHealingVFX()
    {
        // Wait for 1 second
        yield return new WaitForSeconds(1);
        Healing.Stop();
    }

    public void Die()
    {
        Debug.Log("Die");
    }
}
