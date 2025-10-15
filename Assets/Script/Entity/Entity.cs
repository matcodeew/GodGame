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
    public virtual bool Initialize()
    {
        if (TryGetComponent<NavMeshAgent>(out NavMeshAgent NavAgent))
        {
            agent = NavAgent;
            agent.updateRotation = false;
        }
        return true;
    }
    public void GoTo(Vector2 target)
    {
        agent.isStopped = false;
        Vector3 pos = new Vector3(target.x, transform.position.y, target.y);
        agent.SetDestination(pos);
    }
}
