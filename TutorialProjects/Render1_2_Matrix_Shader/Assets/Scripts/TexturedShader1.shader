// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TexturedShader1"
{
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_MainTex_ST("Texture Scale Translate", Vector) = (0,0,0,0)
   }

    SubShader
    {
		Pass{
		CGPROGRAM
		
		#pragma vertex MyVertexProgram
		#pragma fragment MyFragmentProgram
	
		struct VertexData {
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
			};

		struct Interpolators {
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

		sampler2D _MainTex;
		float4 _MainTex_ST;

		Interpolators MyVertexProgram(VertexData v
			) {
				Interpolators i;
				i.position = UnityObjectToClipPos(v.position);
				i.uv = v.uv;
				i.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
				return i;
			}

		float4 MyFragmentProgram(Interpolators i) : SV_TARGET{
			return tex2D(_MainTex, i.uv);
		}

		ENDCG
		}

    }
}
