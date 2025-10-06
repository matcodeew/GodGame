using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UpdateStatsComponent), typeof(NavMeshAgent))]
public class Villager : Entity
{
    public TaskType currentCityTask = TaskType.NONE;
    public TaskType currentPersonalTask = TaskType.NONE;
    [HideInInspector] public TaskType interruptedTasks = TaskType.NONE;

    public override bool Initialize()
    {
        agent.speed = entityStats.speed.GetCurrentSpeed();
        return base.Initialize();
    }

    #region Init Event
    private void OnEnable()
    {
        EventBus.Subscribe<TaskRequest>(EventType.NewTaskCreated, AttributeCityTask);
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe<TaskRequest>(EventType.NewTaskCreated, AttributeCityTask);
    }
    #endregion
    private void AttributeCityTask(TaskRequest request)
    {
        if (request == null || currentCityTask != TaskType.NONE) return;

        currentCityTask = TaskManager.Instance.GetNextTask().taskType;
    }
}