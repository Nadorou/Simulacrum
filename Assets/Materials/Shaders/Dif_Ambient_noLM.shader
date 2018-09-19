// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/Dif_Ambient_noLM" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB)", 2D) = "white" {}
    //_BumpMap ("Normalmap", 2D) = "bump" {}
}

SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 300

CGPROGRAM
#pragma surface surf Lambert
#include "UnityLightingCommon.cginc"
#include "CalmPlacesCG.cginc"

sampler2D _MainTex;
//sampler2D _BumpMap;
fixed4 _Color;

//Globals
half _GlobalPorosity;
half _GlobalSaturation;
fixed4 _GlobalTint;


// sampler2D unity_Lightmap;
// float4 unity_LightmapST;



struct Input {
    float2 uv_MainTex;
    /*float2 uv_BumpMap;
	float2 texcoord1;
	float3 worldNormal;
	INTERNAL_DATA*/
};

void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
    o.Albedo = c.rgb + UNITY_LIGHTMODEL_AMBIENT;
	//o.Albedo = applyDynamicAmbientColor(IN.worldNormal, IN.texcoord1, o.Albedo, _GlobalTint); 
    o.Alpha = c.a;
    //o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
}
ENDCG
}

FallBack "Legacy Shaders/Diffuse"
}
