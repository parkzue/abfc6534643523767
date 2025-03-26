using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : Damageable
{
    protected EnemyState enemyState;
    private Color enemyColor = Color.green;
    private bool hasSeenPlayer = false;
    private float timeLooking = 0.0f;
    [SerializeField] private float maxTimeLooking = 5.0f;
    protected Vector3 lastSeenTargetLocation;
    protected Vector3 lookDirection = Vector3.forward;

    private Vector3 visionLeft = Vector3.left;
    private Vector3 visionRight = Vector3.right;
    [SerializeField] protected float sightRange = 20f;
    [SerializeField] protected float visionConeAngleHalf = 30f;
    
    //action
    [SerializeField] protected ActionState actionState;

    //target
    [SerializeField] protected Player currentTarget;
    
    //events
    public event Action OnStateChange;

    //NOTE: experimenting, maybe change to EnemyVision
    public enum EnemyState 
    {
        Idle,
        LookingForTarget,
        CanSeeTarget
    }

    public enum ActionState
    {
        None,
        InProgress
    }

    private bool IsInVisionCone(Vector3 targetPosition) 
    {
        Vector3 targetDirection = (targetPosition - transform.position).normalized;
        float targetAngle = Vector3.Angle(lookDirection, targetDirection);
        if (targetAngle < visionConeAngleHalf) 
        { 
            return true;
        }
        return false;
    }

    private void DebugDrawDirectionVectors()
    {
        Debug.DrawLine(transform.position, transform.position + visionLeft * (visionLeft.magnitude + 16), Color.red);
        Debug.DrawLine(transform.position, transform.position + visionRight * (visionRight.magnitude + 16), Color.red);
        Debug.DrawLine(transform.position, transform.position + lookDirection * (lookDirection.magnitude + 16), Color.green);
    }

    private void CalculateVision()
    {
        visionLeft = Quaternion.AngleAxis(-visionConeAngleHalf, Vector3.up) * lookDirection;
        visionRight = Quaternion.AngleAxis(visionConeAngleHalf, Vector3.up) * lookDirection;
    }

    //Ray needs to hit a hitbox and the hitbox needs to be tagged as player in engine
    private bool CanSeePlayer()
    {
        RaycastHit hit;
        Player target = FindAnyObjectByType<Player>();
        if (target == null) return false;

        if (Physics.Raycast(transform.position, target.GetPlayerPosition() - transform.position, out hit, sightRange))
        {
            HitBox hitbox = hit.collider.GetComponent<HitBox>();
            if (hitbox != null)
            {
                if (enemyState == EnemyState.Idle && !IsInVisionCone(hitbox.transform.position))
                {
                    return false;
                }
                if (hit.collider.CompareTag("Player"))
                {
                    currentTarget = target;
                    this.hasSeenPlayer = true;
                    return true;
                }
            }
        }
        return false;
    }

    private void ChangeState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                enemyState = EnemyState.Idle;
                hasSeenPlayer = false;
                break;
            case EnemyState.LookingForTarget:
                enemyState = EnemyState.LookingForTarget;
                break;
            case EnemyState.CanSeeTarget:
                enemyState = EnemyState.CanSeeTarget;
                break;
        }
        OnStateChange?.Invoke();
    }

    protected float GetTargetDistance() 
    {
        if(currentTarget == null)
            { return -1; }
        return Vector3.Distance(transform.position, currentTarget.GetPlayerPosition());
    }

    private void ColorController() 
    {
        OnStateChange += EnemyBehaviour_ChangeColor;
    }

    //temporary function that changes the child object color
    private void EnemyBehaviour_ChangeColor()
    {
        Transform model = transform.Find("Model");
        if (model == null) { return; }

        Renderer rend = model.GetComponent<Renderer>();
        switch (this.enemyState) 
        {
            case EnemyState.Idle:
                enemyColor = Color.green;
                break;
            case EnemyState.LookingForTarget: 
                enemyColor = Color.yellow;
                break;
            case EnemyState.CanSeeTarget: 
                enemyColor = Color.red; 
                break;
        }        
        rend.material.color = enemyColor;
    }

    private void DetermineState() 
    {
        //can see the player
        if (CanSeePlayer())
        {
            ChangeState(EnemyState.CanSeeTarget);
            this.timeLooking = 0;
            if(currentTarget != null)
                lastSeenTargetLocation = currentTarget.GetPlayerPosition();
        }
        //cannot see the player right now but has seen them
        else if (!CanSeePlayer() && hasSeenPlayer)
        {
            //give up looking, return to idle
            if (this.timeLooking >= maxTimeLooking) 
            {
                ChangeState(EnemyState.Idle);
                this.timeLooking = 0;
                return;
            }
            //looks for player
            ChangeState(EnemyState.LookingForTarget);
            this.timeLooking += Time.deltaTime;
        }
        else 
        {
            ChangeState(EnemyState.Idle);
        }
    }

    private void RotateWithLookDirection() 
    {
        if(lookDirection != Vector3.zero) 
        {
            Quaternion rotation = Quaternion.LookRotation(lookDirection);
            Vector3 eulerRotation = rotation.eulerAngles;
            eulerRotation.x = 0f;
            transform.rotation = Quaternion.Euler(eulerRotation);
        }
    }

    protected virtual void Awake() 
    {
        ColorController();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        CalculateVision();
        DetermineState();
        RotateWithLookDirection();        
        DebugDrawDirectionVectors();
    }
}
