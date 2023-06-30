using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reproduction : ScriptableObject
{
    System.Random rand = new System.Random();

    public GameObject animalPrefab;
    public GameObject animalParent;

    public Material maleColor;
    public Material femaleColor;

    int mutationPercentage = 25;

    public void CreateBabyAnimals(int minLitterSize, int maxLitterSize, Vector3 motherPosition, AnimalBehavior father, AnimalBehavior mother)
    {
        System.Random rand = new System.Random();

        AnimalBehavior animalTraits;
        int numOfBabies = rand.Next(minLitterSize, maxLitterSize);

        for (int i = 0; i < numOfBabies; i++)
        {
            var newBaby = Instantiate(
                animalPrefab,
                motherPosition,
                Quaternion.identity);

            Global.animalCount++;
            newBaby.name = "Animal1_" + Global.animalCount;
            animalTraits = newBaby.GetComponent<AnimalBehavior>();
            animalTraits.Setup(true);

            if (animalTraits.sex == 0)
            {
                newBaby.GetComponent<MeshRenderer>().material = maleColor;
                newBaby.transform.Find("hat").GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                newBaby.GetComponent<MeshRenderer>().material = femaleColor;
            }

            CalculateTraits(animalTraits, mother, father);

            newBaby.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            newBaby.transform.parent = animalParent.transform;
            newBaby.transform.Find("OrganismInfoCanvas").GetComponent<OrganismInfoUI>().isVisible = false;
        }
    }

    private void CalculateTraits(AnimalBehavior babyTraits, AnimalBehavior mother, AnimalBehavior father)
    {
        babyTraits.lifespan = PickParentForGenes(mother,father).lifespan * Mutation();
        babyTraits.viewDistance = PickParentForGenes(mother, father).viewDistance * Mutation();
        babyTraits.idleSpeed = PickParentForGenes(mother, father).idleSpeed * Mutation();
        babyTraits.searchSpeed = PickParentForGenes(mother, father).searchSpeed * Mutation();
    }

    private AnimalBehavior PickParentForGenes(AnimalBehavior mother, AnimalBehavior father)
    {
        int p = rand.Next(0, 2);
        if (p == 0)
        {
            return mother;
        }
        else
        {
            return father;
        }
    }

    private float Mutation()
    {
        float n = rand.Next(100-mutationPercentage, 100+mutationPercentage);
        return n / 100;
    }
}
