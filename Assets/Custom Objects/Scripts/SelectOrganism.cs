using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using static UnityEngine.GraphicsBuffer;

public class SelectOrganism : MonoBehaviour
{
    // Start is called before the first frame update
    public new Camera camera;
    public Canvas textCanvas;
    GameObject previousOrganism;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit, 25f))
            {
                Transform objectHit = hit.transform;

                if (objectHit.name.Contains("Animal"))
                {
                    DisplayOrganismToolTip(objectHit.gameObject);
                }
                else
                {
                    ClearToolTips();
                }
            }
            else
            {
                ClearToolTips();
            }
        }
    }

    void DisplayOrganismToolTip(GameObject organismObject)
    {
        ClearToolTips();
        organismObject.transform.Find("OrganismInfoCanvas").gameObject.GetComponent<OrganismInfoUI>().isVisible = true;
        organismObject.transform.Find("OrganismInfoCanvas").gameObject.SetActive(true);
        previousOrganism = organismObject;
    }

    void ClearToolTips()
    {
        if (previousOrganism != null)
        {
            previousOrganism.transform.Find("OrganismInfoCanvas").gameObject.GetComponent<OrganismInfoUI>().isVisible = false;

        }
    }
}
