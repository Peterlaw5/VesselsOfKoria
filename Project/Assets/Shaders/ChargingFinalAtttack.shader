// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Vok/ChargingFinalAttack"
{
	Properties
	{
		_StartInvisible("StartInvisible", Float)=0
		_MainTex("Texture", 2D) = "white" {}
		_Tex2("texture 2",2D)="white"{}
		_EmissiveAmount("_EmissiveAmount", Float) = 2.0
	    [HDR]_Color("color",color) = (0,0,0,0)
		_NoiseTex("Texture", 2D) = "white" {}
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		
		_Level("Dissolution level", Range(0.0, 1.0)) = 0.1
	}
		SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			Lighting On
			ZWrite Off
			Fog{ Mode Off }

			CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
			// make fog work
	#pragma multi_compile DUMMY PIXELSNAP_ON

	#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			sampler2D _Tex2;
			sampler2D _MainTex;
			sampler2D _NoiseTex;
			float _EmissiveAmount;
			float4 _Color;
			float _Level;
			float4 _MainTex_ST;
			bool _StartInvisible;
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv = v.uv;
				o.color.rgba = v.color.rgba;
	#ifdef PIXELSNAP_ON
				o.vertex = UnityPixelSnap(o.vertex);
	#endif		

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				float cutout = tex2D(_NoiseTex, i.uv).r;
				fixed4 col = tex2D(_MainTex, i.uv);
				
				if (cutout < _Level)
				{
					float4 emissive = i.color * col.a*_Color;
					clip(i.color.a - 0.5);
					col = tex2D(_MainTex, i.uv)*emissive;
					col += emissive * pow(_EmissiveAmount, 2.2);
				}
				else  
				{
					discard;
				}
					

			
				

				
				return col;
			}
			ENDCG
		}
	}
}