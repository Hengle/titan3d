// v3dKeyTime.h : header file
//
// Author : johnson
// Modifer :	
// Create Timer :	 2006-4-16   21:27
// Modify Timer :	 
//--------------------------------------------------------------------------------------------------
#pragma once
#include "../precompile.h"

//===========================================================================
// Summary:
//		v3dKeyTime�����Ĺؼ�֡��Ϣ
//===========================================================================
class TimeKeys
{
public:
	//-----------------------------------------------------------------------
	// Summary:
	//		v3dKeyTime�Ĺ��캯��
	//-----------------------------------------------------------------------
	TimeKeys();

	//-----------------------------------------------------------------------
	// Summary:
	//		v3dKeyTime����������
	//-----------------------------------------------------------------------
	~TimeKeys();

	//-----------------------------------------------------------------------
	// Summary:
	//		v3dKeyTime�Ŀ������캯��
	//-----------------------------------------------------------------------
	TimeKeys(const TimeKeys & ref);

	//-----------------------------------------------------------------------
	// Summary:
	//		v3dKeyTime�Ŀ�����ֵ����
	//-----------------------------------------------------------------------
	TimeKeys & operator = (const TimeKeys & ref);

	//-----------------------------------------------------------------------
	// Summary:
	//		������������.���ظ�����.
	//-----------------------------------------------------------------------
	void FinalCleanup();

	//-----------------------------------------------------------------------
	// Summary:
	//		���ñ��������鿴�Ƿ���Ҫ˫����Ⱦ.
	// Parameters:
	//		uTime - �������,ָ���õ���ֵ��Ϣ��ʱ��.�ڲ���ʱ�䰴����ʱ�䳤ȡ��.
	//		uFrm1 - �������,��ֵ��ǰһ֡
	//		uFrm2 - �������,��ֵ�ĺ�һ֡
	//		fSlerp - �������,��ֵ�ĺ�һ֡��Ӱ��ϵ��
	//		��ֵ���=uFrm1+(uFrm2-uFrm1)*fSlerp
	// Returns:
	//		����������ݶ�׼������,���ط���ֵ,����,������.
	//���ֵ����������������ʧ��.
	//-----------------------------------------------------------------------
	vBOOL GetTweenTimeParam(INT64 uTime,UINT & uFrm1,UINT & uFrm2,FLOAT & fSlerp) const;

	//-----------------------------------------------------------------------
	// Summary:
	//		����ؼ�֡��Ϣ���ļ�.
	// Returns:
	//		�ɹ����ط���,����,������.
	//-----------------------------------------------------------------------
	template<class _iotype>
	vBOOL Save(_iotype & file) const;

	//-----------------------------------------------------------------------
	// Summary:
	//		���عؼ�֡����.
	// Returns:
	//		�ɹ����ط���,����,������.
	//-----------------------------------------------------------------------
	template<class _iotype>
	vBOOL Load(_iotype & file);

	//-----------------------------------------------------------------------
	// Summary:
	//		�õ��ؼ�֡��Ϣ�ܵ�ʱ��.
	// Returns:
	//		���عؼ�֡��Ϣ�ܵ�ʱ��.��λ����.
	//-----------------------------------------------------------------------
	UINT GetDuration() const{ return m_uDuration;}

	//-----------------------------------------------------------------------
	// Summary:
	//		�õ��ؼ�֡��Ϣ�ܵ�֡��.
	// Returns:
	//		���عؼ�֡��Ϣ�ܵ�֡��.
	//-----------------------------------------------------------------------
	UINT GetFrameCount() const{ return m_uFrameCount;}

	//-----------------------------------------------------------------------
	// Summary:
	//		�õ��ؼ�֡��Ŀ.
	// Returns:
	//		���عؼ�֡��Ŀ.
	//-----------------------------------------------------------------------
	UINT GetKeyCount() const{ return m_uKeyCount;}

	//-----------------------------------------------------------------------
	// Summary:
	//		�õ��ؼ�֡��ʱ���б�����.
	// Returns:
	//		���عؼ�֡��ʱ���б�����.����ͨ��GetKeyCount()�õ�.
	//-----------------------------------------------------------------------
	UINT * GetTimes() const{return m_pTimes;}

	//-----------------------------------------------------------------------
	// Summary:
	//		�õ�ָ���ؼ�֡��ʱ��.
	// Parameters:
	//		uIndex - ָ���Ĺؼ�֡.
	// Returns:
	//		����ָ���ؼ�֡��ʱ��.��λ����.
	//-----------------------------------------------------------------------
	UINT & operator [](size_t uIndex){ return m_pTimes[uIndex];}

	//-----------------------------------------------------------------------
	// Summary:
	//		�õ�ָ���ؼ�֡��ʱ��.
	// Parameters:
	//		uIndex - ָ���Ĺؼ�֡.
	// Returns:
	//		����ָ���ؼ�֡��ʱ��.��λ����.
	//-----------------------------------------------------------------------
	UINT operator [](size_t uIndex) const{ return m_pTimes[uIndex];}

	//-----------------------------------------------------------------------
	// Summary:
	//		�����ؼ�֡����.
	// Parameters:
	//		uKeyCount - �ؼ�֡��Ŀ.
	// Returns:
	//		���عؼ�֡��ʱ���б�����.
	//-----------------------------------------------------------------------
	UINT * CreateTimes(UINT uKeyCount);

	//-----------------------------------------------------------------------
	// Summary:
	//		���ùؼ�֡��ʱ�䳤�Ⱥ���֡����Ϣ.
	// Parameters:
	//		uDuration - �ؼ�֡��Ϣ�ܵ�ʱ��.
	//		uFrameCount - �ؼ�֡��Ϣ�ܵ�֡��.
	//-----------------------------------------------------------------------
	void __SetDurationFrame(UINT uDuration,UINT uFrameCount)
	{
		m_uDuration = uDuration;
		m_uFrameCount = uFrameCount;
	}

	//-----------------------------------------------------------------------
	// Summary:
	//		�õ��ڲ�ʹ�õ���Դ�ĳߴ�.
	// Returns:
	//		�����ڲ�ʹ�õ���Դ�ĳߴ�,��λBYTE.
	//-----------------------------------------------------------------------
	UINT GetResSize() const{ return m_uResSize;}
protected:
	UINT			m_uDuration;			//�ؼ�֡��Ϣ�ܵ�ʱ��
	UINT			m_uFrameCount;			//�ؼ�֡��Ϣ�ܵ�֡��
	UINT			m_uKeyCount;			//�ؼ�֡��Ŀ
    UINT			m_uResSize;				//ʹ�õ���Դ�ĳߴ�
	UINT *			m_pTimes;				//�ؼ�֡��ʱ���б���Ϣ
	
};

template<class _iotype>
vBOOL TimeKeys::Save(_iotype & file) const
{
	file.Write(&m_uDuration,sizeof(m_uDuration));
	file.Write(&m_uFrameCount,sizeof(m_uFrameCount));
	file.Write(&m_uKeyCount,sizeof(m_uKeyCount));

	file.Write(m_pTimes,sizeof(UINT) * m_uKeyCount);
	return TRUE;
}

template<class _iotype>
vBOOL TimeKeys::Load(_iotype & file)
{
	UINT uKeyCount = 0;
	m_uDuration = 0;
	m_uFrameCount = 0;
	file.Read(&m_uDuration, sizeof(m_uDuration));
	file.Read(&m_uFrameCount, sizeof(m_uFrameCount));
	file.Read(&uKeyCount, sizeof(uKeyCount));
	if (uKeyCount <= 0)
		return FALSE;

	CreateTimes(uKeyCount);
	file.Read(m_pTimes, sizeof(UINT) * uKeyCount);

	UINT uTimeBase = m_pTimes[0];
	for (UINT i = 1; i < uKeyCount; ++i)
	{
		m_pTimes[i] -= uTimeBase;
	}

	if (m_uDuration > m_pTimes[uKeyCount - 1])
		return FALSE;

	m_uResSize = sizeof(UINT) * uKeyCount;
	return TRUE;
}
