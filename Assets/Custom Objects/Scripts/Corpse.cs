using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse : MonoBehaviour
{
    int decayRate = 1;
    int decayAmount = 10;

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
        if (decayAmount <= 0) 
        {
            GameObject thisCorpse = gameObject;
            Destroy(thisCorpse);

        }
        else
        {
            decayAmount -= decayRate;
        }
    }
}
