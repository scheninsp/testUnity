Shader "Custom/MultipleLightShader1"
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
			LOD 200
			CGPROGRAM

			#pragma target 3.0

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#include "MyLightingForShadows.cginc"


			ENDCG
		}
			
		
		Pass {
			Tags {
				"LightMode" = "ForwardAdd"
			}
			LOD 200
			Blend One One
			ZWrite Off

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_fwdadd
			//#pragma multi_compile DIRECTIONAL DIRECTIONAL_COOKIE POINT SPOT

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#include "MyLightingForShadows.cginc"

			ENDCG
		}
		
    }
}
