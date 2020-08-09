using UnityEngine;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(menuName = "Rendering/My Pipeline")]
public class MyPipelineAsset1 : RenderPipelineAsset
{
    [SerializeField]
    bool dynamicBatching;

    [SerializeField]
    bool instancing;

    public enum ShadowMapSize
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096
    }

    [SerializeField]
    ShadowMapSize shadowMapSize = ShadowMapSize._1024;

    [SerializeField]
    float shadowDistance = 100f;

    public enum ShadowCascades
    {
        Zero = 0,
        Two = 2,
        Four = 4
    }

    [SerializeField]
    ShadowCascades shadowCascades = ShadowCascades.Four;

    [SerializeField, HideInInspector]
    float twoCascadesSplit = 0.25f;

    [SerializeField, HideInInspector]
    Vector3 fourCascadesSplit = new Vector3(0.067f, 0.2f, 0.467f);

    [SerializeField, Range(0.01f, 2f)]
    float shadowFadeRange = 1f;
    //a factor for smoothing transition of realtime shadow at shadowDistance

    protected override IRenderPipeline InternalCreatePipeline()
    {
        Vector3 shadowCascadeSplit = shadowCascades == ShadowCascades.Four ?
                fourCascadesSplit : new Vector3(twoCascadesSplit, 0f);

        return new MyPipeline1(dynamicBatching, instancing, 
            (int)shadowMapSize, shadowDistance, shadowFadeRange,
            (int)shadowCascades, shadowCascadeSplit);
    }


}
