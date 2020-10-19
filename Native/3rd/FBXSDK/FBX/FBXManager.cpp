#include "FBXManager.h"
#include "..\..\CSharpAPI.h"

NS_BEGIN


#define new VNEW
#ifdef IOS_REF
#undef  IOS_REF
#define IOS_REF (*(mSDKManager->GetIOSettings()))
#endif

RTTI_IMPL(EngineNS::FBXManager, VIUnknown);

FBXManager::FBXManager()
{

}


FBXManager::~FBXManager()
{
	Cleanup();
}

void FBXManager::Cleanup()
{
	if (mSDKManager)
	{
		mSDKManager->Destroy();
		mSDKManager = NULL;
	}
}

int numTabs = 0;

void PrintTabs()  //��ӡtabs�������xml������Ч��  
{
	for (int i = 0; i < numTabs; i++)
	{
		printf("\t");
	}
}


/**
*���ݽڵ����ԵĲ�ͬ�������ַ��������Ƿ��ؽڵ����Ե�����
**/
FbxString GetAttributeTypeName(FbxNodeAttribute::EType type)
{
	switch (type)
	{
	case FbxNodeAttribute::eUnknown: return "UnknownAttribute";
	case FbxNodeAttribute::eNull: return "Null";
	case FbxNodeAttribute::eMarker: return "marker";  //��ˡ���  
	case FbxNodeAttribute::eSkeleton: return "Skeleton"; //����  
	case FbxNodeAttribute::eMesh: return "Mesh"; //����  
	case FbxNodeAttribute::eNurbs: return "Nurbs"; //����  
	case FbxNodeAttribute::ePatch: return "Patch"; //Patch��Ƭ  
	case FbxNodeAttribute::eCamera: return "Camera"; //�����  
	case FbxNodeAttribute::eCameraStereo: return "CameraStereo"; //����  
	case FbxNodeAttribute::eCameraSwitcher: return "CameraSwitcher"; //�л���  
	case FbxNodeAttribute::eLight: return "Light"; //�ƹ�  
	case FbxNodeAttribute::eOpticalReference: return "OpticalReference"; //��ѧ��׼  
	case FbxNodeAttribute::eOpticalMarker: return "OpticalMarker";
	case FbxNodeAttribute::eNurbsCurve: return "Nurbs Curve";//NURBS����  
	case FbxNodeAttribute::eTrimNurbsSurface: return "Trim Nurbs Surface"; //������У�  
	case FbxNodeAttribute::eBoundary: return "Boundary"; //�߽�  
	case FbxNodeAttribute::eNurbsSurface: return "Nurbs Surface"; //Nurbs����  
	case FbxNodeAttribute::eShape: return "Shape"; //��״  
	case FbxNodeAttribute::eLODGroup: return "LODGroup"; //  
	case FbxNodeAttribute::eSubDiv: return "SubDiv";
	default: return "UnknownAttribute";
	}
}



/**
*��ӡһ������
**/
void PrintAttribute(FbxNodeAttribute* pattribute)
{
	if (!pattribute)
	{
		return;
	}
	FbxString typeName = GetAttributeTypeName(pattribute->GetAttributeType());
	FbxString attrName = pattribute->GetName();
	PrintTabs();

	//FbxString.Buffer() ����������Ҫ���ַ�����  
	printf("<attribute type='%s' name='%s'/>\n ", typeName.Buffer(), attrName.Buffer());
}


/**
*��ӡ��һ���ڵ�����ԣ����ҵݹ��ӡ�������ӽڵ������;
**/
void PrintNode(FbxNode* pnode)
{
	PrintTabs();

	const char* nodeName = pnode->GetName(); //��ȡ�ڵ�����  

	FbxDouble3 translation = pnode->LclTranslation.Get();//��ȡ���node��λ�á���ת������  
	FbxDouble3 rotation = pnode->LclRotation.Get();
	FbxDouble3 scaling = pnode->LclScaling.Get();

	//��ӡ�����node�ĸ�������  
	printf("<node name='%s' translation='(%f,%f,%f)' rotation='(%f,%f,%f)' scaling='(%f,%f,%f)'>\n",
		nodeName,
		translation[0], translation[1], translation[2],
		rotation[0], rotation[1], rotation[2],
		scaling[0], scaling[1], scaling[2]);

	numTabs++;

	//��ӡ���node ����������  
	for (int i = 0; i < pnode->GetNodeAttributeCount(); i++)
	{
		PrintAttribute(pnode->GetNodeAttributeByIndex(i));
	}

	//�ݹ��ӡ������node������  
	for (int j = 0; j < pnode->GetChildCount(); j++)
	{
		PrintNode(pnode->GetChild(j));
	}

	numTabs--;
	PrintTabs();
	printf("</node>\n");
}


vBOOL FBXManager::Init()
{
	// Create the FBX SDK memory manager object.
	// The SDK Manager allocates and frees memory
	// for almost all the classes in the SDK.
	mSDKManager = FbxManager::Create();
	if (mSDKManager == NULL)
		return FALSE;
	// create an IOSettings object
	FbxIOSettings * ios = FbxIOSettings::Create(mSDKManager, IOSROOT);
	mSDKManager->SetIOSettings(ios);
	return TRUE;
}



NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI0(vBOOL, EngineNS, FBXManager, Init);

	//CSharpReturnAPI0(const char*, EngineNS, GfxMesh, GetGeomName);
	//CSharpReturnAPI0(UINT, EngineNS, GfxMesh, GetAtomNumber);

	//CSharpReturnAPI3(bool, EngineNS, GfxMesh, Init, const char*, GfxMeshPrimitives*, GfxMdfQueue*);
	//CSharpReturnAPI2(bool, EngineNS, GfxMesh, SetMaterial, UINT, GfxMaterialPrimitive*);
	//CSharpAPI1(EngineNS, GfxMesh, SetMeshPrimitives, GfxMeshPrimitives*);
	//CSharpAPI1(EngineNS, GfxMesh, SetGfxMdfQueue, GfxMdfQueue*);

	//CSharpAPI1(EngineNS, GfxMesh, Save2Xnd, XNDNode*);
	//CSharpReturnAPI1(bool, EngineNS, GfxMesh, LoadXnd, XNDNode*);
}