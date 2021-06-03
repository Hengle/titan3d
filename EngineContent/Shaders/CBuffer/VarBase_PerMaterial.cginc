#ifndef __VARBASE_PERINSTANCE_SHADERINC__
#define __VARBASE_PERINSTANCE_SHADERINC__

cbuffer cbPerMaterial : register( b2 )
{
	// ShaderParamAnalyse Start
	float3 PerInst_Noused0;
	float DepthBiasPerSceneMesh; //this will be useful when editor is easy to use;

	// ShaderParamAnalyse End

	#include "MaterialVar"

};

#endif