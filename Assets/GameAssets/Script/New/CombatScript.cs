using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class CombatScript : MonoBehaviour
{
    private EnemyManager enemyManager;
    private EnemyDetection enemyDetection;
    private MovementInput movementInput;
    private Animator animator;

    [Header("Target")]
    private EnemyAI lockedTarget;

    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown;

    [Header("States")]
    public bool isAttackingEnemy = false;
    public bool isCountering = false;
    public bool isRolling = false;
    public bool isCrouching = false;
    public bool isBlocking = false;

    [Header("Public References")]
    [SerializeField] private Transform punchPosition;
    [SerializeField] private ParticleSystemScript punchParticle;

    //Coroutines
    private Coroutine counterCoroutine;
    private Coroutine attackCoroutine;
    private Coroutine damageCoroutine;
    private Coroutine rollCoroutine;

    [Space]

    //Events
    public UnityEvent<EnemyAI> OnTrajectory;
    public UnityEvent<EnemyAI> OnHit;
    public UnityEvent<EnemyAI> OnCounterAttack;
    public UnityEvent<EnemyAI> OnRollAttack;

    int animationCount = 0;
    string[] attacks;

    void Start()
    {
        enemyManager = FindAnyObjectByType<EnemyManager>();
        animator = GetComponent<Animator>();
        enemyDetection = GetComponentInChildren<EnemyDetection>();
        movementInput = GetComponent<MovementInput>();
    }

    void Update()
    {
        if (isAttackingEnemy || isCountering)
        {
            animator.SetBool("Crouching", false);
            isCrouching = false;
        }
    }

    void AttackCheck()
    {
        if (isAttackingEnemy)
            return;

        //Check to see if the detection behavior has an enemy set
        if (enemyDetection.CurrentTarget() == null)
        {
            if (enemyManager.AliveEnemyCount() == 0)
            {
                Attack(null, 0);
                return;
            }
            else
            {
                lockedTarget = enemyManager.RandomEnemy();
            }
        }

        //If the player is moving the movement input, use the "directional" detection to determine the enemy
        if (enemyDetection.InputMagnitude() > .2f)
            lockedTarget = enemyDetection.CurrentTarget();

        //Extra check to see if the locked target was set
        if(lockedTarget == null)
            lockedTarget = enemyManager.RandomEnemy();

        //AttackTarget
        Attack(lockedTarget, TargetDistance(lockedTarget));
    }

    public void Attack(EnemyAI target, float distance)
    {
        //Types of attack animation
        attacks = new string[] { "Attack1", "Attack2", "Attack3", };

        //Attack nothing in case target is null
        if (target == null)
        {
            return;
        }

        if (distance < 5)
        {
            animationCount = (int)Mathf.Repeat((float)animationCount + 1, (float)attacks.Length);
            string attackString = isLastHit() ? attacks[Random.Range(0, attacks.Length)] : attacks[animationCount];
            AttackType(attackString, attackCooldown, target, .4f);
        }
        else
        {
            lockedTarget = null;
        }

    }
    

    void AttackType(string attackTrigger, float cooldown, EnemyAI target, float movementDuration)
    {
        animator.SetTrigger(attackTrigger);

        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackCoroutine(isLastHit() ? 1.5f : cooldown));

        //Check if last enemy
        if (isLastHit())
            StartCoroutine(FinalBlowCoroutine());

        if (target == null)
            return;

        target.StopMoving();
        MoveTorwardsTarget(target, movementDuration);

        IEnumerator AttackCoroutine(float duration)
        {
            movementInput.acceleration = 0;
            isAttackingEnemy = true;
            movementInput.enabled = false;
            yield return new WaitForSeconds(duration);
            isAttackingEnemy = false;
            yield return new WaitForSeconds(.2f);
            movementInput.enabled = true;
            LerpCharacterAcceleration();
        }

        IEnumerator FinalBlowCoroutine()
        {
            Time.timeScale = .5f;
            yield return new WaitForSecondsRealtime(2);
            Time.timeScale = 1f;
        }
    }

    void MoveTorwardsTarget(EnemyAI target, float duration)
    {
        OnTrajectory.Invoke(target);
        transform.DOLookAt(target.transform.position, .4f);
        transform.DOMove(TargetOffset(target.transform), duration);
    }

    void CounterCheck()
    {
        //Initial check
        if (isCountering || isRolling || isAttackingEnemy || !enemyManager.AnEnemyIsPreparingAttack())
            return;

        lockedTarget = ClosestCounterEnemy();
        OnCounterAttack.Invoke(lockedTarget);

        if (TargetDistance(lockedTarget) > 2)
        {
            Attack(lockedTarget, TargetDistance(lockedTarget));
            return;
        }

        float duration = .5f;
        animator.SetTrigger("Dodge");
        
        transform.DOLookAt(lockedTarget.transform.position, .2f);
        transform.DOMove(transform.position + lockedTarget.transform.forward, duration);

        if (counterCoroutine != null)
            StopCoroutine(counterCoroutine);
            counterCoroutine = StartCoroutine(CounterCoroutine(duration));

        IEnumerator CounterCoroutine(float duration)
        {
            isCountering = true;
            movementInput.enabled = false;
            yield return new WaitForSeconds(duration);
            Attack(lockedTarget, TargetDistance(lockedTarget));
            isCountering = false;
            movementInput.enabled = true;
        }

    }

    void RollCheck()
    {
        if (isCountering || isRolling || isAttackingEnemy)
            return;

        float duration = .7f;

        if(isCrouching)
        {
            lockedTarget = ClosestCounterEnemy();
            OnRollAttack.Invoke(lockedTarget);

            animator.SetTrigger("CrouchRoll");
            transform.DOMove(transform.position + transform.forward * 3f, duration );

            if(rollCoroutine != null)
                StopCoroutine(rollCoroutine);
            rollCoroutine = StartCoroutine(RollCoroutine(duration - .12f));
        }

        else
        {
            lockedTarget = ClosestCounterEnemy();
            OnRollAttack.Invoke(lockedTarget);

            animator.SetTrigger("Roll");
            if (movementInput.toggleSprint)
                transform.DOMove(transform.position + transform.forward * 5f, duration);
            else
                transform.DOMove(transform.position + transform.forward * 3f, duration);

            if(rollCoroutine != null)
                StopCoroutine(rollCoroutine);
            rollCoroutine = StartCoroutine(RollCoroutine(duration));
        }

        /*void BlockCheck()
        {
            if (isCountering || isRolling || isAttackingEnemy)
                return;
            
            if(isCountering)
            {
                animator.SetBool
            }
        }*/
            
        IEnumerator RollCoroutine(float duration)
        {
            isRolling = true;
            movementInput.enabled = false;
            yield return new WaitForSeconds(duration);
            isRolling = false;
            movementInput.enabled = true;
        }
    }

    void CrouchCheck()
    {
        if (isAttackingEnemy || isRolling || isCountering)
            return;
        
        if (!isCrouching)
        {
            animator.SetBool("Crouching", true);
            movementInput.toggleSprint = false;
            movementInput.acceleration = .75f;
            isCrouching = true;
        }
        else
        {
            animator.SetBool("Crouching", false);
            movementInput.toggleSprint = false;
            movementInput.acceleration = 1f;
            isCrouching = false;
        }
    }

    float TargetDistance(EnemyAI target)
    {
        return Vector3.Distance(transform.position, target.transform.position);
    }

    public Vector3 TargetOffset(Transform target)
    {
        Vector3 position;
        position = target.position;
        return Vector3.MoveTowards(position, transform.position, .45f);
    }

    public void HitEvent()
    {
        if (lockedTarget == null || enemyManager.AliveEnemyCount() == 0)
            return;

        OnHit.Invoke(lockedTarget);

        //Polish
        //punchParticle.PlayParticleAtPosition(punchPosition.position);
    }

    public void DamageEvent()
    {
        animator.SetTrigger("Taking Damage");

        animator.SetBool("Crouching", false);
        isCrouching = false;

        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);
            
        damageCoroutine = StartCoroutine(DamageCoroutine());

        IEnumerator DamageCoroutine()
        {
            movementInput.enabled = false;
            yield return new WaitForSeconds(.5f);
            movementInput.enabled = true;
            LerpCharacterAcceleration();
        }
    }

    EnemyAI ClosestCounterEnemy()
    {
        float minDistance = 100;
        int finalIndex = 0;

        for (int i = 0; i < enemyManager.allEnemies.Length; i++)
        {
            EnemyAI enemy = enemyManager.allEnemies[i].enemyScript;

            if (enemy.IsPreparingAttack())
            {
                if (Vector3.Distance(transform.position, enemy.transform.position) < minDistance)
                {
                    minDistance = Vector3.Distance(transform.position, enemy.transform.position);
                    finalIndex = i;
                }
            }
        }

        return enemyManager.allEnemies[finalIndex].enemyScript;

    }

    void LerpCharacterAcceleration()
    {
        movementInput.acceleration = 0;
        if (movementInput.toggleSprint)
            DOVirtual.Float(0, 2.5f, .6f, ((acceleration)=> movementInput.acceleration = acceleration));
        else
            DOVirtual.Float(0, 1f, .6f, ((acceleration)=> movementInput.acceleration = acceleration));
    }

    bool isLastHit()
    {
        if (lockedTarget == null)
            return false;

        return enemyManager.AliveEnemyCount() == 1 && lockedTarget.health <= 1;
    }


    #region Input

    private void OnCounter()
    {
        CounterCheck();
    }

    private void OnAttack()
    {
        AttackCheck();
    }

    private void OnRoll()
    {
        RollCheck();
    }

    private void OnCrouch()
    {
        CrouchCheck();
    }

    /*private void OnBlock()
    {
        BlockCheck();
    }*/

    #endregion

}