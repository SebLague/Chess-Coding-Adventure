Shader "Vis/Quad"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "Queue"="Transparent" }
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			float3 PosA, PosB, PosC, PosD;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			float4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				float3 objectSpaceVert = v.vertex;
				float ta = objectSpaceVert.x < -0.4 && objectSpaceVert.y > 0.4;
				float tb = objectSpaceVert.x > 0.4 && objectSpaceVert.y > 0.4;
				float tc = objectSpaceVert.x < -0.4 && objectSpaceVert.y < -0.4;
				float td = objectSpaceVert.x > 0.4 && objectSpaceVert.y < -0.4;
				objectSpaceVert = PosA * ta + PosB * tb + PosC * tc + PosD * td;

				o.vertex = UnityObjectToClipPos(float4(objectSpaceVert, v.vertex.w));
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return _Color;
			}
			ENDCG
		}
	}
}
