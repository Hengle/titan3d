#include "FBXAnalyzer.h"
#include "FBXManager.h"
#include "..\..\..\Graphics\Mesh\GfxMeshPrimitives.h"

NS_BEGIN
#define new VNEW
#ifdef IOS_REF
#undef  IOS_REF
#define IOS_REF (*(fbxManager->GetFbxSDKManager()->GetIOSettings()))
#endif

RTTI_IMPL(EngineNS::FBXAnalyzer, VIUnknown);

FBXAnalyzer::FBXAnalyzer()
{
}

FBXAnalyzer::~FBXAnalyzer()
{
	Cleanup();
}

void FBXAnalyzer::Cleanup()
{
	if (mGeometryConverter)
	{
		delete mGeometryConverter;
		mGeometryConverter = NULL;
	}
}

bool FBXAnalyzer::Init(FBXManager* fbxManager, const char* pName)
{
	mFbxScene = FbxScene::Create(fbxManager->GetFbxSDKManager(), "");
	return true;
}

int FBXAnalyzer::GetPrimitivesNum()
{
	if (mFbxScene == NULL)
	{
		//Log("Must load file first");
		return 0;
	}
	FbxNode* pRootNode = mFbxScene->GetRootNode();
	auto node = GetFbxNodeByType(pRootNode, FbxNodeAttribute::eMesh);
	if (node == 0)
	{
		//Log(fbx don't have mesh)
		return 0;
	}
	FbxMesh* pMesh = node->GetMesh();
	if (pMesh == 0)
	{
		//Log(fbx don't have mesh)
		return 0;
	}


	int triangleCount = pMesh->GetPolygonCount();
	return triangleCount;
}

vBOOL FBXAnalyzer::SetGeomtryMeshStream(IRenderContext* rc, GfxMeshPrimitives* primitivesMesh)
{
	FbxNode* pRootNode = mFbxScene->GetRootNode();
	auto node = GetFbxNodeByType(pRootNode, FbxNodeAttribute::eMesh);
	if (node == NULL)
	{
		//Log(fbx don't have mesh)
		return FALSE;
	}
	FbxMesh* pMesh = node->GetMesh();
	if (pMesh == NULL)
	{
		//Log(fbx don't have mesh)
		return FALSE;
	}
	FbxLayer* baseLayer = pMesh->GetLayer(0);
	if (baseLayer == NULL)
	{
		//Log(There is no geometry information in mesh)
		return FALSE;
	}
	pMesh->RemoveBadPolygons();

	FillGeomtryVertexStream(rc, primitivesMesh, EVertexSteamType::VST_Position, pMesh);
	FillGeomtryVertexStream(rc, primitivesMesh, EVertexSteamType::VST_UV, pMesh);
	FillGeomtryVertexIndex(rc, primitivesMesh, EIndexBufferType::IBT_Int16, pMesh);

	//
	// fill   normal  Tangent Binormal
	//
	//FillGeomtryVertexStream(rc, primitivesMesh, EVertexSteamType::VST_Normal, pMesh);
	//FillGeomtryVertexStream(rc, primitivesMesh, EVertexSteamType::VST_Tangent, pMesh);
	//FillGeomtryVertexStream(rc, primitivesMesh, EVertexSteamType::VST_Tangent, pMesh);

	return TRUE;
}

vBOOL FBXAnalyzer::FillGeomtryVertexStream(IRenderContext* rc, GfxMeshPrimitives* primitivesMesh, EVertexSteamType stream, FbxMesh* pMesh)
{
	if (primitivesMesh == NULL)
		return FALSE;
	FbxLayer* baseLayer = pMesh->GetLayer(0);
	if (baseLayer == NULL)
	{
		//Log(There is no geometry information in mesh)
		return FALSE;
	}
	void* data = NULL;
	UINT size;
	UINT stride;
	if (stream == EVertexSteamType::VST_Position)
	{
		data = GetGeomtryMeshPosition(pMesh, size, stride);
		primitivesMesh->SetGeomtryMeshStream(rc, stream, data, size, stride, 0);
		delete[] data;
	}
	if (stream == EVertexSteamType::VST_Normal)
	{
		FbxLayerElementNormal* layerElementNormal = baseLayer->GetNormals();
		data = GetGeomtryMeshNormal(layerElementNormal, size, stride);
		primitivesMesh->SetGeomtryMeshStream(rc, stream, data, size, stride, 0);
		delete[] data;
	}
	//if (stream == EVertexSteamType::VST_Tangent)
	//{
	//  FbxLayerElementTangent* layerElementTangent = baseLayer->GetTangents();
	//	data = GetGeomtryMeshTangent(layerElementTangent, size, stride);
	//	primitivesMesh->SetGeomtryMeshStream(rc, stream, data, size, stride, 0);
	//	delete[] data;
	//}
	if (stream == EVertexSteamType::VST_Tangent)
	{
		FbxLayerElementBinormal* layerElementBinormal = baseLayer->GetBinormals();
		data = GetGeomtryMeshBinormal(layerElementBinormal, size, stride);
		primitivesMesh->SetGeomtryMeshStream(rc, stream, data, size, stride, 0);
		delete[] data;
	}
	if (stream == EVertexSteamType::VST_UV)
	{
		data = GetGeomtryMeshUV(pMesh, size, stride);
		primitivesMesh->SetGeomtryMeshStream(rc, stream, data, size, stride, 0);
		delete[] data;
	}

	primitivesMesh->GetGeomtryMesh()->SetIsDirty(TRUE);
	return TRUE;
}

vBOOL FBXAnalyzer::FillGeomtryVertexIndex(IRenderContext* rc, GfxMeshPrimitives* primitivesMesh, EIndexBufferType type, FbxMesh* pMesh)
{
	if (primitivesMesh == NULL)
		return FALSE;
	void* data = NULL;
	UINT size;
	data = GetGeomtryMeshIndex(pMesh, size);
	primitivesMesh->SetGeomtryMeshIndex(rc, data, size, EIndexBufferType::IBT_Int16, 0);
	delete[] data;
	return TRUE;
}

vBOOL FBXAnalyzer::LoadFile(FBXManager* fbxManager, const char* pName)
{
	auto sdkManager = fbxManager->GetFbxSDKManager();
	mFbxScene = FbxScene::Create(sdkManager, "");
	if (mFbxScene == NULL)
		return FALSE;
	mGeometryConverter = new FbxGeometryConverter(fbxManager->GetFbxSDKManager());
	int lFileMajor, lFileMinor, lFileRevision;
	int lSDKMajor, lSDKMinor, lSDKRevision;
	int i, lAnimStackCount;
	bool lStatus;
	char lPassword[1024];

	// Get the version number of the FBX files generated by the
	// version of FBX SDK that you are using.
	FbxManager::GetFileFormatVersion(lSDKMajor, lSDKMinor, lSDKRevision);

	// Create an importer.
	FbxImporter* lImporter = FbxImporter::Create(sdkManager, "");

	// Initialize the importer by providing a filename.
	const bool lImportStatus = lImporter->Initialize(pName, -1, sdkManager->GetIOSettings());

	// Get the version number of the FBX file format.
	lImporter->GetFileVersion(lFileMajor, lFileMinor, lFileRevision);

	if (!lImportStatus)  // Problem with the file to be imported
	{
		FbxString error = lImporter->GetStatus().GetErrorString();
		//Log("Call to FbxImporter::Initialize() failed.");
		//Log("Error returned: %s", error.Buffer());

		if (lImporter->GetStatus().GetCode() == FbxStatus::eInvalidFileVersion)
		{
			//Log("FBX version number for this FBX SDK is %d.%d.%d",lSDKMajor, lSDKMinor, lSDKRevision);
			//Log("FBX version number for file %s is %d.%d.%d",pFilename, lFileMajor, lFileMinor, lFileRevision);
		}

		return FALSE;
	}

	//Log("FBX version number for this FBX SDK is %d.%d.%d",lSDKMajor, lSDKMinor, lSDKRevision);

	if (lImporter->IsFBX())
	{
		//Log("FBX version number for file %s is %d.%d.%d",pFilename, lFileMajor, lFileMinor, lFileRevision);

		// In FBX, a scene can have one or more "animation stack". An animation stack is a
		// container for animation data.
		// You can access a file's animation stack information without
		// the overhead of loading the entire file into the scene.

		//Log("Animation Stack Information");

		lAnimStackCount = lImporter->GetAnimStackCount();

		//Log("    Number of animation stacks: %d", lAnimStackCount);
		//Log("    Active animation stack: \"%s\"",lImporter->GetActiveAnimStackName());

		for (i = 0; i < lAnimStackCount; i++)
		{
			FbxTakeInfo* lTakeInfo = lImporter->GetTakeInfo(i);

			//Log("    Animation Stack %d", i);
			//Log("         Name: \"%s\"", lTakeInfo->mName.Buffer());
			//Log("         Description: \"%s\"",lTakeInfo->mDescription.Buffer());

			// Change the value of the import name if the animation stack should
			// be imported under a different name.
			//Log("         Import Name: \"%s\"", lTakeInfo->mImportName.Buffer());

			// Set the value of the import state to false
			// if the animation stack should be not be imported.
			//Log("         Import State: %s", lTakeInfo->mSelect ? "true" : "false");
		}

		// Import options determine what kind of data is to be imported.
		// The default is true, but here we set the options explictly.

		IOS_REF.SetBoolProp(IMP_FBX_MATERIAL, true);
		IOS_REF.SetBoolProp(IMP_FBX_TEXTURE, true);
		IOS_REF.SetBoolProp(IMP_FBX_LINK, true);
		IOS_REF.SetBoolProp(IMP_FBX_SHAPE, true);
		IOS_REF.SetBoolProp(IMP_FBX_GOBO, true);
		IOS_REF.SetBoolProp(IMP_FBX_ANIMATION, true);
		IOS_REF.SetBoolProp(IMP_FBX_GLOBAL_SETTINGS, true);
	}

	// Import the scene.
	lStatus = lImporter->Import(mFbxScene);

	if (lStatus == false &&     // The import file may have a password
		lImporter->GetStatus().GetCode() == FbxStatus::ePasswordError)
	{
		//Log("Please enter password: ");

		lPassword[0] = '\0';

		FBXSDK_CRT_SECURE_NO_WARNING_BEGIN
			scanf("%s", lPassword);
		FBXSDK_CRT_SECURE_NO_WARNING_END
			FbxString lString(lPassword);

		IOS_REF.SetStringProp(IMP_FBX_PASSWORD, lString);
		IOS_REF.SetBoolProp(IMP_FBX_PASSWORD_ENABLE, true);


		lStatus = lImporter->Import(mFbxScene);

		if (lStatus == false && lImporter->GetStatus().GetCode() == FbxStatus::ePasswordError)
		{
			//Log("Incorrect password: file not imported.");
		}
	}

	// Destroy the importer
	lImporter->Destroy();
	return TRUE;
}

void* FBXAnalyzer::GetGeomtryMeshPosition(FbxMesh* pMesh, UINT& size, UINT& stride)
{
	// Must do this before triangulating the mesh due to an FBX bug in TriangulateMeshAdvance
	int LayerSmoothingCount = pMesh->GetLayerCount(FbxLayerElement::eSmoothing);
	for (int i = 0; i < LayerSmoothingCount; i++)
	{
		FbxLayerElementSmoothing const* SmoothingInfo = pMesh->GetLayer(0)->GetSmoothing();
		if (SmoothingInfo && SmoothingInfo->GetMappingMode() != FbxLayerElement::eByPolygon)
		{
			mGeometryConverter->ComputePolygonSmoothingFromEdgeSmoothing(pMesh, i);
		}
	}

	if (!pMesh->IsTriangleMesh())
	{
		int kk = 0;
	}

	int triangleCount = pMesh->GetPolygonCount();
	UINT16 vertexCounter = 0;
	int pointCount = pMesh->GetControlPointsCount();
	v3dxVector3* pVertex = new v3dxVector3[pointCount/*triangleCount*3*/];

	for (int i = 0; i < triangleCount; ++i)
	{
		for (int j = 0; j < 3; j++)
		{
			int ctrlPointIndex = pMesh->GetPolygonVertex(i, j);
			FbxVector4* pCtrlPoint = pMesh->GetControlPoints();
			v3dxVector3* vertex = &pVertex[ctrlPointIndex];
			vertex->x = (float)pCtrlPoint[ctrlPointIndex].mData[0];
			vertex->z = (float)pCtrlPoint[ctrlPointIndex].mData[1];
			vertex->y = (float)pCtrlPoint[ctrlPointIndex].mData[2];
			vertexCounter++;
		}
	}
	size = ((UINT) sizeof(v3dxVector3) *pointCount/*triangleCount  *3*/);
	stride = sizeof(v3dxVector3);
	return pVertex;
}

void* EngineNS::FBXAnalyzer::GetGeomtryMeshNormal(FbxLayerElementNormal* layerElementNormal, UINT& size, UINT& stride)
{
	FbxLayerElement::EReferenceMode NormalReferenceMode(FbxLayerElement::eDirect);
	FbxLayerElement::EMappingMode NormalMappingMode(FbxLayerElement::eByControlPoint);
	if (layerElementNormal)
	{
		NormalReferenceMode = layerElementNormal->GetReferenceMode();
		NormalMappingMode = layerElementNormal->GetMappingMode();
	}

	return NULL;
}
void* EngineNS::FBXAnalyzer::GetGeomtryMeshTangent(FbxLayerElementTangent* layerElementTangent, UINT& size, UINT& stride)
{
	FbxLayerElement::EReferenceMode TangentReferenceMode(FbxLayerElement::eDirect);
	FbxLayerElement::EMappingMode TangentMappingMode(FbxLayerElement::eByControlPoint);
	if (layerElementTangent)
	{
		TangentReferenceMode = layerElementTangent->GetReferenceMode();
		TangentMappingMode = layerElementTangent->GetMappingMode();
	}

	return NULL;
}
void* EngineNS::FBXAnalyzer::GetGeomtryMeshBinormal(FbxLayerElementBinormal* layerElementBinormal, UINT& size, UINT& stride)
{
	FbxLayerElement::EReferenceMode BinormalReferenceMode(FbxLayerElement::eDirect);
	FbxLayerElement::EMappingMode BinormalMappingMode(FbxLayerElement::eByControlPoint);
	if (layerElementBinormal)
	{
		BinormalReferenceMode = layerElementBinormal->GetReferenceMode();
		BinormalMappingMode = layerElementBinormal->GetMappingMode();
	}

	return NULL;
}
void* FBXAnalyzer::GetGeomtryMeshIndex(FbxMesh* pMesh, UINT& size)
{
	int triangleCount = pMesh->GetPolygonCount();
	int* fbxIndex = pMesh->GetPolygonVertices();
	int vertexCounter = 0;
	UINT16* index = new UINT16[triangleCount * 3];
	for (int i = 0; i < triangleCount; ++i)
	{
		for (int j = 0; j < 3; j++)
		{
			int ctrlPointIndex = pMesh->GetPolygonVertex(i, j);
			index[i * 3 + 2 - j] = (UINT16)ctrlPointIndex;
		}
	}
	size = ((UINT) sizeof(UINT16) *triangleCount * 3);
	return index;
}



void* FBXAnalyzer::GetGeomtryMeshUV(FbxMesh* pMesh, UINT& size, UINT& stride)
{
	int triangleCount = pMesh->GetPolygonCount();
	int vertexCounter = 0;
	int pointCount = pMesh->GetControlPointsCount();

	v3dxVector2* puv = new v3dxVector2[triangleCount * 3 * 1];
	for (int i = 0; i < triangleCount; ++i)
	{
		for (int j = 0; j < 3; j++)
		{
			int index = (3 * i + j);
			int ctrlPointIndex = pMesh->GetPolygonVertex(i, j);
			int textureUVIndex = pMesh->GetTextureUVIndex(i, j);
			// index = ctrlPointIndex;
			for (int k = 0; k < 1; ++k)
			{
				int uvLayer = k;
				v3dxVector2* pUV = &puv[ctrlPointIndex];
				if (uvLayer >= 2 || pMesh->GetElementUVCount() <= uvLayer)
				{
					return false;
				}
				FbxGeometryElementUV* pVertexUV = pMesh->GetElementUV(uvLayer);
				switch (pVertexUV->GetMappingMode())
				{
				case FbxGeometryElement::eByControlPoint:
				{
					switch (pVertexUV->GetReferenceMode())
					{
					case FbxGeometryElement::eDirect:
					{
						pUV->x = (float)pVertexUV->GetDirectArray().GetAt(ctrlPointIndex).mData[0];
						pUV->y = (float)pVertexUV->GetDirectArray().GetAt(ctrlPointIndex).mData[1];
					}
					break;
					case FbxGeometryElement::eIndexToDirect:
					{
						int id = pVertexUV->GetIndexArray().GetAt(ctrlPointIndex);
						pUV->x = (float)pVertexUV->GetDirectArray().GetAt(id).mData[0];
						pUV->y = 1 - (float)pVertexUV->GetDirectArray().GetAt(id).mData[1];
					}
					break;
					default:
						break;
					}
				}
				break;
				case FbxGeometryElement::eByPolygonVertex:
				{
					switch (pVertexUV->GetReferenceMode())
					{
					case FbxGeometryElement::eDirect:
					case FbxGeometryElement::eIndexToDirect:
					{
						pUV->x = (float)pVertexUV->GetDirectArray().GetAt(ctrlPointIndex).mData[0];
						pUV->y = (float)pVertexUV->GetDirectArray().GetAt(ctrlPointIndex).mData[1];
					}
					break;
					default:
						break;
					}
				}
				break;
				}
			}
		}
	}
	size = ((UINT) sizeof(v3dxVector2) *triangleCount * 3);
	//size = ((UINT) sizeof(v3dxVector2) *pointCount);
	stride = sizeof(v3dxVector2);
	return puv;
}

FbxNode * FBXAnalyzer::GetFbxNodeByType(FbxNode *pNode, FbxNodeAttribute::EType type)
{
	if (pNode->GetNodeAttribute())
	{
		if (pNode->GetNodeAttribute()->GetAttributeType() == type)
		{
			auto name = pNode->GetName();
			return pNode;
		}
	}
	else
	{
		for (int i = 0; i < pNode->GetChildCount(); ++i)
		{
			auto node = GetFbxNodeByType(pNode->GetChild(i), type);
			if (node != NULL)
				return node;
		}
	}
	return NULL;
}

NS_END

using namespace EngineNS;

extern "C"
{
	Cpp2CS2(EngineNS, FBXAnalyzer, LoadFile);
	Cpp2CS2(EngineNS, FBXAnalyzer, SetGeomtryMeshStream);
	Cpp2CS0(EngineNS, FBXAnalyzer, GetPrimitivesNum);

	//CSharpAPI1(EngineNS, GfxMesh, SetMeshPrimitives, GfxMeshPrimitives*);
	//CSharpAPI1(EngineNS, GfxMesh, SetGfxMdfQueue, GfxMdfQueue*);

	//CSharpAPI1(EngineNS, GfxMesh, Save2Xnd, XNDNode*);
	//CSharpReturnAPI1(bool, EngineNS, GfxMesh, LoadXnd, XNDNode*);

	//CSharpReturnAPI0(bool, EngineNS, FBXAnalyzer, Init);
	//CSharpReturnAPI0(const char*, EngineNS, GfxMesh, GetGeomName);
	//CSharpReturnAPI0(UINT, EngineNS, GfxMesh, GetAtomNumber);
	//CSharpReturnAPI1(GfxMesh*, EngineNS, FBXManager, GetGfxMesh, const char*);
}