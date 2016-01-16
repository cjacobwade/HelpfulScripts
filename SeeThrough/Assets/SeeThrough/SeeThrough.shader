Shader "Custom/SeeThrough" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_DoClip ("Do Clip", float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// This file is where the magic happens! It should be in the same folder as this file
		#include "SeeThroughUtils.cginc"

		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert addshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float4 worldPosition;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		half _DoClip;

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.worldPosition = mul(_Object2World, v.vertex);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			float blockingTarget = 0.99 - IsFragBlockingAnyTarget(IN.worldPosition);
			clip(lerp(blockingTarget, 1.0, step(0.1, _DoClip)));
		}
		ENDCG
	}
	FallBack "Diffuse"
}
