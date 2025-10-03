using UnityEngine;

public class Three : InteractibleStructure
{
    [SerializeField] private int maxWoods;
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
        //Give one Wood To Villagers
        //villager.AddRessource(RessourceType.Woods);
    }

    public override void DestroyStructure()
    {
        base.DestroyStructure();
    }
}
