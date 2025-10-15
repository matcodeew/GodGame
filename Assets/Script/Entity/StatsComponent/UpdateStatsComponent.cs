using System;
using UnityEngine;

[RequireComponent(typeof(Villager))]
public class UpdateStatsComponent : MonoBehaviour
{
    private Villager villager;

    [Header("TIMERS")]
    [SerializeField] private float eatingTime = 5f;
    [SerializeField] private float sleepingTime = 10f;
    [SerializeField] private float LifeTimeSecond = /*10 * 60f*/ 10; //10minutes

    private void Awake()
    {
        villager = GetComponent<Villager>();
    }

    private void Start()
    {
        TimerManager.StartTimer(LifeTimeSecond, new Action(() => villager.VillagerDeath())); //programme Death
    }
    private void Update()
    {
        UpdateFullnessValue();
        UpdateTirednessValue();
    }

    #region Fullness (Hunger)
    private void UpdateFullnessValue()
    {
        if (villager.currentPersonalTask == TaskType.Eat || villager.currentPersonalTask == TaskType.Sleep)
            return;

        villager.entityStats.hanger.SetCurrentFullness(
            villager.entityStats.hanger.GetCurrentFullness() +
            Time.deltaTime * villager.entityStats.hanger.GetFullnessTimeRate()
        );

        if (villager.entityStats.hanger.GetCurrentFullness() >= villager.entityStats.hanger.GetMaxFullness() / 2f)
        {
            villager.currentPersonalTask = TaskType.Eat;
            HandleEating();
        }
    }

    private void HandleEating()
    {
        if (villager.AssignedCity == null) return;

        villager.AssignedCity.cityStats.currentFoods = Mathf.Max(0, villager.AssignedCity.cityStats.currentFoods - 5);

        villager.GoHomeToEat(eatingTime);
    }
    #endregion

    #region Tiredness (Sleep)
    private void UpdateTirednessValue()
    {
        if (villager.currentPersonalTask == TaskType.Sleep)
            return;

        villager.entityStats.tiredness.SetCurrentTiredness(
            villager.entityStats.tiredness.GetCurrentTiredness() +
            Time.deltaTime * villager.entityStats.tiredness.GetTirednessTimeRate()
        );

        if (villager.entityStats.tiredness.IsTired())
        {
            HandleSleeping();
        }

        UpdateSpeedWithTiredness();
    }

    private void HandleSleeping()
    {
        if (villager.AssignedHouse == null) return;

        villager.interruptedTasks = villager.currentCityTask;
        villager.currentCityTask = TaskType.NONE;
        villager.currentPersonalTask = TaskType.Sleep;

        villager.GoTo(new Vector2(
            villager.AssignedHouse.transform.position.x,
            villager.AssignedHouse.transform.position.z
        ));

        TimerManager.StartTimer(sleepingTime, new Action(() =>
        {
            villager.entityStats.tiredness.SetCurrentTiredness(0);
            villager.currentPersonalTask = TaskType.NONE;
            villager.ResumePreviousTask();
        }));
    }
    #endregion

    #region Speed Adjustment
    private void UpdateSpeedWithTiredness()
    {
        float currentTired = villager.entityStats.tiredness.GetCurrentTiredness();
        float maxTired = villager.entityStats.tiredness.GetMaxTiredness();
        float tirednessRatio = Mathf.Clamp01(currentTired / maxTired);

        float newSpeed = Mathf.Lerp(
            villager.entityStats.speed.GetMaxSpeed(),
            villager.entityStats.speed.GetMinSpeed(),
            tirednessRatio
        );

        villager.entityStats.speed.SetCurrentSpeed(newSpeed);
    }
    #endregion
}
