using UnityEngine;
using UnityEngine.AI;

namespace GodGame
{
    public class HouseBuilderManager : MonoBehaviour
    {
        [Header("House Settings")]
        private GameObject housePrefab;
        [SerializeField] private float buildCheckInterval = 5f;
        [SerializeField] private int woodCostPerHouse = 20;
        [SerializeField] private float minDistanceBetweenHouses = 8f;
        [SerializeField] private float buildRadiusMin = 5f;
        [SerializeField] private float buildRadiusMax = 10f;

        private City city;
        private float nextBuildCheckTime = 0f;

        private void Awake()
        {
            city = GetComponent<City>();
            housePrefab = GameManager.Instance.House.prefab;
        }

        private void Update()
        {
            if (Time.time > nextBuildCheckTime)
            {
                TryBuildHouse();
                nextBuildCheckTime = Time.time + buildCheckInterval;
            }
        }

        public House TryBuildHouse()
        {
            // Conditions minimales
            if (city.AllCitizen.Count < 2) return null;
            if (city.cityStats.currentWoods < woodCostPerHouse) return null;
            if(city.cityStats.currentCityzen < city.cityStats.maxPopulation) return null;


            Vector3? validPos = FindValidHousePosition();
            if (validPos.HasValue)
            {
                return BuildHouse(validPos.Value);
            }
            return null;
        }

        private Vector3? FindValidHousePosition()
        {
            for (int i = 0; i < 10; i++)
            {
                float radius = Random.Range(buildRadiusMin, buildRadiusMax);
                Vector2 randomCircle = Random.insideUnitCircle * radius;
                Vector3 potentialPos = city.transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

                bool tooClose = false;
                if (city.houses != null && city.houses.Count > 0)
                {
                    foreach (var h in city.houses)
                    {
                        if (Vector3.Distance(h.transform.position, potentialPos) < minDistanceBetweenHouses)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                }

                if (!tooClose)
                    return potentialPos;
            }

            return null;
        }

        private House BuildHouse(Vector3 position)
        {
            if(housePrefab == null) return null;

            city.cityStats.currentWoods -= woodCostPerHouse;

            var go = Instantiate(housePrefab, position, Quaternion.identity, GameManager.Instance.House.parent);
            if (go.TryGetComponent(out House house))
            {
                //GameManager.Instance.navMesh.BuildNavMesh();
                city.houses.Add(house);
                house.Initialize(city);
                city.cityStats.maxPopulation += 5;
                return house;   
            }
            return null;
        }
    }
}
