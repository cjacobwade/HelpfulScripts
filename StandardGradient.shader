Shader "Custom/StandardGradient"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Color2 ("Color2", Color) = (1, 1, 1, 1)

		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_GradientScale ("Gradient Scale", Float) = 1.0
		_GradientOffset ("Gradient Offset", Float) = 0.0

		_BumpMap ("Normal (RGB)", 2D) = "bump" {}
		_Bump("Normal Strength", Float) = 1

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard vertex:vert fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;

		half _Bump;
		half _GradientScale;
		half _GradientOffset;

		struct Input
		{
			float2 uv_MainTex;
			float4 localPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _Color2;

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.localPos = v.vertex;
		}

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 main = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 c = main * _Color;
			fixed4 c2 = main * _Color2;
			o.Albedo = lerp(c.rgb, c2.rgb, saturate((IN.localPos.y + _GradientOffset) * _GradientScale));

			o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_MainTex), _Bump);

			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
