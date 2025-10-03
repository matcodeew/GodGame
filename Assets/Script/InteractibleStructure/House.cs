using System.Collections.Generic;

public class House : InteractibleStructure, IBuildable
{
    private List<Villager> resident = new();
    private int maxPlace = 5;



    public bool AsPlace() => resident.Count > maxPlace;
    public void AddResident(Villager villager)
    {
        if (resident.Contains(villager) && AsPlace()) return;

        resident.Add(villager);
    }

    public void RemoveResident(Villager villager)
    {
        if (!resident.Contains(villager)) return;

        resident.Remove(villager);
    }

    public override void Interact(Villager villager)
    {
        base.Interact(villager);
        for (int i = 0; i < resident.Count; i++)
        {
            print($"resident n{i} is {resident[i].name}");
        }
    }

    public void Build()
    {
        BuildSfx();
        throw new System.NotImplementedException();
    }

    public void BuildSfx()
    {
        throw new System.NotImplementedException();
    }
}
