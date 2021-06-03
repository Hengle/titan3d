/********************************************************************
	created:	2002/12/25
	created:	25:12:2002   20:39
	filename: 	geometry\v3dxOBB.h
	file path:	geometry
	file base:	v3dxOBB
	file ext:	h
	author:		johnson
	modify:		
	
	purpose:	
*********************************************************************/
#ifndef __v3dxOBB__h__25_12_2002_20_39__
#define __v3dxOBB__h__25_12_2002_20_39__
#include "v3dxMatrix4.h"
#include "v3dxPlane3.h"
#include "v3dxBox3.h"

#pragma pack(push,4)

struct v3dTransUtility
{
	//�������任�����FromTM��ToTM����Ա任
	//ToTM�����任����Ŀ��ռ�
	//FromTM���Ӵ�Ŀ��任
	static  v3dxMatrix4 GetRelativeTM( const v3dxMatrix4& ToTM , const v3dxMatrix4& FromTM );

	//����Ա任�任��Ϊ���Ա任
	static  v3dxMatrix4 GetAbsTM( const v3dxMatrix4& BaseTM , const v3dxMatrix4& RelativeTM );

	//�Ѿ�������λ�ñ任��Ϊ�������
	static  v3dxVector3 GetRelativePos( const v3dxMatrix4& BaseTM , const v3dxVector3& AbsPos );
	//�Ѿ������������任��Ϊ�������
	static  v3dxVector3 GetRelativeNormal( const v3dxMatrix4& BaseTM , const v3dxVector3& AbsNormal );
};

class v3dxOBB
{
public:
	static  bool ComputeBestObbMatrix(size_t vcount,     // number of input data points
		const float *points,     // starting address of points array.
		size_t vstride,    // stride between input points.
		const float *weights,    // *optional point weighting values.
		size_t wstride,    // weight stride for each vertex.
		float *matrix);

	//����obb�Ƿ��ڱ�obb��6��������棬Ҫ�����ж�2��obb�Ƿ��ཻ������Ҫ�öԷ�obb�������Լ�
	//����,������������obb�����ſ�Խƽ�������ܲ����ж�
	 vBOOL IsFastOutRef( const v3dxOBB& obb , const v3dxMatrix4& tar_tm );
	//��ȷ�����Ƿ�obb�ཻ
	//tar_tm��Ҫ��:
	//��Ա�OBB����ϵ��Ŀ��OBB�ı任����
	 bool IsOverlap( const v3dxOBB& obb , const v3dxMatrix4& tar_tm ) const;
	//�߶��ཻ
	//pvFrom,pvDir��Ҫ�任����OBB������ռ�����
	 bool IsIntersect( float *pfT_n,
		v3dxVector3 *pvPoint_n,
		float *pfT_f,
		v3dxVector3 *pvPoint_f,
		const v3dxVector3 *pvFrom,
		const v3dxVector3 *pvDir );
	//vector����
	//v��Ҫ�任����OBB������ռ�����
	inline bool In( const v3dxVector3& v ){
		if( v.x>m_vExtent.x || v.x<-m_vExtent.x ||
			v.y>m_vExtent.y || v.y<-m_vExtent.y || 
			v.z>m_vExtent.z || v.z<-m_vExtent.z )
			return false;
		return true;
	}

	inline v3dxPlane3 GetXPlane1() const{
		return v3dxPlane3( -1.f , 0.f , 0.f , -m_vExtent.x );
	}
	inline v3dxPlane3 GetXPlane2() const{
		return v3dxPlane3( 1.f , 0.f , 0.f , m_vExtent.x );
	}
	inline v3dxPlane3 GetYPlane1() const{
		return v3dxPlane3( 0.f , -1.f , 0.f , -m_vExtent.y );
	}
	inline v3dxPlane3 GetYPlane2() const{
		return v3dxPlane3( 0.f , 1.f , 0.f , m_vExtent.y );
	}
	inline v3dxPlane3 GetZPlane1() const{
		return v3dxPlane3( 0.f , 0.f , -1.f , -m_vExtent.z );
	}
	inline v3dxPlane3 GetZPlane2() const{
		return v3dxPlane3( 0.f , 0.f , 1.f , m_vExtent.z );
	}
	inline v3dxVector3 GetCorner( int e ) const{
		switch( e ){
			case BOX3_CORNER_xyz:
				return v3dxVector3(-m_vExtent.x,-m_vExtent.y,-m_vExtent.z);
			case BOX3_CORNER_xyZ:
				return v3dxVector3(-m_vExtent.x,-m_vExtent.y,m_vExtent.z);
			case BOX3_CORNER_xYz:
				return v3dxVector3(-m_vExtent.x,m_vExtent.y,-m_vExtent.z);
			case BOX3_CORNER_xYZ:
				return v3dxVector3(-m_vExtent.x,m_vExtent.y,m_vExtent.z);
			case BOX3_CORNER_Xyz:
				return v3dxVector3(m_vExtent.x,-m_vExtent.y,-m_vExtent.z);
			case BOX3_CORNER_XyZ:
				return v3dxVector3(m_vExtent.x,-m_vExtent.y,m_vExtent.z);
			case BOX3_CORNER_XYz:
				return v3dxVector3(m_vExtent.x,m_vExtent.y,-m_vExtent.z);
			case BOX3_CORNER_XYZ:
				return m_vExtent;
			default:
				return v3dxVector3::ZERO;
		}
	}
public:
	v3dxVector3			m_vExtent;
};

#pragma pack(pop)

#endif//#ifndef __v3dxOBB__h__25_12_2002_20_39__
