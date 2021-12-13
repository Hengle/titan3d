#include "IGLShaderProgram.h"
#include "IGLVertexShader.h"
#include "IGLPixelShader.h"
#include "IGLInputLayout.h"
#include "IGLCommandList.h"
#include "IGLRenderContext.h"
#include "IGLShaderReflector.h"
#include "../Utility/GraphicsProfiler.h"

#define new VNEW

NS_BEGIN

IGLShaderProgram::IGLShaderProgram()
{
	mNoPSProfilering = FALSE;
}

IGLShaderProgram::~IGLShaderProgram()
{
	Cleanup();
}

void IGLShaderProgram::Cleanup()
{
	mProgram.reset();
}

vBOOL IGLShaderProgram::LinkShaders(IRenderContext* rc)
{
	auto sdk = GLSdk::ImmSDK;
	ASSERT(mProgram);
	ASSERT(mProgram->IsValidBuffer());
	ASSERT(sdk->IsGLThread());
	ASSERT(mInputLayout != nullptr);

	auto glvs = mVertexShader.UnsafeConvertTo<IGLVertexShader>();
	sdk->AttachShader(mProgram, glvs->mShader);

	auto glps = mPixelShader.UnsafeConvertTo<IGLPixelShader>();
	sdk->AttachShader(mProgram, glps->mShader);

	sdk->LinkProgram(mProgram, this);

	GLint Result = GL_FALSE;
	int InfoLogLength;
	sdk->GetProgramiv(mProgram, GL_LINK_STATUS, &Result);
	sdk->GetProgramiv(mProgram, GL_INFO_LOG_LENGTH, &InfoLogLength);

	if (InfoLogLength > 0) 
	{
		std::vector<char> ProgramErrorMessage;
		ProgramErrorMessage.resize(InfoLogLength + 1);
		ProgramErrorMessage[InfoLogLength] = (char)0;
		sdk->GetProgramInfoLog(mProgram, InfoLogLength, &InfoLogLength, &ProgramErrorMessage[0]);
		VFX_LTRACE(ELTT_Graphics, "glGetProgramInfoLog = %s\r\n", &ProgramErrorMessage[0]);
	}

	sdk->DetachShader(mProgram, (mVertexShader.UnsafeConvertTo<IGLVertexShader>())->mShader);
	sdk->DetachShader(mProgram, (mPixelShader.UnsafeConvertTo<IGLPixelShader>())->mShader);

	if (Result == GL_FALSE)
	{
		VFX_LTRACE(ELTT_Graphics, "GL_LINK_STATUS = GL_FALSE\r\n");
		return FALSE;
	}

	IGLShaderReflector::ReflectProgram((IGLRenderContext*)rc, mProgram, EST_UnknownShader, mReflector);

	for (size_t i = 0; i < mInputLayout->mDesc->Layouts.size(); i++)
	{
		GLint loc = -1;

		auto& elem = mInputLayout->mDesc->Layouts[i];
		std::string attribName = std::string("in_var_") + elem.SemanticName.c_str();
		if (elem.SemanticIndex > 0)
			attribName += std::to_string(elem.SemanticIndex);

		sdk->GetAttribLocation(&loc, mProgram, attribName.c_str());
		if (loc == -1)
		{
			if (elem.SemanticIndex == 0)
			{
				attribName = std::string("in_var_") + elem.SemanticName.c_str() + "0";
				sdk->GetAttribLocation(&loc, mProgram, attribName.c_str());
			}
		}
		elem.GLAttribLocation = loc;
	}
	return TRUE;
}

/*
glBindBufferBase(GL_UNIFORM_BUFFER, 0, rain_buffer);
glGetUniformLocation();
glGetUniformBlockIndex();
glGetUniformIndices();
glGetActiveUniformsiv();
glGetActiveUniformsiv()
*/

void IGLShaderProgram::ApplyShaders(ICommandList* cmd)
{
	auto sdk = ((IGLCommandList*)cmd)->mCmdList;
	if (cmd->mProfiler != nullptr && cmd->mProfiler->mNoPixelShader)
	{
		if (mNoPSProfilering == FALSE)
		{
			sdk->UseProgram(mProgram, this);
			IPixelShader* ps = cmd->mProfiler->mPSEmpty;
			auto glps = (IGLVertexShader*)ps;
			sdk->AttachShader(mProgram, glps->mShader);
			mNoPSProfilering = TRUE;
		}
	}
	else
	{
		if (mNoPSProfilering == TRUE)
		{
			sdk->UseProgram(mProgram, this);
			auto glps = mPixelShader.UnsafeConvertTo<IGLVertexShader>();
			sdk->AttachShader(mProgram, glps->mShader);
			mNoPSProfilering = FALSE;
		}
	}
	if (cmd->mCurrentState.TrySet_ShaderProgram(this)==false)
		return;
	sdk->UseProgram(mProgram, this);
}

bool IGLShaderProgram::Init(IGLRenderContext* rc, const IShaderProgramDesc* desc)
{
	auto sdk = GLSdk::ImmSDK;
	
	mProgram = sdk->CreateProgram();

	BindInputLayout(desc->InputLayout);
	BindVertexShader(desc->VertexShader);
	BindPixelShader(desc->PixelShader);
	
	return true;
}

NS_END