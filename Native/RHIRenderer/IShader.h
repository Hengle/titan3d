#pragma once
#include "IRenderResource.h"

NS_BEGIN

class XNDNode;
class IRenderContext;
class ICommandList;
struct IConstantBufferDesc;
class IConstantBuffer;

struct TextureBindInfo;
struct SamplerBindInfo;
class ShaderReflector;

enum EShaderType
{
	EST_UnknownShader,
	EST_VertexShader,
	EST_PixelShader,
	EST_ComputeShader,
};

inline EShaderType GetShaderTypeFrom(std::string sm)
{
	if (sm == "vs_4_0" || sm == "vs_5_0")
		return EST_VertexShader;
	else if (sm == "ps_4_0" || sm == "ps_5_0")
		return EST_PixelShader;
	else if (sm == "cs_4_0" || sm == "cs_5_0")
		return EST_ComputeShader;
	ASSERT(false);
	return EST_VertexShader;
}

class IShaderDesc : public VIUnknown
{
private:
	std::vector<BYTE>		Codes;
	Hash64					HashCode;
public:
	std::string				Es300Code;
	std::string				MetalCode;
public:
	EShaderType				ShaderType;
	ShaderReflector*		Reflector;
public:
	RTTI_DEF(IShaderDesc, 0x965458df5b039800, true);
	IShaderDesc()
	{
		ShaderType = EST_VertexShader;
		Reflector = nullptr;
	}
	IShaderDesc(EShaderType type)
	{
		ShaderType = type;
		Reflector = nullptr;
	}
	~IShaderDesc();
	void SetGLCode(const char* code){
		Es300Code = code;
	}
	void SetMetalCode(const char* code) {
		MetalCode = code;
	}
	const char* GetGLCode() const {
		return Es300Code.c_str();
	}
	const char* GetMetalCode() const {
		return MetalCode.c_str();
	}
	void SetShaderType(EShaderType type)
	{
		ShaderType = type;
	}
	EShaderType GetShaderType()
	{
		return ShaderType;
	}
	void SetCodes(BYTE* ptr, int count)
	{
		Codes.resize(count);
		memcpy(&Codes[0], ptr, count);
		HashCode = HashHelper::CalcHash64(ptr, count);
	}
	virtual Hash64 GetHash64() override
	{
		return HashCode;
	}
	inline const std::vector<BYTE>& GetCodes() const {
		return Codes;
	}
	
	void Save2Xnd(XNDNode* node, DWORD platforms);
	vBOOL LoadXnd(XNDNode* node);

	UINT GetCBufferNum();
	UINT GetSRVNum();
	UINT GetSamplerNum();
	vBOOL GetCBufferDesc(UINT index, IConstantBufferDesc* info);
	vBOOL GetSRVDesc(UINT index, TextureBindInfo* info);
	vBOOL GetSamplerDesc(UINT index, SamplerBindInfo* info);
	UINT FindCBufferDesc(const char* name);
	UINT FindSRVDesc(const char* name);
	UINT FindSamplerDesc(const char* name);
};

class IShader : public IRenderResource
{
protected:
	AutoRef<IShaderDesc>		mDesc;
public:
	IShader();
	~IShader();

	IShaderDesc* GetDesc() {
		return mDesc;
	}
	
	//��shader���constantbuffer
	UINT32 GetConstantBufferNumber() const;
	bool GetConstantBufferDesc(UINT32 Index, IConstantBufferDesc* desc);
	const IConstantBufferDesc* FindCBufferDesc(const char* name);
	
	UINT GetShaderResourceNumber() const;
	bool GetShaderResourceBindInfo(UINT Index, TextureBindInfo* info) const;
	const TextureBindInfo* FindTextureBindInfo(const char* name);

	UINT GetSamplerNumber() const;
	bool GetSamplerBindInfo(UINT Index, SamplerBindInfo* info) const;
	const SamplerBindInfo* FindSamplerBindInfo(const char* name);
};

class IShaderDefinitions : VIUnknown
{
public:
	RTTI_DEF(IShaderDefinitions, 0x807c2f725b0bf4db, true);
	std::vector<MacroDefine>	Definitions;
	void AddDefine(const char* name, const char* value);
	const MacroDefine* FindDefine(const char* name) const;
	void ClearDefines();
	void RemoveDefine(const char* name);
	void MergeDefinitions(IShaderDefinitions* def);
};

NS_END