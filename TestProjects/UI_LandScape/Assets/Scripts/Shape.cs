using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Shape : MonoBehaviour
{
    MeshRenderer meshRenderer;

    private DateTime timer1;
    private DateTime timer2;
    private TimeSpan dur1;

    //state 0: normal
    //state 1: warning
    //state 2: attacking
    private int state = 0;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        for (int i = 1; i < 3; i++)
        {
            this.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void SetMaterial(Material material)
    {
        meshRenderer.material = material;
    }


    Color color;  //for saving color

    //use PropertyID to generate only one Id throughout a session 
    static int colorPropertyId = Shader.PropertyToID("_Color");
    static MaterialPropertyBlock shaderPropertyBlock;

    public void SetColor(Color color)
    {
        this.color = color;
        //meshRenderer.material.color = color;  //this create a new material everytime

        //use propertyBlock to avoid making new materials
        if (shaderPropertyBlock == null)
        {
            shaderPropertyBlock = new MaterialPropertyBlock();
        }
        shaderPropertyBlock.SetColor(colorPropertyId, color);
        meshRenderer.SetPropertyBlock(shaderPropertyBlock);
    }

    public void Reclaim()
    { 
        for(int i=1; i<3; i++)
        {
            this.transform.GetChild(i).gameObject.SetActive(false);
        }
        state = 0;
        dur1 = new TimeSpan();

    }

    public void StartWarning()
    {
        if(state == 0)
        {
            StartCoroutine(Warning1());
        }
    }

    private IEnumerator Warning1()
    {
        if (state == 0)
        {
            timer1 = DateTime.Now;
            state = 1;
            this.transform.GetChild(1).gameObject.SetActive(true);
        }

        while(state == 1 && dur1.TotalSeconds < 2.01f)
        {
            timer2 = DateTime.Now;
            dur1 = timer2.Subtract(timer1);

            float currentLerpVal = (float)dur1.TotalSeconds;
            currentLerpVal = 2 * (currentLerpVal - Mathf.Floor(currentLerpVal));
            if (currentLerpVal > 1f)
            {
                currentLerpVal = -currentLerpVal + 2;
            }

            Color newColor = this.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
            newColor.a = Mathf.Lerp(60 / 255f, 160 / 255f, currentLerpVal);
            this.transform.GetChild(1).GetComponent<SpriteRenderer>().color = newColor;

            yield return null;
        }

        //exit
        state = 0;
        dur1 = new TimeSpan();
        this.transform.GetChild(1).gameObject.SetActive(false);
        yield break;

    }

    public void Attack()
    {

    }
}
