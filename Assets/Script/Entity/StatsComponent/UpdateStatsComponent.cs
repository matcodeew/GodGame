using System;
using UnityEngine;

public class UpdateStatsComponent : MonoBehaviour
{
    private Villager villager;

    [Header("Timers")]
    [SerializeField] private float eatingDuration = 5f;
    [SerializeField] private float sleepingDuration = 10f;

    private void Awake()
    {
        villager = GetComponent<Villager>();
    }

    private void Update()
    {
        UpdateFullness();
        UpdateTiredness();
    }

    #region Fullness / Hunger
    private void UpdateFullness()
    {
        // Ignore si le villageois est déjà en train de manger ou dormir
        if (villager.currentPersonalTask == TaskType.Eat || villager.currentPersonalTask == TaskType.Sleep)
            return;

        var hunger = villager.entityStats.hanger;

        // Met à jour la faim progressivement
        hunger.SetCurrentFullness(
            hunger.GetCurrentFullness() + Time.deltaTime * hunger.GetFullnessTimeRate()
        );

        // Si il a faim, on déclenche le comportement associé
        if (hunger.IsAnger())
        {
            Debug.Log($"?? {villager.entityStats.GetEntityName()} a faim !");
            hunger.SetCurrentFullness(0);
        }

        HandleEatingBehavior();
    }

    private void HandleEatingBehavior()
    {
        var hunger = villager.entityStats.hanger;

        bool hasCityTask = villager.currentCityTask != TaskType.NONE;

        // Seuils de faim
        float halfFullness = hunger.GetMaxFullness() / 2f;
        float almostFull = hunger.GetMaxFullness() * 0.75f;

        // Pas de tâche = mange tranquille
        if (hunger.GetCurrentFullness() >= halfFullness && !hasCityTask)
        {
            StartEating(() => villager.currentPersonalTask = TaskType.NONE);
        }
        // En tâche active = interrompt pour manger
        else if (hunger.GetCurrentFullness() >= almostFull && hasCityTask)
        {
            TaskType oldTask = villager.currentCityTask;

            villager.interruptedTasks = oldTask;
            villager.currentCityTask = TaskType.NONE;

            StartEating(() =>
            {
                villager.currentPersonalTask = TaskType.NONE;
                villager.currentCityTask = oldTask;
                villager.interruptedTasks = TaskType.NONE;
            });
        }
    }

    private void StartEating(Action onFinish)
    {
        villager.currentPersonalTask = TaskType.Eat;
        villager.GoTo(Vector3.zero); // TODO : changer vers un vrai point "table / maison"
        villager.entityStats.hanger.SetCurrentFullness(0);

        TimerManager.StartTimer(eatingDuration, onFinish);
    }
    #endregion

    #region Tiredness
    private void UpdateTiredness()
    {
        // Empêche la fatigue de se mettre à jour pendant le sommeil ou la reproduction
        if (villager.currentPersonalTask == TaskType.Sleep || villager.currentCityTask == TaskType.MakeBaby)
            return;

        var tired = villager.entityStats.tiredness;

        // Met à jour la fatigue progressivement
        tired.SetCurrentTiredness(
            tired.GetCurrentTiredness() + Time.deltaTime * tired.GetTirednessTimeRate()
        );

        if (tired.IsTired())
        {
            villager.interruptedTasks = villager.currentCityTask;
            villager.currentCityTask = TaskType.NONE;
            villager.currentPersonalTask = TaskType.Sleep;

            villager.GoTo(Vector3.zero); // TODO : lit ou maison

            TimerManager.StartTimer(sleepingDuration, new Action(() =>
            {
                tired.SetCurrentTiredness(0);
                villager.currentPersonalTask = TaskType.NONE;
            }));
        }

        UpdateSpeedBasedOnTiredness();
    }
    private void UpdateSpeedBasedOnTiredness()
    {
        var tired = villager.entityStats.tiredness;
        var speed = villager.entityStats.speed;

        float ratio = Mathf.Clamp01(tired.GetCurrentTiredness() / tired.GetMaxTiredness());
        float newSpeed = Mathf.Lerp(speed.GetMaxSpeed(), speed.GetMinSpeed(), ratio);

        speed.SetCurrentSpeed(newSpeed);
    }
    #endregion
}
