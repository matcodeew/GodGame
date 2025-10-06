using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;

    [SerializeField] private List<TaskRequest> taskQueue = new();

    private void Awake()
    {
        if (Instance is null) Instance = this;
    }

    public void AddTask(TaskType type, int priority)
    {
        TaskRequest newTask = new TaskRequest(type, priority);

        if (taskQueue.Count > 0)
        {
            foreach (var task in taskQueue)
            {
                if (task.taskType == newTask.taskType)
                {
                    return;
                }
                continue;
            }
        }
        taskQueue.Add(newTask);


        taskQueue.Sort((a, b) => a.priority.CompareTo(b.priority));

        EventBus.Publish(EventType.NewTaskCreated, newTask);
    }

    public bool HaveTask() => taskQueue.Count > 0;

    public TaskRequest GetNextTask()
    {
        if (taskQueue.Count == 0) return null;

        TaskRequest task = taskQueue[0];
        taskQueue.RemoveAt(0);
        return task;
    }
}
