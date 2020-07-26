using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
    MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
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
    { //no action
    }

}
