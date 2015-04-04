Shader "Custom/FogColor" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_FogColor ("Fog Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_FogTex ("Fog Tex (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass {

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"

            struct v2f 
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos;
                fixed3 color : COLOR0;
                UNITY_FOG_COORDS(1)
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                o.color = v.normal * 0.5 + 0.5;
                o.screenPos = ComputeScreenPos(o.pos);
                o.uv = float4( v.texcoord.xy, 0, 0 );
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

		fixed4 _FogColor;
		sampler2D _MainTex;
		sampler2D _FogTex;
			
            fixed4 frag (v2f i) : SV_Target
            {
            	// Surface color before fog:
            	//float4 c = float4( i.color, 1 ); // this shows surface normal
            	float4 c = tex2D( _MainTex, i.uv); //this shows texture
            //	float4 c = float4( 1, 1, 1, 1 ); // this shows white
            
            	// Fog Color
            	//float4 fogColor = tex2D(_FogTex, i.screenPos.xy/i.screenPos.w); // fades to a screenspace texture
            	float4 fogColor = tex2D(_FogTex, i.uv); // fades to a different model texture
            	//float4 fogColor = _FogColor; //switch to this for a color specified on this material
            
            	UNITY_APPLY_FOG_COLOR(i.fogCoord, c, fogColor);
                return c;
            }
            ENDCG

        }
	} 
	FallBack "Diffuse"
}
