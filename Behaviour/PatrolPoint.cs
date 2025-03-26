using UnityEngine;

public class PatrolPoint : MonoBehaviour
{
    [SerializeField] private PatrolPoint next;
    private Transform pointTransform;

    public PatrolPoint GetNextPatrolPoint() 
    {
        PatrolPoint nextPoint;
        if (next != null)
        {
            nextPoint = next;
        }
        else 
        {
            nextPoint = this;
            Debug.Log("Didn't find next patrol point for: " + gameObject.name);
        }
        return nextPoint;
    }

    private void Start()
    {
        pointTransform = transform;
    }
}
