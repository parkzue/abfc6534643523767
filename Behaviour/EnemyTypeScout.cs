using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTypeScout : EnemyBehaviour
{
    [SerializeField] private float shootCooldownCurrent = 1f;    
    private float gracePeriod = 2f; //time it takes for entity AI to activate when spawned
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private Vector3 targetDirection;
    [SerializeField] private float timeLookingAtDirection;
    
    //prefabs
    [SerializeField] private Projectile projectile;
    [SerializeField] private GameObject slashPrefab;

    //action
    [SerializeField] private EnemyScoutAction currentAction;
    private float aimTime;
    private float actionTimer;
    private float meleeCooldown;
    private float meleeActionDistance = 7f;

    private void InitializeEnemyScout()
    {
        health.Amount = 100;
        sightRange = 40f;
        visionConeAngleHalf = 45f;
    }

    protected override void CheckHealthStatus()
    {
        if (health != null && health.Amount <= 0)
        {
            Debug.Log("Enemy Scout health 0, dies");
            Destroy(gameObject);
        }
    }

    //------------DECIDE ACTIONS----------------

    private void DetermineAction()
    {
        //cannot act
        if (gracePeriod >= 0)
        {
            gracePeriod -= Time.deltaTime;
            return;
        }
        //is enemy is performing an action, update current
        if (currentAction != EnemyScoutAction.None)
        {
            UpdateAction();
        }
        //otherwise decide what to do
        else
        {
            //Debug.Log("Deciding new action for EnemyScout");
            DecideNewAction();
        }
    }

    private void UpdateAction()
    {
        switch (currentAction) 
        {
            case EnemyScoutAction.None:
                //
                break;
            case EnemyScoutAction.CombatMove:
                CombatMove();
                break;
            case EnemyScoutAction.Aiming:
                TakeAim();
                break;
            case EnemyScoutAction.IdleAction:
                UpdateIdleAction();
                break;
            case EnemyScoutAction.CombatFollowTarget:
                CombatFollowTarget();
                break;
            case EnemyScoutAction.CombatMelee:
                CombatMelee(); 
                break;
        }
    }

    private void DecideNewAction()
    {
        switch (enemyState)
        {
            case EnemyState.Idle:
                StartLookAroundAction();
                break;
            case EnemyState.LookingForTarget:
                //LookingForTargetAction();
                break;
            case EnemyState.CanSeeTarget:
                DecideNewCombatAction();
                break;
        }
    }

    private void UpdateIdleAction()
    {
        if (enemyState == EnemyState.CanSeeTarget) 
        {
            DecideNewCombatAction();
            return;
        }
        LookAround();
    }

    private void DecideNewCombatAction()
    {
        //is in melee range
        if(currentTarget != null && GetTargetDistance() <= meleeActionDistance && meleeCooldown <= 0) 
        {
            StartCombatMeleeAction();
            return;
        }        
        //not in melee range + shot available
        if (shootCooldownCurrent <= 0)
        {
            StartShootAtPlayerAction();
            return;
        }        

        //no offensive actions available
        int random = UnityEngine.Random.Range(0, 10);
        Debug.Log("no offensive action abailable, select defensive " + random);
        if (random > 5)
            StartCombatMoveAction(shootCooldownCurrent);
        else
            StartFollowTargetAction(shootCooldownCurrent);

    }

    //-------AI ACTIONS------------

    private void StartCombatMeleeAction() 
    {
        Debug.Log("Enemy Scout: combat melee action");
        currentAction = EnemyScoutAction.CombatMelee;
        actionTimer = 1f;
    }

    private void CombatMelee() 
    {
        //try to reach target during actionTimer
        if(actionTimer > 0) 
        {
            //reached melee distance
            if (GetTargetDistance() <= 1.2f) 
            {
                PerformMeleeAttack();
                return;
            }
            //move towards target
            if (currentTarget != null)
            {
                lookDirection = currentTarget.GetPlayerPosition() - transform.position;
                Vector3 target = currentTarget.GetPlayerPosition();
                target.y = transform.position.y; //don't move vertically at the moment, need to tie to physics engine tho
                transform.position = Vector3.MoveTowards(transform.position, target, 7f * Time.deltaTime);
            }
            actionTimer -= Time.deltaTime;
            return;
        }

        //otherwise perform melee as it is
        PerformMeleeAttack();
    }

    private void PerformMeleeAttack() 
    {
        Quaternion rotation = Quaternion.LookRotation(lookDirection);
        GameObject effect = Instantiate(slashPrefab, transform.position, rotation);
        Destroy(effect, 2f);
        //action complete
        meleeCooldown = 3f;
        actionTimer = 0;
        currentAction = EnemyScoutAction.None;
    }

    private void StartLookAroundAction() 
    {
        Debug.Log("enemy scout idle look around action");
        currentAction = EnemyScoutAction.IdleAction;
        actionState = ActionState.InProgress;
    }

    private void CombatFollowTarget()
    {
        if (actionTimer > 0)
        {
            if (currentTarget != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(currentTarget.GetPlayerPosition() - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                lookDirection = transform.forward;
            }
            actionTimer -= Time.deltaTime;
            return;
        }
        //action complete
        currentAction = EnemyScoutAction.None;
    }

    private void TakeAim()
    {
        if (currentTarget != null)
        {
            lookDirection = currentTarget.GetPlayerPosition() - transform.position;
        }
        aimTime += Time.deltaTime;
        if (aimTime >= 3)
        {
            ShootAtPlayer();
            aimTime = 0;
            currentAction = EnemyScoutAction.None;
            actionState = ActionState.None;
        }
    }

    private void StartFollowTargetAction(float time)
    {
        Debug.Log("Enemy Scout: combat follow target action");
        currentAction = EnemyScoutAction.CombatFollowTarget;
        actionTimer = time;
    }

    private void StartCombatMoveAction(float time = 1)
    {
        Debug.Log("Enemy Scout: combat move action");
        currentAction = EnemyScoutAction.CombatMove;
        actionState = ActionState.InProgress;
        actionTimer = time;
    }

    private void CombatMove()
    {
        if (actionTimer > 0) 
        {
            //do movement
            actionTimer -= Time.deltaTime;
            return;
        }
        currentAction = EnemyScoutAction.None;
        actionState = ActionState.None;
    }

    private void ShootAtPlayer()
    {
        Debug.Log("enemy scout shooting at target");
        if (projectile != null)
        {
            Quaternion rotation = Quaternion.LookRotation(lookDirection);
            Projectile projectileScript = Instantiate(projectile, transform.position, rotation);
            if(projectileScript != null) 
            {
                projectileScript.sender = gameObject;
            }
        }
        shootCooldownCurrent = 4f;
    }

    private void StartShootAtPlayerAction() 
    {
        currentAction = EnemyScoutAction.Aiming;
        actionState = ActionState.InProgress;
        Debug.Log("enemy scout aiming");
    }

    private void LookAround()
    {
        if (timeLookingAtDirection > 5) //don't leave at 5
        {
            lookDirection = EntityFunctions.GenerateRandomDirectionXZ();
            timeLookingAtDirection = 0;
            currentAction = EnemyScoutAction.None;
        }
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            lookDirection = transform.forward;
        }
        timeLookingAtDirection += Time.deltaTime;
    }

    //----------------END OF ACTIONS-----------------

    private void ReduceCooldowns()
    {
        if (shootCooldownCurrent > 0)
            shootCooldownCurrent -= Time.deltaTime;
        if (meleeCooldown > 0)
            meleeCooldown -= Time.deltaTime;
    }

    private void Awake()
    {
        base.Awake();
        InitializeEnemyScout();
    }

    private void Update()
    {
        base.Update();
        ReduceCooldowns();
        DetermineAction();
    }
}
