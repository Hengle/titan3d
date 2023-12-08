#pragma once
#include "NxConstraint.h"

NS_BEGIN

namespace NxPhysics
{
	using NxActorPair = std::pair<NxAutoRef<NxActor>, NxAutoRef<NxActor>>;
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
			* ��������Է��̣�ţ��-����ɭ����
			* �����Ͼ���ÿ������dp[i] = S * Grad[C(p[i])];
			* ����������Ҫ�������ϵ��S���ٿ�������Ӱ�죬���������ĵ�����Ȩ����w = 1 / m��
			* S = CFunc(p) / Sum( (|Grad[C(p)]|^2) * w[i] )
			* |Grad[C(p)]|^2����ģƽ��������ͨ��LengthSquared()���
			*/
			NxReal dv = NxReal::Zero();
			for (int i = 0; i < num; i++)
			{
				dv += gradient[i].LengthSquared() * w[i];
			}
			
			auto timed_compliance = compliance / NxReal::Pow(stepTime, 2);
			return (c - timed_compliance * lagrange) / (dv + timed_compliance);
		}
	};
	class NxDistanceJoint : public NxJoint
	{
		NxReal mRagrange = NxReal::Zero();
	public:
		ENGINE_RTTI(NxDistanceJoint);
		NxActorPair mActorPair;

		NxReal mCompliance = NxReal::Zero();//��ȶ�Ӧ�ڸնȵĵ�������λ����/ţ��
		NxReal mLimitMin;
		NxReal mLimitMax;
		virtual void ResetRagrange() override
		{
			mRagrange = NxReal::Zero();
		}
		inline NxVector3 FixDistance(const NxVector3& p0, const NxVector3& p1, const NxReal& limitMin, const NxReal& limitMax)
		{
			auto offset = p1 - p0;
			auto distance = offset.Length();
			if (distance <= NxReal::Epsilon())
			{
				return NxVector3::Zero();
			}
			if (distance < limitMin)
			{
				auto percent = (distance - limitMin) / distance;
				auto result = offset * percent;
				return -result;
			}
			else if (distance > limitMax)
			{
				return -(offset / distance * (distance - limitMax));
			}
			else
			{
				return NxVector3::Zero();
			}
		}
		virtual void SolveConstraint(NxScene* scene, const NxReal& time) override;

		static NxReal CalcLimitMin(const NxRigidBody* body0, const NxRigidBody* body1);
	};
}

NS_END

