using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancedMaterialProperties : MonoBehaviour
{
    [SerializeField]
    Color color = Color.white;

    [SerializeField, Range(0f, 1f)]
    float smoothness = 0.5f;

    [SerializeField, Range(0f, 1f)]
    float metallic;

    //speed up by using variable initialization once
    static MaterialPropertyBlock propertyBlock;
    static int colorID = Shader.PropertyToID("_Color");
    static int smoothnessId = Shader.PropertyToID("_Smoothness");
    static int metallicId = Shader.PropertyToID("_Metallic");


    private void Awake()
    {
        OnValidate();
    }

    //support in editor preview
    private void OnValidate()
    {
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
        propertyBlock.SetColor(colorID, color);
        propertyBlock.SetFloat(smoothnessId, smoothness);
        propertyBlock.SetFloat(metallicId, metallic);

        GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);
    }
}
