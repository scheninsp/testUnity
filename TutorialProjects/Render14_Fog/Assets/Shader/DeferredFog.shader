Shader "Custom/DeferredFog" {

	Properties{
		_MainTex("Source", 2D) = "white" {}
	}

		SubShader{
			Cull Off
			ZTest Always
			ZWrite Off

			Pass {
			CGPROGRAM

				#pragma vertex VertexProgram
				#pragma fragment FragmentProgram

				#pragma multi_compile_fog
				
				#define FOG_DISTANCE
				//#define FOG_SKYBOX

				#include "UnityCG.cginc"
				
				//_CameraDepthTexture is the depth buffer of main camera
				sampler2D _MainTex, _CameraDepthTexture;
				float3 _FrustumCorners[4];

				struct VertexData {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct Interpolators {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					#if defined(FOG_DISTANCE)
						float3 ray : TEXCOORD1;
					#endif
				};

				Interpolators VertexProgram(VertexData v) {
					Interpolators i;
					i.pos = UnityObjectToClipPos(v.vertex);
					i.uv = v.uv;
					#if defined(FOG_DISTANCE)
						i.ray = _FrustumCorners[v.uv.x + 2 * v.uv.y];
					#endif
					return i;
				}

				float4 FragmentProgram(Interpolators i) : SV_Target {
						float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
						//from [x,y,z,w] to 1 clip space value		
						depth = Linear01Depth(depth);
						
						#if defined(FOG_DISTANCE)
							float viewDistance = length(i.ray * depth);
						#else
							//[0,1] -> [-near_end, far_end_of_clip_space - near_end]
							float viewDistance = depth * _ProjectionParams.z - _ProjectionParams.y;
						#endif

						UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
						unityFogFactor = saturate(unityFogFactor);
						#if !defined(FOG_SKYBOX)
							if (depth > 0.9999) {
								unityFogFactor = 1;
							}
						#endif
						//if fog disabled 
						#if !defined(FOG_LINEAR) && !defined(FOG_EXP) && !defined(FOG_EXP2)
							unityFogFactor = 1;
						#endif

						float3 sourceColor = tex2D(_MainTex, i.uv).rgb;
						float3 foggedColor =
							lerp(unity_FogColor.rgb, sourceColor, unityFogFactor);
						return float4(foggedColor, 1);
					}

			ENDCG
			}
	}
}