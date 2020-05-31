Shader "Custom/ReflectionShader1"
{
	Properties
	{
		_Tint("Tint", Color) = (1, 1, 1, 1)
		_MainTex("Albedo", 2D) = "white" {}
		_Smoothness("Smoothness", Range(0, 1)) = 0.5
		[Gamma] _Metallic("Metallic", Range(0,1)) = 0.1
	}
		SubShader
		{
			Pass{
				Tags {
					"LightMode" = "ForwardBase"
				}
				CGPROGRAM
				#define FORWARD_BASE_PASS

				#pragma target 3.0

				#pragma vertex MyVertexProgram
				#pragma fragment MyFragmentProgram
				#pragma multi_compile _ SHADOWS_SCREEN  //enable shadows receiver
				

				#include "MyLightingForReflection1.cginc"


				ENDCG
			}


			Pass {
				Tags {
					"LightMode" = "ForwardAdd"
				}
				Blend One One
				ZWrite Off

				CGPROGRAM

				#pragma target 3.0

			//fwadd with shadows of secondary lights
			#pragma multi_compile_fwdadd_fullshadows

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#include "MyLightingForReflection1.cginc"

			ENDCG
		}

		Pass{
			Tags {
				"LightMode" = "ShadowCaster"  //shadow caster
			}
			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_shadowcaster //compliant with pointlight

			#pragma vertex MyShadowVertexProgram
			#pragma fragment MyShadowFragmentProgram
			#pragma multi_compile_shadowcaster

			#include "MyShadows.cginc"


			ENDCG

		}

		}
}
