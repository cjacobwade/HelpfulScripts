Shader "Particle/Airwave"
{
	Properties
	{
		_TintColor("Color", Color) = (1, 1, 1, 1)
		_WaveNormal("Wave Normal", 2D) = "white" {}
		_WaveStrength("Wave Strength", Float) = 1
	}

	SubShader
	{
		// Draw ourselves after all opaque geometry
		Tags{ "Queue" = "Transparent+1" "RenderType"="Transparent" }

		// Grab the screen behind the object into _BackgroundTexture
		GrabPass { "_BackgroundTexture"}

		Blend SrcAlpha OneMinusSrcAlpha

		// Render the object with the texture generated above, and invert the colors
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _BackgroundTexture;
			sampler2D _WaveNormal;

			float _WaveStrength;
			fixed4 _TintColor;

			// Soft particles
			sampler2D_float _CameraDepthTexture;
			float _InvFade;

			struct v2f
			{
				float4 grabPos : TEXCOORD0;
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
				float2 uv_WaveNormal : TEXCOORD1;

#ifdef SOFTPARTICLES_ON
				float4 projPos : TEXCOORD2;
#endif
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				// use UnityObjectToClipPos from UnityCG.cginc to calculate 
				// the clip-space of the vertex
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

#ifdef SOFTPARTICLES_ON
				o.projPos = ComputeScreenPos(o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
#endif

				// use ComputeGrabScreenPos function from UnityCG.cginc
				// to get the correct texture coordinate
				o.grabPos = ComputeGrabScreenPos(o.pos);

				o.color = v.color;
				o.uv_WaveNormal = v.texcoord;

				return o;
			}

			half4 frag(v2f i) : SV_Target
			{

#ifdef SOFTPARTICLES_ON
				float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
				float partZ = i.projPos.z;
				float fade = saturate(_InvFade * (sceneZ - partZ));
				i.color.a *= fade;
#endif

				half3 wave = UnpackNormal(tex2D(_WaveNormal, i.uv_WaveNormal.xy));

				i.grabPos.xy += wave.xy / wave.z * _WaveStrength * i.color.a;
				half4 bgcolor = tex2Dproj(_BackgroundTexture, i.grabPos);
				return bgcolor;
			}
			ENDCG
		}

	}
	FallBack "Particle/AlphaBlended"
}