using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.AI;

public class AnimalGeneration : MonoBehaviour
{
    public Terrain spawnArea;

    System.Random rand = new System.Random();
    public GameObject animalPrefab;
    GameObject animalParent;

    int maxAnimals;
    int minAnimals;

    int animalSpawnAreaSize;

    public Material maleColor;
    public Material femaleColor;

    // Start is called before the first frame update
    void Start()
    {
        animalParent = GameObject.Find("Animals");

        animalSpawnAreaSize = (int) spawnArea.terrainData.size.x;

        CreateStartingAnimals();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateStartingAnimals()
    {
        AnimalBehavior animalTraits;
        //int numOfAnimals = rand.Next(minAnimals, maxAnimals);
        //int numOfAnimals = 1;
        int numOfAnimals = 200;

        for (int i = 0; i < numOfAnimals; i++)
        {
            Vector3 newPosition = new(0,0,0);
            while (true)
            {
                newPosition = new Vector3(rand.Next(0, animalSpawnAreaSize), 0, rand.Next(0, animalSpawnAreaSize));
                var y = Terrain.activeTerrain.SampleHeight(newPosition);
                newPosition.y = y+2;
                if (!IsSpawnOverlapping(newPosition) /*&& !IsOffNavMesh(newPosition)*/)
                {
                    break;
                }
            }

            var newAnimal = Instantiate(
                animalPrefab,
                newPosition,
                Quaternion.identity);

            Global.animalCount++;
            newAnimal.name = "Animal1_" + Global.animalCount;
            newAnimal.transform.parent = animalParent.transform;

            animalTraits = newAnimal.GetComponent<AnimalBehavior>();
            animalTraits.Setup(false);

            if (animalTraits.sex == 0)
            {
                newAnimal.GetComponent<MeshRenderer>().material = maleColor;
                newAnimal.transform.Find("hat").GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                newAnimal.GetComponent<MeshRenderer>().material = femaleColor;
            }

            if (animalTraits.age < animalTraits.maturityAge)
            {
                newAnimal.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
        }
    }

    bool IsSpawnOverlapping(Vector3 newAnimalPosition)
    {
        Collider[] hitColliders = Physics.OverlapSphere(newAnimalPosition, 2);
        // there will always be the collision with the plane
        if (hitColliders.Length > 1)
        {
            return true;
        }
        return false;
    }

    bool IsOffNavMesh(Vector3 newAnimalPosition)
    {
        NavMeshHit hit;

        // Check for nearest point on navmesh to agent, within onMeshThreshold
        if (NavMesh.SamplePosition(newAnimalPosition, out hit, 1f, 1))
        {
            // Check if the positions are vertically aligned
            if (Mathf.Approximately(newAnimalPosition.x, hit.position.x)
                && Mathf.Approximately(newAnimalPosition.z, hit.position.z))
            {
                return false;
            }
        }

        return true;
    }
}
