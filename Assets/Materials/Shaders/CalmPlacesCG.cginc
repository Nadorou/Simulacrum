// Calm Places CG Include

struct SurfaceOutputLambertWeathered
{
    fixed3 Albedo;  // diffuse color
    fixed3 Normal;  // tangent space normal, if written
    fixed Emission;
    half Specular;  // specular power in 0..1 range
    fixed Gloss;    // specular intensity
    fixed Alpha;    // alpha for transparencies
};

inline fixed4 LightingLambertWeathered(SurfaceOutputLambertWeathered s, fixed3 lightDir, fixed3 viewDir, fixed atten)
{
    fixed3 h = normalize(lightDir + viewDir);
    float nh = saturate(dot(s.Normal, h));
 
    fixed NdotL = saturate(dot(s.Normal, lightDir));
    //fixed HdotA = dot(normalize(s.Normal + s.AnisoDir.rgb), h);
 
    fixed4 c;
    c.rgb = pow(((pow(s.Albedo, 2.2) * _LightColor0.rgb * NdotL) * _LightColor0.rgb) * (atten * 2), 0.45);
    c.a = 1;
    //clip(s.Alpha - _Cutoff);
    return c;
}


//These are PostProcess style functions

inline fixed3 Porosity(fixed3 baseColor, fixed porosity)
{
	return baseColor - porosity;
}

inline fixed3 Saturation(fixed3 baseColor,fixed saturation)
{
	return lerp(dot( baseColor, fixed3(0.22, 0.707, 0.071)),baseColor, saturation);
}

inline fixed4 Overlay(fixed4 a, fixed4 b)
{
	fixed4 r = a < .5 ? 2.0 * a * b : 1.0 - 2.0 * (1.0 - a) * (1.0 - b);
	r.a = b.a;
	return r;
}

//Function for applying dynamic ambient color
inline float3 applyDynamicAmbientColor(float3 worldN, float2 lmUV, float3 albedo, fixed4 globalTint,fixed3 worldP)
{

	float3 base = albedo;

	half3 lm = Saturation(DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, lmUV)).rgb,0);
	fixed4 shadowmask = UnityGetRawBakedOcclusions (lmUV, worldP);

	fixed4 lightMask = lerp(unity_AmbientGround,1,lm.r) * lerp(unity_AmbientGround,1,shadowmask.r);

	albedo *= globalTint.rgb;
	albedo += Saturation(ShadeSH9 (float4(worldN,1)),5)*.15;
	albedo *= saturate(lightMask);

	return albedo;

	/* old
	//albedo += Saturation(ShadeSH9 (float4(worldN,.1)),5)*.15 *  saturate(pow(1-lm,2)*50);
	//albedo *= .5;
	//albedo += Saturation(ShadeSH9 (float4(worldN,1)),5)*.15;
	//albedo -= (1-lm);
	//albedo *= lm * globalTint.rgb * 2;
	*/
}

inline float3 applyPP(float3 worldN, float2 lmUV, float3 albedo, fixed4 globalTint) //duplicate will remove later
{
	float3 base = albedo;
	half3 lm = Saturation(DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, lmUV)).rgb,0);
	albedo += Saturation(ShadeSH9 (float4(worldN,.1)),5)*.15 *  saturate(pow(1-lm,2)*50);
	albedo *=  globalTint.rgb;
	return albedo;
}
