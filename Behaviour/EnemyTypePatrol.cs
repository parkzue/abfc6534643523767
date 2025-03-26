using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTypePatrol : EnemyBehaviour
{
    Rigidbody rb;
    [SerializeField] private float baseMoveSpeed = 12f;
    private float timeAtScoutingLocation = 0.0f;
    private float maxTimeAtLocation = 2f;
    private Vector3 currentScoutingLocation;
    private float stopDistance = 0.2f;
    [SerializeField] private float gracePeriod = 2f;

    [SerializeField] private bool patrolTypeRoaming;  //non-roaming patrol follows a path and needs a point
    [SerializeField] PatrolPoint currentPatrolPoint;

    //attack
    private float meleeActionCooldown = 2f;
    [SerializeField] private GameObject slashPrefab;
    private float meleeActionDistance = 3f;

    private void MoveToPlayer()
    {
        if (currentTarget != null)
        {
            Vector3 direction = currentTarget.GetPlayerPosition() - transform.position;
            direction.Normalize();
            rb.MovePosition(transform.position + (direction * baseMoveSpeed * Time.deltaTime));
        }
    }

    private void MoveTowardsScoutingLocation() 
    {
        lookDirection = currentScoutingLocation - transform.position;
        Vector3 moveDirection = Vector3.Normalize(currentScoutingLocation - transform.position);       
        rb.MovePosition(transform.position + (moveDirection * baseMoveSpeed * Time.deltaTime));        
    }
    
    //NOTE: add some distance that counds as in location, and then proceed to do something else (look nearby?)
    private void MoveToLastSeenTargetLocation()
    {
        Vector3 moveDirection = lastSeenTargetLocation - transform.position;
        moveDirection.Normalize();
        rb.MovePosition(transform.position + (moveDirection * baseMoveSpeed * Time.deltaTime));
    }

    private void DetermineAction()
    {
        if(gracePeriod >= 0) 
        {
            gracePeriod -= Time.deltaTime;
            return;
        }
        switch (enemyState) 
        {
            case EnemyState.Idle:
                IdleAction();
                break;
            case EnemyState.LookingForTarget:
                LookingForTargetAction();
                break;
            case EnemyState.CanSeeTarget:
                InCombatAction();
                break;
        }
    }
    
    private void InCombatAction()
    {
        MoveToPlayer();
        if (currentTarget != null)
        {
            lookDirection = currentTarget.GetPlayerPosition() - transform.position;
            if (GetTargetDistance() <= meleeActionDistance && meleeActionCooldown <= 0) 
            {
                PerformMeleeAttack();
            }
        }
        //Debug.Log("In combat action");
    }
    private void PerformMeleeAttack()
    {
        Quaternion rotation = Quaternion.LookRotation(lookDirection);
        GameObject effect = Instantiate(slashPrefab, transform.position, rotation);
        Destroy(effect, 2f);
        //action complete
        meleeActionCooldown = 3f;
    }

    private void LookingForTargetAction()
    {
        float distance = Vector3.Distance(transform.position, lastSeenTargetLocation);
        if(distance > stopDistance) 
        {
            MoveToLastSeenTargetLocation();
        }       
    }

    private void IdleAction() 
    {
        float distanceFromLocation = Vector3.Distance(transform.position, currentScoutingLocation);
        if (distanceFromLocation < stopDistance)
        {
            timeAtScoutingLocation += Time.deltaTime;

            //time at target position
            if(timeAtScoutingLocation > maxTimeAtLocation) 
            {
                //roaming aimlessly
                if (patrolTypeRoaming)
                {
                    currentScoutingLocation = MapHandler.GenerateIdleTargetPosition();
                    Debug.Log("Generated new idle target position");
                    timeAtScoutingLocation = 0;
                }
                //follows certain path
                else 
                {
                    //get next patrol point
                    currentPatrolPoint = currentPatrolPoint.GetNextPatrolPoint();
                    currentScoutingLocation = currentPatrolPoint.transform.position;
                    Debug.Log("found new idle target point");
                    timeAtScoutingLocation = 0;
                }
            }
            return;
        }
        MoveTowardsScoutingLocation();
    }

    private void ReduceCooldowns() 
    {
        if(meleeActionCooldown > 0)
            meleeActionCooldown -= Time.deltaTime;
    }

    private void InitializeEnemyPatrol() 
    {
        currentPatrolPoint = FindFirstObjectByType<PatrolPoint>(); //WIP, just find 1st for testing
        currentScoutingLocation = transform.position; //start where it is placed
    }

    void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        InitializeEnemyPatrol();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        DetermineAction();
        ReduceCooldowns();
    }

}
