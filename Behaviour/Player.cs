using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : Damageable
{
    [Obsolete("should create player target instance and use GetPlayerPosition for position vector")]
    public static Transform playerTransform;
    private bool canUseActions = true;
    [SerializeField] private Projectile projectilePrefab;

    public Vector3 GetPlayerPosition() 
    {
        Vector3 playerPosition = transform.position;
        //with the current model this heps us aim at the middle level instead of ground,
        //this will likely change with dedicated aim point(s)
        playerPosition.y += 1;   
        return playerPosition;
    }

    private void InitializePlayer() 
    {
        health.Amount = 40;
    }

    protected override void CheckHealthStatus()
    {
        if (health.Amount <= 0) 
        {
            Debug.Log("Player goes down");
        }
    }

    private void UserInput_OnPrimaryAction(object sender, System.EventArgs e)
    {
        PrimaryAction();
    }

    private void PrimaryAction()
    {
        RaycastHit hit;
        Vector3 targetPoint;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
            //Debug.Log("camera target ray hit: " + hit.collider.name);
        }
        else 
        {
            targetPoint = ray.GetPoint(150);
        }

        Quaternion rotation = Quaternion.LookRotation(targetPoint - transform.position);
        Projectile projectileScript = Instantiate(projectilePrefab, transform.position, rotation);
        if (projectileScript != null ) 
        {
            projectileScript.sender = gameObject;
        }
    }

    void Awake() 
    {
        playerTransform = transform; //deprecated method
        InitializePlayer();
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        UserInput.Instance.OnPrimaryAction += UserInput_OnPrimaryAction;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
