// MIT License - Chris Wade, 2017

Shader "Custom/ForwardDecal"
{
	Properties
	{
		_Color ("Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Diffuse", 2D) = "white" {}
		_BumpMap ("Bump Map", 2D) = "bump" {}
	}
	SubShader
	{
		Tags{"RenderType" = "Transparent" "Queue" = "Transparent" "DisableBatching"="True"}
		LOD 200

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		// Some things you won't want to project on. We need to use the Stencil buffer 
		// The Stencil Buffer associates each drawn fragment with a number, then lets us reference that in later renders of the same fragment
		
		// First, add this to the shaders of the things you don't want to project onto:
		/*
		Stencil
		{
			Ref 1
			Comp Always
			Pass Replace
		}
		*/

		// Then uncomment this here so this shader knows to reject things
		/*
		Stencil
		{
			Ref 0
			Comp Equal
		}
		*/

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			#include "UnityCG.cginc"
			#include "UnityStandardUtils.cginc"

			struct appdata
            {
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 localPos : TEXCOORD0;
				float4 screenUV : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.pos = UnityObjectToClipPos(v.vertex);
				o.localPos = v.vertex;

				o.screenUV = ComputeScreenPos(o.pos);
				return o;
			}

			sampler2D _MainTex;
			sampler2D _MaskTex;
			sampler2D _BumpMap;

			sampler2D_float _CameraDepthTexture;
			sampler2D_float _CameraDepthNormalsTexture;
			sampler2D _NormalsCopy;

			fixed4 _Color;
			float4 _LightColor0; 

			fixed4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);

				// Get view space vector toward this fragment
				float3 ray = mul(UNITY_MATRIX_MV, i.localPos).xyz * float3(-1, -1, 1);
				ray *= (_ProjectionParams.z / ray.z); // Far clip dist/viewspace distance

				float2 screenUV = i.screenUV.xy / i.screenUV.w;
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
				depth = Linear01Depth(depth);

				float4 vpos = float4(ray * depth, 1); // View space projection onto surface in depth buffer
				float3 wpos = mul(unity_CameraToWorld, vpos).xyz; // Transform to world space
				float3 opos = mul(unity_WorldToObject, float4(wpos, 1)).xyz; // Transform to object space

				clip(float3(0.5, 0.5, 0.5) - abs(opos.xyz)); // Clip any fragments outside our box

				// Next we want to clip any surfaces not facing us
				float4 dn = tex2D(_CameraDepthNormalsTexture, screenUV);
				float3 normal = DecodeViewNormalStereo(dn) * float3(1, 1, -1);
				float3 worldN = mul((half3x3)unity_CameraToWorld, normal); // Transform normals to world space

				float3 orientation = mul(unity_ObjectToWorld, float3(0, 1, 0)); // Transform objects up to world space
				clip(dot(worldN, orientation) + 0.2); // Clip anything that's not facing mostly up

				float2 texUV = opos.xz + 0.5; // Treat surface projection's object space xz position as UVs (use z instead of y because we're using object's y as the projection direction)
				fixed4 col = tex2D(_MainTex, texUV) * _Color;

				float3 orientationX = mul((float3x3)unity_ObjectToWorld, float3(1, 0, 0));
				float3 orientationZ = mul((float3x3)unity_ObjectToWorld, float3(0, 0, 1));
				half3x3 norMat = half3x3(orientationX, orientationZ, orientation);
				normal = UnpackNormal(tex2D(_BumpMap, texUV));
				normal = normal * 0.5 + 0.5;
				
				float3 viewDir = normalize(_WorldSpaceCameraPos - wpos);

				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
				float3 diffuse = _LightColor0.rgb * col.rgb
					* saturate(dot((worldN + normal) * 0.5, lightDir) * 2) + 0.1;

				return float4(diffuse, col.a);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
