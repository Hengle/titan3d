#pragma once
#include "../BaseHead.h"
#include "TypeUtility.h"

NS_BEGIN

TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct=8)
class CoreSDK
{
public:
	static void* MemoryCopy(void* tar, void* src, UINT size)
	{
		return memcpy(tar, src, size);
	}
	static void StrCpy(void* tar, const void* src)
	{
		if (src == nullptr)
		{
			((char*)tar)[0] = 0;
			return;
		}
		strcpy((char*)tar, (const char*)src);
	}
	static UINT StrLen(const void* s)
	{
		if (s == nullptr)
			return 0;
		return (UINT)strlen((const char*)s);
	}
	static int StrCmp(const void* s1, const void* s2)
	{
		if (s1 == 0 && s2 == 0)
			return 0;
		else if (s1 != 0 && s2 == 0)
			return 1;
		else if (s1 == 0 && s2 != 0)
			return -11;
		else
			return strcmp((const char*)s1, (const char*)s2);
	}
};

TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
class BigStackBuffer
{
	BYTE*		mBuffer;
	int			mSize;
public:
	TR_CONSTRUCTOR()
	BigStackBuffer(int size)
	{
		mBuffer = new BYTE[size];
		memset(mBuffer, 0, size);
		mSize = size;
	}
	TR_CONSTRUCTOR()
	BigStackBuffer(int size, const char* text)
	{
		if ((int)strlen(text) > size)
			size = (int)strlen(text) * 2;
		mBuffer = new BYTE[size];
		memset(mBuffer, 0, size);
		mSize = size;

		strcpy((char*)mBuffer, text);
	}
	~BigStackBuffer()
	{
		delete[] mBuffer;
		mBuffer = nullptr;
	}
	TR_FUNCTION()
	void* GetBuffer() {
		return mBuffer;
	}
	TR_FUNCTION()
	int GetSize() const{
		return mSize;
	}
	TR_FUNCTION()
	void DestroyMe()
	{
		delete this;
	}
};

NS_END
