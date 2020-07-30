using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Shape : MonoBehaviour
{
    MeshRenderer meshRenderer;

    //state 0: normal
    //state 1: warning
    //state 2: attacking
    public int state = 0;

    private const float attackerZscaleMax = 4.8f;
    private float attackerXYScale;
    private float halfZsize;

    private const int warningObjectChildIndex = 1;
    private const int attackerObjectChildIndex = 2;

    private const float warningDuration = 2f;
    private const int warningRepeatTimes = 2;
    private const float attackDuration = 1f;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        for (int i = 1; i < 3; i++)
        {
            this.transform.GetChild(i).gameObject.SetActive(false);
        }

        halfZsize = this.transform.localScale.z / 2;
        attackerXYScale = this.transform.GetChild(attackerObjectChildIndex).localScale.x;
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
        this.transform.rotation = new Quaternion(0, 0, 0, 0);
    }

    public void StartAttack1()
    {
        if(state == 0)
        {
            StartCoroutine(Attack1());
        }
    }

    private IEnumerator Attack1()
    {
        DateTime timer1 = new DateTime();
        DateTime timer2 = new DateTime(); 
        TimeSpan dur1 = new TimeSpan(); 

        //enter Warning state
        if (state == 0)
        {
            timer1 = DateTime.Now;
            state = 1;
            this.transform.GetChild(warningObjectChildIndex).gameObject.SetActive(true);
        }

        //In warning state
        while(state == 1 && dur1.TotalSeconds < warningDuration +0.01f)
        {
            timer2 = DateTime.Now;
            dur1 = timer2.Subtract(timer1);

            float currentLerpVal = MyMath.TriangleFunction((float)dur1.TotalSeconds, warningDuration / warningRepeatTimes);

            Color newColor = this.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
            newColor.a = Mathf.Lerp(60 / 255f, 160 / 255f, currentLerpVal);
            this.transform.GetChild(warningObjectChildIndex).GetComponent<SpriteRenderer>().color = newColor;

            yield return null;
        }

        //enter Attack state
        state = 2;
        timer1 = DateTime.Now;
        dur1 = new TimeSpan();
        this.transform.GetChild(warningObjectChildIndex).gameObject.SetActive(false);
        this.transform.GetChild(attackerObjectChildIndex).gameObject.SetActive(true);

        //In attack state
        while (state == 2 && dur1.TotalSeconds < attackDuration + 0.01f)
        {
            timer2 = DateTime.Now;
            dur1 = timer2.Subtract(timer1);

            float currentLerpVal = MyMath.TriangleFunction((float)dur1.TotalSeconds, attackDuration);
            /*
            float currentLerpVal = (float)dur1.TotalSeconds;
            currentLerpVal = 2 * (currentLerpVal - Mathf.Floor(currentLerpVal));
            if (currentLerpVal > 1f)
            {
                currentLerpVal = -currentLerpVal + 2;
            }*/

            Vector3 newScale = new Vector3(attackerXYScale, attackerXYScale, Mathf.Lerp(0, attackerZscaleMax, currentLerpVal));
            Vector3 newPosition = new Vector3(0,0, -halfZsize - newScale.z/2);

            this.transform.GetChild(attackerObjectChildIndex).transform.localPosition = newPosition;
            this.transform.GetChild(attackerObjectChildIndex).transform.localScale = newScale;

            yield return null;
        }

        //exit
        state = 0;
        dur1 = new TimeSpan();
        this.transform.GetChild(warningObjectChildIndex).gameObject.SetActive(false);
        this.transform.GetChild(attackerObjectChildIndex).gameObject.SetActive(false);
        yield break;

    }


}
