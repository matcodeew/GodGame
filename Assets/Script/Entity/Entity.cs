using UnityEngine;
using UnityEngine.AI;

public class Entity : MonoBehaviour
{
    public BaseStatsComponents entityStats;
    public NavMeshAgent agent;

    private void Awake()
    {
        //Initialize();
    }
    public virtual bool Initialize(City city)
    {
        if (TryGetComponent<NavMeshAgent>(out NavMeshAgent NavAgent))
        {
            agent = NavAgent;
        }
        print($"Initialize {entityStats.GetEntityName()} ");
        return true;
    }
    public void GoTo(Vector3 target)
    {
        agent.SetDestination(target);
    }
}
