using UnityEngine;

[System.Serializable]
public struct HangerComponent
{
    [SerializeField] private float CurrentHanger;
    [SerializeField] private float MaxHanger;
    [SerializeField] private float HangerTimeRate;
    HangerComponent(float currentFullness, float fullnessTimeRate, float maxFullness = 100.0f)
    {
        CurrentHanger = currentFullness;
        MaxHanger = maxFullness;
        HangerTimeRate = fullnessTimeRate;
    }

    //FullnessTimeRate Get/Set
    public float GetFullnessTimeRate() => HangerTimeRate;
    public void SetFullnessTimeRate(float newFullnessTimeRate) => HangerTimeRate = newFullnessTimeRate;


    //CurrentFullness Get/Set
    public float GetCurrentFullness() => CurrentHanger;
    public void SetCurrentFullness(float newCurrentFullness) => CurrentHanger = newCurrentFullness;


    //MaxFullness Get/Set
    public float GetMaxFullness() => MaxHanger;
    public void SetMaxFullness(float newMaxFullness) => MaxHanger = newMaxFullness;

    public bool IsAnger() => CurrentHanger >= MaxHanger;
}
