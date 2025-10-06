using UnityEngine;


[System.Serializable]
public class TaskRequest
{
    public TaskRequest(TaskType _type = TaskType.NONE, int _priority = -1)
    {
        taskType = _type;
        priority = _priority;
    }

    public TaskType taskType;
    public int priority;
}
