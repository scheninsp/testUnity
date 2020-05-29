Shader "Custom/TangentSpaceShader"
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
		
			#define BINORMAL_PER_FRAGMENT

			//#include "UnityCG.cginc"
			#include "UnityStandardBRDF.cginc"
			#include "UnityStandardUtils.cginc"

			struct VertexData {
				float4 position : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 uv : TEXCOORD0;
			};

			struct Interpolators {
				float4 position : SV_POSITION;
				float4 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
				#if defined(BINORMAL_PER_FRAGMENT)
					float4 tangent : TEXCOORD2;  
				#else
					float3 tangent : TEXCOORD2;
					float3 binormal : TEXCOORD3;
				#endif
				float3 worldPos : TEXCOORD4;
			};

			sampler2D _MainTex, _DetailTex;
			float4 _MainTex_ST, _DetailTex_ST;
			float4 _Tint;
			float _Smoothness;
			float _Metallic;
			sampler2D _NormalMap, _DetailNormalMap;
			float _BumpScale, _DetailBumpScale;

			float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign) {
				return cross(normal, tangent.xyz) *
					(binormalSign * unity_WorldTransformParams.w);
			}

			Interpolators MyVertexProgram(VertexData v) {
				Interpolators i;
				i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);
				i.position = UnityObjectToClipPos(v.position);
				i.worldPos = mul(unity_ObjectToWorld, v.position);
				i.normal = UnityObjectToWorldNormal(v.normal);

				#if defined(BINORMAL_PER_FRAGMENT)
					i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
				#else
					i.tangent = UnityObjectToWorldDir(v.tangent.xyz);
					i.binormal = CreateBinormal(i.normal, i.tangent, v.tangent.w);
				#endif

				return i;
			}

			void InitializeFragmentNormal(inout Interpolators i) {

				/* unity conversion */
				float3 mainNormal = UnpackScaleNormal(tex2D(_NormalMap, i.uv.xy), _BumpScale);
				float3 detailNormal = UnpackScaleNormal(tex2D(_DetailNormalMap, i.uv.zw), _DetailBumpScale);

				//tangent space normal
				float3 tangentSpaceNormal = BlendNormals(mainNormal, detailNormal);

				#if defined(BINORMAL_PER_FRAGMENT)
					float3 binormal = CreateBinormal(i.normal, i.tangent.xyz, i.tangent.w);
				#else
					float3 binormal = i.binormal;
				#endif

				//tangentSpaceNormal is xzy order
				//TBN vectors does not need to be normalized
				i.normal = normalize(
					tangentSpaceNormal.x * i.tangent +
					tangentSpaceNormal.y * binormal +
					tangentSpaceNormal.z * i.normal
				);
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
