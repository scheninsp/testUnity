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

    [SerializeField, ColorUsage(false, true)]
    Color emissionColor = Color.black;

    [SerializeField]
    float pulseEmissionFreqency;

    //speed up by using variable initialization once
    static MaterialPropertyBlock propertyBlock;
    static int colorID = Shader.PropertyToID("_Color");
    static int smoothnessId = Shader.PropertyToID("_Smoothness");
    static int metallicId = Shader.PropertyToID("_Metallic");
    static int emissionColorId = Shader.PropertyToID("_EmissionColor");


    private void Awake()
    {
        OnValidate();
        if (pulseEmissionFreqency <= 0f)
        {
            enabled = false;
        }
    }

    void Update()
    {  //pulse emission light
        Color originalEmissionColor = emissionColor;
        emissionColor *= 0.5f +
            0.5f * Mathf.Cos(2f * Mathf.PI * pulseEmissionFreqency * Time.time);
        OnValidate();
        DynamicGI.SetEmissive(GetComponent<MeshRenderer>(), emissionColor);
        emissionColor = originalEmissionColor;
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
        propertyBlock.SetColor(emissionColorId, emissionColor);

        GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);
    }
}
