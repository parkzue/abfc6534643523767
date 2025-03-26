using UnityEngine;

/*
    The master class for bosses, define simple things like boss encounter in progress, can see player...
    that every boss needs to make more specific decisions
 */
public class BossBehaviour : Damageable
{
    protected float aiUpdateInterval = 0.1f;
    protected float timeSinceLastUpdate = 0f;
    [SerializeField] protected BossState state;
    [SerializeField] protected GameObject currentTarget;
    private bool bossEncounterStarted = false;

    public enum BossState 
    {
        Idle,
        Combat,
        CombatLostVisual
    }

    //this in the boss specific class when updating its AI
    protected void BaseUpdateAI() 
    {
        DetermineState();
    }

    private void DetermineState()
    {
        //note: VERY simple for now, testing stuff...
        GameObject target = GameObject.Find("Player");
        if (Vector3.Distance(gameObject.transform.position, target.transform.position) < 25) 
        {
            bossEncounterStarted = true;
            state = BossState.Combat;
        }
        else if(bossEncounterStarted) //for now just stay in combat mode if started
        {
             state = BossState.Combat;
        }
    }

}
