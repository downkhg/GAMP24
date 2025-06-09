#pragma once
#include <iostream>

using namespace std;

namespace Fake
{
	template<typename Type>
	struct SNode {
		Type nData;
		SNode* pNext;
		SNode* pPre;
	};

	// SNode ����� ���� << ������ �����ε�
	template<typename Type>
	std::ostream& operator<<(std::ostream& os, const SNode<Type>& node)
	{
		os << "SNode { data: " << node.nData
			<< ", pPre: " << node.pPre
			<< ", pNext: " << node.pNext << " }";
		return os;
	}

	template<typename Type>
	class list
	{
		SNode<Type>* pBegin = NULL;
		SNode<Type>* pEnd = NULL;

		int nSize = 0;

	public:
		list(int size)
		{
			for (int i = 0; i < size; i++)
			{
				push_back(0);
			}
		}

		SNode<Type>* begin()
		{
			return pBegin;
		}

		SNode<Type>* end()
		{
			return (pEnd+1);
		}

		void resize(int size)
		{
			for (int i = nSize; i < size; i++)
			{
				push_back(0);
			}
		}

		void push_back(Type data)
		{
			SNode<Type>* pTemp = NULL;
			SNode<Type>* pNode = pEnd;

			pTemp = new SNode<Type>();
			pTemp->nData = data;
			if (pNode != NULL) //0x04 != NULL > T
				pNode->pNext = pTemp;

			if (pBegin == NULL)
				pBegin = pTemp;

			pEnd = pTemp;

			nSize++;
		}

		SNode<Type>* insert(SNode<Type>* where,Type insert)
		{
			SNode<Type>* pNode = pBegin;
			SNode<Type>* pInsert = NULL;

			pNode = where;

			pInsert = new SNode<Type>();
			pInsert->nData = insert;
			pInsert->pNext = pNode->pNext;
			pNode->pNext = pInsert;
			nSize++;
			return pInsert;
		}

		void erase(SNode<Type>* where)
		{
			SNode<Type>* pPre = where->pPre;
			SNode<Type>* pNode = where;

			if (pPre != NULL)//N == N -> T -> !T ->F
				pPre->pNext = pNode->pNext;

			delete pNode;

			nSize--;
		}

		void clear()
		{
			SNode<Type>* pPre = NULL;
			SNode<Type>* pNode = pBegin;

			while (pNode != NULL)
			{
				pPre = pNode->pPre;
				
				if (pBegin == pNode)
					pBegin = pNode->pNext;

				if (pPre != NULL)//N == N -> T -> !T ->F
					pPre->pNext = pNode->pNext;

				delete pNode;
			}
			pBegin = NULL;
			nSize = 0;
		}
	};

	void ListMain()
	{
		cout << "####### ListMain ######" << endl;
		//���ø�: �ڷ����� �����Ҽ��ְ���.
		list<int> container(1);//�����̳ʻ����� ũ�⸦ ���������ϴ�.
		container.begin()->nData = 10;
		SNode<int>* it;
		cout << "PrintPtr:";
		for (it = container.begin(); it != container.end(); it++)
			cout << "[" << (int)it << "]" << it->nData << ",";
		cout << endl;
		container.resize(3); //�迭�� ũ�⸦ �����Ѵ�.
		cout << "PrintPtr:";
		for (it = container.begin(); it != container.end(); it++)
			cout << "[" << (int)&*it << "]" << it->nData << ",";
		cout << endl;
		//1.�߰� 2.���� 3.���� 4.��λ���

		cout << "PrintPtr:";
		for (it = container.begin(); it != container.end(); it++)
			cout << "[" << (int)&*it << "]" << *it << ",";
		cout << endl;

		container.push_back(20);

		cout << "PrintPtr:";
		for (it = container.begin(); it != container.end(); it++)
			cout << "[" << (int)&*it << "]" << *it << ",";
		cout << endl;
		auto itFind = container.begin();
		//itFind++;
		cout << "Find++:" << *itFind << endl;

		container.insert(itFind, 30);

		cout << "PrintPtr[Insert]:";
		for (it = container.begin(); it != container.end(); it++)
			cout << "[" << (int)it << "]" << it->nData << ",";
		cout << endl;


		itFind = container.begin();//find(container.begin(), container.end(), 30);

		cout << "Find(30):" << *itFind << endl;
		itFind--;
		cout << "Find(30)--:" << *itFind << endl;

		itFind = container.begin();// find(container.begin(), container.end(), 10);

		itFind++;
		itFind++;
		itFind++;
		itFind++;
		cout << "Find(10)+4:" << *itFind << endl;

		auto itErase = container.begin();
		container.erase(itErase);
		cout << "PrintPtr[Erase]:";
		for (it = container.begin(); it != container.end(); it++)
			cout << "[" << (int)&*it << "]" << *it << ",";
		cout << endl;

		container.push_back(40);
		cout << "PrintPtr[insert]:";
		for (it = container.begin(); it != container.end(); it++)
			cout << "[" << (int)&*it << "]" << *it << ",";
		cout << endl;

		container.clear(); //��λ���
		cout << "Clear:";
		for (it = container.begin(); it != container.end(); it++)
			cout << "[" << &*it << "]" << *it << ",";
		cout << endl;
	}
}