// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#if !defined(MYSHADOWS_INCLUDED)
#define MYSHADOWS_INCLUDED

#include "UnityCG.cginc"

struct VertexData {
	float4 position : POSITION;
	float3 normal : NORMAL;
};

#if defined(SHADOWS_CUBE)  //point light

	struct Interpolators {
		float4 position : SV_POSITION;
		float3 lightVec : TEXCOORD0;
	};

	Interpolators MyShadowVertexProgram(VertexData v) {
		Interpolators i;
		i.position = UnityObjectToClipPos(v.position);
		i.lightVec =
			mul(unity_ObjectToWorld, v.position).xyz - _LightPositionRange.xyz;
		return i;
	}

	float4 MyShadowFragmentProgram(Interpolators i) : SV_TARGET{
		//manually calculate depth of fragment to pointlight
		float depth = length(i.lightVec) + unity_LightShadowBias.x;
		depth *= _LightPositionRange.w;
		return UnityEncodeCubeShadowDepth(depth);
	}

#else
	float4 MyShadowVertexProgram(VertexData v) : SV_POSITION{
		float4 position = UnityClipSpaceShadowCasterPos(v.position.xyz, v.normal);
		return UnityApplyLinearShadowBias(position); 
	}

	half4 MyShadowFragmentProgram() : SV_TARGET{
		return 0;
	}
#endif

#endif