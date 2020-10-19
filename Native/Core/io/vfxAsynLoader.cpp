#include "../precompile.h"
#include "vfxAsynLoader.h"
#include "../vfxSampCounter.h"

#define new VNEW

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wswitch"
#pragma clang diagnostic ignored "-Wunknown-pragmas"
#endif

extern void* GLoadThreadId;
static INT64 GEngineTick = 0;
extern "C"
{
	void Thread_SetName(LPCSTR name);

	LPCSTR vfxMemory_GetDebugInfo(void* memory);
	INT64 HighPrecision_GetTickCount();
}

typedef void (WINAPI *FNativeCallManage)(int type, int op, LPCSTR pszFile, int line);
FNativeCallManage GOnLoadCall = nullptr;

 INT64 vfxGetEngineTick()
{
	return GEngineTick;
}
// INT64 vfxGetHPEngineTick()
//{
//	return GHPEngineTick;
//}
 void vfxSetEngineTick(INT64 time)
{
	//GHPEngineTick = time;
	//GEngineTick = time / 1000;
	GEngineTick = time;
}

extern void VMem_CheckMemChain();

typedef void(WINAPI *FOnAsyncLoadObject)(int count, LPCSTR classType, LPCSTR sourceFile);
FOnAsyncLoadObject GOnAsyncLoadObject = NULL;

UINT GAsyncFrame = 0;
INT64 GLastAsyncTime = 0;

vBOOL GLoadThreadPaused = FALSE;
vLoadAndFreeThread* vLoadAndFreeThread::GetInstance()
{
	static vLoadAndFreeThread o;
	return &o;
}

vLoadAndFreeThread::~vLoadAndFreeThread()
{
	EmptyAllPool();
}

int vLoadAndFreeThread::GetAllCallBacksNumber()
{
	int num = 0;
	num += (int)(vLoadPipe::GetInstance()->m_Loads.size());
	num += (int)(vFreePipe::GetInstance()->m_Frees.size());
	return num;
}

void vLoadAndFreeThread::EmptyAllPool()
{
	if (vfxThread::GetCurrentThreadId() == GLoadThreadId)
	{
		ASSERT(FALSE);//��Ӧ�÷���
		auto& loads = vLoadPipe::GetInstance()->m_Loads;
		while (loads.size() > 0)
		{
			vLoadPipe::GetInstance()->OnLoad();
		}
	}
	else
	{
		auto& loads = vLoadPipe::GetInstance()->m_Loads;
		if (GLoadThreadPaused == FALSE)
		{
			while (loads.size() > 0)
			{
				Sleep(50);
			}
		}
		else
		{
			auto& loads = vLoadPipe::GetInstance()->m_Loads;
			while (loads.size() > 0)
			{
				vLoadPipe::GetInstance()->OnLoad();
			}
		}
	}
	vFreePipe::GetInstance()->OnFreeTick(true);
	if (GetAllCallBacksNumber() > 0)
	{
		EmptyAllPool();
	}
}

void vLoadAndFreeThread::ForceInvalidateResource(IResource* pRes)
{
	StreamingState state = pRes->GetStreamState();
	switch (state)
	{
	case SS_Invalid:
		break;
	case SS_Pending:
	{
		vLoadPipe::GetInstance()->Remove(pRes);
		return;
	}
	break;
	case SS_Streaming:
		break;
	case SS_Valid:
	{
		pRes->Invalidate();
		pRes->__SetStreamState(SS_Invalid);
	}
	break;
	case SS_PendingKill:
	{
		vFreePipe::GetInstance()->Remove(pRes);
		pRes->Invalidate();
		pRes->__SetStreamState(SS_Invalid);
		return;
	}
	break;
	case SS_Killing:
		break;
	case SS_Killed:
		break;
	}
}

vLoadPipe GLoadPipe;
vLoadPipe* vLoadPipe::GetInstance()
{
	return &GLoadPipe;
}
vFreePipe GFreePipe;
vFreePipe* vFreePipe::GetInstance()
{
	return &GFreePipe;
}

#ifdef WIN64
const size_t c_uMalloc0xCC = 0xCCCCCCCCCCCCCCCC;
#else
const size_t c_uMalloc0xCC = 0xCCCCCCCC;
#endif
vLoadPipe::vLoadPipe()
	: mOutputLoadInfo(false)
{
	dccc = c_uMalloc0xCC;
	mForcePreUseCount = 0;
	mAsyncCount = 0;
	mWaitObject = NULL;
	mStreamingObject = NULL;
}

vLoadPipe::~vLoadPipe()
{
	Cleanup();
}

void vLoadPipe::Cleanup()
{
}

size_t vLoadPipe::GetAsyncLoadNumber()
{
	return m_Loads.size();
}

void vLoadPipe::Push(IResource* OpObj, FResourceLoadFinished* fn)
{
	VAutoLock(mLocker);
	if (vLoadAndFreeThread::GetInstance()->mThreadRunning == FALSE)
	{
		return;
	}

	auto state = OpObj->GetStreamState();
	if (state == SS_Valid)
		return;

	if (state == SS_Pending || state == SS_Streaming)
	{
		if (m_Loads.size() > 0)
		{
			return;
		}
		else
		{
			//This is a bug,but we don't know why now;
			VFX_LTRACE(ELTT_Warning, "This is an amazing bug!!!StreamingState is Pending while LoadPipe is empty...\nResName: %s;\n",
				OpObj->GetName());
		}
	}

#if defined(DEBUG)
	auto i = m_Loads.find(OpObj);
	if (i != m_Loads.end())
	{
		VFX_LTRACE(ELTT_Error, "vLoadPipe::Push finded object\r\n");
		return;
	}
#endif
	//m_Loads.insert(std::make_pair(OpObj,OpObj));
	WaitResource wr;
	wr.ResObject = OpObj;
	wr.CallBack = fn;
	m_Loads[OpObj] = wr;
	OpObj->AddRef();
	OpObj->__SetStreamState(SS_Pending);
	OpObj->mPreUseCounterBeforeStreaming++;
}

vLoadPipe::WaitResource vLoadPipe::PopNoRelease()
{
	VAutoLock(mLocker);
	INT64 curTime = -1;

	ResContain::iterator index = m_Loads.end();
	for (auto i = m_Loads.begin(); i != m_Loads.end(); i++)
	{
		IResource* cur = i->second.ResObject;
		if (cur->mLoadPriority == INT_MAX)
		{
			index = i;
			break;
		}

		INT64 loadValue = cur->GetAccessTime() * cur->mPreUseCounterBeforeStreaming * cur->mLoadPriority;
		if (curTime < loadValue)
		{
			curTime = loadValue;
			index = i;
		}
	}
	if (index == m_Loads.end())
		return vLoadPipe::WaitResource();

	auto pRes = index->second.ResObject;
	mStreamingObject = pRes;
	auto wr = index->second;
	m_Loads.erase(index);
	return wr;
}
void vLoadPipe::Remove(IResource* OpObj)
{
	VAutoLock(mLocker);
	auto iter = m_Loads.find(OpObj);
	if (iter != m_Loads.end())
	{
		m_Loads.erase(iter);
		ASSERT(OpObj->GetStreamState() == SS_Pending);
		OpObj->__SetStreamState(SS_Invalid);
		OpObj->Release();
	}
}

vBOOL bPreUseForce = FALSE;
bool vLoadPipe::PreUse(IResource* OpObj, bool bForce, INT64 timeOut, FResourceLoadFinished* fn)
{
	AUTO_SAMP("vLoadPipe.PreUse");
	if (OpObj == NULL)
		return false;
	if (bPreUseForce)
		bForce = TRUE;

	bool IsCallByIOThread = vfxThread::GetCurrentThreadId() == GLoadThreadId;

	if (bForce)
	{//ǿ������ʹ��
		AUTO_SAMP("vLoadPipe.PreUse.Force");
		StreamingState curState = OpObj->GetStreamState();
#pragma region ����ʹ��
		switch (curState)
		{
		case SS_Streaming:
		{//ǿ������ʹ�����ڼ����̼߳��ص���Դ�������¼��ȴ����ؽ���
			VFX_LTRACE(ELTT_Resource, vT("PreUse %s True Streaming Resource--wait begin\r\n"), OpObj->GetName());
			if (IsCallByIOThread == false)
			{
				while (OpObj->GetStreamState() != SS_Valid)
				{
					Sleep(50);
					if (mStreamingObject != OpObj && m_Loads.size() == 0)
					{
						if (OpObj->GetStreamState() == SS_Valid)
							return true;
						VFX_LTRACE(ELTT_Error, vT("PreUse %s True Streaming Resource(State=%d)--m_Loads.size() == 0\r\n"), OpObj->GetName(), OpObj->GetStreamState());
						//ASSERT(false);
						return false;
					}
					//if (mStreamingObject == OpObj)
					{
						//auto nowTime = HighPrecision_GetTickCount();
						//if (v3dDevice::mIsEditorMode == FALSE && nowTime - GLastAsyncTime > timeOut)
						//{
						//	if (OpObj->GetStreamState() == SS_Valid)
						//		return true;
						//	VFX_LTRACE(ELTT_Error, vT("PreUse %s True Streaming Resource 2s-- mStreamingObject == OpObj\r\n"), OpObj->GetName());
						//	//ASSERT(false && "");
						//	return false;
						//}
					}
				}
			}
			else
			{
				//����Streaming��ʱ���ڲ�PreUse True�Լ���
				if (mStreamingObject == OpObj)
				{
					VFX_LTRACE(ELTT_Resource, vT("PreUse %s True when Streaming on loading thread\r\n"), OpObj->GetName());
					ASSERT(false);
					return false;
				}
			}

			VFX_LTRACE(ELTT_Resource, vT("PreUse %s True Streaming Resource--wait end\r\n"), OpObj->GetName());
			return true;
		}
		case SS_Pending://���ڵȴ�������
		{
			Remove(OpObj);
		}
		case SS_Killed://�õ����������Ѿ����ͷ���Դ��
		case SS_Invalid://��Դ�����ã��ող�����û�б��õ���ʱ��
		{
			if (IsCallByIOThread == false && vLoadAndFreeThread::GetInstance()->mThreadRunning)
			{
				auto savePriority = OpObj->mLoadPriority;
				OpObj->mLoadPriority = INT_MAX;
				this->Push(OpObj, fn);
				//int StreamingTimes = 0;

				const int PreUseCallType = 1;
				if (GOnLoadCall != nullptr)
					GOnLoadCall(PreUseCallType, 1, __FILE__, __LINE__);
				while (OpObj->GetStreamState() != SS_Valid)
				{
					if (OpObj->GetStreamState() == SS_Streaming)
					{
						Sleep(50);
					}
					else
					{
						Sleep(50);
					}
					if (mStreamingObject != OpObj && m_Loads.size() == 0)
					{
						if (OpObj->GetStreamState() == SS_Valid)
						{
							if (GOnLoadCall != nullptr)
								GOnLoadCall(PreUseCallType, 0, __FILE__, __LINE__);
							return true;
						}
						VFX_LTRACE(ELTT_Error, vT("PreUse %s True Streaming Resource(State=%d)--m_Loads.size() == 0\r\n"), OpObj->GetName(), OpObj->GetStreamState());
						//ASSERT(false && "");
						if (GOnLoadCall != nullptr)
							GOnLoadCall(PreUseCallType, 0, __FILE__, __LINE__);
						return false;
					}
					//if (mStreamingObject == OpObj)
					{
						auto nowTime = HighPrecision_GetTickCount();
						//if (v3dDevice::mIsEditorMode == FALSE && nowTime - GLastAsyncTime > timeOut)//200ms timeout
						//{
						//	if (OpObj->GetStreamState() == SS_Valid)
						//	{
						//		if (GOnLoadCall != nullptr)
						//			GOnLoadCall(PreUseCallType, 0, __FILE__, __LINE__);
						//		return true;
						//	}
						//	VFX_LTRACE(ELTT_Error, vT("PreUse %s True Streaming Resource 200ms-- mStreamingObject == OpObj\r\n"), OpObj->GetName());
						//	if (GOnLoadCall != nullptr)
						//		GOnLoadCall(PreUseCallType, 0, __FILE__, __LINE__);
						//	return false;
						//}
					}
				}
				OpObj->mLoadPriority = savePriority;
				//VFX_LTRACE(3, vT("PreUse %s True Streaming Resource--end\r\n"), OpObj->GetName());
			}
			else
			{
				if (mStreamingObject == OpObj)
				{
					VFX_LTRACE(ELTT_Resource, vT("PreUse %s True when Streaming on loading thread\r\n"), OpObj->GetName());
					ASSERT(false);
					return false;
				}
				else
				{
					OpObj->__SetStreamState(SS_Streaming);
					if (OpObj->Restore())
					{
						OpObj->__SetStreamState(SS_Valid);
						OpObj->mPreUseCounterBeforeStreaming = 0;
					}
					else
					{
						OpObj->__SetStreamState(SS_Invalid);
					}
				}
			}

			ASSERT(OpObj->GetStreamState() == SS_Valid);
		}
		break;
		case SS_Valid://����״̬
			break;
		case SS_PendingKill:
		case SS_Killing:
			///�ӵȴ�ɾ�������ϻ���
		{
			vFreePipe::GetInstance()->Remove(OpObj);
			if (OpObj->GetStreamState() == SS_Valid)
			{
				return true;
			}
			else if (OpObj->GetStreamState() == SS_Invalid || OpObj->GetStreamState() == SS_Killed)
			{
				return PreUse(OpObj, true, timeOut, fn);
			}
			ASSERT(false);
			return false;
		}
		break;
		}
		return true;
#pragma endregion
	}
	else
	{
		VAutoObjectLockerEx(OpObj, 1);
		StreamingState curState = OpObj->GetStreamState();
		switch (curState)
		{
		case SS_Killed://�õ����������Ѿ����ͷ���Դ��
		case SS_Invalid://��Դ�����ã��ող�����û�б��õ���ʱ��
		{
			Push(OpObj, fn);
		}
		break;
		case SS_Pending://���ڵȴ�������
		{
			if (m_Loads.size() == 0 && mWaitObject != OpObj)
			{
				//This is a bug,but we don't know why now;
				VFX_LTRACE(ELTT_Warning, "Preuse false:This is an amazing bug!!!StreamingState is Pending while LoadPipe is empty...\nResName: %s;\n",
					OpObj->GetName());
				Push(OpObj, fn);
				break;
			}
		}
		case SS_Valid://����״̬
			OpObj->mPreUseCounterBeforeStreaming++;
		case SS_Streaming://������
						  ///����״̬������Ҫ�������ȴ����ؽ���
						  //if (m_Loads.size() == 0)
						  //{
						  //	//This is a bug,but we don't know why now;
						  //	VFX_LTRACE(ELTT_Warning, "Preuse false:This is an amazing bug!!!StreamingState is SS_Streaming while LoadPipe is empty...\nResName: %s;\n",
						  //		OpObj->GetName());
						  //	Push(OpObj);
						  //}
			break;
		case SS_PendingKill:
			///�ӵȴ�ɾ�������ϻ���
			vFreePipe::GetInstance()->Remove(OpObj);
			break;
		case SS_Killing:
			///ʲôҲ��ɣ��ȴ�ɾ����ϣ�Ȼ�������õ�ʱ������Streaming
			break;
		}
		return true;
	}
}

IResource*	vLoadPipe::InvalidStreamingObject()
{
	auto pObj = mStreamingObject;
	if (pObj)
	{
		pObj->__SetStreamState(SS_Invalid);
		mStreamingObject = NULL;
	}
	return pObj;
}
void vLoadPipe::LoadResource(const WaitResource& wr)
{
	auto pRes = wr.ResObject;
	//��������
	vBOOL Ret = pRes->Restore();

#pragma region �޸ļ�����ϵ���Դ״̬�����Ҵ������ؽ����¼�
	{
		mAsyncCount++;
		if (GOnAsyncLoadObject)
		{
			GOnAsyncLoadObject(mAsyncCount, pRes->GetRtti()->ClassName.c_str(), pRes->GetName());
		}

		if (Ret)
		{//�ɹ�
			pRes->__SetStreamState(SS_Valid);

			if (wr.CallBack != nullptr)
			{
				wr.CallBack(pRes);
			}
		}
		else
		{//ʧ��
			pRes->__SetStreamState(SS_Invalid);
			if (GOnAsyncLoadObject)
				GOnAsyncLoadObject(-4, pRes->GetRtti()->ClassName.c_str(), pRes->GetName());
		}

		pRes->mPreUseCounterBeforeStreaming = 0;

		if (mWaitObject == pRes)
		{
			mWaitObject = NULL;
		}
	}
#pragma endregion
}

void vLoadPipe::TestDccc(const char* name)
{
	if (dccc != c_uMalloc0xCC)
	{
		VFX_LTRACE(ELTT_Error, "Native Memory Over Range: %s\r\n", name);
	}
	else
	{
		//VFX_LTRACE(ELTT_Error, "TestPass: %s\r\n", name);
	}
}
void vLoadPipe::OnLoad()
{
	auto pThread = vLoadAndFreeThread::GetInstance();

	IResource* pRes = NULL;
	WaitResource wr;
	{
		VAutoLock(pThread->mLocker);
#pragma region ȡ��Ҫ���ص���Դ
		if (m_Loads.size() == 0)
		{//û����Դ��Ҫ����
			mWaitObject = NULL;

			if (GOnAsyncLoadObject)
				GOnAsyncLoadObject(-2, "AsyncIOThread LoadPipe is empty", "");

			Sleep(50);
			return;
		}

		size_t number = m_Loads.size();

		//ȡ���ʼ�ĵȴ����ض���pRes
		wr = PopNoRelease();
		if (wr.ResObject == NULL)
		{
			return;
		}
		pRes = wr.ResObject;
		mStreamingObject = pRes;

		if (pRes->GetStreamState() != SS_Pending)
		{
			VFX_LTRACE(ELTT_Resource, vT("StreamState isn't Pending, it's %d\r\n"), (int)pRes->GetStreamState());
			return;
		}
		pRes->__SetStreamState(SS_Streaming);

		//���Գ���ɱ��û�������˵���Դ
		{
			int refCount = pRes->AddRef();
			refCount -= 1;
			pRes->Release();
			if (refCount == 1)
			{
				//VFX_LTRACE(3,vT("Resource(%d) ref count is zero\r\n"),pRes->GetName());
				if (GOnAsyncLoadObject)
					GOnAsyncLoadObject(-3, pRes->GetRtti()->ClassName.c_str(), pRes->GetName());
				pRes->__SetStreamState(SS_Invalid);
				pRes->Release();
				return;
			}
		}

		if (mOutputLoadInfo)
		{
			VStringA outInfo = VStringA_FormatV("vfxAsynLoader Number = %d\r\n", number);
			VFX_LTRACE(ELTT_Resource, outInfo.c_str());
		}
#pragma endregion
	}

	TestDccc("vLoadPipe.OnLoad 1");
	LoadResource(wr);
	TestDccc("vLoadPipe.OnLoad 2");

	{
		mStreamingObject = NULL;
	}

	pRes->Release();//�����������AddRef������Ҫ��ӦRelease
}

//////////////////////////////////////////////////////////////////////////

void vFreePipe::Push(IResource* OpObj, UINT Delay, INT64 time)
{
	VAutoLock(mLocker);
	auto iter = m_Frees.find(OpObj);
	if (iter != m_Frees.end())
	{
		ASSERT(iter->second.OpObj->GetStreamState() == SS_PendingKill);
		return;
	}
	if (Delay == 0)
	{//ȱʡҪ1000�������ͷ�
		Delay = 1000;
	}
	if (time == 0)
	{
		time = vfxGetEngineTick();
	}
	OpObj->AddRef();
	OpObj->__SetStreamState(SS_PendingKill);
	vFreePair fp;
	fp.OpObj = OpObj;
	fp.PushTime = time;
	fp.DelayTime = Delay;
	m_Frees.insert(std::make_pair(OpObj, fp));
}

void vFreePipe::Remove(IResource* OpObj)
{
	VAutoLock(mLocker);
	auto iter = m_Frees.find(OpObj);
	if (iter != m_Frees.end())
	{
		ASSERT(OpObj->GetStreamState() == SS_PendingKill);
		m_Frees.erase(iter);
		OpObj->__SetStreamState(SS_Valid);
		OpObj->Release();
	}
}

vBOOL vFreePipe::FreeObj(IResource* OpObj, INT64 time, UINT delay)
{
	VAutoObjectLocker(OpObj);

	//if (delay == 0)
	//{
	//	StreamingState curState = OpObj->GetStreamState();
	//	switch (curState)
	//	{
	//	case SS_Killed://�õ����������Ѿ����ͷ���Դ��
	//	case SS_Invalid://��Դ�����ã��ող�����û�б��õ���ʱ��
	//		break;
	//	case SS_Pending://���ڵȴ�������
	//	{///�Ӽ��ض���Out����Ϊ��û�м��أ����Կ���ֱ������ΪKilled
	//		vLoadPipe::GetInstance()->Remove(OpObj);
	//	}
	//	break;
	//	case SS_Streaming://������
	//					  ///û������,��Դ��ʱ���ͷţ����ؽ��������ϵͳ��Ҫ�ٱ���Դ�������ͷ�
	//		return FALSE;
	//	case SS_Valid://����״̬
	//	{
	//		VAutoLock(mLocker);//������Ҫ��ֹ�����߳�ͬʱ����ͬһ�������FreeObj
	//		OpObj->__SetStreamState(SS_Killing);
	//		OpObj->Invalidate();
	//		OpObj->__SetStreamState(SS_Killed);
	//	}
	//	break;
	//	case SS_PendingKill:
	//	case SS_Killing:
	//		///�Ѿ��������ͷŶ�����
	//		break;
	//	}
	//}
	//else
	{
		StreamingState curState = OpObj->GetStreamState();
		switch (curState)
		{
		case SS_Killed://�õ����������Ѿ����ͷ���Դ��
		case SS_Invalid://��Դ�����ã��ող�����û�б��õ���ʱ��
			break;
		case SS_Pending://���ڵȴ�������
		{///�Ӽ��ض���Out����Ϊ��û�м��أ����Կ���ֱ������ΪKilled
			vLoadPipe::GetInstance()->Remove(OpObj);
			OpObj->__SetStreamState(SS_Invalid);
		}
		break;
		case SS_Streaming://������
			return FALSE;///û������,��Դ��ʱ���ͷţ����ؽ��������ϵͳ��Ҫ�ٱ���Դ�������ͷ�
		case SS_Valid://����״̬
		{
			Push(OpObj, delay, time);
		}
		break;
		case SS_PendingKill:
		case SS_Killing:
			///�Ѿ��������ͷŶ�����
			break;
		}
	}
	return TRUE;
}

void vFreePipe::Cleanup()
{
	VAutoLock(mLocker);
	for (auto it = m_Frees.begin(); it != m_Frees.end(); it++)
	{
		IResource* pRes = it->second.OpObj;
		ASSERT(pRes->GetStreamState() == SS_PendingKill);
		pRes->__SetStreamState(SS_Killing);
		pRes->Invalidate();
		pRes->__SetStreamState(SS_Killed);
		pRes->Release();
	}
	m_Frees.clear();
}

void vFreePipe::OnFreeTick(bool bForce)
{
	//ASSERT_SYNCCHECK;

	{
		VAutoLock(mLocker);
		int nFreeCount = 0;

		std::list<vFreePair> delList;
		for (auto it = m_Frees.begin(); it != m_Frees.end(); it++)
		{
			if (bForce || vfxGetEngineTick() - it->second.PushTime >= it->second.DelayTime)
			{
				delList.push_back(it->second);
			}
		}

		for (auto it = delList.begin(); it != delList.end(); it++)
		{
			IResource* pRes = it->OpObj;
			StreamingState state = pRes->GetStreamState();
			if (state != SS_PendingKill)
			{
				ASSERT(false);
			}
			pRes->__SetStreamState(SS_Killing);
			pRes->Invalidate();
			pRes->__SetStreamState(SS_Killed);

			auto dit = m_Frees.find(pRes);
			if (dit != m_Frees.end())
			{
				m_Frees.erase(dit);
				pRes->Release();
			}
		}
	}
}

extern "C"  void vfxMemory_SmallAllocatorCheck(const char* name);
extern "C"
{
	 void vLoadPipe_DoCppAwait()
	{
		GAsyncFrame++;
		
		GLastAsyncTime = HighPrecision_GetTickCount();
	}
	 void vLoadPipe_SetNCM(FNativeCallManage fn)
	{
		GOnLoadCall = fn;
	}
	 VFX_API void vLoadPipe_OnLoad()
	{
		vLoadPipe::GetInstance()->OnLoad();
	}
	 void vLoadPipe_EmptyAllPool()
	{
		vLoadAndFreeThread::GetInstance()->EmptyAllPool();
	}
	 void vLoadPipe_SetPauseAsyncIO(vBOOL pause)
	{
		GLoadThreadPaused = pause;
	}
	 void vLoadPipe_SetThreadRunFlag(vBOOL bRun)
	{
		GLoadThreadId = vfxThread::GetCurrentThreadId();
		vLoadAndFreeThread::GetInstance()->mThreadRunning = bRun;
	}
	 void vLoadPipe_SetPreUseForceMode( vBOOL force )
	{
		bPreUseForce = force;
	}
	 LPCSTR vLoadPipe_InvalidStreamingObject()
	{
		auto pObj = vLoadPipe::GetInstance()->InvalidStreamingObject();
		if (pObj)
		{
			return pObj->GetName();
		}
		return "";
	}
	VFX_API int vLoadPipe_GetAsyncLoadNumber()
	{
		return (int)vLoadPipe::GetInstance()->GetAsyncLoadNumber();
	}

	 void vLoadPipe_SetOutputLoadInfo(vBOOL bOutput)
	{
		bOutput==0? vLoadPipe::GetInstance()->mOutputLoadInfo = false : vLoadPipe::GetInstance()->mOutputLoadInfo = true;
	}

	 void vLoadPipe_SetAsyncLoadObjectCallBack(FOnAsyncLoadObject cb)
	{
		GOnAsyncLoadObject = cb;
	}
	
	 void vFreePipe_OnFreeTick(vBOOL force)
	{
		vFreePipe::GetInstance()->OnFreeTick(force?true:false);
	}

	 void TestMemOverRange(const char* name)
	{
		/*auto obj = vfx::io::vLoadPipe::GetInstance();
		obj->TestDccc(name);*/
		vfxMemory_SmallAllocatorCheck(name);
	}
};

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic pop
#endif