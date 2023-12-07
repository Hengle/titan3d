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
	};
	class NxDistanceJoint : public NxJoint
	{
	public:
		ENGINE_RTTI(NxDistanceJoint);
		NxActorPair mActorPair;

		NxReal mLimitMin;
		NxReal mLimitMax;
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

