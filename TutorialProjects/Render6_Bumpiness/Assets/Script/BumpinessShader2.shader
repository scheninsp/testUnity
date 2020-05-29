Shader "Custom/BumpinessShader2"
{
	Properties{
	_Tint("Tint", Color) = (1, 1, 1, 1)
	_MainTex("Albedo", 2D) = "white" {}
	[NoScaleOffset] _NormalMap("Normals", 2D) = "bump" {}
	_BumpScale("Bump Scale", Float) = 1
	_Smoothness("Smoothness", Range(0, 1)) = 0.5
	[Gamma] _Metallic("Metallic", Range(0,1)) = 0.1
	//unity function of metallic is supposed in gamma
	_DetailTex("Detail Texture", 2D) = "gray" {}
	[NoScaleOffset] _DetailNormalMap("Detail Normals", 2D) = "bump" {}
	_DetailBumpScale("Detail Bump Scale", Float) = 1 

	}

		SubShader{
		Pass{
			Tags {
			"LightMode" = "ForwardBase"
			}

			 CGPROGRAM
		
			#pragma target 3.0

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram
		//#include "UnityCG.cginc"
		#include "UnityStandardBRDF.cginc"
		#include "UnityStandardUtils.cginc"

		struct VertexData {
			float4 position : POSITION;
			float3 normal : NORMAL;
			float4 uv : TEXCOORD0;
		};

		struct Interpolators {
			float4 position : SV_POSITION;
			float4 uv : TEXCOORD0;
			float3 normal : TEXCOORD1;
			float3 worldPos : TEXCOORD2;
		};

		sampler2D _MainTex, _DetailTex;
		float4 _MainTex_ST, _DetailTex_ST;
		float4 _Tint;
		float _Smoothness;
		float _Metallic;
		sampler2D _NormalMap, _DetailNormalMap;
		float _BumpScale, _DetailBumpScale;

		Interpolators MyVertexProgram(VertexData v) {
			Interpolators i;
			i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
			i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);
			i.position = UnityObjectToClipPos(v.position);
			i.worldPos = mul(unity_ObjectToWorld, v.position);
			i.normal = mul(transpose((float3x3)unity_WorldToObject), v.normal);  //world space normals
			i.normal = normalize(i.normal);
			return i;
		}

		void InitializeFragmentNormal(inout Interpolators i) {
			/* manual conversion of normals
			i.normal.xy = tex2D(_NormalMap, i.uv).wy * 2-1;
			i.normal.xy *= _BumpScale;
			i.normal.z = sqrt(1 - saturate(dot(i.normal.xy, i.normal.xy)));
			*/

			/* unity conversion */
			float3 mainNormal = UnpackScaleNormal(tex2D(_NormalMap, i.uv.xy), _BumpScale);
			float3 detailNormal = UnpackScaleNormal(tex2D(_DetailNormalMap, i.uv.zw), _DetailBumpScale);
			
			//different blending of normals
			//i.normal = (mainNormal + detailNormal) * 0.5;
			//i.normal =
			//float3(mainNormal.xy / mainNormal.z + detailNormal.xy / detailNormal.z, 1);
			//i.normal =
			//	float3(mainNormal.xy + detailNormal.xy, mainNormal.z * detailNormal.z);
			
			//whiteout blending 
			i.normal = BlendNormals(mainNormal, detailNormal);

			i.normal = i.normal.xzy;
			i.normal = normalize(i.normal);
		}

		float4 MyFragmentProgram(Interpolators i) : SV_TARGET{

			InitializeFragmentNormal(i);

			float3 lightDir = _WorldSpaceLightPos0.xyz;
			float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
			float3 lightColor = _LightColor0.rgb;
			float3 albedo = tex2D(_MainTex, i.uv.xy).rgb * _Tint.rgb;  //use Texture as albedo
			albedo *= tex2D(_DetailTex, i.uv.zw).rgb * unity_ColorSpaceDouble;

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
