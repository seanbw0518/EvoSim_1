using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Global : MonoBehaviour
{
    public int statisticReadRate;

    public static float tickRate = 2;
    public static int animalCount = 0;

    public static List<int> animalCountHistory = new List<int>();
    public static List<int> averageHungerHistory = new List<int>();
    public static List<int> averageThirstHistory = new List<int>();
    public static List<int> averageStaminaHistory = new List<int>();
    public static List<int> averageAgeHistory = new List<int>();

    public static List<decimal> averageViewDistanceHistory = new List<decimal>();
    public static List<decimal> averageIdleSpeedHistory = new List<decimal>();
    public static List<decimal> averageSearchSpeedHistory = new List<decimal>();

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(RetrieveStatistics), 0, statisticReadRate);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            using var animalCountHistoryFile = File.CreateText("C:\\Users\\seanb\\EvoSim_1\\animalCountHistory.csv");
            foreach (var arr in animalCountHistory)
            {
                animalCountHistoryFile.WriteLine(string.Join(",", arr));
            }

            using var averageHungerHistoryFile = File.CreateText("C:\\Users\\seanb\\EvoSim_1\\averageHungerHistory.csv");
            foreach (var arr in averageHungerHistory)
            {
                averageHungerHistoryFile.WriteLine(string.Join(",", arr));
            }

            using var averageThirstHistoryFile = File.CreateText("C:\\Users\\seanb\\EvoSim_1\\averageThirstHistory.csv");
            foreach (var arr in averageThirstHistory)
            {
                averageThirstHistoryFile.WriteLine(string.Join(",", arr));
            }

            using var averageStaminaHistoryFile = File.CreateText("C:\\Users\\seanb\\EvoSim_1\\averageStaminaHistory.csv");
            foreach (var arr in averageStaminaHistory)
            {
                averageStaminaHistoryFile.WriteLine(string.Join(",", arr));
            }

            using var averageAgeHistoryFile = File.CreateText("C:\\Users\\seanb\\EvoSim_1\\averageAgeHistory.csv");
            foreach (var arr in averageAgeHistory)
            {
                averageAgeHistoryFile.WriteLine(string.Join(",", arr));
            }


            using var averageViewDistanceHistoryFile = File.CreateText("C:\\Users\\seanb\\EvoSim_1\\averageViewDistanceHistory.csv");
            foreach (var arr in averageViewDistanceHistory)
            {
                averageViewDistanceHistoryFile.WriteLine(string.Join(",", arr));
            }

            using var averageIdleSpeedHistoryFile = File.CreateText("C:\\Users\\seanb\\EvoSim_1\\averageIdleSpeedHistory.csv");
            foreach (var arr in averageIdleSpeedHistory)
            {
                averageIdleSpeedHistoryFile.WriteLine(string.Join(",", arr));
            }

            using var averageSearchSpeedHistoryFile = File.CreateText("C:\\Users\\seanb\\EvoSim_1\\averageSearchSpeedHistory.csv");
            foreach (var arr in averageSearchSpeedHistory)
            {
                averageSearchSpeedHistoryFile.WriteLine(string.Join(",", arr));
            }


        }
    }

    void RetrieveStatistics()
    {

        animalCountHistory.Add(animalCount);

        if (animalCount > 0)
        {
            var thirstSum = 0;
            var hungerSum = 0;
            var staminaSum = 0;
            var ageSum = 0;

            var viewDistanceSum = 0f;
            var idleSpeedSum = 0f;
            var searchSpeedSum = 0f;

            foreach (Transform animal in gameObject.transform.Find("Animals").transform)
            {
                if (animal.gameObject.name.Contains("Animal1_"))
                {
                    var animalBehavior = animal.GetComponent<AnimalBehavior>();

                    hungerSum += (int)(100 * ((float)animalBehavior.hunger / (float)animalBehavior.maxHunger));
                    thirstSum += (int)(100 * ((float)animalBehavior.thirst / (float)animalBehavior.maxThirst));
                    staminaSum += (int)(100 * ((float)animalBehavior.stamina / (float)animalBehavior.maxStamina));
                    ageSum += (int)(100 * ((float)animalBehavior.age / (float)animalBehavior.lifespan));

                    viewDistanceSum += animalBehavior.viewDistance;
                    idleSpeedSum += animalBehavior.idleSpeed;
                    searchSpeedSum += animalBehavior.searchSpeed;

                }
            }

            int averageHunger = (hungerSum / animalCount);
            averageHungerHistory.Add(averageHunger);

            int averageThirst = (thirstSum / animalCount);
            averageThirstHistory.Add(averageThirst);

            int averageStamina = (staminaSum / animalCount);
            averageStaminaHistory.Add(averageStamina);

            int averageAge = (ageSum / animalCount);
            averageAgeHistory.Add(averageAge);

            decimal averageViewDistance = Math.Round((decimal)(viewDistanceSum / animalCount), 2);
            averageViewDistanceHistory.Add(averageViewDistance);

            decimal averageIdleSpeed = Math.Round((decimal)(idleSpeedSum / animalCount), 2);
            averageIdleSpeedHistory.Add(averageIdleSpeed);

            decimal averageSearchSpeed = Math.Round((decimal)(searchSpeedSum / animalCount), 2);
            averageSearchSpeedHistory.Add(averageSearchSpeed);
        }
    }

}
