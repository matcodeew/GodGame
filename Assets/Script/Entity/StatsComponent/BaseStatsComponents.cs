using UnityEngine;


[System.Serializable]
public struct BaseStatsComponents
{
    [SerializeField] private string entityName;
    public string GetEntityName() => entityName;

    //Health

    [SerializeField] public HealthComponent health;

    //Speed

    [SerializeField] public SpeedComponent speed;

    //Hanger

    [SerializeField] public HangerComponent hanger;

    //tiredness
    [SerializeField] public TirednessComponent tiredness;
}