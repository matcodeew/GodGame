using System.Collections.Generic;
using UnityEngine;

public class VillagerRessourceManager : MonoBehaviour
{
    public Dictionary<RessourceType, int> villagerRessource = new();
    [SerializeField] private int maxRessourceQuantity = 15;

    private RessourceType currentType;

    private void Awake()
    {
        villagerRessource.Add(RessourceType.Wood, 0);
        villagerRessource.Add(RessourceType.Food, 0);
    }

    public void AddRessource(RessourceType type, int quantity)
    {
        currentType = type;
        if (!villagerRessource.ContainsKey(type)) return;
        if (CantPickOtherRessource()) return;

        villagerRessource[type] += quantity;
        EventBus.Publish(EventType.VillagerTakeRessource, type);
    }


    public void ClearRessources()
    {
        villagerRessource[RessourceType.Wood] = 0;
        villagerRessource[RessourceType.Food] = 0;
    }
    public bool CantPickOtherRessource() => villagerRessource[currentType] >= maxRessourceQuantity;
}
