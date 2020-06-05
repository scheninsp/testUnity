using UnityEngine;
using System;

public class DeferredFogEffect : MonoBehaviour
{

    public Shader deferredFog;

    [NonSerialized] 
    Material fogMaterial;

    [NonSerialized]
    Camera deferredCamera;

    [NonSerialized]
    Vector3[] frustumCorners;

    [NonSerialized]
    Vector4[] vectorArray;

    [ImageEffectOpaque]  //add this procedure after opaque, before transparent render
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (fogMaterial == null)
        {
            deferredCamera = GetComponent<Camera>();
            frustumCorners = new Vector3[4];
            vectorArray = new Vector4[4];
            fogMaterial = new Material(deferredFog);
        }

        deferredCamera.CalculateFrustumCorners(
            new Rect(0f, 0f, 1f, 1f),
            deferredCamera.farClipPlane,
            deferredCamera.stereoActiveEye,
            frustumCorners
        );
        //output to frustumCorners[4][3]
        //bottom-left, top-left, top-right, bottom-right


        //adjust final output to texture coordinate order:
        // bottom-left, bottom-right, top-left, top-right.
        vectorArray[0] = frustumCorners[0];
        vectorArray[1] = frustumCorners[3];
        vectorArray[2] = frustumCorners[1];
        vectorArray[3] = frustumCorners[2];
        fogMaterial.SetVectorArray("_FrustumCorners", vectorArray);  //set property value

        Graphics.Blit(source, destination, fogMaterial);
    }
}
