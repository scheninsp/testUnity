Shader "UnityChan/Skin"
{
	Properties
	{
		_Color ("Main Color", Color) = (1, 1, 1, 1)
		_ShadowColor ("Shadow Color", Color) = (0.8, 0.8, 1, 1)
		_EdgeThickness ("Outline Thickness", Float) = 1
				
		_MainTex ("Diffuse", 2D) = "white" {}
		_FalloffSampler ("Falloff Control", 2D) = "white" {}
		_RimLightSampler ("RimLight Control", 2D) = "white" {}
	}

	SubShader
	{


		Pass
		{
			Tags
			{
				"RenderType" = "Opaque"
				"Queue" = "Geometry"
				"LightMode" = "ForwardBase"
			}

			Cull Back
			ZTest LEqual

			CGPROGRAM
			#pragma multi_compile_fwdbase
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			#include "CharaSkin.cg"
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

			#pragma multi_compile_fwdadd

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityStandardBRDF.cginc"
			#include "AutoLight.cginc"
			
			#define POINT
			#include "CharaSkinAdd.cg"

			ENDCG
		}

		
		Pass
		{
			Cull Front
			ZTest Less

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "CharaOutline.cg"
			ENDCG
		}

	}

	FallBack "Transparent/Cutout/Diffuse"
}
