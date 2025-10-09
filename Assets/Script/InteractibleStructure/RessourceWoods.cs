using System.Collections;
using UnityEngine;

public class RessourceWoods : InteractibleStructure
{
    [SerializeField] private int totalRessources = 15;
    [SerializeField] private float gatherInterval = 1.5f;

    private bool isBeingHarvested = false;

    public override void Interact(Villager villager)
    {
        base.Interact(villager);

        if (!isBeingHarvested && totalRessources > 0)
        {
            StartCoroutine(HarvestLoop(villager));
        }
    }

    private IEnumerator HarvestLoop(Villager villager)
    {
        isBeingHarvested = true;

        while (totalRessources > 0 && villager != null /* && !villager.RessourceIsFull()*/)
        {
            yield return new WaitForSeconds(gatherInterval);

            totalRessources--;
            villager.AddRessource(RessourceType.Wood, 1);
        }

        isBeingHarvested = false;

        if (totalRessources <= 0)
        {
            villager.NotifyResourceDepleted();
            DestroyStructure();
        }
    }
}
