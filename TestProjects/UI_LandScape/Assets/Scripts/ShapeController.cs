using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeController : MonoBehaviour
{
    //make a property
    public int ShapeId
    {
        get
        {
            return shapeId;
        }
        set
        {
            if (shapeId == int.MinValue && value != int.MinValue)
            {
                shapeId = value;
            }
            else
            {
                Debug.LogError("Not allowed to change shapeId");
            }
        }
    }
    int shapeId = int.MinValue;


    public int MaterialId
    {
        get;
        private set; //forbidden
    }
    int materialId = int.MinValue;

    //state 0 : both not activated
    //state 1 : shape activated, destructedShape deactivated
    //state 2 : shape deactivated, destructedShape activated
    public int state = 0;

    void Awake()
    {
        this.transform.GetChild(0).gameObject.SetActive(true);
        this.transform.GetChild(1).gameObject.SetActive(false);
        state = 1;
    }

    void Update()
    {
        if(this.transform.GetChild(0).gameObject.activeSelf == false &&
            this.transform.GetChild(1).gameObject.activeSelf == false)
        {
            state = 0;
        }
    }

    public void DestroyShape()
    {
        this.transform.GetChild(0).gameObject.SetActive(false);
        this.transform.GetChild(1).gameObject.SetActive(true);
        this.transform.GetChild(1).GetComponent<DestructedShape>().Explode();
        state = 2;
    }

    public void SetMaterial(Material material, int materialId)
    {
        this.transform.GetChild(0).GetComponent<Shape>().SetMaterial(material);
        this.transform.GetChild(1).GetComponent<DestructedShape>().SetMaterial(material);
    }

    public void Reclaim()
    {
        this.transform.GetChild(0).GetComponent<Shape>().Reclaim();
        this.transform.GetChild(1).GetComponent<DestructedShape>().Reclaim();

        this.transform.GetChild(0).gameObject.SetActive(true);
        this.transform.GetChild(1).gameObject.SetActive(false);
        state = 1;
    }
}
