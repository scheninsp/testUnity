//Update with Deferred Lighting

Shader "Custom/FogShader1"
{
	Properties
	{
		_Tint("Tint", Color) = (1, 1, 1, 1)
		_MainTex("Albedo", 2D) = "white" {}
		_Smoothness("Smoothness", Range(0, 1)) = 0.5
		[Gamma] _Metallic("Metallic", Range(0,1)) = 0.1
		[NoScaleOffset] _MetallicMap("Metallic", 2D) = "white" {}

		[NoScaleOffset] _NormalMap("Normals", 2D) = "bump" {}
		_BumpScale("Bump Scale", Float) = 1

		_DetailTex("Detail Albedo", 2D) = "gray" {}
		[NoScaleOffset] _DetailNormalMap("Detail Normals", 2D) = "bump" {}
		_DetailBumpScale("Detail Bump Scale", Float) = 1

		[NoScaleOffset] _EmissionMap("Emission", 2D) = "black" {}
		_Emission("Emission", Color) = (0, 0, 0)

		_AlphaCutoff("Alpha Cutoff", Range(0, 1)) = 0.5
		//[HideInInspector] properties are determined by other properties
		[HideInInspector] _SrcBlend("_SrcBlend", Float) = 1
		[HideInInspector] _DstBlend("_DstBlend", Float) = 0
		[HideInInspector] _ZWrite("_ZWrite", Float) = 1


	}

		CGINCLUDE
		
		//default settings
		#define BINORMAL_PER_FRAGMENT
		#define FOG_DISTANCE  //this does not matters when enable deferred fog
			//deferred fog is controlled in DeferredFog.shader

		ENDCG

		SubShader
		{
			Pass{
				Tags {
					"LightMode" = "ForwardBase"
				}
				Blend [_SrcBlend] [_DstBlend]
				ZWrite [_ZWrite]

				CGPROGRAM

				#pragma target 3.0

				#pragma vertex MyVertexProgram
				#pragma fragment MyFragmentProgram
				
				#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
				#pragma shader_feature _METALLIC_MAP
				#pragma shader_feature _ _SMOOTHNESS_ALBEDO _SMOOTHNESS_METALLIC
				#pragma shader_feature _EMISSION_MAP

				#pragma multi_compile _ SHADOWS_SCREEN //enable shadows receiver
				#pragma multi_compile _ VERTEXLIGHT_ON
				#pragma multi_compile_fog

				#define FORWARD_BASE_PASS

				#include "MyLightingForFog1.cginc"


				ENDCG
			}


			Pass {
				Tags {
					"LightMode" = "ForwardAdd"
				}
				Blend [_SrcBlend] One
				ZWrite Off

				CGPROGRAM

				#pragma target 3.0
				
				#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
				#pragma shader_feature _METALLIC_MAP

				//fwadd with shadows of secondary lights
				#pragma multi_compile_fwdadd_fullshadows
				#pragma multi_compile_fog

				#pragma vertex MyVertexProgram
				#pragma fragment MyFragmentProgram

				#include "MyLightingForFog1.cginc"

				ENDCG
			}

			Pass{
				Tags {
					"LightMode" = "ShadowCaster"  //shadow caster
				}
				CGPROGRAM

				#pragma target 3.0
				
				#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
				#pragma shader_feature _SMOOTHNESS_ALBEDO
				#pragma shader_feature _SEMITRANSPARENT_SHADOWS //enable semi-transparent shadow

				#pragma multi_compile_shadowcaster //compliant with pointlight

				#pragma vertex MyShadowVertexProgram
				#pragma fragment MyShadowFragmentProgram

				#include "MyShadows.cginc"
				

				ENDCG

			}


			Pass{
				Tags {
					"LightMode" = "Deferred"
				}

				CGPROGRAM

				#pragma target 3.0
				#pragma exclude_renderers nomrt //exclude unsupported platforms

				#pragma shader_feature _ _RENDERING_CUTOUT   //transparent are rendered elsewhere
				#pragma shader_feature _METALLIC_MAP
				#pragma shader_feature _ _SMOOTHNESS_ALBEDO _SMOOTHNESS_METALLIC
				#pragma shader_feature _NORMAL_MAP
				#pragma shader_feature _OCCLUSION_MAP
				#pragma shader_feature _EMISSION_MAP
				#pragma shader_feature _DETAIL_MASK
				#pragma shader_feature _DETAIL_ALBEDO_MAP
				#pragma shader_feature _DETAIL_NORMAL_MAP
				
				#pragma multi_compile _ UNITY_HDR_ON  //support LDR version shader
				
				#pragma multi_compile_fog

				#pragma vertex MyVertexProgram
				#pragma fragment MyFragmentProgram

				#define DEFERRED_PASS  //define here

				#include "MyLightingForFog1.cginc"

				ENDCG
			}
		}

		CustomEditor "MyLightingShaderGUI"

}
