using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Callbacks;

public class MyPipelineShaderPreprocessor : IPreprocessShaders
{
    MyPipelineAsset1 pipelineAsset;
    int shaderVariantCount, strippedCount;

    static MyPipelineShaderPreprocessor instance;

    bool stripCascadedShadows, stripLODCrossFading;

    static ShaderKeyword cascadedShadowsHardKeyword =
    new ShaderKeyword("_CASCADED_SHADOWS_HARD");
    static ShaderKeyword cascadedShadowsSoftKeyword =
        new ShaderKeyword("_CASCADED_SHADOWS_SOFT");

    static ShaderKeyword lodCrossFadeKeyword =
    new ShaderKeyword("LOD_FADE_CROSSFADE");

    public MyPipelineShaderPreprocessor()
    {
        instance = this;
        pipelineAsset = GraphicsSettings.renderPipelineAsset as MyPipelineAsset1;
        if (pipelineAsset == null)
        {
            return;
        }
        stripCascadedShadows = !pipelineAsset.HasShadowCascades;
        stripLODCrossFading = !pipelineAsset.HasLODCrossFading;

    }

    public int callbackOrder
    { //order to use when multiple preprocessors exist
        get
        {
            return 0;
        }
    }

    public void OnProcessShader(
    Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        if (pipelineAsset == null)
        {  //use preprocessor only in MyPipeline1
            return;
        }

        shaderVariantCount += data.Count;
        
        //remove shader stripped
        for (int i = 0; i < data.Count; i++)
        {
            if (Strip(data[i]))
            {
                data.RemoveAt(i--);
                strippedCount += 1;
            }
        }
    }

    [PostProcessBuild(0)]   //0 is callback order for log
    static void LogVariantCount(BuildTarget target, string path)
    {  //Log
        instance.LogVariantCount();
        instance = null;
    }

    void LogVariantCount()
    {
        if (pipelineAsset == null)
        {
            return;
        }

        int finalCount = shaderVariantCount - strippedCount;
        int percentage =
            Mathf.RoundToInt(100f * finalCount / shaderVariantCount);

        Debug.Log("Included " + finalCount + " shader variants out of " +
            shaderVariantCount + " (" + percentage + "%).");
    }

    bool Strip(ShaderCompilerData data)
    {
        return
            stripCascadedShadows && (
                data.shaderKeywordSet.IsEnabled(cascadedShadowsHardKeyword) ||
                data.shaderKeywordSet.IsEnabled(cascadedShadowsSoftKeyword)
            ) ||
            stripLODCrossFading &&
            data.shaderKeywordSet.IsEnabled(lodCrossFadeKeyword);
    }


}
