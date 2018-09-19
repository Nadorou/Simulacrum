// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/VoidSky"
{
	Properties
	{
		_TopColor ("Top Color", Color) = (1,1,1,1)
		_MiddleColor ("Middle Color", Color) = (.5,.5,.5,1)
		_BottomColor ("Bottom Color", Color) = (0,0,0,1)
		_BackColor ("Back Color", Color) = (0,0,0,1)
		_Height("Height", Range (0,10)) = 1
		_Middle("Mid Point", Range (0,1)) = 1
		_BackWrap("Back Wrap", Range (-10,10)) = 1
		//_SkyExponent("Sky Exponent", Range (0,50)) = 1
		//_StarsOpacity("Stars Opacity", Range (0,1)) = .3
		//_CloudsOpacity("Clouds Opacity", Range (0,1)) = .3
		//[NoScaleOffset]_StarsTex ("Star Texture", 2D) = "grey" {}
		//[NoScaleOffset]_CloudsTex ("Clouds Texture", 2D) = "grey" {}
		//[NoScaleOffset]_Tex ("First Cubemap", Cube) = "grey" {}
	}
	SubShader
	{
		Tags { "RenderType"="Background" "Queue"="Background" }
		LOD 100
		ZWrite Off

		Pass
		{
			
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			 #pragma fragmentoption ARB_precision_hint_fastest
			// make fog work
			//#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				
			};

			struct v2f
			{
				float3 uv : TEXCOORD0;
				//UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _StarsTex;
			float4 _StarTex_ST;
			sampler2D _CloudsTex;

			samplerCUBE _Tex;

			half4 _TopColor;
			half4 _MiddleColor;
			half4 _BottomColor;
			half4 _BackColor;
			half _Height;
			half _Middle;
			half _BackWrap;
			half _SkyExponent;
			half _StarsOpacity;
			half _CloudsOpacity;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//o.uv = v.uv;
				o.uv = v.vertex.xyz;
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture

				half2 uv = i.uv;
				uv.y += _Height;
				//half4 tex = texCUBElod (_Tex, float4(i.uv,0));
				//half4 starsTxt = tex2D(_StarsTex, i.uv.xz * 2)  * _StarsOpacity;
				//half4 cloudsTxt = tex2D(_CloudsTex, i.uv.xy * float2(1,5)) * _CloudsOpacity;

				//half stars = saturate(lerp(starsTxt.r, 0,(pow(1-uv.y,_SkyExponent * .7) / _Height)));
				//half clouds = saturate(lerp(cloudsTxt.r, 0,(pow(uv.y,_SkyExponent * .5) / _Height)));

				//half4 gradient = lerp(_TopColor,_BottomColor, (pow(1-uv.y,_SkyExponent) / _Height));
				
				half4 BM = lerp(_BottomColor*.5, _MiddleColor, (uv.y) / _Middle) * step(uv.y, _Middle);

				BM += lerp(_MiddleColor, _TopColor, (uv.y - _Middle) / (1 - _Middle)) * (1 - step(uv.y, _Middle));

				BM = lerp(BM*1.1,_BackColor,saturate((uv.x * .5 +.5)/_BackWrap)*5);
				
				fixed up = saturate(uv.y  - .6);
				up = pow(up,3)*1;
				BM += up;
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, gradient);
				//return gradient + (txt * .15);
				//return gradient + stars + clouds;

				return saturate(BM);
				
			}
			ENDCG
		}
	}
}
