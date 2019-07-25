Shader "Vok/Alpha Timed Shader"
{
	Properties
	{
		_MainTex("Mask Texture", 2D) = "black" { }
		_MainColor("Main Color", Color) = (1.0, 1.0, 1.0, 1.0)
	}
	SubShader
	{
		
		Tags{"Queue" = "Transparent"}

		// Change blend type
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Cull Front

			CGPROGRAM

				// Tell to hardware which shader's function name
			#pragma vertex VertShader
			#pragma fragment FragShader

				// Get access to unity 
			#include "UnityCG.cginc"

				// Same name as property means having a standard value
			sampler2D _MainTex;
			float4 _MainTex_ST; // name has to be exactly the same with the main texture 

			float4 _MainColor;

			// Input data
			struct AppData
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct VertexToFragment
			{
				float4 pos: SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			VertexToFragment VertShader(AppData input)
			{
				VertexToFragment output;

				output.pos = UnityObjectToClipPos(input.vertex);

				output.uv = TRANSFORM_TEX(input.uv, _MainTex);

				return output;
			}

			float4 FragShader(VertexToFragment input) : COLOR
			{
				float4 textureColor = tex2D(_MainTex, input.uv);
				return textureColor * _MainColor;
			}
			ENDCG
		}
		Pass
		{
			Cull Back

			CGPROGRAM

			// Tell to hardware which shader's function name
			#pragma vertex VertShader
			#pragma fragment FragShader

								// Get access to unity 
			#include "UnityCG.cginc"

			// Same name as property means having a standard value
			sampler2D _MainTex;
			float4 _MainTex_ST; // name has to be exactly the same with the main texture 

			float4 _MainColor;

			// Input data
			struct AppData
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct VertexToFragment
			{
				float4 pos: SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			VertexToFragment VertShader(AppData input)
			{
				VertexToFragment output;

				output.pos = UnityObjectToClipPos(input.vertex);

				output.uv = TRANSFORM_TEX(input.uv, _MainTex);

				return output;
			}

			float4 FragShader(VertexToFragment input) : COLOR
			{
				float4 textureColor = tex2D(_MainTex,input.uv);
				return textureColor * _MainColor;
			}
			ENDCG
		}

	}
    //FallBack "Diffuse"
}
