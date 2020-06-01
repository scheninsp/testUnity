using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MyLightingShaderGUI : ShaderGUI
{
    Material target;
    MaterialEditor editor;
    MaterialProperty[] properties;

    static GUIContent staticLabel = new GUIContent();

    //emission HDR
    static ColorPickerHDRConfig emissionConfig =
    new ColorPickerHDRConfig(0f, 99f, 1f / 99f, 3f);

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
            RecordAction("Smoothness Source");  //support undo
            SetKeyword("_METALLIC_MAP", map.textureValue);
        }
    }

    void DoSmoothness()
    {
        MaterialProperty slider = FindProperty("_Smoothness");
        EditorGUI.indentLevel += 2;
        editor.ShaderProperty(slider, MakeLabel(slider));
        EditorGUI.indentLevel -= 2;
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
