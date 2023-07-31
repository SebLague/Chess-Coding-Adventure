Shader "Vis/ShadedAlpha"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 200
   
        CGPROGRAM
 
        #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma target 3.0

		sampler2D _MainTex;
		

		struct Input
		{
			float2 uv_MainTex;
		};

		float4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = _Color;
			o.Metallic = 0;
			o.Smoothness = 0.2;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
