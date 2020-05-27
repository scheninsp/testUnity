﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/MetallicLightShader1"
{
	Properties{
	_Tint("Tint", Color) = (1, 1, 1, 1)
	_MainTex("Albedo", 2D) = "white" {}
	_Smoothness("Smoothness", Range(0, 1)) = 0.5
	[Gamma] _Metallic ("Metallic", Range(0,1)) = 0.1 
	//unity function of metallic is supposed in gamma
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
		#include "UnityStandardUtils.cginc"

		struct VertexData {
			float4 position : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
		};

		struct Interpolators {
			float4 position : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 normal : TEXCOORD1;
			float3 worldPos : TEXCOORD2;
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;
		float4 _Tint;
		float _Smoothness;
		float _Metallic;

		Interpolators MyVertexProgram(VertexData v) {
			Interpolators i;
			i.uv = TRANSFORM_TEX(v.uv, _MainTex);
			i.position = UnityObjectToClipPos(v.position);
			i.worldPos = mul(unity_ObjectToWorld, v.position);
			i.normal = mul(transpose((float3x3)unity_WorldToObject), v.normal);  //world space normals
			i.normal = normalize(i.normal);
			return i;
		}

		float4 MyFragmentProgram(Interpolators i) : SV_TARGET{

			float3 lightDir = _WorldSpaceLightPos0.xyz;
			float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
			float3 lightColor = _LightColor0.rgb;
			float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;  //use Texture as albedo
			
			/*simple version
			float3 specularTint = albedo * _Metallic;
			float oneMinusReflectivity = 1 - _Metallic;
			albedo *= oneMinusReflectivity;
			*/

			/*unity version*/
			float3 specularTint; 
			float oneMinusReflectivity; 
			albedo = DiffuseAndSpecularFromMetallic(
				albedo, _Metallic, specularTint, oneMinusReflectivity
			);

			float3 diffuse = albedo * lightColor * DotClamped(lightDir, i.normal);

			float3 halfVector = normalize(lightDir + viewDir);
			float3 specular = specularTint * pow(DotClamped(halfVector, i.normal),
				_Smoothness * 100);

			return float4(specular + diffuse ,1);
		}

		ENDCG
		}
	}
}
