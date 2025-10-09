using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CityStatsComponents
{
    public int nbsVillagers;

    public int currentFoods;
    public int maxFoods;

    public int currentWoods;
    public int maxWoods;

    public int currentFaith;
    public int maxFaith;
}

public class City : MonoBehaviour
{
    public string civilizationName = string.Empty;

    public List<Villager> AllCitizen = new();

    public CityStatsComponents cityStats;

    public List<Villager> villagersMakingBaby = new List<Villager>();

    [Header("Born Param")]
    [SerializeField, Range(0, 1)] private float RessourceRatioNeeded;
    public float BornTime = 10.0f;

    [Header("DEBUG")]
    [SerializeField] private Villager villagerPrefab;


    City(string civilizationName, CityStatsComponents cityStats)
    {
        this.civilizationName = civilizationName;
        this.cityStats = cityStats;
    }

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<RessourceType>(EventType.VillagerTakeRessource, UpdateCityRessource);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<RessourceType>(EventType.VillagerTakeRessource, UpdateCityRessource);
    }

    public void Initialize()
    {
        cityStats.nbsVillagers = AllCitizen.Count;
        cityStats.currentFoods = 50;
        cityStats.maxFoods = 100;
        cityStats.currentWoods = 50;
        cityStats.maxWoods = 100;
        cityStats.currentFaith = 0;
    }

    [ContextMenu("TEST_SpawnVillager")]

    public void TEST_SpawnVillager()
    {
        Villager newVillager = Instantiate(villagerPrefab);
        newVillager.Initialize(this);
        AddCitizen(newVillager);
    }

    public void AddCitizen(Villager villager)
    {
        if (villager is null) return;
        if (AllCitizen.Contains(villager)) return;

        AllCitizen.Add(villager);
        UpdateNbsCitizen();
    }

    public void RemoveCitizen(Villager villager)
    {
        if (villager is null) return;
        if (!AllCitizen.Contains(villager)) return;

        AllCitizen.Remove(villager);
        UpdateNbsCitizen();
    }

    private void Update()
    {
        CheckOrder();
    }


    private void PostResourceTask(int current, int max, TaskType taskType)
    {
        if (max <= 0) return;

        float ratio = (float)current / max;

        if (ratio < 0.25f)
            TaskManager.Instance.AddTask(taskType, 0);
        else if (ratio < 0.5f)
            TaskManager.Instance.AddTask(taskType, 1);
        else if (ratio < 0.75f)
            TaskManager.Instance.AddTask(taskType, 2);
    }


    private void UpdateNbsCitizen()
    {
        cityStats.nbsVillagers = AllCitizen.Count;
        EventBus.Publish(EventType.UPDATE_UINbsCitizen, cityStats.nbsVillagers);
    }

    public void UpdateCityRessource(RessourceType type)
    {
        switch (type)
        {
            case (RessourceType.Food): cityStats.currentFoods++; break;
            case (RessourceType.Wood): cityStats.currentWoods++; break;
            default: return;
        }
    }


    private void CheckOrder()
    {
        if (CanHaveReproduceTask())
        {
            TaskManager.Instance.AddTask(TaskType.MakeBaby, 5);
        }
        PostResourceTask(cityStats.currentFoods, cityStats.maxFoods, TaskType.GatherFood);
        PostResourceTask(cityStats.currentWoods, cityStats.maxWoods, TaskType.GatherWood);
        PostResourceTask(cityStats.currentFaith, cityStats.maxFaith, TaskType.Pray);
    }

    private bool CanHaveReproduceTask()
    {
        bool haveEnoughFoods = cityStats.currentFoods / cityStats.maxFoods >= RessourceRatioNeeded;
        bool haveEnoughWoods = cityStats.currentWoods / cityStats.maxWoods >= RessourceRatioNeeded;

        // bool hasEnoughSpace = city.CurrentPopulation < city.MaxPopulation;
        return haveEnoughFoods && haveEnoughWoods;
    }

    public void SpawnNewVillager(Villager parentA, Villager parentB)
    {
        Vector3 spawnPos = (parentA.transform.position + parentB.transform.position) / 2f + Random.insideUnitSphere * 2f;
        spawnPos.y = 0;

        Villager newVillager = Instantiate(villagerPrefab, spawnPos, Quaternion.identity);
        newVillager.Initialize(this);
        AddCitizen(newVillager);
    }
}