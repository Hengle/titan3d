#include "MemStream.h"
#include "../CoreSDK.h"

#define new VNEW

NS_BEGIN

MemStreamWriter::MemStreamWriter()
{
	mDataStream = nullptr;
	mBufferSize = 0;
	mPosition = 0;
}

MemStreamWriter::~MemStreamWriter()
{
	Safe_DeleteArray(mDataStream);
	mBufferSize = 0;
	mPosition = 0;
}

MemStreamWriter::MemStreamWriter(UINT size)
{
	mDataStream = new BYTE[size];
	mBufferSize = size;
	mPosition = 0;
}

void MemStreamWriter::ResetBufferSize(UINT64 size)
{
	Safe_DeleteArray(mDataStream);
	if (size > 0)
	{
		mBufferSize = size;
		mDataStream = new BYTE[size];
	}
	mPosition = 0;
}

bool MemStreamWriter::Seek(UINT64 offset)
{
	if (mBufferSize < offset)
	{
		return false;
	}
	mPosition = offset;
	return true;
}

//void MemStreamWriter::WriteAsZSTD(const void* pSrc, UINT t)
//{
//	auto len = CoreSDK::CompressBound_ZSTD(t);
//	auto pDst = new BYTE[len + 10];
//	auto wSize = (UINT)CoreSDK::Compress_ZSTD(pDst, len, pSrc, t, 1);
//	Write(wSize);
//	Write(pDst, wSize);
//	delete[] pDst;
//}

void MemStreamWriter::Write(const void* pSrc, UINT t)
{
	if (mBufferSize < mPosition + t)
	{
		auto sz = (mBufferSize + t) * 2;
		BYTE* nBuffer = new BYTE[sz];
		if (mPosition > 0)
		{
			memcpy(nBuffer, mDataStream, mPosition);
		}
		Safe_DeleteArray(mDataStream);
		mDataStream = nBuffer;
		mBufferSize = sz;
	}
	memcpy(&mDataStream[mPosition], pSrc, (size_t)t);
	mPosition += t;
}

UINT MemStreamReader::Read(void* pSrc, UINT t)
{
	if (mLength < mPosition + t)
	{
		return 0;
	}
	memcpy(pSrc, &mProxyPointer[mPosition], (size_t)t);
	mPosition += t;
	return t;
}

//UINT MemStreamReader::ReadAsZSTD(const void* pSrc, UINT t)
//{
//	UINT wSize = 0;
//	Read(wSize);
//	auto pDst = new BYTE[wSize];
//	Read(pDst, wSize);
//	CoreSDK::Decompress_ZSTD(pSrc, t, pDst, wSize);
//	delete[] pDst;
//}

NS_END