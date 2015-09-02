Shader "Custom/MultiPassXRay" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		
		_EmissionColorA ( "Emission Color A", Color ) = ( 1, 1, 1, 1 )
		_EmissionColorB ( "Emission Color B", Color ) = ( 1, 1, 1, 1 )
		_EmissionStrength ( "Emission Strength", float ) = 1.0
		
		_NoiseScale ( "Noise Scale", float ) = 3.0
		_NoiseSpeed ( "Noise Speed", float ) = 1.0
		
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		// PASS 1:
		Zwrite On
		
		CGPROGRAM
		
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
		
		// PASS 2:
		
		Zwrite On
		ZTest Greater
		Lighting Off
		
		CGPROGRAM		
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		
		// hash based 3d value noise
	    // function taken from [url]https://www.shadertoy.com/view/XslGRr[/url]
	    // Created by inigo quilez - iq/2013
	    // License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
	     
	    float hash( float n )
		{
		    return frac(sin(n)*43758.5453);
		}
		
	    float noise( float3 x )
	    {
	        // The noise function returns a value in the range -1.0f -> 1.0f
	     
	        float3 p = floor(x);
	        float3 f = frac(x);
	     
	        f       = f*f*(3.0-2.0*f);
	        float n = p.x + p.y*57.0 + 113.0*p.z;
	     
	        return lerp(lerp(lerp( hash(n+0.0), hash(n+1.0),f.x),
	                       lerp( hash(n+57.0), hash(n+58.0),f.x),f.y),
	                   lerp(lerp( hash(n+113.0), hash(n+114.0),f.x),
	                       lerp( hash(n+170.0), hash(n+171.0),f.x),f.y),f.z);
	    }
		
		struct Input 
		{
			float4 screenPos;
		};

		half _Glossiness;
		half _Metallic;
		
		fixed4 _EmissionColorA;
		fixed4 _EmissionColorB;
		half _EmissionStrength;
		
		half _NoiseSpeed;
		half _NoiseScale;

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
			screenUV *= noise( screenUV.xyx * 30 ) * 8;
		
			// Albedo comes from a texture tinted by color
			fixed4 c = lerp( _EmissionColorA, _EmissionColorB, noise( screenUV.xyx * _NoiseScale + _Time.y * _NoiseSpeed ) );
			o.Albedo = c.rgb;
			o.Emission = c * _EmissionStrength;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
