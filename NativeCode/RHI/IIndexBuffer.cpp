#include "IIndexBuffer.h"
#include "IRenderContext.h"

#define new VNEW

NS_BEGIN

IIndexBuffer::IIndexBuffer()
{
	//mHasPushed = false;
	//GpuBufferSize = 0;
}

IIndexBuffer::~IIndexBuffer()
{
}

void IIndexBuffer::DoSwap(IRenderContext* rc)
{
	//if (mIndexes.size() == 0)
	//{
	//	mHasPushed = false;
	//	return;
	//}

	////AUTO_SAMP("Native.IVertexBuffer.UpdateGPUBuffData");
	//GpuBufferSize = (UINT)mIndexes.size();

	//UpdateGPUBuffData(rc->GetImmCommandList(), (void*)&mIndexes[0], GpuBufferSize);
	//mIndexes.clear();

	//mHasPushed = false;
}

NS_END
