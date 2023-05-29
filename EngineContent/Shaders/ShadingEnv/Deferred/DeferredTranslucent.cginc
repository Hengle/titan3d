#ifndef _DEFERRED_TRANSLUCENT_
#define _DEFERRED_TRANSLUCENT_

#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/LightCommon.cginc"
#include "../../Inc/Math.cginc"
#include "../../Inc/ShadowCommon.cginc"
#include "../../Inc/FogCommon.cginc"
#include "../../Inc/MixUtility.cginc"
#include "../../Inc/SysFunction.cginc"

#include "Material"
#include "MdfQueue"

#include "../../Inc/SysFunctionDefImpl.cginc"
#include "../CommonLighting.cginc"

#ifdef MDFQUEUE_FUNCTION
MDFQUEUE_FUNCTION
#endif

//WARNING:don't change vs_main or ps_main's parameters name cause we also use it in c++;It's an appointment;
PS_INPUT VS_Main(VS_INPUT input1)
{
	VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
	PS_INPUT output = (PS_INPUT)0;
	Default_VSInput2PSInput(output, input);

#if defined(VS_NO_WorldTransform)
	output.PointLightIndices = PointLightIndices;
	output.SpecialData.x = PointLightNum;
#endif

	MTL_OUTPUT mtl = (MTL_OUTPUT)0;
	//mtl template stuff;
	{
#ifdef MDFQUEUE_FUNCTION
		MdfQueueDoModifiers(output, input);
#endif

#ifdef DO_VS_MATERIAL
		DO_VS_MATERIAL(output, mtl);
#endif
	}

#if !defined(VS_NO_WorldTransform)
	output.vPosition.xyz += mtl.mVertexOffset;
	
	float4 wp4 = mul(float4(output.vPosition.xyz, 1), WorldMatrix);
	output.vWorldPos = wp4.xyz;
	output.vNormal = normalize(mul(float4(output.vNormal.xyz, 0), WorldMatrix).xyz);
	output.vTangent.xyz = normalize(mul(float4(output.vTangent.xyz, 0), WorldMatrix).xyz);
#endif
	output.vPosition = mul(wp4, GetViewPrjMtx(false));

	return output;
}

struct PS_OUTPUT
{
	float4 RT0 : SV_Target0;
};

PS_OUTPUT PS_Main(PS_INPUT input)
{
	PS_OUTPUT output = (PS_OUTPUT)0;

	MTL_OUTPUT mtl = Default_PSInput2Material(input);
	//mtl template stuff;
	{
#ifndef DO_PS_MATERIAL
#define DO_PS_MATERIAL DoDefaultPSMaterial
#endif
		DO_PS_MATERIAL(input, mtl);

#ifdef MDFQUEUE_FUNCTION_PS
		MdfQueueDoModifiersPS(input, mtl);
#endif
	}

	half Alpha = (half)mtl.mAlpha;
	half AlphaTestThreshold = (half)mtl.mAlphaTest;
	half3 Albedo = sRGB2Linear((half3)mtl.mAlbedo);

#ifdef ALPHA_TEST
	clip(Alpha - AlphaTestThreshold);
#endif // AlphaTest

	{
		half3 Emissive = (half3)mtl.mEmissive;
		half3 BaseShading = half3(0.0h, 0.0h, 0.0h);

#ifdef MTL_ID_UNLIT
		BaseShading = (half3)mtl.mAlbedo;
#else
		//lighting for translucent
		half3 diffColor, specColor;
		GetDirLightingColor(diffColor, specColor, input.vWorldPos, mtl, 1.0h);
		half3 skyColor = GetSkyColor(Albedo, mtl, 1.0h);
		BaseShading = Linear2sRGB(diffColor + specColor + skyColor);
		//BaseShading = (half3)mtl.mAlbedo;
#endif

		BaseShading += Emissive;

		//BaseShading.b = (half)floor(BaseShading.b * AO_M);
		output.RT0.rgb = BaseShading;
		output.RT0.a = Alpha;
	}

	return output;
}

#endif//#ifndef _DEFERRED_TRANSLUCENT_