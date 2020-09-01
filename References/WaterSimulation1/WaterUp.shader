Shader "Water/WaterSys"
{
    Properties
    {
        _BaseColor("BaseColor",Color) = (1,1,1,1)
        _InColor("InColor",Color) = (1,1,1,1)
        _FadeStrengh("Fade",Range(0,0.5)) = 0.1
        [Normal]_NormalTex ("Texture", 2D) = "white" {}
        _FoamNoiseTex("FoamNoiseTex",2D) = "white"{}
        _FoamScale("FoamScale",float) = 1
        _FoamStrengh("FoamStrengh",float) = 1
        _FoamSpeed("FoamSpeed",Range(0,1)) = 0
        _XSpeed("XSpeed",Range(0,1)) = 0.2
        _YSpeed("YSpeed",Range(0,1)) = 0.2
        _DistortAmount("DistortAmount",Range(0,100)) = 5
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "Assets/Shaders/1UPLight.cginc"
    #include "Assets/Shaders/1UPUtility.cginc"
    sampler2D _NormalTex,_FoamNoiseTex;
    float4 _NormalTex_ST;
    float _XSpeed,_YSpeed,_DistortAmount,_FoamSpeed,_FoamStrengh,_FoamScale,_FadeStrengh;
    float4 _FoamNoiseTex_TexelSize;
    fixed4 _BaseColor,_InColor;

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        GrabPass{
            "_Refract_Texture"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                BASE_DATA_INPUT
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos:TEXCOORD2;
                BASE_DATA_INPUT
            };

            uniform sampler2D _Reflect_Texture,_CameraDepthTexture;
            float2 _Refract_Texture_TexelSize,_Reflect_Texture_TexelSize;
            sampler2D _Refract_Texture;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _NormalTex);
                o.screenPos = ComputeGrabScreenPos(o.vertex);
                BASE_DATA_VERTEX
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                i.screenPos.xy /= i.screenPos.w;
                BASE_DATA_FRAG
                MATRIX_T2W
                float3 n1 = UnpackNormal(tex2D(_NormalTex,i.uv + float2(_XSpeed,_YSpeed) * 0.1 * _Time.y));
                float3 n2 = UnpackNormal(tex2D(_NormalTex,i.uv - float2(_XSpeed,_YSpeed) * 0.1 * _Time.y));
                float3 offset = BlendNormalRNM(n1,n2);
                
                float z2 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.screenPos.xy));
                float z1 = i.screenPos.w;
                float diff = z2 - z1;
                float fade = saturate(diff * _FadeStrengh);

                float2 grabUV = i.screenPos.xy + offset.xy * _Refract_Texture_TexelSize.xy * _DistortAmount * fade;
                float2 reflectUV = i.screenPos.xy + offset.xy * _Reflect_Texture_TexelSize.xy * _DistortAmount;
                //水的深度
                fixed3 water_color = lerp(_BaseColor,_InColor, fade);

                //伪折射
                fixed4 col = tex2D(_Refract_Texture,grabUV.xy);
                col.rgb = lerp(col.rgb,water_color,fade);

                //反射
                float f = 0.8 - saturate(dot(worldViewDir,worldNormal));
                fixed3 reflectCol = tex2D(_Reflect_Texture,reflectUV.xy);

                //泡沫
                fixed3 noise_color = tex2D(_FoamNoiseTex,(i.uv * _FoamScale + _FoamNoiseTex_TexelSize.xy * 100 * _FoamSpeed));
                col.xyz += noise_color * _FoamStrengh; 
                
                col.xyz = col * (1 - f) + f * reflectCol;

                return col;
            }
            ENDCG
        }
    }
}
