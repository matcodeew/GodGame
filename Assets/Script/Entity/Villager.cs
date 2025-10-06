using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UpdateStatsComponent), typeof(NavMeshAgent))]
public class Villager : Entity
{
    public TaskType currentCityTask = TaskType.NONE;
    public TaskType currentPersonalTask = TaskType.NONE;
    [HideInInspector] public TaskType interruptedTasks = TaskType.NONE;

    [SerializeField, Range(0, 10)] private float takeOtherOrderTime = 1.5f;
    private float deltaTime;

    public override bool Initialize()
    {
        agent.speed = entityStats.speed.GetCurrentSpeed();
        return base.Initialize();
    }

    private void Start()
    {
        if (TaskManager.Instance is null) throw new ArgumentNullException(nameof(TaskManager.Instance), "Instance of TaskManager is NULL");
    }

    private void Update()
    {
        deltaTime += Time.deltaTime;
        if (deltaTime >= takeOtherOrderTime)
        {
            deltaTime = 0.0f;
            if (currentCityTask == TaskType.NONE && TaskManager.Instance.HaveTask())
            {
                currentCityTask = TaskManager.Instance.GetNextTask().taskType;
            }
        }
    }
}