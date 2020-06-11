using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancedColor : MonoBehaviour
{
    [SerializeField]
    Color color = Color.white;

    //speed up by using variable initialization once
    static MaterialPropertyBlock propertyBlock;
    static int colorID = Shader.PropertyToID("_Color");

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
        GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);
    }
}
