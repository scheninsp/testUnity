Shader "Custom/VertexLightShader"
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

				#pragma target 3.0

				#pragma multi_compile _ VERTEXLIGHT_ON

				#pragma vertex MyVertexProgram
				#pragma fragment MyFragmentProgram

				#define FORWARD_BASE_PASS  //control shperical harmonics

				#include "MyLighting.cginc"


				ENDCG
			}

		/*
			Pass {
				Tags {
					"LightMode" = "ForwardAdd"
				}
				Blend One One
				ZWrite Off

				CGPROGRAM

				#pragma target 3.0

				#pragma multi_compile_fwdadd
			//#pragma multi_compile DIRECTIONAL DIRECTIONAL_COOKIE POINT SPOT

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#include "MyLighting.cginc"

			ENDCG
		}
		*/

		}
}
