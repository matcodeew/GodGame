using System;
using System.Collections;
using UnityEngine;

public class Tree : InteractibleStructure
{
    [SerializeField] private int maxWoods;
    [SerializeField] private RessourceType ressourceType = RessourceType.Wood;
    private int currentUsedWoods;

    public override void Interact(Villager villager)
    {
        base.Interact(villager);

        if (currentUsedWoods >= maxWoods)
        {
            DestroyStructure();
            return;
        }

        currentUsedWoods++;

        villager.AddRessource(ressourceType);
    }

    public override void DestroyStructure()
    {
        base.DestroyStructure();
    }
}
