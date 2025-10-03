using System;
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
    public List<Villager> AllCitizen = new();
    public List<Villager> freeCitizen = new();

    private float orderTime = 5.0f;
    private float elapseTime;

    public CityStatsComponents cityStats;

    public void Initialize()
    {
        cityStats.nbsVillagers = AllCitizen.Count;
        cityStats.currentFoods = 10;
        cityStats.currentWoods = 10;
        cityStats.currentFaith = 0;
    }

    public void AddCitizen(Villager villager)
    {
        if (AllCitizen.Contains(villager)) return;
        AllCitizen.Add(villager);
        UpdateNbsCitizen();
    }

    public void RemoveCitizen(Villager villager)
    {
        if (!AllCitizen.Contains(villager)) return;
        AllCitizen.Remove(villager);
        UpdateNbsCitizen();
    }

    private void CheckOrder()
    {
        if (elapseTime > 0)
        {
            elapseTime -= Time.deltaTime;
            return;
        }

        elapseTime = orderTime;

        PostResourceTask(cityStats.currentFoods, cityStats.maxFoods, TaskType.GatherFood);
        PostResourceTask(cityStats.currentWoods, cityStats.maxWoods, TaskType.GatherWood);
        PostResourceTask(cityStats.currentFaith, cityStats.maxFaith, TaskType.Pray);
    }

    private void PostResourceTask(int current, int max, TaskType taskType)
    {
        if (max <= 0) return;

        float ratio = (float)current / max;

        if (ratio < 0.25f)
            TaskManager.Instance.AddTask(taskType, 0); // priorité haute
        else if (ratio < 0.5f)
            TaskManager.Instance.AddTask(taskType, 1);
        else if (ratio < 0.75f)
            TaskManager.Instance.AddTask(taskType, 2);
    }

    public void ReturnVillagerToFreeCitizen(Villager returnVillager)
    {
        if (!freeCitizen.Contains(returnVillager))
            freeCitizen.Add(returnVillager);
    }

    private void UpdateNbsCitizen()
    {
        cityStats.nbsVillagers = AllCitizen.Count;
        EventBus.Publish(EventType.UPDATE_UINbsCitizen, cityStats.nbsVillagers);
    }
}
