using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    int decayDuration = 25;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("Decay", 0, Global.tickRate);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Decay()
    {
        if (decayDuration <= 0) 
        {
            GameObject thisFood = gameObject;
            Destroy(thisFood);

        }
        else
        {
            decayDuration--;
        }
    }
}
