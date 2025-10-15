using GodGame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[System.Serializable]
public struct OrderImage
{
    public Sprite FoodOrderSprite;
    public Sprite WoodOrderSprite;
    public Sprite EatOrderSprite;
    public Sprite SleepOrderSprite;
    public Sprite ReproduceOrderSprite;
}

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

    [Header("Order Image Ref")]
    [SerializeField] private GameObject orderImageContainer;
    [SerializeField] private float orderImageTime;
    [SerializeField] private OrderImage orderImage;

    [Header("DeathImage")]
    [SerializeField] private GameObject DeathImageObject;
    [SerializeField] private Animator DeathGif;

    private string UpdateOrderImageTimerID;
    private bool isDead = false;


    private bool isFrozen = false;
    private bool isSick = false;



    [Header("Villager Skin possible")]
    [SerializeField] private Image SkinContainer;
    [SerializeField] private List<Sprite> skins = new();
    [SerializeField] private Sprite defaultSkin;

    [Header("Movement")]
    [SerializeField] private Animator VillagerAnimator;


    // --- INITIALIZATION ---
    public override bool Initialize()
    {
        SkinContainer.sprite = skins.Count > 0? skins[UnityEngine.Random.Range(0, skins.Count)] : defaultSkin is not null ? defaultSkin : null;
        base.Initialize();

        agent = GetComponent<NavMeshAgent>();
        ressourceManager = GetComponent<VillagerRessourceManager>();

        agent.speed = entityStats.speed.GetCurrentSpeed();
        FindOrCreateNewCity();

        return true;
    }


    private void Update()
    {
        if (isDead || isFrozen || isSick) return;

        StartWalkAnimation();

        if (AssignedCity != null)
        {
            HandleTaskAssignment();
        }
        UpdateState();
    }

    private void StartWalkAnimation()
    {
        if (agent.velocity.sqrMagnitude > 0.1f && isDead == false && isFrozen == false)
        {
            VillagerAnimator.SetBool("IsWalking", true);
        }
        else
        {
            VillagerAnimator.SetBool("IsWalking", false);
        }
    }


    public void ApplyFreeze(float duration)
    {
        if (isDead || isFrozen) return;

        StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        isFrozen = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(duration);

        agent.isStopped = false;
        isFrozen = false;
    }

    public void ApplySickness(float duration)
    {
        if (isDead || isSick) return;

        StartCoroutine(SicknessRoutine(duration));
    }

    private IEnumerator SicknessRoutine(float duration)
    {
        isSick = true;
        float elapsed = 0f;
        float tickInterval = 1f;
        int iteration = Mathf.FloorToInt(duration / tickInterval);
        while (elapsed < duration)
        {
            entityStats.health.SetCurrentHelth(Mathf.Clamp(entityStats.health.GetCurrentHelth() - entityStats.health.GetMaxHelth() / iteration, 0, entityStats.health.GetMaxHelth()));
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }
        if (entityStats.health.GetCurrentHelth() <= 0)
        {
            VillagerDeath();
        }
        isSick = false;
    }








    public void VillagerDeath()
    {
        //----Stop Movement
        isDead = true;

        if (agent is not null)
        {
            agent.isStopped = true;
        }

        currentCityTask = TaskType.NONE;
        currentPersonalTask = TaskType.NONE;



        CancelOrderImage();

        DeathImageObject.SetActive(true);

        TimerManager.StartTimer(2.5f, new Action(() =>
        {
            DeathImageObject.SetActive(false);
            AssignedHouse?.UnAssignVillagers(this);
            EventBus.Publish<int>(EventType.UPDATE_UI_NbsCitizen, --AssignedCity.cityStats.currentCityzen);

            AssignedCity?.AllCitizen.Remove(this);
            Destroy(gameObject);
        }));


    }

    public void CancelOrderImage()
    {
        orderImageContainer.SetActive(false);
        TimerManager.CancelTimer(UpdateOrderImageTimerID);
    }

    public void AssignNewOrderImage()
    {
        if (isDead) return;

        orderImageContainer.transform.GetChild(0).GetComponent<Image>().sprite = GetImageByTask();
        orderImageContainer.SetActive(true);

        UpdateOrderImageTimerID = TimerManager.StartTimer(orderImageTime, new Action(() => orderImageContainer?.SetActive(false)));
    }

    private Sprite GetImageByTask()
    {
        switch (currentCityTask)
        {
            case (TaskType.GatherWood): return orderImage.WoodOrderSprite;
            case (TaskType.GatherFood): return orderImage.FoodOrderSprite;
            case (TaskType.MakeBaby): return orderImage.ReproduceOrderSprite;
        }
        switch (currentPersonalTask)
        {
            case (TaskType.Eat): return orderImage.EatOrderSprite;
            case (TaskType.Sleep): return orderImage.SleepOrderSprite;
            case (TaskType.MakeBaby): return orderImage.ReproduceOrderSprite;
        }
        return null;
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
            Vector3 randomDir = UnityEngine.Random.insideUnitSphere * wanderRadius + transform.position;
            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                GoTo(new Vector2(hit.position.x, hit.position.z));
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


        yield return PauseAfterTask(UnityEngine.Random.Range((minPauseTime), maxPauseTime));
    }

    private IEnumerator PauseAfterTask(float pauseTime)
    {
        currentState = VillagerState.Pausing;

        Vector3 pauseDestination = transform.position + UnityEngine.Random.insideUnitSphere * 2f;
        pauseDestination.y = transform.position.y;

        agent.isStopped = false;
        GoTo(new Vector2(pauseDestination.x, pauseDestination.z));

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
                AssignNewOrderImage();
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

        StartCoroutine(PauseAfterTask(UnityEngine.Random.Range((minPauseTime), maxPauseTime)));
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
