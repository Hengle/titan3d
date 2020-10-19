#include "GfxSkinModifier.h"
#include "../../Bricks/Animation/Skeleton/GfxSkeleton.h"
#include "../../Bricks/Animation/GfxBoneAnim.h"
#include "../../Bricks/Animation/Pose/GfxSkeletonPose.h"
#include "../../Bricks/Animation/Runtime/GfxAnimationRuntime.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxSkinModifier, EngineNS::GfxModifier);

GfxSkinModifier::GfxSkinModifier()
{

}

GfxSkinModifier::~GfxSkinModifier()
{

}

bool GfxSkinModifier::Init(GfxModifierDesc* desc)
{
	return true;
}

void GfxSkinModifier::Save2Xnd(XNDNode* node)
{
	/*auto subSkeNode = node->AddNode("SubSkeleton", 0, 0);
	mSkeleton->Save2Xnd(subSkeNode);*/
	//auto sktNode = node->AddNode("Socket");
	//if (sktNode != nullptr)
	//{// �ҽӵ�

	//}
}

vBOOL GfxSkinModifier::LoadXnd(XNDNode* node)
{
	//auto subSkeNode = node->GetChild("SubSkeleton");
	//if (subSkeNode != nullptr)
	//{//��vmsʹ�õ�����С�Ǽ�
	//	mSkeleton = new GfxSkeleton();
	//	mSkeleton->LoadXnd(subSkeNode);
	//}
	//auto sktNode = node->GetChild("Socket");
	//if (sktNode != nullptr)
	//{// �ҽӵ�

	//}
	//auto animTreeNode = node->GetChild(vT("AnimTreeNode"));
	//if (animTreeNode != nullptr)
	//{

	//}
	return TRUE;
}

void GfxSkinModifier::SetSkeleton(GfxSkeleton* skeleton)
{
	mSkeleton.StrongRef(skeleton);
}

void GfxSkinModifier::TickLogic(IRenderContext* rc, GfxMesh* mesh, vTimeTick time)
{

}

GfxModifier* GfxSkinModifier::CloneModifier(IRenderContext* rc)
{
	auto result = new GfxSkinModifier();
	result->mName = mName;
	//result->mSkeleton = mSkeleton->CloneSkeleton();
	return result;
}

vBOOL GfxSkinModifier::SetToRenderStream(IConstantBuffer* cb, int AbsBonePos, int AbsBoneQuat, GfxSkeletonPose* animPose)
{
	//�����mSkeleton��mesh�Լ��ģ���������AnimationPose������ģ������Լ���skeleton��tick
	if (mSkeleton == nullptr)
		return FALSE;
	if (animPose == nullptr)
		return FALSE;
	//shader buffer��С�ĳ��� 360������֧�֡�Shaders\CoreShader\Modifier\SkinModifier.var
	v3dxQuaternion* absPos = (v3dxQuaternion*)cb->GetVarPtrToWrite(AbsBonePos, (int)mSkeleton->mBones.size() * sizeof(v3dxQuaternion));
	if (absPos == nullptr)
		return FALSE;
	v3dxQuaternion* absQuat = (v3dxQuaternion*)cb->GetVarPtrToWrite(AbsBoneQuat, (int)mSkeleton->mBones.size() * sizeof(v3dxQuaternion));
	if (absPos == nullptr)
		return FALSE;
	auto& bones = mSkeleton->mBones;
	for (size_t i = 0; i < bones.size(); i++)
	{
		auto bone = bones[i];
		if (bone != nullptr)
		{
			//�������translateRetarget,������������ѡ��ѡ�����Լ���shared���Լ���transform�ȵ�
			GfxBoneTransform trans;
			GfxBoneDesc* shared = NULL;
			auto outBone = animPose->FindBonePose(bone->mSharedData->NameHash);
			if (outBone)
			{
				trans = outBone->Transform;
				//shared = outBone->ReferenceBone->GetBoneDesc();
				shared = bone->mSharedData;
			}
			if (shared == NULL)
			{
				continue;
			}
			*((v3dxVector3*)absPos) = trans.Position + trans.Rotation * shared->InvPos;
			//*((v3dxVector3*)absPos) = (*(v3dxVector3*)(&trans.Position)) + (*(v3dxQuaternion*)(&trans.Rotation)) * (*(v3dxVector3*)(&shared->InvPos));
			absPos->w = 0;

			//*absQuat = shared->InvQuat * trans.Rotation;
			*absQuat = shared->InvQuat * trans.Rotation;
			//*absQuat = shared->InvQuat *(*(v3dxQuaternion*)(&trans.Rotation));
			v3dxVector3 transRot, invRot, absRot;
			v3dxYawPitchRollQuaternionRotation(trans.Rotation, &invRot);
			v3dxYawPitchRollQuaternionRotation(shared->InvQuat, &invRot);
			v3dxYawPitchRollQuaternionRotation(*absQuat, &absRot);
		}
		absPos++;
		absQuat++;
	}
	return TRUE;
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI0(GfxSkeleton*, EngineNS, GfxSkinModifier, GetSkeleton);
	CSharpAPI1(EngineNS, GfxSkinModifier, SetSkeleton, GfxSkeleton*);
	CSharpReturnAPI4(vBOOL, EngineNS, GfxSkinModifier, SetToRenderStream, IConstantBuffer*, int, int, GfxSkeletonPose*);
}