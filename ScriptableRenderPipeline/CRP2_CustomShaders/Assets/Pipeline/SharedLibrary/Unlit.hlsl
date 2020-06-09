#ifndef MYRP_UNLIT_INCLUDED
#define MYRP_UNLIT_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

//difference between HLSL and CG, you need to explicitly declare
CBUFFER_START(UnityPerFrame)
float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
float4x4 unity_ObjectToWorld;
CBUFFER_END

#define UNITY_MATRIX_M unity_ObjectToWorld

//this must come after our self-defined macros
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

//Instanced Color
UNITY_INSTANCING_BUFFER_START(PerInstance)  //PerInstance is defined in macro
	UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(PerInstance)


struct VertexInput {
	float4 pos : POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
	float4 clipPos : SV_POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};


VertexOutput UnlitPassVertex(VertexInput input) {
	VertexOutput output;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);  //fragment shader also need this
	float4 worldPos = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0));
	output.clipPos = mul(unity_MatrixVP, worldPos);
	return output;
}

float4 UnlitPassFragment(VertexOutput input) : SV_TARGET{
	UNITY_SETUP_INSTANCE_ID(input);  //you have to re-setup instance id in fragment shader
	return UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color);
}

#endif