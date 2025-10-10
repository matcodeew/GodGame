using System;
using UnityEngine;

public class UpdateStatsComponent : MonoBehaviour
{
    private Villager villager;

    [SerializeField] private float eatingTime;
    [SerializeField] private float SpeepingTime;

    private void Awake()
    {
        villager = GetComponent<Villager>();
    }

    private void Update()
    {
        UpdateFullnessValue();
        UpdateTirednessValue();
    }

    #region UpdateStats
    private void UpdateTaskByAnger()
    {
        if (villager.entityStats.hanger.GetCurrentFullness() >= villager.entityStats.hanger.GetMaxFullness() / 2 && villager.currentCityTask == TaskType.NONE)
        {
            villager.currentPersonalTask = TaskType.Eat;

            //Take Food //civilisation.SetFood(-1);
            //GoTo(FoodStorage.location);


            villager.entityStats.hanger.SetCurrentFullness(0);


            TimerManager.StartTimer(eatingTime, new Action(() => villager.currentPersonalTask = TaskType.NONE));
        }
        else if (villager.entityStats.hanger.GetCurrentFullness() >=
                villager.entityStats.hanger.GetMaxFullness() - villager.entityStats.hanger.GetMaxFullness() / 4 && villager.currentCityTask != TaskType.NONE)
        {
            villager.interruptedTasks = villager.currentCityTask;
            villager.currentCityTask = TaskType.NONE;
            villager.currentPersonalTask = TaskType.Eat;

            //Stop currentTask
            //Take Food //civilisation.SetFood(-1);
            //GoTo(FoodStorage.location);

            villager.entityStats.hanger.SetCurrentFullness(0);

            TimerManager.StartTimer(eatingTime, new Action(() =>
            {
                villager.currentCityTask = villager.interruptedTasks;
                villager.interruptedTasks = TaskType.NONE;
            }));
        }
    }
    public void UpdateFullnessValue()
    {
        if (villager.currentPersonalTask == TaskType.Eat || villager.currentPersonalTask == TaskType.Sleep) return;

        if (villager.entityStats.hanger.IsAnger())
        {
            print($"Entity {villager.entityStats.GetEntityName()}, is Anger");
            villager.entityStats.hanger.SetCurrentFullness(0);
        }
        villager.entityStats.hanger.SetCurrentFullness(
            Time.deltaTime * villager.entityStats.hanger.GetFullnessTimeRate() + villager.entityStats.hanger.GetCurrentFullness() * 1/*GameManager.tickSpeed*/);

        UpdateTaskByAnger();
    }

    public void UpdateTirednessValue()
    {
        if (villager.currentPersonalTask != TaskType.Sleep)
        {
            villager.entityStats.tiredness.SetCurrentTiredness(
          Time.deltaTime * villager.entityStats.tiredness.GetTirednessTimeRate() + villager.entityStats.tiredness.GetCurrentTiredness() * 1/*GameManager.tickSpeed*/);

            if (villager.entityStats.tiredness.IsTired())
            {
                villager.interruptedTasks = villager.currentCityTask;
                villager.currentCityTask = TaskType.NONE;
                villager.currentPersonalTask = TaskType.Sleep;

                TimerManager.StartTimer(SpeepingTime /*GameManager.DayTotalTime*/, new Action(() =>
                {
                    villager.entityStats.tiredness.SetCurrentTiredness(0);
                    villager.currentPersonalTask = TaskType.NONE;
                }));
            }
            UpdateSpeedWithTiredness();
        }
    }

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