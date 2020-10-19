#pragma once
#include "GfxParticle.h"

NS_BEGIN

class GfxParticleModifier;
class GfxParticleSubState;

/*
���ӷ���������
1�����������������ȷ�����ӵ���ʱ�ĳ�ʼ��̬
2���������Ĭ�ϵĵ㷢��������
3����Ϊ�ķ��塢������(׶��)������/�����塢ģ��������
*/
class GfxParticleEmitterShape : public VIUnknown
{
public:
	RTTI_DEF(GfxParticleEmitterShape, 0x2259be565ba31f9a, true);
	GfxParticleEmitterShape();
	void SetEmitter(GfxParticleSubState *pEmitter);
	virtual ~GfxParticleEmitterShape();
	virtual void GenEmissionPose(GfxParticleState *pParticle);

	VDef_ReadWrite(vBOOL, IsEmitFromShell, m);
	VDef_ReadWrite(vBOOL, IsRandomDirection, m);
	VDef_ReadWrite(vBOOL, RandomDirAvailableX, m);
	VDef_ReadWrite(vBOOL, RandomDirAvailableY, m);
	VDef_ReadWrite(vBOOL, RandomDirAvailableZ, m);
	VDef_ReadWrite(vBOOL, RandomDirAvailableInvX, m);
	VDef_ReadWrite(vBOOL, RandomDirAvailableInvY, m);
	VDef_ReadWrite(vBOOL, RandomDirAvailableInvZ, m);
	VDef_ReadWrite(vBOOL, RandomPosAvailableX, m);
	VDef_ReadWrite(vBOOL, RandomPosAvailableY, m);
	VDef_ReadWrite(vBOOL, RandomPosAvailableZ, m);
	VDef_ReadWrite(vBOOL, RandomPosAvailableInvX, m);
	VDef_ReadWrite(vBOOL, RandomPosAvailableInvY, m);
	VDef_ReadWrite(vBOOL, RandomPosAvailableInvZ, m);
protected:
	virtual void GenEmissionPosition(GfxParticleState *pParticle);
	virtual void GenEmissionDirection(GfxParticleState *pParticle);
	void CalcEmitterAxis(v3dxVector3& vX, v3dxVector3& vY, v3dxVector3& vZ);
	void ProcessOffsetPosition(v3dxVector3 &vec);
	float GetAvaiableRandomPosValue(int type);

public:
	virtual GfxParticleEmitterShape *Clone(GfxParticleEmitterShape* pNew = NULL);

	vBOOL mIsEmitFromShell;	// �Ƿ����������淢��
	
	vBOOL mIsRandomDirection;// �Ƿ�������򣨷��������������õ�����췽���䣩
	vBOOL mRandomDirAvailableX;		// �������X�����
	vBOOL mRandomDirAvailableY;		// �������Y�����
	vBOOL mRandomDirAvailableZ;		// �������Z�����
	vBOOL mRandomDirAvailableInvX;	// �������-X�����
	vBOOL mRandomDirAvailableInvY;	// �������-Y�����
	vBOOL mRandomDirAvailableInvZ;	// �������-Z�����
	
	vBOOL mRandomPosAvailableX;		// ���λ��X�����
	vBOOL mRandomPosAvailableY;		// ���λ��Y�����
	vBOOL mRandomPosAvailableZ;		// ���λ��Z�����
	vBOOL mRandomPosAvailableInvX;	// ���λ��-X�����
	vBOOL mRandomPosAvailableInvY;	// ���λ��-Y�����
	vBOOL mRandomPosAvailableInvZ;	// ���λ��-Z�����
protected:
	TObjectHandle<GfxParticleSubState>	mHost;
};

class GfxParticleEmitterShapeBox : public GfxParticleEmitterShape
{
public:
	RTTI_DEF(GfxParticleEmitterShapeBox, 0x0a842f1d5ba31fb5, true);
	GfxParticleEmitterShapeBox();
	virtual void GenEmissionPosition(GfxParticleState *pParticle) override;

	v3dxVector3 GetSize();
	void SetSize(const v3dxVector3& size);
	virtual GfxParticleEmitterShape *Clone(GfxParticleEmitterShape* pNew) override;

	VDef_ReadWrite(float, SizeX, m);
	VDef_ReadWrite(float, SizeY, m);
	VDef_ReadWrite(float, SizeZ, m);
public:
	float mSizeX;
	float mSizeY;
	float mSizeZ;
};

class GfxParticleEmitterShapeCone : public GfxParticleEmitterShape
{
public:
	enum enDirectionType
	{
		EDT_ConeDirUp,
		EDT_ConeDirDown,
		EDT_EmitterDir,
		EDT_NormalOutDir,
		EDT_NormalInDir,
		EDT_OutDir,
		EDT_InDir,
	};

public:
	RTTI_DEF(GfxParticleEmitterShapeCone, 0x9400c01b5ba31fc7, true);
	GfxParticleEmitterShapeCone();
protected:
	virtual void GenEmissionPosition(GfxParticleState *pParticle) override;
	virtual void GenEmissionDirection(GfxParticleState *pParticle) override;
private:
	float GetCrossSectionRadius(float sliderVertical);
public:
	virtual GfxParticleEmitterShape *Clone(GfxParticleEmitterShape* pNew) override;

	float mAngle;	// ���Ե��б�Ƕ�
	float mRadius;	// ������뾶
	float mLength;	// ���ȳ���
	enDirectionType mDirType;

	VDef_ReadWrite(float, Angle, m);
	VDef_ReadWrite(float, Radius, m);
	VDef_ReadWrite(float, Length, m);
	VDef_ReadWrite(enDirectionType, DirType, m);
};

class GfxParticleEmitterShapeSphere : public GfxParticleEmitterShape
{
public:
	RTTI_DEF(GfxParticleEmitterShapeSphere, 0xed1aefe65ba31fd4, true);
	GfxParticleEmitterShapeSphere();
	virtual void GenEmissionPose(GfxParticleState *pParticle) override;

protected:
	virtual void GenEmissionPosition(GfxParticleState *pParticle) override;
	virtual void GenEmissionDirection(GfxParticleState *pParticle) override;
public:
	virtual GfxParticleEmitterShape *Clone(GfxParticleEmitterShape* pNew) override;

	float mRadius;	// �뾶
	vBOOL mIsRadialOutDirection;
	vBOOL mIsRadialInDirection;
	vBOOL mIsHemiSphere;// �Ƿ��ǰ�����
	VDef_ReadWrite(float, Radius, m);
	VDef_ReadWrite(vBOOL, IsRadialOutDirection, m);
	VDef_ReadWrite(vBOOL, IsRadialInDirection, m);
	VDef_ReadWrite(vBOOL, IsHemiSphere, m);
};

class GfxParticleEmitterShapeMesh : public GfxParticleEmitterShape
{
public:
	RTTI_DEF(GfxParticleEmitterShapeMesh, 0x340af8aa5ba31fe2, true);
	GfxParticleEmitterShapeMesh();
	void SetPoints(v3dxVector3* points, int num);
protected:
	virtual void GenEmissionPosition(GfxParticleState *pParticle) override;
	virtual void GenEmissionDirection(GfxParticleState *pParticle) override;
public:
	std::vector<v3dxVector3>	mEmitterPoints;
};

NS_END