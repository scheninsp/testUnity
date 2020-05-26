// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Shader1"
{
   SubShader{
	   Pass{
			CGPROGRAM

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram
			#pragma enable_d3d11_debug_symbols
			
			float4 MyVertexProgram(float4 position : POSITION,
				out float3 localPosition : TEXCOORD0) :SV_POSITION {
				
				localPosition = position.xyz;
				return UnityObjectToClipPos(position);;
			}

			float4 MyFragmentProgram(float4 position : POSITION, 
				float3 localPosition : TEXCOORD0 ) : SV_TARGET {
				return float4(localPosition + 0.5,1.0);
			}

			ENDCG
		}
   }
}
