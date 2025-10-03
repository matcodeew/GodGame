using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    private List<TaskRequest> taskQueue = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddTask(TaskType type, int priority)
    {
        TaskRequest newTask = new TaskRequest { taskType = type, priority = priority };
        taskQueue.Add(newTask);

        // Tri par priorité
        taskQueue.Sort((a, b) => a.priority.CompareTo(b.priority));
    }

    public TaskRequest GetNextTask()
    {
        if (taskQueue.Count == 0) return null;

        TaskRequest task = taskQueue[0];
        taskQueue.RemoveAt(0);
        return task;
    }
}
