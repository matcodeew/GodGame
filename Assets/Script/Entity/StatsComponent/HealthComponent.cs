using UnityEngine;

[System.Serializable]
public struct HealthComponent
{
    [SerializeField] private float CurrentHealth;
    [SerializeField] private float MaxHealth;
    HealthComponent(float currentHealth, float maxHealth)
    {
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
    }

    //CurrentHealth Get/Set
    public float GetCurrentHelth() => CurrentHealth;
    public void SetCurrentHelth(float newCurrentHealth) => CurrentHealth = newCurrentHealth;


    //MaxHealth Get/Set
    public float GetMaxHelth() => MaxHealth;
    public void SetMaxHelth(float newMaxHealth) => MaxHealth = newMaxHealth;


    public bool IsAlive() => CurrentHealth > MaxHealth;
}