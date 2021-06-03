#ifndef _PICKED_SETUP_
#define _PICKED_SETUP_

#include "../../../Inc/VertexLayout.cginc"
#include "../../../Inc/LightCommon.cginc"
#include "../../../Inc/Math.cginc"
#include "../../../Inc/ShadowCommon.cginc"
#include "../../../Inc/FogCommon.cginc"
#include "../../../Inc/MixUtility.cginc"
#include "../../../Inc/SysFunction.cginc"

#include "Material"
#include "MdfQueue"

//WARNING:don't change vs_main or ps_main's parameters name cause we also use it in c++;It's an appointment;
PS_INPUT VS_Main(VS_INPUT input)
{
	PS_INPUT output = (PS_INPUT)0;
	Default_VSInput2PSInput(output, input);

#if defined(MDF_INSTANCING)
	output.PointLightIndices = PointLightIndices;
	output.SpecialData.x = PointLightNum;
#endif

	MTL_OUTPUT mtl = (MTL_OUTPUT)0;
	{
#ifdef MDFQUEUE_FUNCTION
		MdfQueueDoModifiers(output, input);
#endif

#ifdef DO_VS_MATERIAL
		DO_VS_MATERIAL(output, mtl);
#endif
	}

#if !defined(MDF_INSTANCING)
	output.vPosition.xyz += mtl.mVertexOffset;

	output.vWorldPos = mul(float4(output.vPosition.xyz, 1), WorldMatrix).xyz;
	output.vNormal = normalize(mul(float4(output.vNormal.xyz, 0), WorldMatrix).xyz);
	output.vTangent.xyz = normalize(mul(float4(output.vTangent.xyz, 0), WorldMatrix).xyz);
#endif
	output.vPosition = mul(float4(output.vWorldPos, 1), ViewPrjMtx);

	//output.psCustomUV0.xy = float2(output.vPosition.xy / output.vPosition.w) * float2(0.5f, -0.5f) + float2(0.5f, 0.5f);
	//output.psCustomUV0.z = float(output.vPosition.z / output.vPosition.w);
	output.psCustomUV0.w = output.vPosition.w;

	return output;
}

struct PS_OUTPUT
{
	half2 RT0 : SV_Target0;
};

PS_OUTPUT PS_Main(PS_INPUT input)
{
	PS_OUTPUT output = (PS_OUTPUT)0;
	half PerPixelViewerDistance = (half)input.psCustomUV0.w;
	output.RT0 = half2((half)1.0h, PerPixelViewerDistance * rcp((half)gZFar));

	return output;
}
#endif