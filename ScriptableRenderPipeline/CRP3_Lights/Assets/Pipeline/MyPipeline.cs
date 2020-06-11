using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class MyPipeline : RenderPipeline
{

    CullResults cull;  //fix here to reduce GC consumption

    CommandBuffer cameraBuffer = new CommandBuffer { name = "Render Camera" };

    Material errorMaterial;  //error fallback

    DrawRendererFlags drawFlags;

    //transfer to light buffer
    const int maxVisibleLights = 16;

    static int visibleLightColorsId =
        Shader.PropertyToID("_VisibleLightColors");
    static int visibleLightDirectionsOrPositionsId =
        Shader.PropertyToID("_VisibleLightDirectionsOrPositions");
    static int visibleLightAttenuationsId =
        Shader.PropertyToID("_VisibleLightAttenuations");
    static int visibleLightSpotDirectionsId =
        Shader.PropertyToID("_VisibleLightSpotDirections");
    static int lightIndicesOffsetAndCountID =
        Shader.PropertyToID("unity_LightIndicesOffsetAndCount");

    Vector4[] visibleLightColors = new Vector4[maxVisibleLights];
    Vector4[] visibleLightDirectionsOrPositions = new Vector4[maxVisibleLights];
    Vector4[] visibleLightAttenuations = new Vector4[maxVisibleLights];
    Vector4[] visibleLightSpotDirections = new Vector4[maxVisibleLights];


    public MyPipeline(bool dynamicBatching, bool instancing)  //constructor with configurations
    {
        GraphicsSettings.lightsUseLinearIntensity = true;

        if (dynamicBatching)
        {
            drawFlags = DrawRendererFlags.EnableDynamicBatching;
        }

        if (instancing)
        {
            drawFlags |= DrawRendererFlags.EnableInstancing;
        }
    }

    public override void Render(
    ScriptableRenderContext renderContext, Camera[] cameras)
    {
        base.Render(renderContext, cameras);

        foreach (var camera in cameras)
        {
            Render(renderContext, camera);
        }
    }

    void Render(ScriptableRenderContext context, Camera camera)
    {
        //Culling
        ScriptableCullingParameters cullingParameters;
        if (!CullResults.GetCullingParameters(camera, out cullingParameters))
        {
            return;
        }

#if UNITY_EDITOR
        //add UI to SceneWindow
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
#endif

        CullResults.Cull(ref cullingParameters, context, ref cull);

        //Setup Camera parameters
        context.SetupCameraProperties(camera);  //set MVP matrix, etc.

        //Setup Command Buffer and Execute
        CameraClearFlags clearFlags = camera.clearFlags;
          
        cameraBuffer.ClearRenderTarget(
            (clearFlags & CameraClearFlags.Depth) != 0,
            (clearFlags & CameraClearFlags.Color) != 0,
            camera.backgroundColor
        );

        if (cull.visibleLights.Count > 0)
        {
            ConfigureLights();
        }
        else
        {
            cameraBuffer.SetGlobalVector(
                lightIndicesOffsetAndCountID, Vector4.zero
            );
        }

        cameraBuffer.BeginSample("Render Camera");  //start a sub-level

        //transfer light properties to buffers in shader
        cameraBuffer.SetGlobalVectorArray(
            visibleLightColorsId, visibleLightColors
        );
        cameraBuffer.SetGlobalVectorArray(
            visibleLightDirectionsOrPositionsId, visibleLightDirectionsOrPositions
        );
        cameraBuffer.SetGlobalVectorArray(
            visibleLightAttenuationsId, visibleLightAttenuations
        );
        cameraBuffer.SetGlobalVectorArray(
            visibleLightSpotDirectionsId, visibleLightSpotDirections
        );

        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();

        //Draw Opaque, Unlit Shader
        var drawSettings = new DrawRendererSettings(
            camera, new ShaderPassName("SRPDefaultUnlit"))
        {
            flags = drawFlags,
        };
        
        if(cull.visibleLights.Count > 0)
        {
            drawSettings.rendererConfiguration = RendererConfiguration.PerObjectLightIndices8;
        }

        drawSettings.sorting.flags = SortFlags.CommonOpaque;

        var filterSettings = new FilterRenderersSettings(true)
        {
            renderQueueRange = RenderQueueRange.opaque
        };

        context.DrawRenderers(
            cull.visibleRenderers, ref drawSettings, filterSettings);
	

        //Draw Skybox
        context.DrawSkybox(camera);

        //Draw Transparent
        drawSettings.sorting.flags = SortFlags.CommonTransparent;  
        filterSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(
            cull.visibleRenderers, ref drawSettings, filterSettings
        );
		
		//Conditions for default pipelines
        DrawDefaultPipeline(context, camera);

        cameraBuffer.EndSample("Render Camera");   //end the sub-level
        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();

        context.Submit();
    }

    [Conditional ("UNITY_EDITOR"), Conditional("DEVELOPEMENT_BUILD")]
    void DrawDefaultPipeline(ScriptableRenderContext context, Camera camera) {
        if (errorMaterial == null)
        {
            Shader errorShader = Shader.Find("Hidden/InternalErrorShader");
            errorMaterial = new Material(errorShader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        var drawSettings = new DrawRendererSettings(
            camera, new ShaderPassName("ForwardBase"));
        drawSettings.SetShaderPassName(1, new ShaderPassName("PrepassBase"));
        drawSettings.SetShaderPassName(2, new ShaderPassName("Always"));
        drawSettings.SetShaderPassName(3, new ShaderPassName("Vertex"));
        drawSettings.SetShaderPassName(4, new ShaderPassName("VertexLMRGBM"));
        drawSettings.SetShaderPassName(5, new ShaderPassName("VertexLM"));

        drawSettings.SetOverrideMaterial(errorMaterial, 0);  //errorMaterial use ForwardBase

        var filterSettings = new FilterRenderersSettings(true);

        context.DrawRenderers(
            cull.visibleRenderers, ref drawSettings, filterSettings);

    }

    void ConfigureLights()
    {
        for (int i=0 ; i < cull.visibleLights.Count; i++)
        {
            if (i == maxVisibleLights)
            {
                break;
            }
            VisibleLight light = cull.visibleLights[i];
            visibleLightColors[i] = light.finalColor;

            Vector4 attenuation = Vector4.zero;
            attenuation.w = 1f;   //if no spot light, factor =1

            if (light.lightType == LightType.Directional)
            {
                Vector4 v = light.localToWorld.GetColumn(2);
                v.x = -v.x;
                v.y = -v.y;
                v.z = -v.z;  //change to vector surfaceToLight
                visibleLightDirectionsOrPositions[i] = v;
            }
            else
            {
                visibleLightDirectionsOrPositions[i] = light.localToWorld.GetColumn(3);
                attenuation.x = 1f /
                    Mathf.Max(light.range * light.range, 0.00001f);

                if (light.lightType == LightType.Spot)
                {
                    Vector4 v = light.localToWorld.GetColumn(2);
                    v.x = -v.x;
                    v.y = -v.y;
                    v.z = -v.z;
                    visibleLightSpotDirections[i] = v;

                    float outerRad = Mathf.Deg2Rad * 0.5f * light.spotAngle;
                    float outerCos = Mathf.Cos(outerRad);
                    float outerTan = Mathf.Tan(outerRad);
                    float innerCos =
                        Mathf.Cos(Mathf.Atan(((46f / 64f) * outerTan)));
                    float angleRange = Mathf.Max(innerCos - outerCos, 0.001f);
                    attenuation.z = 1f / angleRange;
                    attenuation.w = -outerCos * attenuation.z;
                }
            }

            visibleLightAttenuations[i] = attenuation;

        }

        //number of lights exceeds upper limit
        if (cull.visibleLights.Count > maxVisibleLights)
        {
            int[] lightIndices = cull.GetLightIndexMap();
            for (int i = maxVisibleLights; i < cull.visibleLights.Count; i++)
            {
                lightIndices[i] = -1;
            }
            cull.SetLightIndexMap(lightIndices);
        }

    }



}
