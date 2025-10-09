using System.Collections.Generic;
using Unity;
using UnityEngine;

public static class RessourceLocator
{
    private static Dictionary<RessourceType, List<InteractibleStructure>> ressources = new();

    public static void Initialize()
    {
        ressources[RessourceType.Wood] = new List<InteractibleStructure>();
        ressources[RessourceType.Food] = new List<InteractibleStructure>();
    }

    public static List<InteractibleStructure> GetRessources(RessourceType type)
    {
        if (ressources.TryGetValue(type, out List<InteractibleStructure> list))
            return list;

        return new List<InteractibleStructure>();
    }

    public static void Bind(InteractibleStructure structure, RessourceType type)
    {
        if (structure == null || !ressources.ContainsKey(type))
            return;

        if (!ressources[type].Contains(structure))
            ressources[type].Add(structure);
        Debug.Log($"{type} : {ressources[type].Count}");
    }

    public static void UnBind(InteractibleStructure structure, RessourceType type)
    {
        if (structure == null || !ressources.ContainsKey(type))
            return;
        ressources[type].Remove(structure);
    }
}
