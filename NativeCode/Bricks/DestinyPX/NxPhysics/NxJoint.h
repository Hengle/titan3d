#pragma once
#include "NxConstraint.h"

NS_BEGIN

namespace NxPhysics
{
	class NxJoint : public NxConstraint
	{
	public:
		ENGINE_RTTI(NxJoint);

		virtual void ResetRagrange() = 0;
		inline static NxReal CalcLagrange(const NxReal& lagrange,
			const NxReal& c, const NxVector3* gradient, const NxReal* w, int num , 
			const NxReal& compliance, const NxReal& stepTime)
		{
			/* CFunc()ΪԼ��������Grad[]Ϊ��
			* Լ������CFunc�ҵ�һ��dpʹ�õ���0�������һ��������̩��һ��չ���Ľ���
			* ����1: CFunc(p+dp) = CFunc(p) + Grad[C(p)] * dp = 0
			* ����2: dp = Lambda * Grad[C(p)]
			* ����2���뷽��1�����
			* Lambda = -CFunc(p)/(|Grad[C(p)]|^2)
			* �ٴ��뷽��2���ɵõ�dp = (-CFunc(p)/(|Grad[C(p)]|^2)) * Grad[C(p)]
			* ���ձ�ɶ�Ԫ����Ӧ���������ճ˷�����dp��ֵ
			* �����Ͼ���ÿ������dp[i] = S * Grad[C(p[i])];
			* ����������Ҫ��������������ճ˷���S���ٿ�������Ӱ�죬���������ĵ�����Ȩ����w = 1 / m��
			* S = CFunc(p) / Sum( (|Grad[C(p)]|^2) * w[i] )
			* |Grad[C(p)]|^2����ģƽ��������ͨ��LengthSquared()���
			*/
			NxReal dv = NxReal::Zero();
			for (int i = 0; i < num; i++)
			{
				dv += gradient[i].LengthSquared() * w[i];
			}
			
			auto timed_compliance = compliance / NxReal::Pow(stepTime, 2);
			return (-c - timed_compliance * lagrange) / (dv + timed_compliance);
		}
	};
	class NxContactConstraint : public NxJoint
	{
		NxReal mRagrange = NxReal::Zero();
	public:
		ENGINE_RTTI(NxContactConstraint);
		NxShapePair mShapePair;

		NxRigidBody* GetRigidBody1() const {
			return (NxRigidBody*)mShapePair.first->GetActor();
		}
		NxRigidBody* GetRigidBody2() const {
			return (NxRigidBody*)mShapePair.second->GetActor();
		}

		NxReal mCompliance = NxReal::Zero();//��ȶ�Ӧ�ڸնȵĵ�������λ����/ţ��
		NxVector3 mContactDirection;
		
		inline NxReal GetRagrange() const {
			return mRagrange;
		}
		virtual void ResetRagrange() override
		{
			mRagrange = NxReal::Zero();
			mContactDirection = NxVector3::Zero();
		}
		inline NxVector3 FixDistance(const NxVector3& p0, const NxVector3& p1, const NxReal& limitMin, const NxReal& limitMax)
		{
			auto offset = p1 - p0;
			auto distance = offset.Length();
			if (NxReal::EpsilonEqual(distance, NxReal::Zero()))
			{
				return NxVector3::Zero();
			}
			if (distance < limitMin)
			{
				auto dir = offset / distance;
				return -(dir * (distance - limitMin));
			}
			else if (distance > limitMax)
			{
				auto dir = offset / distance;
				return -(dir * (distance - limitMax));
			}
			else
			{
				return NxVector3::Zero();
			}
		}
		void BuildConstraint();
		virtual void SolveConstraint(NxScene* scene, const NxReal& time) override;
	};
}

NS_END

