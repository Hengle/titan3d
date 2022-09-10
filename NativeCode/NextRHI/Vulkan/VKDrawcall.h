#pragma once
#include "../NxDrawcall.h"
#include "VKShader.h"
#include "VKPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class VKGraphicDraw : public IGraphicDraw
	{
	public:
		~VKGraphicDraw();
	protected:
		virtual void OnGpuDrawStateUpdated() override;
		virtual void OnBindResource(const FEffectBinder* binder, IGpuResource* resource) override;

		virtual void Commit(ICommandList* cmdlist) override;

		void RebuildDescriptorSets();
		static void BindResourceToDescriptSets(VKGpuDevice* device, AutoRef<MemAlloc::FPagedObject<VkDescriptorSet>>& dsSetVS,
			AutoRef<MemAlloc::FPagedObject<VkDescriptorSet>>& dsSetPS,
			const FEffectBinder* binder, IGpuResource* resource);
	public:
		TObjectHandle<VKGpuDevice>				mDeviceRef;
		AutoRef<MemAlloc::FPagedObject<VkDescriptorSet>>		mDescriptorSetVS;
		AutoRef<MemAlloc::FPagedObject<VkDescriptorSet>>		mDescriptorSetPS;
		bool									IsDirty = false;
		UINT									FingerPrient = 0;
	};

	class VKComputeDraw : public IComputeDraw
	{
	public:
		virtual void OnBindResource(const FShaderBinder* binder, IGpuResource* resource) override;
		virtual void Commit(ICommandList* cmdlist) override;

		void RebuildDescriptorSets();
	public:
		TObjectHandle<VKGpuDevice>				mDeviceRef;
		AutoRef<MemAlloc::FPagedObject<VkDescriptorSet>>		mDescriptorSetCS;
		bool									IsDirty = false;
		UINT									FingerPrient = 0;
	};
}

NS_END
