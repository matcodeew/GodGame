using UnityEngine;
using UnityEngine.AI;

public class Entity : MonoBehaviour
{
    public BaseStatsComponents entityStats;
    public NavMeshAgent agent;

    private void Awake()
    {
        if (TryGetComponent<NavMeshAgent>(out NavMeshAgent NavAgent))
        {
            agent = NavAgent;
        }
        Initialize();
    }
    public virtual bool Initialize()
    {
        print($"Initialize {entityStats.GetEntityName()} ");
        return true;
    }
    public void GoTo(Vector3 target)
    {
        agent.SetDestination(target);
    }
}
