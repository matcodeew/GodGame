using UnityEngine;

[System.Serializable]
public struct SpeedComponent
{
    [SerializeField] private float CurrentMovementSpeed;
    [SerializeField] private float MaxMovementSpeed;
    [SerializeField] private float MinMovementSpeed;
    SpeedComponent(float currentMovementSpeed, float maxMovementSpeed, float minMovementSpeed)
    {
        CurrentMovementSpeed = currentMovementSpeed;
        MaxMovementSpeed = maxMovementSpeed;
        MinMovementSpeed = minMovementSpeed;
    }

    //CurrentSpeed Get/Set
    public float GetCurrentSpeed() => CurrentMovementSpeed;
    public void SetCurrentSpeed(float newCurrentSpeed) => CurrentMovementSpeed = newCurrentSpeed;

    //MaxSpeed Get/Set
    public float GetMaxSpeed() => MaxMovementSpeed;
    public void SetMaxSpeed(float newMaxSpeed) => MaxMovementSpeed = newMaxSpeed;

    //MinSpeed Get/Set
    public float GetMinSpeed() => MinMovementSpeed;
    public void SetMinSpeed(float newMinSpeed) => MinMovementSpeed = newMinSpeed;
}
