#include "NxCommandList.h"
#include "NxDrawcall.h"
#include "NxFrameBuffers.h"
#include "../../Base/vfxsampcounter.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	ICmdRecorder::~ICmdRecorder()
	{
		ResetGpuDraws();
	}
	IGpuResource* ICmdRecorder::FindGpuResourceByTagName(const char* name)
	{
		for (auto i : mDrawcallArray)
		{
			IGpuResource* findRes = nullptr;
			i->ForeachGpuResource([&findRes, name](EShaderBindType type, IGpuResource* resource)
				{
					if (resource->TagName == name)
					{
						findRes = resource;
						return false;
					}
					return true;
				});

			if (findRes != nullptr)
				return findRes;
		}
		return nullptr;
	}
	int ICmdRecorder::CountGpuResourceByTagName(const char* name)
	{
		int count = 0;
		for (auto i : mRefBuffers)
		{
			if (i->TagName == name)
			{
				auto p = (IUnknown*)i->GetHWBuffer();
				auto ref = p->AddRef();
				count++;
			}
		}
		/*for (auto i : mDrawcallArray)
		{
			IGpuResource* findRes = nullptr;
			i->ForeachGpuResource([&count, name](EShaderBindType type, IGpuResource* resource)
				{
					if (resource->TagName == name)
					{
						count++;
					}
					return true;
				});
		}*/
		return count;
	}
	void ICmdRecorder::PushGpuDraw(IGpuDraw* draw)
	{
		mDrawcallArray.push_back(draw);
		mPrimitiveNum += draw->GetPrimitiveNum();
	}
	void ICmdRecorder::ResetGpuDraws()
	{
		mDrawcallArray.clear();
		mRefBuffers.clear();
	}
	void ICmdRecorder::FlushDraws(ICommandList* cmdlist, bool bRefBuffer)
	{
		mCmdList.FromObject(cmdlist);
		for (auto i : mDrawcallArray)
		{
			i->Commit(cmdlist, false);
			if (bRefBuffer == false)
				continue;
			i->ForeachGpuResource([this](EShaderBindType type, IGpuResource* resource)
				{
					switch (type)
					{
						case EShaderBindType::SBT_SRV:
							if (resource->TagName == "InstantSRV")
							{
								int xxx = 0;
							}
							mRefBuffers.push_back(((ISrView*)resource)->Buffer);
							break;
						case EShaderBindType::SBT_UAV:
							mRefBuffers.push_back(((IUaView*)resource)->Buffer);
							break;
						case EShaderBindType::SBT_CBuffer:
							mRefBuffers.push_back(((ICbView*)resource)->Buffer);
							break;
						default:
							break;
					}
					return true;
				});
		}
	}
	ICmdRecorder* ICommandList::BeginCommand()
	{
		if (mCmdRecorder == nullptr)
		{
			mCmdRecorder = MakeWeakRef(new ICmdRecorder());
		}
		mCmdRecorder->ResetGpuDraws();
		mPrimitiveNum = 0;
		return mCmdRecorder;
	}
	void ICommandList::EndCommand()
	{
		if (mCmdRecorder == nullptr)
		{
			ASSERT(false);
			return;
		}
	}
	void ICommandList::PushGpuDraw(IGpuDraw* draw)
	{
		if (mCmdRecorder == nullptr)
		{
			ASSERT(false);
			return;
		}
		mCmdRecorder->PushGpuDraw(draw);
	}
	void ICommandList::InheritPass(ICommandList* cmdlist)
	{
		mCurrentFrameBuffers = cmdlist->mCurrentFrameBuffers;
	}
}

NS_END