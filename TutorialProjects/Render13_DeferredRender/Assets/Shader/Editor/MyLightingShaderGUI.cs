using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class MyLightingShaderGUI : ShaderGUI
{
    Material target;
    MaterialEditor editor;
    MaterialProperty[] properties;

    static GUIContent staticLabel = new GUIContent();

    enum SmoothnessSource
    {
        Uniform, Albedo, Metallic
    }

    enum RenderingMode
    {
        Opaque, Cutout, Fade, Transparent
    }

    struct RenderingSettings
    {
        public RenderQueue queue;
        public string renderType;
        public BlendMode srcBlend, dstBlend;
        public bool zWrite;

        public static RenderingSettings[] modes = {
            new RenderingSettings() {
                queue = RenderQueue.Geometry,
                renderType = "",
                srcBlend = BlendMode.One,
                dstBlend = BlendMode.Zero,
                zWrite = true
            },
            new RenderingSettings() {
                queue = RenderQueue.AlphaTest,
                renderType = "TransparentCutout",
                srcBlend = BlendMode.One,
                dstBlend = BlendMode.Zero,
                zWrite = true
            },
            new RenderingSettings() {
                queue = RenderQueue.Transparent,
                renderType = "Transparent",
                srcBlend = BlendMode.SrcAlpha,
                dstBlend = BlendMode.OneMinusSrcAlpha,
                zWrite = false
            },
            new RenderingSettings() {
                queue = RenderQueue.Transparent,
                renderType = "Transparent",
                srcBlend = BlendMode.One,
                dstBlend = BlendMode.OneMinusSrcAlpha,
                zWrite = false
            }
        };
    }

    bool shouldShowAlphaCutoff;

    //emission HDR
    static ColorPickerHDRConfig emissionConfig =
    new ColorPickerHDRConfig(0f, 99f, 1f / 99f, 3f);


    void DoRenderingMode()
    {
        RenderingMode mode = RenderingMode.Opaque;
        shouldShowAlphaCutoff = false;
        if (IsKeywordEnabled("_RENDERING_CUTOUT"))
        {
            shouldShowAlphaCutoff = true;
            mode = RenderingMode.Cutout;
        }
        else if (IsKeywordEnabled("_RENDERING_FADE"))
        {
            mode = RenderingMode.Fade;
        }
        else if (IsKeywordEnabled("_RENDERING_TRANSPARENT"))
        {
            mode = RenderingMode.Transparent;
        }


        EditorGUI.BeginChangeCheck();
        mode = (RenderingMode)EditorGUILayout.EnumPopup(
            MakeLabel("Rendering Mode"), mode
        );
        if (EditorGUI.EndChangeCheck())
        {
            RecordAction("Rendering Mode");
            SetKeyword("_RENDERING_CUTOUT", mode == RenderingMode.Cutout);
            SetKeyword("_RENDERING_FADE", mode == RenderingMode.Fade);
            SetKeyword( "_RENDERING_TRANSPARENT", mode == RenderingMode.Transparent);

            //render queue , render type
            RenderingSettings settings = RenderingSettings.modes[(int)mode];

            foreach (Material m in editor.targets)
            {
                m.renderQueue = (int)settings.queue;
                m.SetOverrideTag("RenderType", settings.renderType);
                m.SetInt("_SrcBlend", (int)settings.srcBlend);
                m.SetInt("_DstBlend", (int)settings.dstBlend);
                m.SetInt("_ZWrite", settings.zWrite ? 1 : 0);
            }
        }

        if (mode == RenderingMode.Fade || mode == RenderingMode.Transparent)
        {
            DoSemitransparentShadows();
        }
    }


    bool IsKeywordEnabled(string keyword)
    {
        return target.IsKeywordEnabled(keyword);
    }

    static GUIContent MakeLabel(string text, string tooltip = null)
    {
        staticLabel.text = text;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }

    static GUIContent MakeLabel(MaterialProperty property, string tooltip = null)
    {
        staticLabel.text = property.displayName;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }

    MaterialProperty FindProperty(string name)
    {
        return FindProperty(name, properties);
    }

    void DoSemitransparentShadows()
    {
        EditorGUI.BeginChangeCheck();

        bool semitransparentShadows =
            EditorGUILayout.Toggle(
                MakeLabel("Semitransp. Shadows", "Semitransparent Shadows"),
                IsKeywordEnabled("_SEMITRANSPARENT_SHADOWS")
            );
        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_SEMITRANSPARENT_SHADOWS", semitransparentShadows);
        }

        if (!semitransparentShadows)
        {
            shouldShowAlphaCutoff = true;
        }
    }

    void DoMetallic()
    {
        MaterialProperty map = FindProperty("_MetallicMap");
        EditorGUI.BeginChangeCheck();

        //metallic factor "_Metallic" will always be 1 if using metallic map
        editor.TexturePropertySingleLine(
            MakeLabel(map, "Metallic (R)"), map,
            map.textureValue ? null : FindProperty("_Metallic")
        );

        if (EditorGUI.EndChangeCheck())  //changes have been made, regenerate GUI 
        {
            SetKeyword("_METALLIC_MAP", map.textureValue);
        }
    }

    void DoSmoothness()
    {
        SmoothnessSource source = SmoothnessSource.Uniform;
        if (IsKeywordEnabled("_SMOOTHNESS_ALBEDO"))
        {
            source = SmoothnessSource.Albedo;
        }
        else if (IsKeywordEnabled("_SMOOTHNESS_METALLIC"))
        {
            source = SmoothnessSource.Metallic;
        }

        MaterialProperty slider = FindProperty("_Smoothness");
        EditorGUI.indentLevel += 2;
        editor.ShaderProperty(slider, MakeLabel(slider));
        EditorGUI.indentLevel += 1;
        EditorGUI.BeginChangeCheck();
        source = (SmoothnessSource)EditorGUILayout.EnumPopup(MakeLabel("Source"), source);
        if (EditorGUI.EndChangeCheck())
        {
            RecordAction("Smoothness Source");  //support undo
            SetKeyword("_SMOOTHNESS_ALBEDO", source == SmoothnessSource.Albedo);
            SetKeyword(
                "_SMOOTHNESS_METALLIC", source == SmoothnessSource.Metallic
            );
        }
        EditorGUI.indentLevel -= 3;
    }

    void DoEmission()
    {
        MaterialProperty map = FindProperty("_EmissionMap");
        EditorGUI.BeginChangeCheck();
        editor.TexturePropertyWithHDRColor(
            MakeLabel("Emission (RGB)"), map, FindProperty("_Emission"),
            emissionConfig, false
        );
        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_EMISSION_MAP", map.textureValue);
        }
    }

    void DoAlphaCutoff()
    {
        MaterialProperty slider = FindProperty("_AlphaCutoff");
        EditorGUI.indentLevel += 2;
        editor.ShaderProperty(slider, MakeLabel(slider));
        EditorGUI.indentLevel -= 2;
    }

    //add normal map
    void DoNormals()
    {
        MaterialProperty map = FindProperty("_NormalMap");
        editor.TexturePropertySingleLine(MakeLabel(map), map, FindProperty("_BumpScale"));
    }



    void DoMain()
    {
        GUILayout.Label("Main Maps", EditorStyles.boldLabel);

        //add selection box for albedo
        MaterialProperty mainTex = FindProperty("_MainTex");
        MaterialProperty tint = FindProperty("_Tint");
        editor.TexturePropertySingleLine(MakeLabel(mainTex, "Albedo (RGB)"), mainTex, tint);

        //add metallic and smoothness 
        DoMetallic();
        DoSmoothness();
        DoEmission();

        //add normal map
        DoNormals();
        if(shouldShowAlphaCutoff == true)
        {
            DoAlphaCutoff();
        }

        //add tiling and offset for main texture
        editor.TextureScaleOffsetProperty(mainTex);

    }

    void DoSecondaryNormals()
    {
        MaterialProperty map = FindProperty("_DetailNormalMap");
        editor.TexturePropertySingleLine(
            MakeLabel(map), map,
            map.textureValue ? FindProperty("_DetailBumpScale") : null
        );
    }

    void DoSecondary()
    {
        GUILayout.Label("Secondary Maps", EditorStyles.boldLabel);

        MaterialProperty detailTex = FindProperty("_DetailTex");
        editor.TexturePropertySingleLine(
            MakeLabel(detailTex, "Albedo (RGB) multiplied by 2"), detailTex
        );

        DoSecondaryNormals();

        editor.TextureScaleOffsetProperty(detailTex);
    }

    void SetKeyword(string keyword, bool state)
    {
        if (state)
        {
            target.EnableKeyword(keyword);
        }
        else
        {
            target.DisableKeyword(keyword);
        }
    }

    public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
    {
        this.target = editor.target as Material;
        this.editor = editor;
        this.properties = properties;

        DoRenderingMode();

        //properties for main texture
        DoMain();

        //properties for secondary texture
        DoSecondary();

    }

    void RecordAction(string label)
    {
        editor.RegisterPropertyChangeUndo(label);
    }

}
