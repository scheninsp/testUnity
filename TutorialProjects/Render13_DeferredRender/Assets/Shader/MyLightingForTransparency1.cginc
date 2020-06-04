#if !defined(MYLIGHTING_INCLUDED)
#define MYLIGHTING_INCLUDED

#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

sampler2D _MainTex, _DetailTex;
float4 _MainTex_ST, _DetailTex_ST;
float4 _Tint;
float _Smoothness;
float _Metallic;
sampler2D _MetallicMap;
sampler2D _NormalMap, _DetailNormalMap;
float _BumpScale, _DetailBumpScale;
sampler2D _EmissionMap;
float3 _Emission;
float _AlphaCutoff;

struct VertexData { 
	float4 vertex : POSITION;  //fix this name for macros
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float4 uv : TEXCOORD0;
};

struct Interpolators {
	float4 pos : SV_POSITION; //fix this name for macros
	float4 uv : TEXCOORD0;
	float3 normal : TEXCOORD1;
	#if defined(BINORMAL_PER_FRAGMENT)
		float4 tangent : TEXCOORD2;
	#else
		float3 tangent : TEXCOORD2;
		float3 binormal : TEXCOORD3;
	#endif
	float3 worldPos : TEXCOORD4;

	#if defined(VERTEXLIGHT_ON)
		float3 vertexLightColor : TEXCOORD5;
	#endif

	SHADOW_COORDS(6)  //unity macro, equal to use TEXCOORD4 , no semicolon 

};

float GetMetallic(Interpolators i) {
#if defined(_METALLIC_MAP)
	return tex2D(_MetallicMap, i.uv.xy).r;  //r channel
#else
	return _Metallic;
#endif
}

//use albedo A as smoothness first
//if not defined or no albedo, use metallic map A as smoothness
float GetSmoothness(Interpolators i) {
	float smoothness = 1;
#if defined(_SMOOTHNESS_ALBEDO)
	smoothness = tex2D(_MainTex, i.uv.xy).a;
#elif defined(_SMOOTHNESS_METALLIC) && defined(_METALLIC_MAP)
	smoothness = tex2D(_MetallicMap, i.uv.xy).a;
#endif
	return smoothness * _Smoothness;
}

float3 GetEmission(Interpolators i) {
#if defined(FORWARD_BASE_PASS)  || defined(DEFERRED_PASS)
#if defined(_EMISSION_MAP)
	return tex2D(_EmissionMap, i.uv.xy) * _Emission;
#else
	return _Emission;
#endif
#else
	return 0;
#endif
}

float GetAlpha(Interpolators i) {
	float alpha = _Tint.a;
#if !defined(_SMOOTHNESS_ALBEDO)
	alpha *= tex2D(_MainTex, i.uv.xy).a;
#endif
	return alpha;
}

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

float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign) {
	return cross(normal, tangent.xyz) *
		(binormalSign * unity_WorldTransformParams.w);
}


Interpolators MyVertexProgram(VertexData v) {
	Interpolators i;

	i.pos = UnityObjectToClipPos(v.vertex);
	i.worldPos = mul(unity_ObjectToWorld, v.vertex);
	i.normal = UnityObjectToWorldNormal(v.normal);
	i.normal = normalize(i.normal);

	#if defined(BINORMAL_PER_FRAGMENT)
		i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
	#else
		i.tangent = UnityObjectToWorldDir(v.tangent.xyz);
		i.binormal = CreateBinormal(i.normal, i.tangent, v.tangent.w);
	#endif

	i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
	i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);

	TRANSFER_SHADOW(i);

	ComputeVertexLightColor(i);

	return i;
}

UnityLight CreateLight(Interpolators i) {

	UnityLight light;
	#if defined(DEFERRED_PASS)
		//do not generate light here in deferred pass
		light.dir = float3(0, 1, 0);
		light.color = 0;
	#else

		//for directional light , _WorldSpaceLightPos0 contains direction
		//fir point light, directions need to be calculated
		#if defined(POINT) || defined(POINT_COOKIE) || defined(SPOT)
			light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
		#else
			light.dir = _WorldSpaceLightPos0.xyz;
		#endif
	#endif

	//attenuation should be 0 at maximum light range
	//this function switches with #define POINT in its Pass{}
	//SHADOW_ATTENUATION(i) is included in UNITY_LIGHT_ATTENUATION
	UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);

	light.color = _LightColor0.rgb * attenuation;

	return light;
}

float3 BoxProjection(
	float3 direction, float3 position,
	float4 cubemapPosition, float3 boxMin, float3 boxMax
) {
#if UNITY_SPECCUBE_BOX_PROJECTION
	UNITY_BRANCH
		if (cubemapPosition.w > 0) {
			float3 factors =
				((direction > 0 ? boxMax : boxMin) - position) / direction;
			float scalar = min(min(factors.x, factors.y), factors.z);
			direction = direction * scalar + (position - cubemapPosition);
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

	//spherical harmonics
#if defined(FORWARD_BASE_PASS) || defined(DEFERRED_PASS)
	indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));

	//reflection
	float3 reflectionDir = reflect(-viewDir, i.normal);

	/* unity reflection*/
	Unity_GlossyEnvironmentData envData;
	envData.roughness = 1 - GetSmoothness(i);

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

	//UNITY_ENABLE_REFLECTION_BUFFERS = 0 when deferred reflections are disabled
	#if defined(DEFERRED_PASS) && UNITY_ENABLE_REFLECTION_BUFFERS
		indirectLight.specular = 0;
	#endif


#endif
	return indirectLight;
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


struct FragmentOutput {
//use 4 buffers for deferred lighting
#if defined(DEFERRED_PASS)
	float4 gBuffer0 : SV_Target0;
	float4 gBuffer1 : SV_Target1;
	float4 gBuffer2 : SV_Target2;
	float4 gBuffer3 : SV_Target3;
#else
	float4 color : SV_Target;
#endif
};


FragmentOutput MyFragmentProgram(Interpolators i) {

	//clip fragments 
	float alpha = GetAlpha(i);
	#if defined(_RENDERING_CUTOUT)
		clip(alpha - _AlphaCutoff);
	#endif
	InitializeFragmentNormal(i);

	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
	float3 albedo = tex2D(_MainTex, i.uv.xy).rgb * _Tint.rgb;  //use Texture as albedo
	albedo *= tex2D(_DetailTex, i.uv.zw) * unity_ColorSpaceDouble;

	float3 specularTint;
	float oneMinusReflectivity;
	albedo = DiffuseAndSpecularFromMetallic(
		albedo, GetMetallic(i), specularTint, oneMinusReflectivity
	);
	//premultiplied alpha
	#if defined(_RENDERING_TRANSPARENT)
		albedo *= alpha;
		alpha = 1 - oneMinusReflectivity + alpha * oneMinusReflectivity;
	#endif

	UnityLight light = CreateLight(i);

	UnityIndirect indirectLight = CreateIndirectLight(i, viewDir);
	
	float4 color = UNITY_BRDF_PBS(
		albedo, specularTint,
		oneMinusReflectivity, GetSmoothness(i),
		i.normal, viewDir,
		CreateLight(i), CreateIndirectLight(i, viewDir)
	);
	color.rgb += GetEmission(i);
	#if defined(_RENDERING_FADE) || defined(_RENDERING_TRANSPARENT)
		color.a = alpha;   //output alpha value for blending fragments
	#endif

	FragmentOutput output;
	#if defined(DEFERRED_PASS)
		//LDR support
		#if !defined(UNITY_HDR_ON)
			color.rgb = exp2(-color.rgb);
		#endif	
	
		//buffer0 : diffuse albedo , surface occlusion
		output.gBuffer0.rgb = albedo;
		output.gBuffer0.a = 1;  
		
		//buffer1 : specular color, smoothness
		output.gBuffer1.rgb = specularTint;
		output.gBuffer1.a = GetSmoothness(i);

		//buffer2 : world-space normals, need a format transform
		output.gBuffer2 = float4(i.normal * 0.5 + 0.5, 1);

		//buffer3 : acuumulated lighting
		output.gBuffer3 = color;


	#else
			output.color = color;
	#endif
	return output;

}

#endif