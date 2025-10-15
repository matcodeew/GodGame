using GodGame;
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
    [SerializeField] private enum VillagerState { Idle, MovingToTask, Interacting, Pausing }
    private VillagerState currentState = VillagerState.Idle;

    [Header("SETTINGS")]
    [SerializeField] private float wanderRadius = 15f;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField, Min(0)] private float minPauseTime;
    [SerializeField] private float maxPauseTime;

    [Header("COMPONENTS")]
    private VillagerRessourceManager ressourceManager;
    public City AssignedCity { get; private set; }
    public House AssignedHouse { get; private set; }


    [Header("DATA")]
    public GameObject currentTarget;
    private Coroutine interactionRoutine;

    // --- INITIALIZATION ---
    public override bool Initialize()
    {
        base.Initialize();

        agent = GetComponent<NavMeshAgent>();
        ressourceManager = GetComponent<VillagerRessourceManager>();

        agent.speed = entityStats.speed.GetCurrentSpeed();
        FindOrCreateNewCity();

        return true;
    }


    private void FindOrCreateNewCity()
    {
        var gm = GameManager.Instance;

        City nearestCity = gm.GetNearestCity(transform.position, 50f);

        if (nearestCity != null)
        {
            AssignedCity = nearestCity;
            nearestCity.AddCitizen(this);
            if (nearestCity.houses != null && nearestCity.houses.Count > 0)
            {
                AssignHouseOrCreateNewOne();
            }

        }
        else
        {
            Vector3 newCityPos = transform.position;
            AssignedCity = gm.CreateNewCity(newCityPos);
            AssignedCity.AddCitizen(this);
        }
    }


    private void AssignHouseOrCreateNewOne()
    {
        if (AssignedCity == null)
        {
            return;
        }

        if (AssignedCity.houses == null || AssignedCity.houses.Count == 0)
        {
            CreateNewHouse();
            return;
        }

        foreach (House house in AssignedCity.houses)
        {
            if (house == null) continue;

            if (house.occupants.Count < house.maxVillagerOnHouse)
            {
                house.AddVillagerIntoHouse(this);
                AssignedHouse = house;
                return; 
            }
        }
        CreateNewHouse();
    }

    private void CreateNewHouse()
    {
        AssignedCity.builderManager.TryBuildHouse();
    }


    public void AssignCity(City c)
    {
        AssignedCity = c;
    }

    public void AssignHouse(House h)
    {
        AssignedHouse = h;
    }



    private void Update()
    {
        if (AssignedCity != null)
        {
            HandleTaskAssignment();
        }
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
                break;

            case VillagerState.Pausing:
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
                GoTo(new Vector2(hit.position.x, hit.position.z));
                print("go To randomPosition");
            }
        }
    }

    // --- MOVEMENT ---
    private void HandleMovement()
    {
       if (agent.isStopped)
           agent.isStopped = false;    

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
            GoTo(new Vector2(currentTarget.transform.position.x, currentTarget.transform.position.z));
            print("Go to task position");
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
        GoTo(new Vector2(pauseDestination.x, pauseDestination.z));
        print("GoTo pause Destination");

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

            float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z),
                new Vector2(res.transform.position.x, res.transform.position.z));

            if (dist < minDist)
            {
                minDist = dist;
                closest = res;
            }
        }

        RessourceLocator.UnBind(closest, closest.type);
        return closest?.gameObject;
    }
    public void AddRessource(RessourceType type, int quantity)
    {
        ressourceManager.AddRessource(type, quantity);
    }

    public void NotifyResourceDepleted()
    {
        if (interactionRoutine != null)
            StopCoroutine(interactionRoutine);

        StartCoroutine(PauseAfterTask(Random.Range((minPauseTime), maxPauseTime)));
    }



    public void GoHomeToEat(float eatingTime)
    {
        if (AssignedHouse == null) return;

        interruptedTasks = currentCityTask;
        currentCityTask = TaskType.NONE;
        currentPersonalTask = TaskType.Eat;

        agent.isStopped = false;
        GoTo(new Vector2(AssignedHouse.transform.position.x, AssignedHouse.transform.position.z));

        StartCoroutine(EatRoutine(eatingTime));
    }

    private IEnumerator EatRoutine(float time)
    {
        currentState = VillagerState.Interacting;
        yield return new WaitForSeconds(time);

        currentPersonalTask = TaskType.NONE;
        entityStats.hanger.SetCurrentFullness(0);

        ResumePreviousTask();
    }

    public void ResumePreviousTask()
    {
        if (interruptedTasks != TaskType.NONE)
        {
            currentCityTask = interruptedTasks;
            interruptedTasks = TaskType.NONE;
            currentState = VillagerState.MovingToTask;
            currentPersonalTask = TaskType.NONE;
            agent.isStopped = false;

            if (currentTarget != null)
            {
                GoTo(new Vector2(currentTarget.transform.position.x, currentTarget.transform.position.z));
            }
        }
        else
        {
            currentState = VillagerState.Idle;
        }
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
