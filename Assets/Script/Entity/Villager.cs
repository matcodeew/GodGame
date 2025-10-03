using System;
using System.Collections.Generic;
using UnityEngine;


public class Villager : Entity
{
    public TaskType currentCityTask = TaskType.NONE;
    public TaskType currentPersonalTask = TaskType.NONE;
    private TaskType interruptedTasks = TaskType.NONE;

    public override bool Initialize()
    {
        agent.speed = entityStats.speed.GetCurrentSpeed();
        return base.Initialize();
    }
   
    private void Update()
    {
        UpdateFullnessValue();
        UpdateTirednessValue();
    }

    #region UpdateStats
    private void UpdateTaskByFullness()
    {
        if (entityStats.hanger.GetCurrentFullness() >= entityStats.hanger.GetMaxFullness() / 2 && currentCityTask == TaskType.NONE)
        {
            currentPersonalTask = TaskType.Eat;

            //Take Food //civilisation.SetFood(-1);
            //GoTo(FoodStorage.location);


            entityStats.hanger.SetCurrentFullness(0);


            TimerManager.StartTimer(2.5f, new Action(() => currentPersonalTask = TaskType.NONE));
        }
        else if (entityStats.hanger.GetCurrentFullness() >=
                entityStats.hanger.GetMaxFullness() - entityStats.hanger.GetMaxFullness() / 4 && currentCityTask != TaskType.NONE)
        {
            interruptedTasks = currentCityTask;
            currentCityTask = TaskType.NONE;
            currentPersonalTask = TaskType.Eat;

            //Stop currentTask
            //Take Food //civilisation.SetFood(-1);
            //GoTo(FoodStorage.location);

            entityStats.hanger.SetCurrentFullness(0);

            TimerManager.StartTimer(2.5f, new Action(() =>
            {
                currentCityTask = interruptedTasks;
                interruptedTasks = TaskType.NONE;
            }));
        }
    }
    public void UpdateFullnessValue()
    {
        if (currentPersonalTask == TaskType.Eat || currentPersonalTask == TaskType.Sleep) return;

        if (entityStats.hanger.IsAnger())
        {
            print($"Entity {entityStats.GetEntityName()}, is Anger");
            entityStats.hanger.SetCurrentFullness(0);
        }
        entityStats.hanger.SetCurrentFullness(
            Time.deltaTime * entityStats.hanger.GetFullnessTimeRate() + entityStats.hanger.GetCurrentFullness() * 1/*GameManager.tickSpeed*/);

        UpdateTaskByFullness();
    }

    public void UpdateTirednessValue()
    {
        if (currentPersonalTask != TaskType.Sleep)
        {
            entityStats.tiredness.SetCurrentTiredness(
          Time.deltaTime * entityStats.tiredness.GetTirednessTimeRate() + entityStats.tiredness.GetCurrentTiredness() * 1/*GameManager.tickSpeed*/);

            if (entityStats.tiredness.IsTired())
            {
                interruptedTasks = currentCityTask;
                currentCityTask = TaskType.NONE;
                currentPersonalTask = TaskType.Sleep;

                TimerManager.StartTimer(10.0f /*GameManager.DayTotalTime*/, new Action(() =>
                {
                    entityStats.tiredness.SetCurrentTiredness(0);
                    currentPersonalTask = TaskType.NONE;
                }));
            }
            UpdateSpeedWithTiredness();
        }
    }

    private void UpdateSpeedWithTiredness()
    {
        float currentTired = entityStats.tiredness.GetCurrentTiredness();
        float maxTired = entityStats.tiredness.GetMaxTiredness();

        float tirednessRatio = Mathf.Clamp01(currentTired / maxTired);

        float newSpeed = Mathf.Lerp(
            entityStats.speed.GetMaxSpeed(),
            entityStats.speed.GetMinSpeed(),
            tirednessRatio
        );

        entityStats.speed.SetCurrentSpeed(newSpeed);
    }
    #endregion
}