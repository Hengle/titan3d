#ifndef __v3dxSphere__H__
#define __v3dxSphere__H__

#include "v3dxVector3.h"

#pragma pack(push,4)

/** A sphere primitive, mostly used for bounds checking. 
���ͼԪ�����ڰ󶨼�⡣
*/
class v3dxSphere
{
protected:
	float mRadius;
	v3dxVector3 mCenter;
public:
	bool IsEmpty()
	{
		return mRadius<0.f ? true : false;
	}
	/** Standard constructor*/
	v3dxSphere() : mRadius(-FLT_MAX), mCenter(v3dxVector3::ZERO) {}

	/** Constructor allowing arbitrary spheres. 
	*/
	v3dxSphere(const v3dxVector3& center, float radius): mRadius(radius), mCenter(center) {}

	/** Returns the radius of the sphere. 
	������İ뾶
	*/
	float getRadius(void) const { 
		return mRadius; 
	}

	/** Sets the radius of the sphere. 
	������İ뾶
	*/
	void setRadius(float radius) { 
		mRadius = radius; 
	}

	/** Returns the center point of the sphere. 
	��������е�
	*/
	const v3dxVector3& getCenter(void) const { 
		return mCenter; 
	}

	/** Sets the center point of the sphere. 
	��������е�
	*/
	void setCenter(const v3dxVector3& center) { 
		mCenter = center; 
	}

	vBOOL IsIn( const v3dxVector3 &pos )
	{
		v3dxVector3 rvec(pos - mCenter);
		float rSqr = rvec.getLength();
		return rSqr <= mRadius;
	}
};

#pragma pack(pop)

#endif