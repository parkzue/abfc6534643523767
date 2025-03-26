using UnityEngine;

[System.Serializable]
public class BossCombatAI
{
    private Transform bossTransform;
    [SerializeField] private BossFightingStyle bossFightingStyle;
    private Player currentTarget;

    private enum BossFightingStyle 
    {
        melee,
        range
    }

    public BossCombatAI(Transform transformRef)
    {
        bossTransform = transformRef;
    }

    public void UpdateCombatAI() 
    {
        if (bossTransform == null) 
        {
            Debug.Log("Missing transform for boss, the guy doesn't know where he is... combatAI unable to function!");
            return;
        }

        //For now, since we don't have targeting system, don't find() with every update in future
        currentTarget = Object.FindFirstObjectByType<Player>();
        if (currentTarget == null) return;

        if (Vector3.Distance(bossTransform.position, currentTarget.GetPlayerPosition()) > 10)
        {
            bossFightingStyle = BossFightingStyle.range;
        }
        else
        {
            bossFightingStyle = BossFightingStyle.melee;
        }
    }
}
