#include "NxJoint.h"
#include "NxActor.h"

NS_BEGIN

namespace NxPhysics
{
	ENGINE_RTTI_IMPL(NxJoint);
	ENGINE_RTTI_IMPL(NxContactConstraint);
	NxReal NxContactConstraint::CalcLimitMin(const NxRigidBody* body0, const NxRigidBody* body1)
	{
		if (body0->mShapes.size() == 0 || body1->mShapes.size() == 0)
		{
			return NxReal::Zero();
		}
		if (body0->mShapes.size() == 1 && body1->mShapes.size() == 1)
		{
			const auto& shape0 = body0->mShapes[0];
			const auto& shape1 = body1->mShapes[0];

			if (shape0->GetRtti() == GetClassObject<NxSphereShape>() &&
				shape1->GetRtti() == GetClassObject<NxSphereShape>())
			{
				return ((NxSphereShape*)shape0.GetPtr())->mDesc.Radius + ((NxSphereShape*)shape1.GetPtr())->mDesc.Radius;
			}
		}

		return NxReal::Zero();
	}
	void NxContactConstraint::SolveConstraint(NxScene* scene, const NxReal& time)
	{
		//����Ϊԭ����ʾ��û���κ��Ż�
		auto& mBody0 = mActorPair.first;
		auto& mBody1 = mActorPair.second;

		//������Ҫ�����ľ���
		auto delta = FixDistance(mBody0->GetTryTransform()->Position, mBody1->GetTryTransform()->Position, mLimitMin, mLimitMax);
		auto len = delta.Length();
		if (NxReal::EpsilonEqual(len, NxReal::Zero()))
			return;
		//����p0��p1�ĵ�����ֱ��Լ����ֱ�Ӽ����ݶȼ��ɣ�
		NxVector3 gradients[2];
		gradients[0] = delta / len;
		gradients[1] = -gradients[0];
		
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
		mBody0->GetTryTransform()->Position += delta_p0;
		mBody1->GetTryTransform()->Position += delta_p1;

		Gradient = gradients[0];
	}
}

NS_END


