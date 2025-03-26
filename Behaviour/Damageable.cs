using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public Health health = new(100);

    protected event EventHandler OnDamageTaken;

    public void TakeDamage(int amount) 
    {
        health.DamageHealth(amount);
        Debug.Log("Damageable entity took " + amount + " damage. Current health: " + health.Amount);
        OnDamageTaken?.Invoke(this, EventArgs.Empty);
    }

    private void Damageable_OnDamageTaken(object sender, EventArgs e)
    {
        CheckHealthStatus();
    }

    /// <summary>
    /// Calls every time entity takes damage. override on specific entity class to determine what should happen.
    /// </summary>
    protected virtual void CheckHealthStatus()
    {
        if(health != null && health.Amount <= 0) 
        {
            Debug.Log("Entity health 0, should die.");    
        }
    }

    protected void Start()
    {
        this.OnDamageTaken += Damageable_OnDamageTaken;
    }
}
