using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    private static Dictionary<string, Coroutine> _activeTimers = new Dictionary<string, Coroutine>();

    #region Timer Helper
    private class TimerHelper : MonoBehaviour { }
    private static TimerHelper _helper;
    private static void EnsureHelperExists()
    {
        if (_helper == null)
        {
            GameObject obj = new GameObject("GlobalTimerHelper");
            _helper = obj.AddComponent<TimerHelper>();
            DontDestroyOnLoad(obj);
        }
    }
    #endregion

    #region Generic Timer

    #region summary
    /// <summary>
    /// Starts a new timer that executes a callback function after a specified duration.
    /// Supports multiple parameters dynamically.
    /// </summary>
    /// <param name="duration">The duration in seconds before the callback is triggered.</param>
    /// <param name="callback">The function to execute when the timer completes.</param>
    /// <param name="parameters">Optional parameters to pass to the callback.</param>
    /// <returns>A unique string identifier for the timer.</returns>
    #endregion
    public static string StartTimer(float duration, Delegate callback, params object[] parameters) //call the timer without a parameter function
    {
        if (duration < 0) throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        if (callback == null) throw new ArgumentNullException(nameof(callback));

        EnsureHelperExists();
        string timerId = Guid.NewGuid().ToString(); //Randomize a string ID
        Coroutine routine = _helper.StartCoroutine(TimerCoroutineWithoutParam(duration, callback, parameters, timerId));

        _activeTimers[timerId] = routine; //Add this timer to the activeTimer dictionary
        return timerId;
    }
    private static IEnumerator TimerCoroutineWithoutParam(float duration, Delegate callback, object[] parameters, string timerId)
    {
        yield return new WaitForSeconds(duration);
        callback?.DynamicInvoke(parameters);
        _activeTimers.Remove(timerId);
    }
    #endregion

    #region Cancel Timer
    public static void CancelTimer(string timerId) //Cancel Timer with ID 
    {
        if (_activeTimers.TryGetValue(timerId, out Coroutine coroutine))
        {
            _helper.StopCoroutine(coroutine);
            _activeTimers.Remove(timerId);
        }
        else
        {
            Debug.LogWarning($"Timer with ID {timerId} not found.");
        }
    }
    public static void CancelAllTimer() //Cancel all Active Timer 
    {
        foreach (var timer in _activeTimers.Values)
        {
            _helper.StopCoroutine(timer);
        }
        _activeTimers.Clear();
    }
    #endregion
}