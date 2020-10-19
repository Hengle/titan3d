#pragma once

#include "../thread/vfxthread.h"
#include "../thread/vfxcritical.h"
#include "../thread/vfxevent.h"
#include "../thread/AsyncObjManager.h"

typedef void (FResourceLoadFinished)(IResource* pObj);
class vLoadPipe;
class vFreePipe;

class vLoadAndFreeThread
{
	friend class vLoadPipe;
	friend class vFreePipe;

	VCritical		mLocker;
public:
	vBOOL			mThreadRunning;
public:
	vLoadAndFreeThread()
	{
		mThreadRunning = FALSE;
	}
	~vLoadAndFreeThread();
	static  vLoadAndFreeThread* GetInstance();

	void ForceInvalidateResource(IResource* pRes);

	void EmptyAllPool();//ֻ�����̵߳��ã��豸��ʧ��ʱ��,��Reset֮ǰ���̵߳���,Ȼ��������ResMgr��InvalidateObjects����֤�����ڵ����ж��󶼱�Invalidate
						
	int GetAllCallBacksNumber();
};

class vLoadPipe
{
public:
	struct WaitResource
	{
		WaitResource()
		{
			ResObject = NULL;
			CallBack = nullptr;
		}
		IResource*				ResObject;
		FResourceLoadFinished*	CallBack;
	};
private:
	size_t							dccc;
	friend vLoadAndFreeThread;
	friend vFreePipe;

	typedef std::map<IResource*, WaitResource>	ResContain;
	ResContain			m_Loads;
	IResource*			mWaitObject;
	IResource*			mStreamingObject;
	VCritical			mLocker;
public:
	bool				mOutputLoadInfo;

	int					mForcePreUseCount;
	int					mAsyncCount;

	vLoadPipe();
	~vLoadPipe();
	void Cleanup();

	void			TestDccc(const char* name);

	bool	PreUse(IResource* pObj, bool bForce, INT64 timeOut = INT64_MAX, FResourceLoadFinished* fn = nullptr);//ֻ�����̵߳��ã�������ض�����Դ
	void	Remove(IResource* OpObj);
	void	Push(IResource* OpObj, FResourceLoadFinished* fn);
	WaitResource PopNoRelease();

	void	OnLoad();
	void	LoadResource(const WaitResource& wr);

	size_t GetAsyncLoadNumber();

	IResource*	InvalidStreamingObject();

	static  vLoadPipe* GetInstance();
};

struct vFreePair
{
	INT64			PushTime;
	UINT			DelayTime;
	IResource*		OpObj;
};

class vFreePipe
{
	friend vLoadAndFreeThread;
	friend vLoadPipe;
	///���Ը���ƽ��������ض��е�ʱ�䣬���ͷ������ͷŶ��еĶ�����Դ
	typedef std::map<IResource*, vFreePair>	FreeContain;
	FreeContain			m_Frees;
	VCritical			mLocker;
public:
	vFreePipe()
	{
		Cleanup();
	}

	static  vFreePipe* GetInstance();

	void	OnFreeTick(bool bForce);

	vBOOL FreeObj(IResource* OpObj, INT64 time, UINT delay);//ֻ�����̵߳��ã��ͷŶ�����Դ

	void Push(IResource* OpObj, UINT Delay, INT64 time);
	void Remove(IResource* OpObj);

	void Cleanup();
};