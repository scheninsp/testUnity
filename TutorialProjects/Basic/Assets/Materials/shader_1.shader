// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/shader_1"
{
	SubShader{
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			fixed4 _MtColor;

			struct v2f {
				float4 pos : POSITION;
				half3 worldNormal : TEXCOORD0;
				//fixed3 worldNormal : COLOR0;

			};

			v2f vert(appdata_base v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = v.normal;
				//o.worldNormal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target{
				fixed3 c = i.worldNormal*0.5 + fixed3(0.5, 0.5, 0.5);
				return fixed4(c, 1);

				//return _MtColor;
			}
			ENDCG
		}
	}
}
