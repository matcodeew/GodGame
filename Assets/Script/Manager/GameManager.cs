using System.Collections.Generic;
using UnityEngine;

namespace GodGame
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        public float TaskMaxTime = 15f;

        [Header("Ressource Lists")]
        public readonly List<RessourceWoods> AllTree = new();
        public readonly List<RessourceFoodBush> AllFoodBush = new();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            RessourceLocator.Initialize();
            RegisterAllRessourcesInScene();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<InteractibleStructure>(EventType.DestroyInteractibleStruct, RemoveRessourceSpot);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InteractibleStructure>(EventType.DestroyInteractibleStruct, RemoveRessourceSpot);
        }

        private void RegisterAllRessourcesInScene()
        {
            AllTree.AddRange(FindObjectsByType<RessourceWoods>(FindObjectsSortMode.InstanceID));
            AllFoodBush.AddRange(FindObjectsByType<RessourceFoodBush>(FindObjectsSortMode.InstanceID));
        }

        public void AddNewTree(RessourceWoods tree)
        {
            if (!AllTree.Contains(tree))
                AllTree.Add(tree);
        }

        public void AddNewFoodBush(RessourceFoodBush bush)
        {
            if (!AllFoodBush.Contains(bush))
                AllFoodBush.Add(bush);
        }

        public void RemoveRessourceSpot(InteractibleStructure ressource)
        {
            switch (ressource)
            {
                case RessourceWoods wood:
                    AllTree.Remove(wood);
                    break;
                case RessourceFoodBush bush:
                    AllFoodBush.Remove(bush);
                    break;
            }
        }
    }
}
