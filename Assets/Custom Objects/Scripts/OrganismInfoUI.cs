using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class OrganismInfoUI : MonoBehaviour
{
    new Camera camera;
    public bool isVisible;
    public Quaternion orientation;

    #region Stats to display
    public int hunger;
    public int thirst;
    public int age;
    public int pregnancyStatus;
    public int stamina;
    public int maxStamina;

    public bool isAtDestination;
    public bool isInWater;
    #endregion

    #region Traits to display
    public float viewDistance;
    public float idleSpeed;
    public float searchSpeed;
    #endregion

    #region name and action
    public string organismName;
    public string currentAction;
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.SetActive(isVisible);
        var nameText = gameObject.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        nameText.text = organismName + ": "+currentAction;
        
        var statsText = gameObject.transform.Find("Stats").GetComponent<TextMeshProUGUI>();
        statsText.text = $"Hunger: {hunger}%\nThirst: {thirst}%\nStamina: {stamina}/{maxStamina}\nAge: {age}%\n";
        //statsText.text = $"Hunger: {hunger}%\nThirst: {thirst}%\nAge: {age}%\nPregnancy: {pregnancyStatus}%";
        //statsText.text = $"Arrived: {isAtDestination}";

        var traitsText = gameObject.transform.Find("Traits").GetComponent<TextMeshProUGUI>();
        traitsText.text = $"ViewDistance: {viewDistance}\nIdleSpeed: {idleSpeed}\nSearchSpeed: {searchSpeed}\n";
        //traitsText.text = $"W:{isInWater}\nD:{isAtDestination}";

        SetOrientation();
    }

    private void SetOrientation()
    {
        var lookPos = camera.transform.position - transform.position;
        orientation = Quaternion.LookRotation(-lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, orientation, Time.deltaTime * 3);
    }
}
