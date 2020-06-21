//this shader copy camera render texture to 
//the right position in a full-screen triangle 

Shader "My Pipeline/PostEffectStack1" {
	SubShader{
		Cull Off
		ZTest Always
		ZWrite Off

		HLSLINCLUDE
		#include "../SharedLibrary/PostEffectStack1.hlsl"
		ENDHLSL

		Pass { //0 copy

			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex DefaultPassVertex
			#pragma fragment CopyPassFragment
			ENDHLSL
		}

		Pass { //1 Blur

			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex DefaultPassVertex
			#pragma fragment BlurPassFragment
			ENDHLSL
		}

		Pass { // 2 DepthStripes
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex DefaultPassVertex
			#pragma fragment DepthStripesPassFragment
			ENDHLSL
		}
	}
}