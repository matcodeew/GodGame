using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UpdateStatsComponent), typeof(NavMeshAgent), typeof(VillagerRessourceManager))]
public class Villager : Entity
{
    [Header("TASK STATES")]
    public TaskType currentCityTask = TaskType.NONE;
    public TaskType currentPersonalTask = TaskType.NONE;
    [HideInInspector] public TaskType interruptedTasks = TaskType.NONE;

    //[Header("INTERNAL STATES")]
    private enum VillagerState { Idle, MovingToTask, Interacting, Pausing }
    private VillagerState currentState = VillagerState.Idle;

    [Header("SETTINGS")]
    [SerializeField] private float wanderRadius = 15f;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private float minPauseTime = 0.0f;
    [SerializeField] private float maxPauseTime;



    [Header("COMPONENTS")]
    private VillagerRessourceManager ressourceManager;
    private City city;

    [Header("DATA")]
    private GameObject currentTarget;
    private Coroutine interactionRoutine;

    // --- INITIALIZATION ---
    public override bool Initialize(City city)
    {
        base.Initialize(city);

        agent = GetComponent<NavMeshAgent>();
        ressourceManager = GetComponent<VillagerRessourceManager>();
        this.city = city;

        agent.speed = entityStats.speed.GetCurrentSpeed();

        return true;
    }


    private void Update()
    {
        HandleTaskAssignment();
        UpdateState();
    }

    // --------------------
    // STATE MACHINE
    // --------------------
    private void UpdateState()
    {
        switch (currentState)
        {
            case VillagerState.Idle:
                HandleIdle();
                break;

            case VillagerState.MovingToTask:
                HandleMovement();
                break;

            case VillagerState.Interacting:
                // Interaction gérée par coroutine
                break;

            case VillagerState.Pausing:
                // Pause après tâche — rien ici, coroutine gère
                break;
        }
    }

    // --- IDLE ---
    private void HandleIdle()
    {
        if (currentCityTask == TaskType.NONE && currentPersonalTask != TaskType.MakeBaby)
        {
            WanderRandomly();
        }
        else
        {
            currentState = VillagerState.MovingToTask;
        }
    }

    private void WanderRandomly()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            Vector3 randomDir = Random.insideUnitSphere * wanderRadius + transform.position;
            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    // --- MOVEMENT ---
    private void HandleMovement()
    {
        if (currentTarget == null)
        {
            currentTarget = FindClosestTarget(currentCityTask);
            if (currentTarget == null)
            {
                // Aucune ressource dispo, reset task
                currentCityTask = TaskType.NONE;
                currentState = VillagerState.Idle;
                return;
            }
            agent.SetDestination(currentTarget.transform.position);
        }

        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (dist <= interactionDistance)
        {
            agent.isStopped = true;
            currentState = VillagerState.Interacting;
            StartInteraction();
        }
    }

    // --- INTERACTION ---
    private void StartInteraction()
    {
        if (interactionRoutine != null) StopCoroutine(interactionRoutine);

        interactionRoutine = StartCoroutine(InteractionRoutine());
    }

    private IEnumerator InteractionRoutine()
    {
        if (currentTarget.TryGetComponent(out InteractibleStructure structure))
        {
            structure.Interact(this);
        }

        yield return new WaitUntil(() => ressourceManager.CantPickOtherRessource());


        yield return PauseAfterTask(Random.Range((minPauseTime), maxPauseTime));
    }

    private IEnumerator PauseAfterTask(float pauseTime)
    {
        currentState = VillagerState.Pausing;

        Vector3 pauseDestination = transform.position + Random.insideUnitSphere * 2f;
        pauseDestination.y = transform.position.y;

        agent.isStopped = false;
        agent.SetDestination(pauseDestination);

        while (Vector3.Distance(transform.position, pauseDestination) > 0.2f)
            yield return null;

        agent.isStopped = true;
        yield return new WaitForSeconds(pauseTime);

        ResetTask();
    }

    private void ResetTask()
    {
        agent.isStopped = false;
        currentTarget = null;
        currentCityTask = TaskType.NONE;
        ressourceManager.ClearRessources();
        currentState = VillagerState.Idle;
    }

    // --- TASK HANDLING ---
    private void HandleTaskAssignment()
    {
        if (currentCityTask == TaskType.NONE && currentPersonalTask == TaskType.NONE && TaskManager.Instance?.HasTasks == true)
        {
            if (currentPersonalTask != TaskType.MakeBaby && currentPersonalTask != TaskType.MakeBaby)
            {
                TaskType taskType = TaskManager.Instance.GetNextTask().taskType;
                currentCityTask = taskType;
            }
        }
    }


    // --- TARGET SELECTION ---
    private GameObject FindClosestTarget(TaskType type)
    {
        RessourceType resType = type switch
        {
            TaskType.GatherWood => RessourceType.Wood,
            TaskType.GatherFood => RessourceType.Food,
            _ => RessourceType.None,
        };

        List<InteractibleStructure> ressources = RessourceLocator.GetRessources(resType);
        if (ressources == null || ressources.Count == 0) return null;


        InteractibleStructure closest = null;
        float minDist = float.MaxValue;

        foreach (var res in ressources)
        {
            if (res == null) continue;
            float dist = Vector3.Distance(transform.position, res.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = res;
            }
        }

        RessourceLocator.UnBind(closest, closest.type);
        return closest?.gameObject;
    }

    // --- API EXTERNE ---
    public void AddRessource(RessourceType type, int quantity)
    {
        ressourceManager.AddRessource(type, quantity);
    }

    public void NotifyResourceDepleted()
    {
        if (interactionRoutine != null)
            StopCoroutine(interactionRoutine);

        StartCoroutine(PauseAfterTask(1f));
    }

    //// ---- MakeBaby -------

    //public void AssignMakeBabyTask(Vector3 meetingPoint)
    //{
    //    currentPersonalTask = TaskType.MakeBaby;
    //    currentCityTask = TaskType.MakeBaby;
    //    currentState = VillagerState.MovingToTask;
    //    agent.SetDestination(meetingPoint);
    //}

    //public bool IsAvailableForTask()
    //{
    //    return currentCityTask == TaskType.NONE;
    //}

    //public bool IsAtDestination(Vector3 point)
    //{
    //    return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 1.2f;
    //}

    //public void OnChildBorn()
    //{
    //    currentPersonalTask = TaskType.NONE;
    //    currentCityTask = TaskType.NONE;
    //    currentState = VillagerState.Idle;
    //    agent.isStopped = false;
    //}

}
