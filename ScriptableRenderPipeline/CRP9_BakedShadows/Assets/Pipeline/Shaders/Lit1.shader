Shader "My Pipeline/Lit1"
{
    Properties
    {
		_Color("Color", Color) = (1, 1, 1, 1)
		_MainTex("Albedo & Alpha", 2D) = "white" {}
		_Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5
		_Smoothness("Smoothness", Range(0, 1)) = 0.5
		_Metallic("Metallic", Range(0, 1)) = 0

		[KeywordEnum(Off, On, Shadows)] _Clipping("Alpha Clipping", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2 
		//Cull mode, using a enum menu

		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0
		[Enum(Off, 0, On, 1)] _ZWrite("Z Write", Float) = 1

		[Toggle(_RECEIVE_SHADOWS)] _ReceiveShadows("Receive Shadows", Float) = 1
		[Toggle(_PREMULTIPLY_ALPHA)] _PremulAlpha("Premultiply Alpha", Float) = 0
		
		[HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 0)

    }
    SubShader
    {
        Pass
        {
			Cull [_Cull]
			Blend[_SrcBlend][_DstBlend]
			ZWrite [_ZWrite]

			HLSLPROGRAM

			#pragma target 3.5
			
			#pragma multi_compile_instancing  //GPU Instancing
			
			#pragma shader_feature _CLIPPING_ON  //CLIPPING_OFF and SHADOWS are the same
			#pragma shader_feature _RECEIVE_SHADOWS
			#pragma shader_feature _PREMULTIPLY_ALPHA

			#pragma multi_compile _ _CASCADED_SHADOWS_HARD _CASCADED_SHADOWS_SOFT
			#pragma multi_compile _ _SHADOWS_HARD
			#pragma multi_compile _ _SHADOWS_SOFT  
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile _ _SHADOWMASK _DISTANCE_SHADOWMASK _SUBTRACTIVE_LIGHTING

			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment

			#include "../SharedLibrary/Lit1.hlsl"

			ENDHLSL
        }

		Pass {
			Tags {
				"LightMode" = "ShadowCaster"
			}

			Cull[_Cull]

			HLSLPROGRAM

			#pragma target 3.5

			#pragma multi_compile_instancing

			#pragma shader_feature _CLIPPING_OFF   //CLIPPING_ON and SHADOWS are the same

			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment

			#include "../SharedLibrary/ShadowCaster1.hlsl"

			ENDHLSL
		}

		Pass{
			Tags {
				"LightMode" = "Meta"
			}

			Cull Off

			HLSLPROGRAM

			#pragma vertex MetaPassVertex
			#pragma fragment MetaPassFragment

			#include "../SharedLibrary/Meta1.hlsl"

			ENDHLSL
		}
    }

	
	CustomEditor "LitShaderGUI"

}
