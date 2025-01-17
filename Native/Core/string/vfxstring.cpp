// vfxstring.cpp
// 
// VictoryCore Code
// class VString
//
// Author : johnson
// More author :
// Create time : 2002-6-13
// Modify time : 2006-8-15
//-----------------------------------------------------------------------------
#include "../precompile.h"
#include "vfxstring.h"
#include "../../Bricks/TextConverter/WordCodeHelper.h"
#include <algorithm>

#define new VNEW

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wwritable-strings"
#endif

static char cbuf[BUFSIZ];
static wchar_t wbuf[BUFSIZ];

char* wchar_to_char(const wchar_t* text, int len)
{
	if(!text)
		return NULL;

	//	int wlen = (len == 0) ? wcslen(text) : len;

	int index = 0;

	for (int i = 0; ; i++)
	{
		unsigned short ch = text[i];

		if (0x800 <= ch)
		{
			cbuf[index++] = ((char)(ch >> 12) & 0x0f ) | 0xe0;
			cbuf[index++] = ((char)(ch >> 6) & 0x3f ) | 0x80;
			cbuf[index++] = (char)(ch & 0x3f) | 0x80;
		}
		else if (0x80 <= ch)
		{
			cbuf[index++] = ((char)(ch >> 6) & 0x1f ) | 0xC0;
			cbuf[index++] = (char)(ch & 0x3f) | 0x80;
		}
		else
		{
			if( ch == 0 ){
				break;
			}

			cbuf[index++] = (char)ch;
		}
	}

	cbuf[index] = '\0';
	//#endif

	// memory copy
	size_t size = strlen(cbuf) + 1;
	char* ctext = new char[size];
	memcpy(ctext, cbuf, size);

	return ctext;
}

wchar_t* char_to_wchar(const char* text, int len)
{
	if(!text)
		return NULL;

	size_t clen = (len == 0) ? strlen(text) : len;

	const unsigned char* temp = (const unsigned char*)text;
	int index = 0;

	for (size_t i = 0; i < clen; )
	{
		if ( (0xf0 & temp[i]) == 0xe0 )
		{
			wbuf[index++] = ((wchar_t)(temp[i] & 0x0f) << 12 | (wchar_t)(temp[i + 1] & 0x3f) << 6 | (wchar_t)(temp[i + 2] & 0x3f));
			i += 3;
		}
		else if ( (0xe0 & temp[i]) == 0xc0 )
		{
			wbuf[index++] = ((wchar_t)(temp[i] & 0x1f) << 6 | ((wchar_t)temp[i + 1] & 0x3f));
			i += 2;
		}
		else if ( (0xc0 & temp[i]) == 0x80 ){
			i += 1;
			continue;
		}
		else
		{
			wbuf[index++] = (wchar_t)temp[i];
			i += 1;
		}
	}

	wbuf[index] = '\0';
	//#endif

	// memory copy
	size_t size = wcslen(wbuf) + 1;
	wchar_t* wtext = new wchar_t[size + 1];
	for (size_t i = 0; i < size; i++)
	{
		wtext[i] = wbuf[i];
	}
	//px_memcpy(wtext, wbuf, size);

	return wtext;
}

#define MAX_BUFFLEN 4096


void VStringA_MakeLower(INOUT VStringA &str)
{
	std::transform(str.begin(), str.end(), str.begin(), tolower);
}

void VStringA_MakeUpper(INOUT VStringA &str)
{
	std::transform(str.begin(), str.end(), str.begin(), toupper);
}
void VStringA_ReplaceAll(INOUT VStringA& str, const char* oldStr, const char* newStr)
{
	size_t olen = strlen(oldStr);
	VStringA tempStr = str;
	while (true)
	{
		VStringA::size_type   pos(0);
		if ((pos = tempStr.find(oldStr)) != VStringA::npos)
			tempStr = tempStr.replace(pos, olen, newStr).c_str();
		else
			break;
	}
	str = tempStr;
}
int VStringA_CompareNoCase(INOUT const VStringA& str, LPCSTR rh)
{
	return VStringA_CompareNoCase(str, rh, (int)strlen(rh));
}
int VStringA_CompareNoCase(const VStringA& str, LPCSTR rh, int num)
{
	size_t len = strlen(rh);
	if (num >= 0 && (size_t)num < len)
		len = (size_t)num;
	VBaseStringA::const_iterator p = str.begin();
	size_t p2 = 0;
	while (p != str.end() && p2 != len)
	{
		if (toupper(*p) != toupper(rh[p2]))
			return (toupper(*p) < toupper(rh[p2])) ? -1 : 1;
		++p;
		++p2;
	}
	if (num == -1)
		return 0;
	return (len == str.size()) ? 0 : (str.size() < len) ? -1 : 1;
}
bool VStringA_Contains(const VStringA& str, LPCSTR rh)
{
	return str.find(rh) != VStringA::npos;
}
VStringA VStringA_FormatV(LPCSTR _format, ...)
{
	char szBuffer[MAX_BUFFLEN];
	memset(szBuffer, 0, sizeof(szBuffer));
	va_list ap;
	va_start(ap, _format);
	try
	{
#ifdef WIN32
		_vsnprintf_s(szBuffer, MAX_BUFFLEN, MAX_BUFFLEN, _format, ap);
		//sprintf(szBuffer,_format,ap);
#else
		vsnprintf(szBuffer, MAX_BUFFLEN, _format, ap);
#endif

	}
	catch (...)
	{
		return VStringA("");
	}
	va_end(ap);
	return VStringA(szBuffer);
}

VStringW VStringA_Ansi2Unicode(LPCSTR gbk)
{
	size_t srcLen = strlen(gbk);
	size_t dstLen = srcLen * 5 + 1;
	wchar_t* dst = new wchar_t[dstLen];
	memset(dst, 0, sizeof(WCHAR)*dstLen);
	dstLen = dstLen * sizeof(WCHAR);
#if defined(PLATFORM_WIN)
	WordCodeHelper::ChangeCodeStatic("GB18030//TRANSLIT", "UTF-16LE", gbk, &srcLen, (char*)dst, &dstLen);
#elif defined(ANDROID) || defined(IOS)
	WordCodeHelper::ChangeCodeStatic("UTF-8", "UTF-32LE", gbk, &srcLen, (char*)dst, &dstLen);
#endif
	//dst[srcLen - dstLen/sizeof(WCHAR)] = NULL;
    VStringW result = dst;
	delete[] dst;
	return result;
}
VStringA VStringA_Unicode2Ansi(LPCWSTR unicode)
{
	size_t srcLen = wcslen(unicode);
	size_t dstLen = srcLen * 4 + 1;
	char* dst = new char[dstLen];
	memset(dst, 0, sizeof(char)*dstLen);
	srcLen = srcLen * sizeof(WCHAR);
#if defined(PLATFORM_WIN)
	WordCodeHelper::ChangeCodeStatic("UTF-16LE", "GB18030//TRANSLIT", (char*)unicode, &srcLen, dst, &dstLen);
#elif defined(ANDROID) || defined(IOS)
	WordCodeHelper::ChangeCodeStatic("UTF-32LE", "UTF-8", (char*)unicode, &srcLen, dst, &dstLen);
#endif
	//dst[srcLen - dstLen/sizeof(WCHAR)] = NULL;
	VStringA result = dst;
	delete[] dst;
	return result;
}

VStringA VStringA_Gbk2Utf8(LPCSTR gbk)
{
    if(gbk == NULL)
        return VStringA("");
	size_t srcLen = strlen(gbk);
	size_t dstLen = srcLen * 5 + 1;
	char* dst = new char[dstLen];
	memset(dst, 0, sizeof(char)*dstLen);
	WordCodeHelper::ChangeCodeStatic("GB18030//TRANSLIT", "UTF-8", gbk, &srcLen, (char*)dst, &dstLen);
	VStringA result = dst;
	delete[] dst;
	return result;
}

VStringA VStringA_Utf82Gbk(LPCSTR utf8)
{
    if(utf8 == NULL)
        return VStringA("");
	size_t srcLen = strlen(utf8);
	size_t dstLen = (srcLen * 5) + 1;
	char* dst = new char[dstLen];
	memset(dst, 0, sizeof(char)*dstLen);
	WordCodeHelper::ChangeCodeStatic("UTF-8", "GB18030//TRANSLIT", utf8, &srcLen, (char*)dst, &dstLen);
	VStringA result = dst;
	delete[] dst;
	return result;
}
//////////////////////////////////////////////////////////////////////////
//const VStringA::size_type VStringA::npos = VBaseStringA::npos;
//VStringA VStringA::EmtpyString = "";
//int VStringA::VStringANumber = 0;
//void VStringA::MakeLower()
//{
//	/*if(this->length()>0)
//		_mbslwr_s((unsigned char*)this->c_str(),this->length());*/
//	std::transform(mBaseString.begin(), mBaseString.end(), mBaseString.begin(),tolower);
//}
//
//void VStringA::MakeUpper()
//{
//	/*if(this->length()>0)
//		_mbsupr_s((unsigned char*)this->c_str(),this->length()-1);*/
//	std::transform(mBaseString.begin(), mBaseString.end(), mBaseString.begin(),toupper);
//}
//
////LPSTR VStringA::GetBufferSetLength(size_t size)
////{
////	mBaseString.resize(size);
////	return GetBuffer(0);
////}
//
//void VStringA::Delete(size_t start,size_t len)
//{
//	mBaseString.erase(start,len);
//}
//
//VStringA VStringA::Left(size_t len)
//{
//	return VStringA((LPCSTR)GetBuffer(0),len);
//}
//
//VStringA VStringA::Right(size_t len)
//{
//	return VStringA((LPCSTR)GetBuffer(0)+ mBaseString.length()-len,len);
//}
//
//VStringA VStringA::Mid(size_t start,size_t len)
//{
//	if(start+len>=length())
//		return "";
//	return VStringA((LPCSTR)GetBuffer(0)+start,len);
//}
//
//size_t VStringA::Find(char c,size_t start) const
//{
//	return mBaseString.find(c,start);
//}
//
//size_t VStringA::Find(LPCSTR str,size_t start) const
//{
//	return mBaseString.find(str,start);
//}
//
//size_t VStringA::ReverseFind(char c)
//{
//	return mBaseString.find_last_of(c);
//}
//
//LPSTR VStringA::GetBuffer(size_t size)
//{
//	if(size>length())
//	{
//		mBaseString.reserve(size);
//	}
//	if(mBaseString.size()==0)
//		return "";
//	return (LPSTR)&mBaseString[0];
//}
//
//void VStringA::ReleaseBuffer()
//{
//
//}
//
//void VStringA::ReplaceAll(const char* oldStr,const char* newStr)
//{
//	size_t olen = strlen(oldStr);
//	VStringA tempStr = *this;
//	while(true)   
//	{  
//		VStringA::size_type   pos(0);
//		if( (pos=tempStr.mBaseString.find(oldStr))!=VStringA::npos)  
//			tempStr = tempStr.mBaseString.replace(pos,olen,newStr).c_str();  
//		else
//			break;  
//	}  
//	*this = tempStr;
//}
//
////VStringA VStringA::operator= (LPCSTR str)
////{
////	DWORD iTextLen = (int)wcslen(str) * sizeof(wchar_t) + 1;
////
////	char* pElementText = new char[iTextLen];
////	memset( ( void* )pElementText, 0, sizeof( char ) * ( iTextLen ) );
////
////	_vfxUnicode2Ansi( str,pElementText,iTextLen);
////	VBaseStringA::operator =(pElementText);
////	delete[] pElementText;
////	return *this;
////}
////
////bool VStringA::operator==(const VBaseStringA& rh) const
////{
////	return VBaseStringA::compare(rh)==0;
////}
//
//int VStringA::CompareNoCase(LPCSTR rh) const
//{
//	return CompareNoCase(rh, (int)strlen(rh));
//}
//
//int VStringA::CompareNoCase(LPCSTR rh,int num) const
//{
//	size_t len = strlen(rh);
//	if(num>=0 && (size_t)num<len)
//		len = (size_t)num;
//	VBaseStringA::const_iterator p = mBaseString.begin();
//	size_t p2 = 0;
//	while(p!= mBaseString.end() && p2!=len)
//	{
//		if(toupper(*p)!=toupper(rh[p2])) 
//			return ( toupper(*p) < toupper(rh[p2]) )? -1 : 1;
//		++p;
//		++p2;
//	}
//	if(num==-1)
//		return 0;
//	return (len == mBaseString.size()) ? 0 : (mBaseString.size()<len)? -1 : 1;
//}
//
//VStringA VStringA::Format(LPCSTR _format,...)
//{
//	char szBuffer[MAX_BUFFLEN];
//	memset(szBuffer,0,sizeof(szBuffer));
//	va_list ap;
//	va_start(ap,_format);
//	try
//	{
//#ifdef WIN32
//		_vsnprintf_s(szBuffer,MAX_BUFFLEN,MAX_BUFFLEN,_format,ap);
//#else
//		vsnprintf(szBuffer, MAX_BUFFLEN, _format, ap);
//#endif
//	}
//	catch (...)
//	{
//		return VStringA("");
//	}
//	va_end(ap);
//	this->operator=(szBuffer);
//	return *this;
//}
//
//VStringA VStringA_FormatV(LPCSTR _format,...)
//{
//	char szBuffer[MAX_BUFFLEN];
//	memset(szBuffer,0,sizeof(szBuffer));
//	va_list ap;
//	va_start(ap,_format);
//	try
//	{
//        #ifdef WIN32
//                _vsnprintf_s(szBuffer,MAX_BUFFLEN,MAX_BUFFLEN,_format,ap);
//				//sprintf(szBuffer,_format,ap);
//        #else
//                vsnprintf(szBuffer, MAX_BUFFLEN, _format, ap);
//        #endif
//
//	}
//	catch (...)
//	{
//		return VStringA("");
//	}
//	va_end(ap);
//	return VStringA(szBuffer);
//}
//
//VStringW VStringA::Ansi2Unicode(LPCSTR gbk)
//{
//	size_t srcLen = strlen(gbk);
//	size_t dstLen = srcLen*5 + 1;
//	WCHAR* dst = new WCHAR[dstLen];
//	memset(dst, 0, sizeof(WCHAR)*dstLen);
//	dstLen = dstLen * sizeof(WCHAR);
//#if defined(PLATFORM_WIN)
//	WordCodeHelper::ChangeCodeStatic("GB18030//TRANSLIT", "UTF-16LE", gbk, &srcLen, (char*)dst, &dstLen);
//#elif defined(ANDROID) || defined(IOS)
//	WordCodeHelper::ChangeCodeStatic("UTF-8", "UTF-32LE", gbk, &srcLen, (char*)dst, &dstLen);
//#endif
//	//dst[srcLen - dstLen/sizeof(WCHAR)] = NULL;
//	VStringW result = dst;
//	delete[] dst;
//	return result;
//}
//
//VStringA VStringA::Unicode2Ansi(LPCWSTR unicode)
//{
//	size_t srcLen = wcslen(unicode);
//	size_t dstLen = srcLen*4 + 1;
//	char* dst = new char[dstLen];
//	memset(dst, 0, sizeof(char)*dstLen);
//	srcLen = srcLen * sizeof(WCHAR);
//#if defined(PLATFORM_WIN)
//	WordCodeHelper::ChangeCodeStatic("UTF-16LE", "GB18030//TRANSLIT", (char*)unicode, &srcLen, dst, &dstLen);
//#elif defined(ANDROID) || defined(IOS)
//	WordCodeHelper::ChangeCodeStatic("UTF-32LE", "UTF-8", (char*)unicode, &srcLen, dst, &dstLen);
//#endif
//	//dst[srcLen - dstLen/sizeof(WCHAR)] = NULL;
//	VStringA result = dst;
//	delete[] dst;
//	return result;
//}
//
//VStringA VStringA::Gbk2Utf8(LPCSTR gbk)
//{
//	size_t srcLen = strlen(gbk);
//	size_t dstLen = srcLen*5 + 1;
//	char* dst = new char[dstLen];
//	memset(dst, 0, sizeof(char)*dstLen);
//	WordCodeHelper::ChangeCodeStatic("GB18030//TRANSLIT", "UTF-8", gbk, &srcLen, (char*)dst, &dstLen);
//	VStringA result = dst;
//	delete[] dst;
//	return result;
//
//	/*int len = _vfxAnsi2Unicode(gbk,NULL,-1);
//	wchar_t* wszUtf8 = new wchar_t[len+1];
//	memset(wszUtf8,0,(len+1)*sizeof(wchar_t));
//	_vfxAnsi2Unicode(gbk,wszUtf8,len);
//
//	len = _vfxUnicode2Ansi(wszUtf8,NULL,-1);
//	char* szUtf8 = new char[len+1];
//	memset(szUtf8,0,(len+1)*sizeof(char));
//	_vfxUnicode2Ansi(wszUtf8,szUtf8,len);
//	VStringA result = szUtf8;
//	delete[] szUtf8;
//	delete[] wszUtf8;
//	return result;*/
//}
//
//VStringA VStringA::Utf82Gbk(LPCSTR utf8)
//{
//	size_t srcLen = strlen(utf8);
//	size_t dstLen = (srcLen * 5) + 1;
//	char* dst = new char[dstLen];
//	memset(dst, 0, sizeof(char)*dstLen);
//	WordCodeHelper::ChangeCodeStatic("UTF-8", "GB18030//TRANSLIT", utf8, &srcLen, (char*)dst, &dstLen);
//	VStringA result = dst;
//	delete[] dst;
//	return result;
//	/*int len = _vfxAnsi2Unicode(utf8,NULL,-1);
//	wchar_t* wszGBK = new wchar_t[len+1];
//	memset(wszGBK,0,(len+1)*sizeof(wchar_t));
//	_vfxAnsi2Unicode(utf8,wszGBK,len);
//
//	len = _vfxUnicode2Ansi(wszGBK,NULL,-1);
//	char* szGBK = new char[len+1];
//	memset(szGBK,0,(len+1)*sizeof(char));
//	_vfxUnicode2Ansi(wszGBK,szGBK,len);
//	VStringA result = szGBK;
//	delete[] szGBK;
//	delete[] wszGBK;
//	return result;*/
//}
//
////VString VStringA::Utf82Utf16(LPCSTR utf8)
////{
////	int len = _vfxAnsi2Unicode(utf8,NULL,-1);
////	wchar_t* wszGBK = new wchar_t[len+1];
////	memset(wszGBK,0,len*2+2);
////	_vfxAnsi2Unicode(utf8,wszGBK,len);
////	VString result = wszGBK;
////	delete[] wszGBK;
////	return result;
////}

extern "C"
{
	VFX_API void SDK_Memory_Copy(void* tar, void* src, UINT size)
	{
		memcpy(tar, src, size);
	}
	VFX_API int SDK_Memory_Cmp(void* tar, void* src, UINT size)
	{
		return memcmp(tar, src, size);
	}
}



#if !defined(PLATFORM_WIN)
#pragma clang diagnostic pop
#endif
