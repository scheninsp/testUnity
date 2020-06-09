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

    public MyPipeline(bool dynamicBatching, bool instancing)  //constructor with configurations
    {
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

        cameraBuffer.BeginSample("Render Camera");  //start a sub-level

        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();  

        //Draw Opaque, Unlit Shader
        var drawSettings = new DrawRendererSettings(
            camera, new ShaderPassName("SRPDefaultUnlit"));
        drawSettings.flags = drawFlags;
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

}
