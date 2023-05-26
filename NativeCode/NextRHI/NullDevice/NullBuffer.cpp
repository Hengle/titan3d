#include "NullBuffer.h"
#include "NullCommandList.h"
#include "NullGpuDevice.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	NullBuffer::NullBuffer()
	{
	}

	NullBuffer::~NullBuffer()
	{
	}

	bool NullBuffer::Init(NullGpuDevice* device, const FBufferDesc* desc)
	{
		Desc = *desc;
	
		return true;
	}

	void NullBuffer::UpdateGpuData(UINT subRes, void* pData, const FSubResourceFootPrint* footPrint)
	{
		
	}

	bool NullBuffer::Map(UINT index, FMappedSubResource* res, bool forRead)
	{
		return false;
	}

	void NullBuffer::Unmap(UINT index)
	{
		
	}
	
	NullTexture::NullTexture()
	{
		
	}

	NullTexture::~NullTexture()
	{
		
	}

	bool NullTexture::Init(NullGpuDevice* device, const FTextureDesc* desc)
	{
		Desc = *desc;
		
		return true;
	}
	bool NullTexture::Map(UINT subRes, FMappedSubResource* res, bool forRead)
	{
		return false;
	}
	void NullTexture::Unmap(UINT subRes)
	{
		
	}
	void NullTexture::UpdateGpuData(UINT subRes, void* pData, const FSubResourceFootPrint* footPrint)
	{
		
	}
	IGpuBufferData* NullTexture::CreateBufferData(IGpuDevice* device, UINT mipIndex, ECpuAccess cpuAccess, FSubResourceFootPrint* outFootPrint)
	{
		return nullptr;
	}

	bool NullCbView::Init(NullGpuDevice* device, IBuffer* pBuffer, const FCbvDesc* desc)
	{
		Buffer = pBuffer;
		ShaderBinder = desc->ShaderBinder;
		return true;
	}

	bool NullVbView::Init(NullGpuDevice* device, IBuffer* pBuffer, const FVbvDesc* desc)
	{
		Desc = *desc;
		Buffer = pBuffer;
		return true;
	}

	bool NullIbView::Init(NullGpuDevice* device, IBuffer* pBuffer, const FIbvDesc* desc)
	{
		Desc = *desc;
		Buffer = pBuffer;
		return true;
	}

	NullSrView::NullSrView()
	{
		
	}

	NullSrView::~NullSrView()
	{
		
	}

	bool NullSrView::Init(NullGpuDevice* device, IGpuBufferData* pBuffer, const FSrvDesc* desc)
	{
		Desc = *desc;
		
		return true;
	}
	bool NullSrView::UpdateBuffer(IGpuDevice* device, IGpuBufferData* pBuffer)
	{
		return true;
	}

	NullUaView::NullUaView()
	{
		
	}

	NullUaView::~NullUaView()
	{
		
	}

	bool NullUaView::Init(NullGpuDevice* device, IGpuResource* pBuffer, const FUavDesc* desc)
	{
		Desc = *desc;
		return true;
	}
	
	NullRenderTargetView::NullRenderTargetView()
	{
		
	}

	NullRenderTargetView::~NullRenderTargetView()
	{
		
	}

	bool NullRenderTargetView::Init(NullGpuDevice* device, IGpuResource* pBuffer, const FRtvDesc* desc)
	{
		Desc = *desc;
		
		return true;
	}

	NullDepthStencilView::NullDepthStencilView()
	{
		
	}

	NullDepthStencilView::~NullDepthStencilView()
	{
		
	}

	bool NullDepthStencilView::Init(NullGpuDevice* device, IGpuResource* pBuffer, const FDsvDesc* desc)
	{
		Desc = *desc;
		
		return true;
	}
}
NS_END