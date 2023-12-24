#include "NxJoint.h"
#include "NxActor.h"

NS_BEGIN

namespace NxPhysics
{
	ENGINE_RTTI_IMPL(NxJoint);
	ENGINE_RTTI_IMPL(NxContactConstraint);
	
	void NxContactConstraint::BuildConstraint()
	{
		//mLimitMin = NxContactConstraint::CalcLimitMin(mActorPair.first, mActorPair.second);
		mCompliance = mShapePair.first->mShapeData.Compliance + mShapePair.second->mShapeData.Compliance;
	}
	void NxContactConstraint::SolveConstraint(NxScene* scene, const NxReal& time)
	{
		NxReal len;
		NxVector3 dir;
		//������Ҫ�����ľ���ͷ���
		if (NxShape::Contact(mShapePair.first, mShapePair.second, len, dir) == false)
		{
			return;
		}
		//����Ϊԭ����ʾ��û���κ��Ż�
		auto mBody0 = (NxRigidBody*)mShapePair.first->GetActor();
		auto mBody1 = (NxRigidBody*)mShapePair.second->GetActor();

		//����p0��p1�ĵ�����ֱ��Լ����ֱ�Ӽ����ݶȼ��ɣ�
		NxVector3 gradients[2];
		gradients[0] = dir;
		gradients[1] = -dir;
		
		NxReal w[2];
		w[0] = mBody0->mDesc.InvMass;
		w[1] = mBody1->mDesc.InvMass;

		//Լ������f(x) = |p0 - p1| - d
		//�����Լ��c������Ҫ�����ľ���len
		//������ʵ�����Ż�����Ϊp0,p1�����ĵ���ģ��ƽ������1�����Ա���ϵ��s = c / (w0 + w1)
		auto s = NxJoint::CalcLagrange(mRagrange, len, gradients, w, 2, mCompliance, time);
		mRagrange += s;
		
		//������Dp(i) = s * W(i) * ����(i)
		auto delta_p0 = gradients[0] * (w[0] * s);
		auto delta_p1 = gradients[1] * (w[1] * s);

		//����λ��
		mBody0->GetTransform()->Position += delta_p0;
		mBody1->GetTransform()->Position += delta_p1;

		mBody0->OnUpdatedTransform();
		mBody1->OnUpdatedTransform();

		mContactDirection = gradients[0];
	}
}

NS_END


