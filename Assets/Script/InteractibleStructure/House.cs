using System.Collections.Generic;
using UnityEngine;

namespace GodGame
{
    public class House : MonoBehaviour
    {
        public City parentCity;
        public List<Villager> occupants = new();
        public int maxVillagerOnHouse = 5;

        public void Initialize(City city)
        {
            parentCity = city;
            AssignVillagers();
        }

        private void AssignVillagers()
        {
            var freeVillagers = GetUnhousedVillagers();
            if (freeVillagers.Count == 0) return;

            int nbToAssign = freeVillagers.Count;
            for (int i = 0; i < nbToAssign; i++)
            {
                var v = freeVillagers[Random.Range(0, freeVillagers.Count)];
                v.AssignHouse(this);
                occupants.Add(v);
                freeVillagers.Remove(v);
            }
        }

        public void AddVillagerIntoHouse(Villager villager)
        {
            if(occupants.Contains(villager) && occupants.Count >= maxVillagerOnHouse) return;

            occupants.Add(villager);
        }

        private List<Villager> GetUnhousedVillagers()
        {
            List<Villager> free = new();
            foreach (var v in parentCity.AllCitizen)
            {
                if (v.AssignedHouse == null)
                    free.Add(v);
            }
            return free;
        }
    }
}
