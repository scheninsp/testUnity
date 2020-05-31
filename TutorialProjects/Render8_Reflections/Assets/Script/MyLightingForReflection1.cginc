// Upgrade NOTE: replaced 'UNITY_PASS_TEXCUBE(unity_SpecCube1)' with 'UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1,unity_SpecCube0)'

#if !defined(MYLIGHTING_INCLUDED)
#define MYLIGHTING_INCLUDED

#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

struct VertexData { 
	float4 vertex : POSITION;  //fix this name for macros
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
};

struct Interpolators {
	float4 pos : SV_POSITION; //fix this name for macros
	float2 uv : TEXCOORD0;
	float3 normal : TEXCOORD1;
	float3 worldPos : TEXCOORD2;

#if defined(VERTEXLIGHT_ON)
	float3 vertexLightColor : TEXCOORD3;
#endif

SHADOW_COORDS(4)  //unity macro, equal to use TEXCOORD4 , no semicolon 

};

sampler2D _MainTex;
float4 _MainTex_ST;
float4 _Tint;
float _Smoothness;
float _Metallic;

void ComputeVertexLightColor(inout Interpolators i) {
#if defined(VERTEXLIGHT_ON)
	i.vertexLightColor = Shade4PointLights(
		unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		unity_LightColor[0].rgb, unity_LightColor[1].rgb,
		unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		unity_4LightAtten0, i.worldPos, i.normal
	);
#endif
}

Interpolators MyVertexProgram(VertexData v) {
	Interpolators i;
	i.uv = TRANSFORM_TEX(v.uv, _MainTex);
	i.pos = UnityObjectToClipPos(v.vertex);
	i.worldPos = mul(unity_ObjectToWorld, v.vertex);
	i.normal = mul(transpose((float3x3)unity_WorldToObject), v.normal);  //world space normals
	i.normal = normalize(i.normal);

	TRANSFER_SHADOW(i);

	ComputeVertexLightColor(i);


	return i;
}

UnityLight CreateLight(Interpolators i) {

	UnityLight light;
	//for directional light , _WorldSpaceLightPos0 contains direction
	//fir point light, directions need to be calculated
#if defined(POINT) || defined(POINT_COOKIE) || defined(SPOT)
	light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
#else
	light.dir = _WorldSpaceLightPos0.xyz;
#endif


	//attenuation should be 0 at maximum light range
	//this function switches with #define POINT in its Pass{}
	//SHADOW_ATTENUATION(i) is included in UNITY_LIGHT_ATTENUATION
	UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);

	light.color = _LightColor0.rgb * attenuation;

	light.ndotl = DotClamped(i.normal, light.dir);
	return light;
}

float3 BoxProjection(float3 direction, float3 position,
	float4 cubemapPosition, float3 boxMin, float3 boxMax) {

#if UNITY_SPECCUBE_BOX_PROJECTION
	UNITY_BRANCH
	if (cubemapPosition.w > 0) {  //enable box projection in reflection probe settings

		// ? selection operates separately on each dimension
		// result could combine different dimensions in boxMax and boxMin
		float3 factors = ((direction > 0 ? boxMax : boxMin) - position) / direction;
		
		float scalar = min(min(factors.x, factors.y), factors.z);
		direction = direction * scalar + (position - cubemapPosition);
		//return a direction from cubemapPosition to sample point on box 
	}
#endif

	return direction;

}

UnityIndirect CreateIndirectLight(Interpolators i, float3 viewDir) {

	UnityIndirect indirectLight;
	indirectLight.diffuse = 0;
	indirectLight.specular = 0;
#if defined(VERTEXLIGHT_ON)
	indirectLight.diffuse = i.vertexLightColor;
#endif

#if defined(FORWARD_BASE_PASS)
	//spherical harmonics
	indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
	
	//reflection
	float3 reflectionDir = reflect(-viewDir, i.normal);
	
	/* custom roughness 
	float roughness = 1 - _Smoothness;
	roughness *= 1.7 - 0.7 * roughness;
	float4 envSample = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectionDir,
		roughness * UNITY_SPECCUBE_LOD_STEPS);
	indirectLight.specular = DecodeHDR(envSample, unity_SpecCube0_HDR); 
	*/

	/* unity reflection*/
	Unity_GlossyEnvironmentData envData;
	envData.roughness = 1 - _Smoothness;
	
	//envData.reflUVW = reflectionDir;
	envData.reflUVW = BoxProjection(
		reflectionDir, i.worldPos,
		unity_SpecCube0_ProbePosition,
		unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax
	);

	float3 probe0 = Unity_GlossyEnvironment(
		UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);

	//add another probe
	envData.reflUVW = BoxProjection(
		reflectionDir, i.worldPos,
		unity_SpecCube1_ProbePosition,
		unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax
	);

#if UNITY_SPECCUBE_BLENDING  //if platform support speccube blending
	float interpolator = unity_SpecCube0_BoxMin.w;
	
	UNITY_BRANCH
	if (interpolator < 0.99999) {
		//when probe1 is large enough
		float3 probe1 = Unity_GlossyEnvironment(
			UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0), unity_SpecCube0_HDR, envData
		);
		//interpolate between probes
		indirectLight.specular = lerp(probe1, probe0, interpolator);
	}
	else {
		indirectLight.specular = probe0;
	}
#else
	indirectLight.specular = probe0;
#endif




#endif



	return indirectLight;
}

float4 MyFragmentProgram(Interpolators i) : SV_TARGET{
	i.normal = normalize(i.normal);

	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
	float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;  //use Texture as albedo

	float3 specularTint;
	float oneMinusReflectivity;
	albedo = DiffuseAndSpecularFromMetallic(
		albedo, _Metallic, specularTint, oneMinusReflectivity
	);

	UnityLight light = CreateLight(i);

	UnityIndirect indirectLight = CreateIndirectLight(i, viewDir);
	
	return UNITY_BRDF_PBS(
		albedo, specularTint,
		oneMinusReflectivity, _Smoothness,
		i.normal, viewDir,
		light, indirectLight
	);
}

#endif