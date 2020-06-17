using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

using UnityEngine.Experimental.GlobalIllumination;
using LightType = UnityEngine.LightType;

public class MyPipeline1 : RenderPipeline
{

    CullResults cull;  //fix here to reduce GC consumption

    CommandBuffer cameraBuffer = new CommandBuffer { name = "Render Camera" };

    CommandBuffer shadowBuffer = new CommandBuffer { name = "Render Shadows" };

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
    static int shadowMapId =
        Shader.PropertyToID("_ShadowMap");
    static int worldToShadowMatricesId =
        Shader.PropertyToID("_WorldToShadowMatrices");
    static int shadowBiasId =
        Shader.PropertyToID("_ShadowBias");
    static int shadowMapSizeId =
        Shader.PropertyToID("_ShadowMapSize");
    static int shadowDataId =
        Shader.PropertyToID("_ShadowData");
    //.x = shadow strength,  .y = true if soft shadows 
    //.z >0 is directional, after judged, zw are tileOffset * tileScale
    static int globalShadowDataId =
        Shader.PropertyToID("_GlobalShadowData");
    //_GlobalShadowData : Vector4 ( tileScale, shadowDistance^2, cascadedNumber,...)
    static int cascadedShadowMapId =
        Shader.PropertyToID("_CascadedShadowMap");
    static int worldToShadowCascadeMatricesId =
        Shader.PropertyToID("_WorldToShadowCascadeMatrices");
    static int cascadedShadowMapSizeId =
        Shader.PropertyToID("_CascadedShadowMapSize");
    static int cascadedShadoStrengthId =
        Shader.PropertyToID("_CascadedShadowStrength");
    static int cascadeCullingSpheresId =
        Shader.PropertyToID("_CascadeCullingSpheres");

    Vector4[] visibleLightColors = new Vector4[maxVisibleLights];
    Vector4[] visibleLightDirectionsOrPositions = new Vector4[maxVisibleLights];
    Vector4[] visibleLightAttenuations = new Vector4[maxVisibleLights];
    Vector4[] visibleLightSpotDirections = new Vector4[maxVisibleLights];

    RenderTexture shadowMap, cascadedShadowMap;

    int shadowMapSize;

    const string shadowsSoftKeyword = "_SHADOWS_SOFT";
    const string shadowsHardKeyword = "_SHADOWS_HARD";

    Vector4[] shadowData = new Vector4[maxVisibleLights];
    Matrix4x4[] worldToShadowMatrices = new Matrix4x4[maxVisibleLights];

    int shadowTileCount;

    float shadowDistance;

    int shadowCascades;
    Vector3 shadowCascadeSplit;

    bool mainLightExists;
    Matrix4x4[] worldToShadowCascadeMatrices = new Matrix4x4[5];  //add one for outside all cascades

    const string cascadedShadowsHardKeyword = "_CASCADED_SHADOWS_HARD";
    const string cascadedShadowsSoftKeyword = "_CASCADED_SHADOWS_SOFT";

    Vector4[] cascadeCullingSpheres = new Vector4[4];

    #if UNITY_EDITOR
    //fix lightdata for point light GI and others
        static Lightmapping.RequestLightsDelegate lightmappingLightsDelegate =
            (Light[] inputLights, NativeArray<LightDataGI> outputLights) => {
                LightDataGI lightData = new LightDataGI();
                for (int i = 0; i < inputLights.Length; i++)
                {
                    Light light = inputLights[i];
                    switch (light.type)
                    {
                        case LightType.Directional:
                            var directionalLight = new DirectionalLight();
                            LightmapperUtils.Extract(light, ref directionalLight);
                            lightData.Init(ref directionalLight);
                            break;
                        case LightType.Point:
                            var pointLight = new PointLight();
                            LightmapperUtils.Extract(light, ref pointLight);
                            lightData.Init(ref pointLight);
                            break;
                        case LightType.Spot:
                            var spotLight = new SpotLight();
                            LightmapperUtils.Extract(light, ref spotLight);
                            lightData.Init(ref spotLight);
                            break;
                        case LightType.Area:
                            var rectangleLight = new RectangleLight();
                            LightmapperUtils.Extract(light, ref rectangleLight);
                            lightData.Init(ref rectangleLight);
                            break;
                        default:
                            lightData.InitNoBake(light.GetInstanceID());
                            break;
                    }
                    lightData.falloff = FalloffType.InverseSquared;
                    outputLights[i] = lightData;
                }
            };
    #endif

    public MyPipeline1(bool dynamicBatching, bool instancing,
        int shadowMapSize, float shadowDistance,
        int shadowCascades, Vector3 shadowCascadeSplit)  //constructor with configurations
    {
        GraphicsSettings.lightsUseLinearIntensity = true;
        if (SystemInfo.usesReversedZBuffer)
        {  //set the shadow case for outside all cascades
            worldToShadowCascadeMatrices[4].m33 = 1f;
        }

        if (dynamicBatching)
        {
            drawFlags = DrawRendererFlags.EnableDynamicBatching;
        }

        if (instancing)
        {
            drawFlags |= DrawRendererFlags.EnableInstancing;
        }

        this.shadowMapSize = shadowMapSize;
        this.shadowDistance = shadowDistance;
        this.shadowCascades = shadowCascades;
        this.shadowCascadeSplit = shadowCascadeSplit;

        #if UNITY_EDITOR
            Lightmapping.SetDelegate(lightmappingLightsDelegate);
        #endif
    }

    #if UNITY_EDITOR
    public override void Dispose()
    {  //release delegate
        base.Dispose();
        Lightmapping.ResetDelegate();
    }
    #endif

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
        cullingParameters.shadowDistance = Mathf.Min(shadowDistance, camera.farClipPlane);

#if UNITY_EDITOR
        //add UI to SceneWindow
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
#endif

        CullResults.Cull(ref cullingParameters, context, ref cull);

        //shadows
        if (cull.visibleLights.Count > 0)
        {
            ConfigureLights();
            if (mainLightExists)
            {
                RenderCascadedShadows(context);
            }
            else
            {
                cameraBuffer.DisableShaderKeyword(cascadedShadowsHardKeyword);
                cameraBuffer.DisableShaderKeyword(cascadedShadowsSoftKeyword);
            }

            if (shadowTileCount > 0)
            {
                RenderShadows(context);
            }
            else
            {  //no shadows
                cameraBuffer.DisableShaderKeyword(shadowsHardKeyword);
                cameraBuffer.DisableShaderKeyword(shadowsSoftKeyword);
            }
        }
        else  //no lights
        {
            cameraBuffer.SetGlobalVector(lightIndicesOffsetAndCountID, Vector4.zero);
            cameraBuffer.DisableShaderKeyword(cascadedShadowsHardKeyword);
            cameraBuffer.DisableShaderKeyword(cascadedShadowsSoftKeyword);
            cameraBuffer.DisableShaderKeyword(shadowsHardKeyword);
            cameraBuffer.DisableShaderKeyword(shadowsSoftKeyword);
        }

        //Setup Camera parameters
        context.SetupCameraProperties(camera);  //set MVP matrix, etc.

        //Setup Command Buffer and Execute
        CameraClearFlags clearFlags = camera.clearFlags;

        cameraBuffer.ClearRenderTarget(
            (clearFlags & CameraClearFlags.Depth) != 0,
            (clearFlags & CameraClearFlags.Color) != 0,
            camera.backgroundColor
        );


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

        if (cull.visibleLights.Count > 0)
        {
            drawSettings.rendererConfiguration = RendererConfiguration.PerObjectLightIndices8;
        }
        drawSettings.rendererConfiguration |=
                RendererConfiguration.PerObjectReflectionProbes |
                RendererConfiguration.PerObjectLightmaps |
                RendererConfiguration.PerObjectLightProbe |
                RendererConfiguration.PerObjectLightProbeProxyVolume;

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

        if (shadowMap)
        {
            RenderTexture.ReleaseTemporary(shadowMap);
            shadowMap = null;
        }
        if (cascadedShadowMap)
        {
            RenderTexture.ReleaseTemporary(cascadedShadowMap);
            cascadedShadowMap = null;
        }


    }//end Render


    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPEMENT_BUILD")]
    void DrawDefaultPipeline(ScriptableRenderContext context, Camera camera) {
        //skip the default unity pipelines
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
        mainLightExists = false;
        shadowTileCount = 0;

        for (int i = 0; i < cull.visibleLights.Count; i++)
        {
            if (i == maxVisibleLights)
            {
                break;
            }
            VisibleLight light = cull.visibleLights[i];
            visibleLightColors[i] = light.finalColor;

            Vector4 attenuation = Vector4.zero;
            attenuation.w = 1f;   //if no spot light, factor =1

            Vector4 shadow = Vector4.zero;

            if (light.lightType == LightType.Directional)
            {
                Vector4 v = light.localToWorld.GetColumn(2);
                v.x = -v.x;
                v.y = -v.y;
                v.z = -v.z;  //change to vector surfaceToLight
                visibleLightDirectionsOrPositions[i] = v;

                shadow = ConfigureShadows(i, light.light);
                shadow.z = 1f;

                //directional light index =0, cast shadow, use cascades
                if (i == 0 && shadow.x > 0f && shadowCascades > 0)
                {
                    mainLightExists = true;
                    shadowTileCount -= 1;   //cascaded shadow map is not included in tiled shadow map
                }
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

                    //buffer shadowData
                    shadow = ConfigureShadows(i, light.light);
                }
            }

            visibleLightAttenuations[i] = attenuation;
            shadowData[i] = shadow;

        }

        //number of lights exceeds upper limit
        if (mainLightExists || cull.visibleLights.Count > maxVisibleLights)
        {
            int[] lightIndices = cull.GetLightIndexMap();
            if (mainLightExists)
            {
                //remove main light from diffuseLight render, avoid a second render
                lightIndices[0] = -1;
            }
            for (int i = maxVisibleLights; i < cull.visibleLights.Count; i++)
            {
                lightIndices[i] = -1;
            }
            cull.SetLightIndexMap(lightIndices);
        }

    }

    Vector4 ConfigureShadows(int lightIndex, Light shadowLight)
    {
        Vector4 shadow = Vector4.zero;
        Bounds shadowBounds;
        if (shadowLight.shadows != LightShadows.None &&
            cull.GetShadowCasterBounds(lightIndex, out shadowBounds))
        {
            shadowTileCount += 1;
            shadow.x = shadowLight.shadowStrength;
            shadow.y = shadowLight.shadows == LightShadows.Soft ? 1f : 0f;
        }
        return shadow;
    }

    RenderTexture SetShadowRenderTarget()
    {
        RenderTexture texture = RenderTexture.GetTemporary(shadowMapSize, shadowMapSize, 16, RenderTextureFormat.Shadowmap);

        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        //set GPU prepared for shadowMap
        CoreUtils.SetRenderTarget(shadowBuffer, texture,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                ClearFlag.Depth);

        return texture;

    }

    Vector2 ConfigureShadowTile(int tileIndex, int split, float tileSize)
    {
        Vector2 tileOffset;
        tileOffset.x = tileIndex % split;
        tileOffset.y = tileIndex / split;
        var tileViewport = new Rect(
            tileOffset.x * tileSize, tileOffset.y * tileSize, tileSize, tileSize
        );

        //camera viewport does not affect light VP matrix
        shadowBuffer.SetViewport(tileViewport);

        //scissor edges for soft shadow filter does not get cross multiple shadowmaps
        shadowBuffer.EnableScissorRect(new Rect(
            tileViewport.x + 4f, tileViewport.y + 4f,
            tileSize - 8f, tileSize - 8f
        ));

        return tileOffset;
    }
    void CalculateWorldToShadowMatrix( ref Matrix4x4 viewMatrix, ref Matrix4x4 projectionMatrix,
    out Matrix4x4 worldToShadowMatrix)
    {
        //prepare worldToShadowMatrices for shadow receiver
        if (SystemInfo.usesReversedZBuffer)  //if use z=-1 for near clip plane
        {
            projectionMatrix.m20 = -projectionMatrix.m20;
            projectionMatrix.m21 = -projectionMatrix.m21;
            projectionMatrix.m22 = -projectionMatrix.m22;
            projectionMatrix.m23 = -projectionMatrix.m23;
        }
        //[-1,1] in clip space to [0,1] in texture space
        // var scaleOffset = Matrix4x4.TRS(Vector3.one * 0.5f, Quaternion.identity, Vector3.one * 0.5f);
        var scaleOffset = Matrix4x4.identity;
        scaleOffset.m00 = scaleOffset.m11 = scaleOffset.m22 = 0.5f;
        scaleOffset.m03 = scaleOffset.m13 = scaleOffset.m23 = 0.5f;

        worldToShadowMatrix = scaleOffset * projectionMatrix * viewMatrix;
    }

void RenderShadows(ScriptableRenderContext context)
    {
        int split;   //split to N*N tiles
        if (shadowTileCount <= 1)
        {
            split = 1;
        }
        else if (shadowTileCount <= 4)
        {
            split = 2;
        }
        else if (shadowTileCount <= 9)
        {
            split = 3;
        }
        else
        {
            split = 4;
        }

        float tileSize = shadowMapSize / split;   //maximum light = 16
        float tileScale = 1f / split;

        shadowMap = SetShadowRenderTarget();

        shadowBuffer.BeginSample("Render Shadows");

        shadowBuffer.SetGlobalVector(globalShadowDataId, 
            new Vector4(tileScale, shadowDistance * shadowDistance, 0f));

        context.ExecuteCommandBuffer(shadowBuffer);
        shadowBuffer.Clear();

        int tileIndex = 0;
        bool hardShadows = false;
        bool softShadows = false;
        for (int i = mainLightExists ? 1 : 0 ; i < cull.visibleLights.Count; i++)
        {
            if (i == maxVisibleLights)
            {
                break;
            }
            //shadowStrength <=0 indicates the light needs no shadowmap
            if (shadowData[i].x <= 0f)
            {
                continue;
            }
            
            //render shadowMap for light i
            Matrix4x4 viewMatrix, projectionMatrix;
            ShadowSplitData splitData;  

            bool validShadows;
            if (shadowData[i].z > 0f)
            {
                validShadows =
                    cull.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                        i, 0, 1, Vector3.right, (int)tileSize,
                        cull.visibleLights[i].light.shadowNearPlane,
                        out viewMatrix, out projectionMatrix, out splitData);
            }
            else
            {
                validShadows = cull.ComputeSpotShadowMatricesAndCullingPrimitives(
                i, out viewMatrix, out projectionMatrix, out splitData);
            }

            if (!validShadows)
            {
                shadowData[i].x = 0f;
                continue;
            }


            Vector2 tileOffset = ConfigureShadowTile(tileIndex, split, tileSize);
            shadowData[i].z = tileOffset.x * tileScale;
            shadowData[i].w = tileOffset.y * tileScale;

            shadowBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

            shadowBuffer.SetGlobalFloat(
                shadowBiasId, cull.visibleLights[i].light.shadowBias
            );  // shadow bias

            context.ExecuteCommandBuffer(shadowBuffer);
            shadowBuffer.Clear();

            var shadowSettings = new DrawShadowsSettings(cull, i);
            shadowSettings.splitData.cullingSphere = splitData.cullingSphere;
            context.DrawShadows(ref shadowSettings);

            CalculateWorldToShadowMatrix( ref viewMatrix, ref projectionMatrix,
                out worldToShadowMatrices[i]);

            tileIndex += 1;

            //for generate shader keyword of all soft/all hard shadows
            if (shadowData[i].y <= 0f)
            {
                hardShadows = true;
            }
            else
            {
                softShadows = true;
            }
        }

        shadowBuffer.DisableScissorRect();

        //sample shadowMap
        shadowBuffer.SetGlobalTexture(shadowMapId, shadowMap);

        shadowBuffer.SetGlobalMatrixArray(worldToShadowMatricesId, worldToShadowMatrices);
        //_ShadowData , including shadow strength
        shadowBuffer.SetGlobalVectorArray(shadowDataId, shadowData);


        //soft shadows, tent filter inputs
        float invShadowMapSize = 1f / shadowMapSize;
        shadowBuffer.SetGlobalVector(
            shadowMapSizeId, new Vector4(
                invShadowMapSize, invShadowMapSize, shadowMapSize, shadowMapSize
            )
        );

        CoreUtils.SetKeyword(shadowBuffer, shadowsHardKeyword, hardShadows);
        CoreUtils.SetKeyword(shadowBuffer, shadowsSoftKeyword,softShadows);

        shadowBuffer.EndSample("Render Shadows");
        context.ExecuteCommandBuffer(shadowBuffer);
        shadowBuffer.Clear();

    }


    void RenderCascadedShadows(ScriptableRenderContext context)
    {
        float tileSize = shadowMapSize / 2;  

        cascadedShadowMap = SetShadowRenderTarget();

        shadowBuffer.BeginSample("Render Shadows");

        shadowBuffer.SetGlobalVector(globalShadowDataId,
            new Vector4(0f, shadowDistance * shadowDistance, shadowCascades));

        context.ExecuteCommandBuffer(shadowBuffer);
        shadowBuffer.Clear();

        Light shadowLight = cull.visibleLights[0].light;
        shadowBuffer.SetGlobalFloat(shadowBiasId, shadowLight.shadowBias);

        var shadowSettings = new DrawShadowsSettings(cull, 0);
        var tileMatrix = Matrix4x4.identity;
        tileMatrix.m00 = tileMatrix.m11 = 0.5f;

        for (int i = 0; i < shadowCascades; i++)
        {
            //render shadowMap for light i
            Matrix4x4 viewMatrix, projectionMatrix;
            ShadowSplitData splitData;

            cull.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                   0, i, shadowCascades, shadowCascadeSplit, (int)tileSize,
                    shadowLight.shadowNearPlane,
                    out viewMatrix, out projectionMatrix, out splitData);
           

            Vector2 tileOffset = ConfigureShadowTile(i, 2, tileSize);

            shadowBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

            context.ExecuteCommandBuffer(shadowBuffer);
            shadowBuffer.Clear();

            //select the appropriate cascade
            cascadeCullingSpheres[i] = shadowSettings.splitData.cullingSphere = splitData.cullingSphere;
            // use a squared distance for comparison
            cascadeCullingSpheres[i].w *= splitData.cullingSphere.w;

            context.DrawShadows(ref shadowSettings);

            CalculateWorldToShadowMatrix(ref viewMatrix, ref projectionMatrix,
                out worldToShadowCascadeMatrices[i]);

            tileMatrix.m03 = tileOffset.x * 0.5f;
            tileMatrix.m13 = tileOffset.y * 0.5f;
            worldToShadowCascadeMatrices[i] =
                tileMatrix * worldToShadowCascadeMatrices[i];
        }

        shadowBuffer.DisableScissorRect();

        //sample shadowMap
        shadowBuffer.SetGlobalTexture(cascadedShadowMapId, cascadedShadowMap);

        shadowBuffer.SetGlobalVectorArray(cascadeCullingSpheresId, cascadeCullingSpheres);

        shadowBuffer.SetGlobalMatrixArray(worldToShadowCascadeMatricesId, worldToShadowCascadeMatrices);

        //set keyword of cascaded
        float invShadowMapSize = 1f / shadowMapSize;
        shadowBuffer.SetGlobalVector(
            cascadedShadowMapSizeId, new Vector4(
                invShadowMapSize, invShadowMapSize, shadowMapSize, shadowMapSize
            )
        );
        shadowBuffer.SetGlobalFloat(
            cascadedShadoStrengthId, shadowLight.shadowStrength
        );
        bool hard = shadowLight.shadows == LightShadows.Hard;
        CoreUtils.SetKeyword(shadowBuffer, cascadedShadowsHardKeyword, hard);
        CoreUtils.SetKeyword(shadowBuffer, cascadedShadowsSoftKeyword, !hard);

        shadowBuffer.EndSample("Render Shadows");
        context.ExecuteCommandBuffer(shadowBuffer);
        shadowBuffer.Clear();

    }
}
