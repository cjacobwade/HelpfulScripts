Shader "Sausage/Foliage/GrassGradient"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_Color2("Color2", Color) = (1, 1, 1, 1)

		_GradientScale("Gradient Scale", Float) = 0.15
		_GradientOffset("Gradient Offset", Float) = -3.66

		_Gradient("Gradient Top Highlight", Float) = 1.89
		_Ambient("Ambient", Float) = 0.53

		_GrassHeight("Grass Height", Float) = 0.32
		_GrassWidth("Grass Width", Float) = 0.24

		_WindSpeed("Wind Speed", Float) = 5
		_WindStength("Wind Strength", Float) = 0.08
		_WindDir("Wind Direction", Vector) = (0.4, 0.1, 0.5, 0)
		_WindNoise("Wind Noise Scale", Float) = 8
	}
		
	SubShader
	{
		LOD 200

		Pass
		{ 
			Name "FORWARD"
			Tags
			{
				"Queue" = "Geometry"
				"IgnoreProjector" = "True"
				"RenderType" = "Grass"
				"LightMode" = "ForwardBase"
			}

			CGPROGRAM
			
			#pragma vertex vert 
			#pragma fragment frag
			#pragma geometry geom

			#pragma multi_compile_fwdbase
			#pragma target 4.0

			#include "UnityCG.cginc" 		
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			half _Gradient;
			half _Ambient;

			half _GrassHeight;
			half _GrassWidth;

			half _WindStength;
			half _WindSpeed;
			half4 _WindDir;
			half _WindNoise;

			fixed4 _Color;
			fixed4 _Color2;

			half _GradientScale;
			half _GradientOffset;

			struct v2g
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 norm : TEXCOORD1; 
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 norm : TEXCOORD1;
				float4 worldPos : TEXCOORD2;
				SHADOW_COORDS(3) // put shadows data into TEXCOORD1
			};

      		float hash(float n)
			{
				return frac(sin(n)*43758.5453);
			}

			float noise(float3 v)
			{
				// The noise function returns a value in the range -1.0f -> 1.0f

				float3 p = floor(v);
				float3 f = frac(v);

				f = f * f*(3.0 - 2.0*f);
				float n = p.x + p.y*57.0 + 113.0*p.z;

				return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
					lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
					lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
					lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
			}

			v2g vert(appdata_base v)
			{
				float3 v0 = v.vertex.xyz;

				v2g OUT;

				OUT.pos = v.vertex;
				OUT.norm = v.normal;
				OUT.uv = v.texcoord;

				return OUT;
			}

			[maxvertexcount(3)]
			void geom(point v2g IN[1], inout TriangleStream<g2f> triStream)
			{
				float4 worldPos = mul(unity_ObjectToWorld, IN[0].pos);
				float3 viewDir = _WorldSpaceCameraPos.xyz - worldPos.xyz;
				viewDir.y = 0;
				viewDir = normalize(viewDir);

				float3 forward = -viewDir;
				float3 perpendicularAngle = cross(float3(0, 1, 0), forward);
				float3 faceNormal = cross(perpendicularAngle, IN[0].norm);

				float3 v0 = IN[0].pos.xyz;

				float height = lerp(_GrassHeight * 0.85, _GrassHeight * 1.15, sin(v0.z));
				float3 v1 = IN[0].pos.xyz + IN[0].norm * height;

				float noiseOffset = noise(worldPos.xyz) * _WindNoise;
				v1 += normalize(_WindDir) * (sin(_Time.y * _WindSpeed + noiseOffset) * 0.5 + 0.5) * _WindStength;

				g2f OUT;

				// Quad 1
				half3 worldNormal = UnityObjectToWorldNormal(faceNormal);

				float width = lerp(_GrassWidth * 0.85, _GrassWidth * 1.15, sin(v0.x * 3));

				// BOTW sheen
				half nl = max(0, dot(faceNormal, _WorldSpaceLightPos0.xyz)) + 0.5;
				half4 diff = lerp(nl, pow(nl + 0.31, 20) - 0.5, nl + 0.3) * _LightColor0;

				OUT.worldPos.w = ShadeSH9(half4(worldNormal, 1));

				float3 left = v0 - perpendicularAngle * 0.5 * width;
				OUT.pos = UnityObjectToClipPos(left);
				OUT.worldPos.xyz = mul(unity_ObjectToWorld, left).xyz;
				OUT.norm = faceNormal;
				OUT.uv = float2(1, 0);
				TRANSFER_SHADOW(OUT)
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v1);
				OUT.worldPos.xyz = mul(unity_ObjectToWorld, v0).xyz;

				OUT.uv = float2(1, 1);
				TRANSFER_SHADOW(OUT)
				triStream.Append(OUT);

				float3 right = v0 + perpendicularAngle * 0.5 * width;
				OUT.pos = UnityObjectToClipPos(right);
				OUT.worldPos.xyz = mul(unity_ObjectToWorld, right).xyz;
				OUT.uv = float2(0.5, 0);
				TRANSFER_SHADOW(OUT)
				triStream.Append(OUT);

				triStream.RestartStrip();
			}

			half4 frag(g2f IN) : COLOR
			{
				fixed4 c = lerp(_Color2, _Color, saturate((IN.worldPos.y + _GradientOffset) * _GradientScale));
				//fixed4 c = lerp(_Color2, _Color, saturate((1 + _GradientOffset) * _GradientScale));


				fixed shadow = SHADOW_ATTENUATION(IN);
				fixed3 lighting = IN.worldPos.w; // We're storing ambient light here
				//fixed3 lighting = 1; // We're storing ambient light here
				lighting *= shadow;

				c.rgb *= lerp(lighting, lighting * _Gradient, IN.uv.y) + _Ambient;
				return c;
			}

			ENDCG
		}
	}
}
