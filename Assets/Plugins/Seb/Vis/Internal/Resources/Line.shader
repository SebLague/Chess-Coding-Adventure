Shader "Vis/Line"
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

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			float4 _Color;
			float2 _Size;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float distanceToLineSegment(float2 p, float2 a1, float2 a2)
			{
				float2 lineDelta = a2 - a1;
				float2 pointDelta = p - a1;
				float sqrLineLength = dot(lineDelta, lineDelta);

				if (sqrLineLength == 0) {
					return a1;
				}

				float t = saturate(dot(pointDelta, lineDelta) / sqrLineLength);
				float2 pointOnLineSeg = a1 + lineDelta * t;
				return length(p - pointOnLineSeg);
			}

			float4 frag (v2f i) : SV_Target
			{
				float len = _Size.x;
				float thickness = _Size.y;

				float2 pointOnLine = i.uv * _Size;
				float2 pointA = float2(thickness * 0.5, thickness * 0.5);
				float2 pointB = float2(len - thickness * 0.5, thickness * 0.5);

				float dst = distanceToLineSegment(pointOnLine, pointA, pointB) / (thickness/2);

				float delta = fwidth(dst);
				float alpha = 1 - smoothstep(1 - delta * 2, 1, dst * dst);

				return float4(_Color.rgb, _Color.a * alpha);
			}
			ENDCG
		}
	}
}
