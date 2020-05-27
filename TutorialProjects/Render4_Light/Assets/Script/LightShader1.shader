// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/LightShader1"
{
	Properties{
	_Tint("Tint", Color) = (1, 1, 1, 1)
	_MainTex("Albedo", 2D) = "white" {}
	}

	SubShader{
	Pass{
		Tags {
		"LightMode" = "ForwardBase"
		}

		 CGPROGRAM

		#pragma vertex MyVertexProgram
		#pragma fragment MyFragmentProgram
		//#include "UnityCG.cginc"
		#include "UnityStandardBRDF.cginc"

		struct VertexData {
			float4 position : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
		};

		struct Interpolators {
			float4 position : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 normal : TEXCOORD1;
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;
		float4 _Tint;

		Interpolators MyVertexProgram(VertexData v) {
			Interpolators i;
			i.uv = TRANSFORM_TEX(v.uv, _MainTex);
			i.position = UnityObjectToClipPos(v.position);
			//i.normal = v.normal;
			i.normal = mul(transpose((float3x3)unity_WorldToObject), v.normal);  //world space normals
			i.normal = normalize(i.normal);
			return i;
		}

		float4 MyFragmentProgram(Interpolators i) : SV_TARGET{
			
			//normals after interpolator will be slightly apart from norm 1
			//i.normal = normalize(i.normal);

			float3 lightDir = _WorldSpaceLightPos0.xyz;
			float3 lightColor = _LightColor0.rgb;
			float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;  //use Texture as albedo
			float3 diffuse = albedo * lightColor * DotClamped(lightDir, i.normal);
			return float4(diffuse, 1);
		}

		ENDCG
		}
	}
}
