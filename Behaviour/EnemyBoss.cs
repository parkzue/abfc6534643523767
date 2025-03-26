using UnityEngine;

public class EnemyBoss : BossBehaviour
{
    private Transform characterTransform;
    [SerializeField] private BossCombatAI combatAI;
    [SerializeField] private bool fightStarted = false;
 
    private void UpdateAI() 
    {
        if (state == BossState.Combat && fightStarted == false)
        {
            Debug.Log("Starting boss combat AI");
            this.combatAI = new BossCombatAI(characterTransform);
            fightStarted = true;
        }
        else if (state == BossState.Combat && fightStarted == true) 
        {
            combatAI.UpdateCombatAI();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        //note: experimenting with time based updates to save performance later, need to analyze further
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= aiUpdateInterval)
        {
            BaseUpdateAI();
            UpdateAI();
            timeSinceLastUpdate = 0f;
        }
    }
}
