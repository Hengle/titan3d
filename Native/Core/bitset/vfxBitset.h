#pragma once

#include "../../BaseHead.h"

NS_BEGIN
class XNDAttrib;

class vBitset
{

public:
	typedef unsigned int block_type;		// ��С�洢��Ԫ�������
	typedef unsigned int size_type;		// ��С�洢��Ԫ��Ĵ�С����
private:
	enum { BLOCK_BYTES = sizeof(block_type) };	// ��С�洢��Ԫ����ֽ���
	enum { BLOCK_BITS = 8 * BLOCK_BYTES };	//��С��Ԫ�洢���λ����ƽ̨ͨ���ԣ�����ʹ��numeric_limits

private:
	void leftShift(size_type num);		// ���Ʋ���
	void rightShift(size_type num);		// ���Ʋ���

private:
	size_type m_bitNum;		// bitset��λ�ĸ���
	size_type m_size;		// block_type�ĸ���
	block_type *m_pBits;	// �洢bitλ
	block_type m_mask;		// ����bitset��λ����5����m_pBits[0]=0xFFFFFFFF,m_mask������ʾ
							// m_pBits[0]�ĺ�5λ��Ч

	vBitset(void);

public:
	class vBitsetReference
	{
		friend class vBitset;

	private:
		vBitset * pBitset;	// pointer to the bitset
		size_type myPos;	// position of element in bitset

		vBitsetReference(vBitset& _bitset, size_type pos) :
			pBitset(&_bitset),
			myPos(pos)
		{
		}


	public:
		vBitsetReference & operator=(bool val)
		{
			pBitset->set(myPos, val);
			return (*this);
		}

		vBitsetReference& operator=(const vBitsetReference& bitRef)
		{
			pBitset->set(myPos, bool(bitRef));
			return (*this);
		}

		vBitsetReference& flip()
		{
			pBitset->flip(myPos);
			return (*this);
		}

		bool operator~() const
		{
			return (!pBitset->test(myPos));
		}

		operator bool() const
		{
			return (pBitset->test(myPos));
		}
	};

public:
	vBitset(size_type val);
	vBitset(const vBitset &val);
	~vBitset(void);

	vBitset& operator=(const vBitset& val);
	vBitset& operator <<= (size_type num);		// ���Ƹ�ֵ����
	vBitset& operator >>= (size_type num);		// ���Ƹ�ֵ����
	vBitset& operator &= (const vBitset &val);
	vBitset& operator |= (const vBitset &val);
	vBitset& operator ^= (const vBitset &val);
	bool operator == (const vBitset& val) const;
	vBitset& operator ~();						// ȡ��������
												// bool operator[] (size_type pos) const;
												// vBitsetReference operator[] (size_type pos);

	vBitset& flip();							// ��ת����λ����
	vBitset& flip(size_type pos);				// ��תposλ�Ĳ���
	vBitset& set(size_type pos, bool tagSet = true);	// ����bitset�е�ĳһλ��ֵ
	vBitset& set(bool tagSet = false);			// ����bitset��ÿһλ��ֵ
	bool test(size_type pos) const;				// ������Ӧλ��1����0,1����true,0����false

	size_type size() const;

	bool Save(XNDAttrib* pAttr, bool withBegin = true);
	bool Load(XNDAttrib* pAttr, bool withBegin = true);
};

vBitset operator << (const vBitset&, vBitset::size_type);
vBitset operator >> (const vBitset&, vBitset::size_type);
vBitset operator & (const vBitset&, const vBitset&);
vBitset operator | (const vBitset&, const vBitset&);
vBitset operator ^ (const vBitset&, const vBitset&);

NS_END