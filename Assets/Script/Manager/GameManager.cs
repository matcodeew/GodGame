using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace GodGame
{

    [System.Serializable]
    public struct SpawnablePrefab
    {
        public GameObject prefab;
        public Transform parent;
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        public float TaskMaxTime = 15f;

        [Header("Ressource Lists")]
        public List<RessourceWoods> AllTree = new();
        public List<RessourceFoodBush> AllFoodBush = new();

        [Header(" City Building List")]
        public Dictionary<Vector3, City> cities = new();

        [Header("Spawn Prefab")]

        [SerializeField] public SpawnablePrefab Villager;

        [SerializeField] private SpawnablePrefab City;
        [SerializeField] public SpawnablePrefab House;

        [SerializeField] private SpawnablePrefab Tree;
        [SerializeField] private SpawnablePrefab FoodBush;
        [SerializeField] private SpawnablePrefab Bush;
        [SerializeField] private SpawnablePrefab Rock;

        [SerializeField] private LayerMask placementLayers;


        [SerializeField] public NavMeshSurface navMesh;

        private GameObject PrefabToInstantiate;
        private Transform prefabParent;
        private bool isPlacingBuilding = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            RessourceLocator.Initialize();
            RegisterAllRessourcesInScene();

            navMesh.BuildNavMesh();

        }

        private void OnEnable()
        {
            EventBus.Subscribe<InteractibleStructure>(EventType.DestroyInteractibleStruct, RemoveRessourceSpot);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InteractibleStructure>(EventType.DestroyInteractibleStruct, RemoveRessourceSpot);
        }


        public Dictionary<Vector3, City> GetAllCity() => cities;
        public List<Vector3> GetAllCityPos()
        {
            List<Vector3> citiesPos = new List<Vector3>();
            foreach (var pos in cities.Keys)
            {
                if (citiesPos.Contains(pos)) continue;

                citiesPos.Add(pos);
            }
            return citiesPos;
        }
        public City GetCityByPos(Vector3 pos) => cities[pos];

        public City GetNearestCity(Vector3 position, float maxDistance = 50f)
        {
            City nearest = null;
            float minDist = Mathf.Infinity;

            foreach (var kvp in cities)
            {
                float dist = Vector3.Distance(position, kvp.Key);
                if (dist < minDist && dist <= maxDistance)
                {
                    minDist = dist;
                    nearest = kvp.Value;
                }
            }

            return nearest;
        }

        public City CreateNewCity(Vector3 position)
        {
            if (cities.ContainsKey(position))
                return cities[position];

            if (Instantiate(City.prefab, position, Quaternion.identity).TryGetComponent(out City city))
            {
                cities.Add(position, city);
                //navMesh.BuildNavMesh();
                return city;
            }

            return null;
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

        // ==== CHOIX DU PREFAB À PLACER ====
        public void SpawnVillager() => SetPrefab(Villager.prefab, Villager.parent);
        public void SpawnTree() => SetPrefab(Tree.prefab, Tree.parent);
        public void SpawnRock() => SetPrefab(Rock.prefab, Rock.parent);
        public void SpawnFoodBush() => SetPrefab(FoodBush.prefab, FoodBush.parent);
        public void SpawnBush() => SetPrefab(Bush.prefab, Bush.parent);

        private void SetPrefab(GameObject prefab, Transform parent)
        {
            PrefabToInstantiate = prefab;
            prefabParent = parent;
            isPlacingBuilding = true;
        }

        private void Update()
        {
            if (!isPlacingBuilding) return;

            if (Input.GetMouseButtonDown(0))
                TryPlaceBuilding();
            else if (Input.GetMouseButtonDown(1))
                CancelPlacement();
        }

        private void TryPlaceBuilding()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            print($"try ray");


            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementLayers))
            {
                print($"hit at {hit.point}");
                GameObject newObject = Instantiate(PrefabToInstantiate,
                    hit.point /*+ new Vector3(0, PrefabToInstantiate.transform.localScale.y / 2, 0)*/, Quaternion.identity, prefabParent);

                if (newObject != null)
                {
                    print($"spawn Object");

                    if (newObject.TryGetComponent(out Villager villager))
                    {
                        villager.Initialize();
                    }
                    else if (newObject.TryGetComponent(out InteractibleStructure ressource))
                    {
                        if (ressource is RessourceWoods woods)
                        {
                            AllTree.Add(woods);
                        }
                        else if (ressource is RessourceFoodBush foodBush)
                        {
                            AllFoodBush.Add(foodBush);
                        }
                        navMesh.BuildNavMesh();
                    }
                }
            }
            isPlacingBuilding = false;
            PrefabToInstantiate = null;
        }


        private void CancelPlacement()
        {
            isPlacingBuilding = false;
            PrefabToInstantiate = null;
        }
    }
}
