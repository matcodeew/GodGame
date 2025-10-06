using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GodGame
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public List<InteractibleStructure> AllTree = new();

        private void OnEnable()
        {
            EventBus.Subscribe<InteractibleStructure>(EventType.DestroyInteractibleStruct, OnStructureWasDestroyed);
        }
        private void OnDisable()
        {
            EventBus.Unsubscribe<InteractibleStructure>(EventType.DestroyInteractibleStruct, OnStructureWasDestroyed);
        }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
        private void OnStructureWasDestroyed(InteractibleStructure structure)
        {
            //if (structure is Tree)
            //{
            //    if (AllTree.Contains(structure))
            //    {
            //        AllTree.Remove(structure);
            //    }
            //}
        }
        public List<InteractibleStructure> GetSortedTrees(Vector2 villagerPos)
        {
            return AllTree.OrderBy(tree => Vector2.Distance(villagerPos, tree.buildingWorldPos)).ToList();
        }

        public InteractibleStructure GetNearestTree(Vector2 villagerPos)
        {
            return AllTree.OrderBy(tree => Vector2.Distance(villagerPos, tree.buildingWorldPos)).FirstOrDefault();
        }
    }
}