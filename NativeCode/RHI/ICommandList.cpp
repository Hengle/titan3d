#include "ICommandList.h"
#include "IRenderContext.h"
#include "IDrawCall.h"
#include "ISamplerState.h"
#include "IShaderResourceView.h"
#include "IFence.h"

#include "Utility/GraphicsProfiler.h"
#include "../Base/vfxsampcounter.h"

#define new VNEW

NS_BEGIN

StructImpl(FOnPassBuilt)

bool ICommandList::ContextState::IsCMPState = false;

bool ICommandList::ContextState::IsSameScissors(IScissorRect* lh, IScissorRect* rh)
{
	if (lh == rh)
		return true;

	if (rh != nullptr && lh != nullptr)
	{
		if (lh->Rects.size() != rh->Rects.size())
			return false;
		for (size_t i = 0; i < lh->Rects.size(); i++)
		{
			if (lh->Rects[i].MinX != rh->Rects[i].MinX ||
				lh->Rects[i].MinY != rh->Rects[i].MinY ||
				lh->Rects[i].MaxX != rh->Rects[i].MaxX ||
				lh->Rects[i].MaxY != rh->Rects[i].MaxY)
				return false;
		}
		return true;
	}
	return false;
}

ICommandList::ICommandList()
{
	OnPassBuilt = nullptr;
}

ICommandList::~ICommandList()
{
	for (auto i : mWaitSemaphores)
	{
		i->Release();
	}
	mWaitSemaphores.clear();

	for (auto i : mPassArray)
	{
		i->Release();
	}
	mPassArray.clear();
}

IRenderContext* ICommandList::GetContext()
{
	return mRHIContext.GetPtr();
}

void ICommandList::SetDebugName(const char* name) 
{
	mDebugName = StringHelper::strtowstr(name);
}

bool ICommandList::BeginCommand()
{
	this->mCurrentState.Reset();
	return true;
}

void ICommandList::EndCommand()
{
}

void ICommandList::RemoveWaitSemaphore(ISemaphore* semaphore)
{
	for (auto i = mWaitSemaphores.begin(); i != mWaitSemaphores.end(); i++)
	{
		if (*i == semaphore)
		{
			semaphore->Release();
			mWaitSemaphores.erase(i);
			return;
		}
	}
}

void ICommandList::AddWaitSemaphore(ISemaphore* semaphore)
{
	semaphore->AddRef();
	mWaitSemaphores.push_back(semaphore);
}

void ICommandList::ClearMeshDrawPassArray()
{
	auto ArraySize = mPassArray.size();
	for (UINT32 i = 0; i < ArraySize; i++)
	{
		mPassArray[i]->Release();
	}
	mPassArray.clear();
	mPassArray.reserve(ArraySize);
}

void ICommandList::SetGraphicsProfiler(GraphicsProfiler* profiler)
{
	mProfiler.StrongRef(profiler);
}

void ICommandList::BuildRenderPass(vBOOL bImmCBuffer)
{
	for (auto i = mPassArray.begin(); i != mPassArray.end(); i++)
	{
		(*i)->BuildPass(this, bImmCBuffer);

		if(mPipelineStat!=nullptr)
		{
			if (mPipelineStat->IsOverLimit())
				return;

			mPipelineStat->mLastestDrawCall = *i;
		}
	}
}

void ICommandList::EndRenderPass()
{
	ClearMeshDrawPassArray();
}

void ICommandList::PushDrawCall(IDrawCall* pPass)
{
	if (pPass == nullptr)
		return;
	pPass->AddRef();
	mPassArray.push_back(pPass);
}

NS_END
