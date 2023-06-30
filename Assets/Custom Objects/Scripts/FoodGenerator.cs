using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;

public class FoodGenerator : MonoBehaviour
{
    public Terrain spawnArea;

    System.Random rand = new System.Random();

    GameObject foodParent; 
    public GameObject foodPrefab; 
    int maxFoodPerTick;
    int minFoodPerTick;

    int foodSpawnAreaSize;
    int foodLimit;

    int foodCount = 0;


    void Start()
    {
        foodParent = GameObject.Find("Food");

        maxFoodPerTick = 100;
        minFoodPerTick = 15;
        foodSpawnAreaSize = (int) spawnArea.terrainData.size.x;
        foodLimit = 5000;

        InvokeRepeating("AddFoods", 0, Global.tickRate);
    }

    void Update()
    {
    }

    private void AddFoods()
    {
        int numOfFoods = rand.Next(minFoodPerTick, maxFoodPerTick);

        if (foodParent.transform.childCount + numOfFoods >= foodLimit)
        {
            numOfFoods = 0;
        }

        for (int i = 0; i < numOfFoods; i++)
        {
            Vector3 newPosition = new(0, 0, 0);
            while (true)
            {
                
                newPosition = new Vector3(rand.Next(0, foodSpawnAreaSize), 0 , rand.Next(0, foodSpawnAreaSize));
                var y = Terrain.activeTerrain.SampleHeight(newPosition);
                newPosition.y = y;
                if (!IsSpawnOverlapping(newPosition) && !IsOffNavMesh(newPosition))
                {
                    break;
                }
                //break;
            }

            var newFood = Instantiate(
                foodPrefab,
                newPosition,
                Quaternion.identity);

            foodCount++;
            newFood.name = "Food_" + foodCount;
            newFood.transform.parent = foodParent.transform;
        }
    }

    bool IsSpawnOverlapping(Vector3 newFoodPosition)
    {
        Collider[] hitColliders = Physics.OverlapSphere(newFoodPosition, 2);
        // there will always be the collision with the terrain
        if (hitColliders.Length > 1)
        {
            return true;
        }
        return false;
    }

    bool IsOffNavMesh(Vector3 newFoodPosition)
    {
        NavMeshHit hit;

        // Check for nearest point on navmesh to agent, within onMeshThreshold
        if (NavMesh.SamplePosition(newFoodPosition, out hit, 1f, 1))
        {
            // Check if the positions are vertically aligned
            if (Mathf.Approximately(newFoodPosition.x, hit.position.x)
                && Mathf.Approximately(newFoodPosition.z, hit.position.z))
            {
                return false;
            }
        }

        return true;
    }
}
