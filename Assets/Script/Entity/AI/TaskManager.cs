using GodGame;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    [SerializeField] private List<TaskRequest> taskQueue = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddTask(TaskType type, int priority)
    {
        if (!CanAddTask(type))
        {
            RemoveTasksOfType(type);
            return;
        }

        if (taskQueue.Exists(t => t.taskType == type))
            return;

        taskQueue.Add(new TaskRequest(type, priority));
        taskQueue.Sort((a, b) => a.priority.CompareTo(b.priority));
    }

    private bool CanAddTask(TaskType type)
    {
        return type switch
        {
            TaskType.GatherWood => GameManager.Instance.AllTree?.Count > 0,
            TaskType.GatherFood => GameManager.Instance.AllFoodBush?.Count > 0,
            TaskType.Pray => true,
            TaskType.MakeBaby => true,
            _ => false
        };
    }

    private void RemoveTasksOfType(TaskType type)
    {
        taskQueue.RemoveAll(t => t.taskType == type);
    }

    public bool HasTasks => taskQueue.Count > 0;

    public TaskRequest GetNextTask()
    {
        if (taskQueue.Count == 0) return null;

        TaskRequest task = taskQueue[0];
        taskQueue.RemoveAt(0);
        return task;
    }
}
