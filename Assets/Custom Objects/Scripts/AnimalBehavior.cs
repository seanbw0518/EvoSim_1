using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using Unity.MLAgents.Sensors;

public class AnimalBehavior : MonoBehaviour
{
    System.Random rand = new System.Random();

    Reproduction reproduction;
    OrganismInfoUI infoDisplay;
    NavMeshAgent agent;

    #region Scan Lists
    private List<GameObject> mates = new List<GameObject>();
    private List<Vector3> foodSources = new List<Vector3>();
    private Vector3 lastNearestWaterSource;
    #endregion

    public Material maleColor;
    public Material femaleColor;

    public GameObject corpsePrefab;
    public GameObject animalPrefab;

    RayPerceptionSensorComponent3D[] rayPercepts;

    #region Checkers, Counters
    private enum Action {Idle, GoingToFood, Eating, GoingToWater, Drinking, Breeding, GoingToBreed, Resting}
    private Action currentAction;

    private bool isAtWater;

    int pregnancyStatus;
    bool pregnant;
    public AnimalBehavior fatherTraits;

    bool isInfoDisplayed;
    #endregion

    #region Speeds
    public float idleSpeed;
    public float searchSpeed;
    #endregion

    #region Stamina
    public float maxStamina;

    private float idleStaminaRate;
    private float drinkingStaminaRate;
    private float breedingStaminaRate;
    private float pregnantStaminaRate;
    private float restingStaminaRate;
    private float eatingStaminaRate;
    private float birthingStaminaRate;
    private float searchingStaminaRate;

    private float staminaThreshold;
    #endregion

    #region Thirst
    public int maxThirst;

    private float idleThirstRate;
    private float drinkingThirstRate;
    private float breedingThirstRate;
    private float pregnantThirstRate;

    private float thirstThreshold;
    #endregion

    #region Hunger
    public int maxHunger;

    private float idleHungerRate;
    private float eatingHungerRate;
    private float breedingHungerRate;
    private float pregnantHungerRate;

    private float hungerThreshold;
    #endregion

    #region Aging
    private float ageRate;
    public int maturityAge;
    private int tooOldToBreedAge;
    public float lifespan;
    #endregion

    #region Reproduction
    private int pregnancyDuration;
    private int breedingCooldown;
    private int maxLitterSize;
    #endregion

    #region Primary Stats
    public float hunger;
    public float thirst;
    public float stamina;
    public float age;
    #endregion

    private int idleRange;
    public float viewDistance;

    // 0 is male, 1 is female
    public int sex;

    public void Setup(bool asBaby)
    {
        sex = rand.Next(0, 2);

        idleRange = 5;
        viewDistance = 70;

        #region Speeds
        idleSpeed = 0.5f;
        searchSpeed = 1.8f;
        #endregion

        #region Stamina
        maxStamina = 100;

        idleStaminaRate = idleSpeed * 0.5f;
        searchingStaminaRate = searchSpeed * 0.5f;
        drinkingStaminaRate = -10;
        eatingStaminaRate = -10;
        birthingStaminaRate = 60;
        breedingStaminaRate = 30;
        restingStaminaRate = -8;

        staminaThreshold = 0.6f;
        #endregion

        #region Thirst
        maxThirst = 100;

        idleThirstRate = 0.75f;
        drinkingThirstRate = -15;
        breedingThirstRate = idleThirstRate + 5f;
        pregnantThirstRate = idleThirstRate + 2f;

        thirstThreshold = 0.5f;
        #endregion

        #region Hunger
        maxHunger = 100;

        idleHungerRate = 0.4f;
        eatingHungerRate = -10;
        breedingHungerRate = idleHungerRate + 3.5f;
        pregnantHungerRate = idleHungerRate + 2f;

        hungerThreshold = 0.5f;
        #endregion

        #region Aging
        lifespan = 100;
        ageRate = 0.5f;
        maturityAge = 15;
        if (sex == 0) tooOldToBreedAge = 80;
        else tooOldToBreedAge = 60;
        #endregion

        #region Reproduction
        pregnancyDuration = 8;
        breedingCooldown = 0;

        pregnancyStatus = pregnancyDuration;
        pregnant = false;

        maxLitterSize = 5;
        #endregion

        #region Primary Stats
        if (asBaby)
        {
            stamina = maxStamina * 0.8f;
            hunger = maxHunger * 0.8f;
            thirst = maxThirst * 0.8f;
            age = 0;
        }
        else
        {
            stamina = maxStamina;
            hunger = maxHunger;
            thirst = maxThirst;
            age = rand.Next(15, 25);
        }
        #endregion
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        rayPercepts = gameObject.GetComponents<RayPerceptionSensorComponent3D>();
        foreach (RayPerceptionSensorComponent3D rayPercept in rayPercepts)
        {
            rayPercept.RayLength = viewDistance;
        }

        reproduction = (Reproduction)ScriptableObject.CreateInstance(nameof(Reproduction));
        reproduction.animalPrefab = animalPrefab;
        reproduction.animalParent = GameObject.Find("Animals");
        reproduction.maleColor = maleColor;
        reproduction.femaleColor = femaleColor;

        infoDisplay = gameObject.transform.Find("OrganismInfoCanvas").GetComponent<OrganismInfoUI>();
        currentAction = Action.Idle;
        agent.speed = idleSpeed;

        idleStaminaRate = idleSpeed * 0.5f;
        searchingStaminaRate = searchSpeed * 0.5f;

        InvokeRepeating("Tick", 0, Global.tickRate);
    }

    private void OnDrawGizmos()
    {
        if (currentAction == Action.Idle) Gizmos.color = Color.black;
        else if (currentAction == Action.Resting) Gizmos.color = Color.white;
        else if (currentAction == Action.GoingToFood || currentAction == Action.Eating) Gizmos.color = Color.red;
        else if (currentAction == Action.GoingToBreed || currentAction == Action.Breeding) Gizmos.color = Color.magenta;
        else if (currentAction == Action.GoingToWater || currentAction == Action.Drinking) Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(agent.destination, 2);
        Gizmos.DrawLine(transform.position,agent.destination);
    }

    private void Update()
    {
        // show stats in ui above head
        if (infoDisplay.isVisible) 
        {
            infoDisplay.organismName = name;
            infoDisplay.currentAction = currentAction.ToString();

            infoDisplay.pregnancyStatus = (int)((pregnancyStatus /pregnancyDuration) * 100);
            infoDisplay.age = (int)((age /lifespan) * 100);
            infoDisplay.hunger = (int)((hunger /maxHunger) * 100);
            infoDisplay.thirst = (int)((thirst / maxThirst) * 100);
            infoDisplay.stamina = (int)stamina;

            infoDisplay.maxStamina = (int) maxStamina;

            infoDisplay.viewDistance = viewDistance;
            infoDisplay.idleSpeed = idleSpeed;
            infoDisplay.searchSpeed = searchSpeed;

            infoDisplay.isInWater = isAtWater;
            infoDisplay.isAtDestination = IsAgentAtDestination();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 4 /*Water*/)
        {
            isAtWater = true;
            lastNearestWaterSource = transform.position;

            if (currentAction == Action.GoingToWater) Drink();
        }
        else if (other.name.Contains("Food"))
        {
            if (currentAction == Action.GoingToFood) Eat(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 4 /*Water*/) isAtWater = false;
    }

    void Tick()
    {

        #region Aging
        age += ageRate;

        // when mature, scale to adult size
        if (age == maturityAge) transform.localScale = new (1,1,1);
        if (age > maturityAge) maxStamina -= ((age - maturityAge) / maxStamina)/2;
        #endregion

        #region Pregnancy
        if (sex == 1 /*Female*/ && pregnant)
        {
            // reduce stats more than normal when pregnant
            hunger -= pregnantHungerRate;
            thirst -= pregnantThirstRate;
            stamina -= pregnantStaminaRate;

            if (pregnancyStatus == 0) GiveBirth();
            else pregnancyStatus--;
        }

        #endregion

        #region Hunger
        hunger -= idleHungerRate / (stamina / 100);
        #endregion

        #region Thirst
        thirst -= idleThirstRate / (stamina / 100);
        #endregion

        #region Breeding
        if (breedingCooldown > 0) breedingCooldown--;
        #endregion

        #region Stamina
        if(currentAction == Action.GoingToFood || currentAction == Action.GoingToWater || currentAction == Action.GoingToBreed) stamina -= searchingStaminaRate;
        else if (currentAction == Action.Idle) stamina -= idleStaminaRate;
        else if (currentAction == Action.Resting) stamina -= restingStaminaRate;

        if (stamina > maxStamina) stamina = maxStamina;
        #endregion

        #region Decide what to do
        ScanForFood();
        ScanForWater();

        var nearbyFood = GetNearestFoodSource(foodSources);

        float breedingDecisionWeight = -1;
        float foodDecisionWeight = -1;
        float waterDecisionWeight = -1;
        float restDecisionWeight = -1;

        //breeding
        if (sex == 0 /*Male*/)
        {
            ScanForFemaleMates();
            var nearbyMate = GetNearestMate(mates);

            if (nearbyMate != null && nearbyMate.Item2 != new Vector3(999, 999, 999) && CanBreed())
            {
                breedingDecisionWeight = 1 / ((Vector3.Distance(nearbyMate.Item2, transform.position) / viewDistance));
            }
        }

        // hunger
        if (nearbyFood != null && nearbyFood != new Vector3(999, 999, 999) && hunger < (maxHunger - Math.Abs(eatingHungerRate) - 1)/*hunger < maxHunger * hungerThreshold*/)
        {
            var hSq = Math.Pow(hunger / maxHunger, 2);
            foodDecisionWeight = (float)(1 / ((Vector3.Distance(nearbyFood, transform.position) / viewDistance) * hSq));
        }

        //stamina
        if (stamina < (maxStamina - Math.Abs(restingStaminaRate) - 1))
        {
            var sSq = Math.Pow(stamina / maxStamina, 3);
            restDecisionWeight = (float)(1 / sSq);
        }

        //thirst
        if (lastNearestWaterSource != null && lastNearestWaterSource != new Vector3(999,999,999) && lastNearestWaterSource != new Vector3(0, 0, 0) && thirst < (maxThirst - Math.Abs(drinkingThirstRate) - 1))
        {
            var tSq = Math.Pow(thirst / maxThirst, 2);
            waterDecisionWeight = (float)(1 / ((Vector3.Distance(lastNearestWaterSource, transform.position) / viewDistance) * tSq));
        }
        #endregion

        #region Do action
        List<float> weights = new List<float>() { foodDecisionWeight, waterDecisionWeight, breedingDecisionWeight, restDecisionWeight };
        //Debug.Log(String.Join(",", weights.ToArray()));

        var highest = weights.Max();
        if (highest == -1) Idle();
        else if (highest == foodDecisionWeight) GoEat();
        else if (highest == waterDecisionWeight) GoDrink();
        else if (sex == 0 && highest == breedingDecisionWeight) GoBreed();
        else if (highest == restDecisionWeight) Rest();
        #endregion

        #region Die?
        if (hunger <= 0)
        {
            Die();
            Debug.Log($"Death due to starvation");
        }
        else if(thirst <= 0)
        {
            Die();
            Debug.Log($"Death due to thirst");
        }
        else if(stamina <= 0)
        {
            Die();
            Debug.Log($"Death due to exhaustion");
        }
        else if (age >= lifespan)
        {
            Die();
            Debug.Log($"Death due to old age");
        }
        #endregion
    }

    void Idle()
    {
        if (currentAction != Action.Idle)
        {
            currentAction = Action.Idle;
            agent.speed = idleSpeed * (stamina/100);
        }

        if (IsAgentAtDestination())
        {
            Vector3 randomDestination = transform.position + new Vector3(rand.Next(-idleRange, idleRange), 0, rand.Next(-idleRange, idleRange));
            agent.destination = randomDestination;
        }
        
    }

    void Rest()
    {
        currentAction = Action.Resting;
    }

    void Drink()
    {
        currentAction = Action.Drinking;

        thirst -= drinkingThirstRate;
        stamina -= drinkingStaminaRate;
        if (stamina > maxStamina) stamina = maxStamina;
    }

    void Eat(GameObject food)
    {
        currentAction = Action.Eating;

        hunger -= eatingHungerRate;
        stamina -= eatingStaminaRate;
        if (stamina > maxStamina) stamina = maxStamina;

        Destroy(food);
    }

    void Breed(GameObject nearestMate)
    {
        currentAction = Action.Breeding;

        // 80% success rate of breeding
        var success = rand.Next(0, 10) <= 8;
        var mateStats = nearestMate.GetComponent<AnimalBehavior>();

        hunger -= breedingHungerRate;
        thirst -= breedingThirstRate;
        stamina -= breedingStaminaRate;

        if (success)
        {
            mateStats.pregnant = true;
            mateStats.breedingCooldown = mateStats.pregnancyDuration;
            mateStats.pregnancyStatus = mateStats.pregnancyDuration;
            nearestMate.transform.Find("babybelly").GetComponent<MeshRenderer>().enabled = true;
            
            mateStats.fatherTraits = (AnimalBehavior) this.MemberwiseClone();
        }
        else
        {
            mateStats.breedingCooldown = 3;
        }

        breedingCooldown = 3;
    }

    void GoEat()
    {
        if (currentAction != Action.GoingToFood)
        {
            currentAction = Action.GoingToFood;
            agent.speed = searchSpeed * (stamina / 100);

        }

        Vector3 nearestFood = GetNearestFoodSource(foodSources);
        agent.destination = nearestFood;
    }

    void GoDrink()
    {
        agent.destination = lastNearestWaterSource;
        if (currentAction != Action.GoingToWater)
        {
            currentAction = Action.GoingToWater;
            agent.speed = searchSpeed * (stamina / 100);
        }

        if (IsAgentAtDestination())
        {
            Drink();
        }

    }

    void GoBreed()
    {
        if (currentAction != Action.GoingToBreed)
        {
            currentAction = Action.GoingToBreed;
            agent.speed = searchSpeed * (stamina / 100);
        }

        Tuple<GameObject,Vector3> nearestMate = GetNearestMate(mates);
        agent.destination = nearestMate.Item2;

        if (IsAgentAtDestination())
        {
            Breed(nearestMate.Item1);
        }
    }

    void GiveBirth()
    {
        gameObject.transform.Find("babybelly").GetComponent<MeshRenderer>().enabled = false;

        thirst -= pregnantThirstRate * 2;
        hunger -= pregnantHungerRate * 2;
        stamina -= birthingStaminaRate;

        reproduction.CreateBabyAnimals(1, maxLitterSize, transform.position, fatherTraits, (AnimalBehavior) this.MemberwiseClone());

        breedingCooldown = 5;
        pregnant = false;
    }

    void Die()
    {
        var corpse = Instantiate(
            corpsePrefab,
            new Vector3(transform.position.x, Terrain.activeTerrain.SampleHeight(transform.position)+0.1f, transform.position.z),
            Quaternion.identity);

        corpse.name = "corpse_" + gameObject.name.Split('_')[1];
        corpse.transform.parent = transform.parent;

        Destroy(gameObject);
        Global.animalCount--;
    }

    bool CanBreed()
    {
        if (age >= maturityAge
            && age <= tooOldToBreedAge
            && !pregnant
            && breedingCooldown == 0
            && thirst > maxThirst * thirstThreshold
            && hunger > maxHunger * hungerThreshold
            && stamina > maxStamina * staminaThreshold
            )
        {
            return true;
        }
        else return false;
    }

    #region Helpers
    Tuple<GameObject, Vector3> GetNearestMate(List<GameObject> objects)
    {
        GameObject nearestObject = null;
        Vector3 nearestObjectPosition = new Vector3(999, 999, 999);
        foreach (GameObject obj in objects)
        {
            if (Vector3.Distance(agent.transform.position, obj.transform.position) < Vector3.Distance(agent.transform.position, nearestObjectPosition))
            {
                nearestObjectPosition = obj.transform.position;
                nearestObject = obj;
            }
        }

        return Tuple.Create(nearestObject, nearestObjectPosition);
    }

    Vector3 GetNearestFoodSource(List<Vector3> foodSources)
    {
        Vector3 nearestFood = new Vector3(999, 999, 999);
        foreach (Vector3 foodSource in foodSources)
        {
            if (Vector3.Distance(agent.transform.position, foodSource) < Vector3.Distance(agent.transform.position, nearestFood))
            {
                nearestFood = foodSource;
            }
        }
        return nearestFood;
    }

    void ScanForFood()
    {
        foodSources.Clear();
        Collider[] hitColliders = Physics.OverlapSphere(agent.transform.position, viewDistance);
        foreach (Collider collider in hitColliders)
        {
            var collidedObject = collider.gameObject;
            if (collidedObject.name.Contains("Food")) foodSources.Add(collidedObject.transform.position);
        }
    }

    void ScanForWater()
    {
        List<Vector3> hitWaterPoints = new List<Vector3>();

        foreach (RayPerceptionSensorComponent3D rayPercept in rayPercepts) hitWaterPoints = RayCast(rayPercept);

        if (hitWaterPoints.Count > 0)
        {
            Vector3 newNearestWater = new Vector3(999, 999, 999);
            foreach (Vector3 waterSource in hitWaterPoints)
            {
                if (Vector3.Distance(transform.position, waterSource) < Vector3.Distance(transform.position, newNearestWater)) newNearestWater = waterSource;
            }

            if (Vector3.Distance(lastNearestWaterSource, transform.position) > Vector3.Distance(newNearestWater, transform.position)) lastNearestWaterSource = newNearestWater;
        }
    }

    void ScanForFemaleMates()
    {
        mates.Clear();
        Collider[] hitColliders = Physics.OverlapSphere(agent.transform.position, viewDistance);
        foreach (Collider collider in hitColliders)
        {
            var collidedObject = collider.gameObject;
            var parent = collidedObject.transform.parent;
            // if scanned object is an animal
            if (parent != null && collidedObject.name.Contains("Animal"))
            {
                // if other animal is female & can breed
                if (collidedObject.GetComponent<AnimalBehavior>().sex == 1 && collidedObject.GetComponent<AnimalBehavior>().CanBreed())
                {
                    mates.Add(collider.gameObject);
                }
            }
        }
    }

    bool IsAgentAtDestination()
    {
        if (currentAction == Action.Idle || currentAction == Action.GoingToWater && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance) return true;
        }
        else if (currentAction == Action.GoingToBreed && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance + 0.5) return true;
        }

        return false;
    }

    private List<Vector3> RayCast(RayPerceptionSensorComponent3D rayComponent)
    {
        List<Vector3> result = new List<Vector3>();

        var rayOutputs = RayPerceptionSensor
                .Perceive(rayComponent.GetRayPerceptionInput())
                .RayOutputs;

        if (rayOutputs != null)
        {
            var lengthOfRayOutputs = RayPerceptionSensor
                    .Perceive(rayComponent.GetRayPerceptionInput())
                    .RayOutputs
                    .Length;

            for (int i = 0; i < lengthOfRayOutputs; i++)
            {
                GameObject goHit = rayOutputs[i].HitGameObject;

                if (goHit != null)
                {

                    // Found some of this code to Denormalized length
                    // calculation by looking trough the source code:
                    // RayPerceptionSensor.cs in Unity Github. (version 2.2.1)
                    var rayDirection = rayOutputs[i].EndPositionWorld - rayOutputs[i].StartPositionWorld;
                    var scaledRayLength = rayDirection.magnitude;
                    float rayHitDistance = rayOutputs[i].HitFraction * scaledRayLength;

                    if (goHit.layer == 4 /*Water*/)
                    {
                        Physics.Raycast(gameObject.transform.position, rayDirection, out RaycastHit hit, rayHitDistance + 1);
                        result.Add(hit.point);
                    }
                }
            }
        }
        return result;
    }

    #endregion

}