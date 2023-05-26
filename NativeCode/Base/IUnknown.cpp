#include "IUnknown.h"
#include "BaseHead.h"
#include "thread/vfxcritical.h"
//#include "RHIRenderer/PreHead.h"
#include "CSharpAPI.h"

#define new VNEW

NS_BEGIN

void VIUnknown::DeleteThis()
{
	delete this;
}

UINT64 IWeakReference::EngineCurrentFrame = 0;

FOnManagedObjectHolderDestroy IManagedObjectHolder::OnManagedObjectHolderDestroy = nullptr;

vfxObjectLocker gDefaultObjectLocker;

IWeakReference::IWeakReference()
{
	//Handle = ObjectHandle::NewHandle(this);
	Handle = nullptr;
}

IWeakReference::~IWeakReference()
{
	if (Handle != nullptr)
	{
		Handle->mPtrAddress = nullptr;
		Safe_Release(Handle);
	}
}

void IWeakReference::Cleanup()
{

}

vfxObjectLocker* IWeakReference::GetLocker(int index) const
{
	return &gDefaultObjectLocker;
}

WeakRefHandle* IWeakReference::GetHandle()
{
	if (Handle == nullptr)
	{
		Handle = WeakRefHandle::NewHandle(this);
	}
	Handle->AddRef();
	return Handle;
}

Hash64 IWeakReference::GetHash64() 
{
	return Hash64::Empty;
}

int WeakRefHandle::AddRef()
{
	return ++RefCount;
}

void WeakRefHandle::Release()
{
	RefCount--;
	if (RefCount == 0)
		delete this;
}

WeakRefHandle* WeakRefHandle::NewHandle(IWeakReference* ptr)
{
	auto handle = new WeakRefHandle();
	handle->mPtrAddress = ptr;
	return handle;
}

VDefferedDeleteManager* VDefferedDeleteManager::GetInstance()
{
	static VDefferedDeleteManager obj;
	return &obj;
}

void VDefferedDeleteManager::Cleanup()
{
	Tick(-1);
	IsCleared = 1;
}

VCritical GDefferedDeleteLocker;
void VDefferedDeleteManager::PushObject(IWeakReference* obj)
{
	if (IsCleared == 1)
	{
		delete obj;
		return;
	}
	VAutoLock(GDefferedDeleteLocker);
	auto refCount = obj->AddRef();
	if (refCount != 1)
	{
		ASSERT(false);
		auto pRtti = obj->GetRtti();
		if (pRtti != nullptr)
		{
			VFX_LTRACE(ELTT_Memory, "Object %s will be deleted, Its RefCount = %d\r\n", obj->GetRtti()->Name.c_str(), refCount);
		}
		return;
	}
	
	ObjectPool.push(obj);
}

void VDefferedDeleteManager::Tick(int limitTimes)
{
	while (ObjectPool.size() > 0)
	{
		IWeakReference* cur;
		{
			VAutoLock(GDefferedDeleteLocker);
			cur = ObjectPool.front();
			ObjectPool.pop();
		}
		auto ref = cur->AddRef();
		ASSERT(ref == 2);
		delete cur;
	}
}

int func() { return 0; } // termination version

template<typename Arg1, typename... Args>
int func(const Arg1& arg1, const Args&... args)
{
	//process(arg1); 
	return func(args...); // note: arg1 does not appear here!
}

NS_END
