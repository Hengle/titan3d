#include "NxShape.h"

NS_BEGIN

namespace NxPhysics
{
	ENGINE_RTTI_IMPL(NxShape);
	ENGINE_RTTI_IMPL(NxSphereShape);

	bool NxSphereShape::Init(const NxSphereShapeDesc& desc)
	{
		mDesc = desc;

		mShapeData.Mass = desc.Density * GetVolume();
		mShapeData.InvMass = NxReal::One() / mShapeData.Mass;
		return true;
	}
}

NS_END