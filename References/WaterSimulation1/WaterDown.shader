Shader "Water/WaterDown"
{
    Properties
    {
        _FogColor("FogColor",Color) = (1,1,1,1)
        [Normal]_NormalTex ("Texture", 2D) = "white" {}
        _XSpeed("XSpeed",Range(0,1)) = 0.2
        _YSpeed("YSpeed",Range(0,1)) = 0.2
        _DistortAmount("DistortAmount",Range(0,100)) = 5
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "Assets/Shaders/1UPLight.cginc"
    #include "Assets/Shaders/1UPUtility.cginc"
    sampler2D _NormalTex;
    float4 _NormalTex_ST;
    float _XSpeed,_YSpeed,_DistortAmount;
    fixed4 _FogColor;

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 100

        Pass
        {
            Cull front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

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
                UNITY_FOG_COORDS(3)
                float4 vertex : SV_POSITION;
                float4 screenPos:TEXCOORD2;
                BASE_DATA_INPUT
            };

            sampler2D _Refract_Texture;
            float2 _Refract_Texture_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _NormalTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
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
                
                float2 grabUV = i.screenPos.xy + offset.xy * _Refract_Texture_TexelSize.xy * _DistortAmount;

                //伪折射
                fixed4 col = tex2D(_Refract_Texture,grabUV.xy);

                //漏斗光照区域
                half3 refractDir = refract(-worldViewDir,-worldNormal,1 / 1.33);
                half m = 1 - length(refractDir.xz);
                col.rgb = lerp(_FogColor,col.rgb,m);
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
}
