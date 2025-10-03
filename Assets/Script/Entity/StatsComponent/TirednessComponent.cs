using UnityEngine;

[System.Serializable]
public struct TirednessComponent
{
    [SerializeField] private float CurrentTiredness;
    [SerializeField] private float MaxTiredness;
    [SerializeField] private float TirednessTimeRate;

    TirednessComponent(float currentTiredness, float tirednessTimeRate, float maxTiredness = 100.0f)
    {
        CurrentTiredness = currentTiredness;
        MaxTiredness = maxTiredness;
        TirednessTimeRate = tirednessTimeRate;
    }

    //FullnessTimeRate Get/Set
    public float GetTirednessTimeRate() => TirednessTimeRate;
    public void SetTirednessTimeRate(float newTirednessTimeRate) => TirednessTimeRate = newTirednessTimeRate;


    //CurrentFullness Get/Set
    public float GetCurrentTiredness() => CurrentTiredness;
    public void SetCurrentTiredness(float newCurrentTiredness) => CurrentTiredness = newCurrentTiredness;


    //MaxFullness Get/Set
    public float GetMaxTiredness() => MaxTiredness;
    public void SetMaxTiredness(float newMaxTiredness) => MaxTiredness = newMaxTiredness;

    public bool IsTired() => CurrentTiredness >= MaxTiredness;
}