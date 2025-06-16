#include <iostream>
#include <ostream>
#include <stdexcept> // ���� ó���� ���� �߰�

using namespace std;
namespace Fake
{
	// SNode ����ü�� �״�� ����մϴ�.
	template<typename Type>
	struct SNode {
		Type nData;
		SNode* pNext;
		SNode* pPre;

		// �����ڸ� �߰��Ͽ� ��� ������ ���ϰ� �մϴ�.
		SNode(Type data = Type(), SNode* prev = nullptr, SNode* next = nullptr)
			: nData(data), pPre(prev), pNext(next) {
		}
	};

	// SNode ��ü�� ����ϴ� ������ (������)
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
	private:
		SNode<Type>* m_pHead; // ����Ʈ�� ������ ����Ű�� ���� ��� (��Ƽ��)
		SNode<Type>* m_pTail; // ����Ʈ�� ���� ����Ű�� ���� ��� (��Ƽ��)
		int nSize;

	public:
		// ==================================================================
		// 1. list Ŭ���� ���ο� ���ͷ����� Ŭ���� ����
		// ==================================================================
		class iterator
		{
		private:
			SNode<Type>* m_pNode;
			// list Ŭ������ ���ͷ������� private ����� ������ �� �ֵ��� friend ����
			friend class list<Type>;

			// list Ŭ������ �� �����ڸ� ȣ���� �� ����
			explicit iterator(SNode<Type>* pNode) : m_pNode(pNode) {}

		public:
			iterator() : m_pNode(nullptr) {}

			// ������ ������ (*it): ����� �����Ϳ� ����
			Type& operator*() const {
				if (m_pNode == nullptr) throw std::runtime_error("Dereferencing null iterator");
				return m_pNode->nData;
			}

			// ��� ���� ������ (it->...): ������ó�� ���
			SNode<Type>* operator->() const {
				return m_pNode;
			}

			// ���� ���� (++it): ���� ���� �̵�
			iterator& operator++() {
				if (m_pNode) m_pNode = m_pNode->pNext;
				return *this;
			}

			// ���� ���� (it++): �̵� ���� ���ͷ����͸� ��ȯ
			iterator operator++(int) {
				iterator temp = *this;
				if (m_pNode) m_pNode = m_pNode->pNext;
				return temp;
			}

			// ���� ���� (--it): ���� ���� �̵�
			iterator& operator--() {
				if (m_pNode) m_pNode = m_pNode->pPre;
				return *this;
			}

			// ���� ���� (it--): �̵� ���� ���ͷ����͸� ��ȯ
			iterator operator--(int) {
				iterator temp = *this;
				if (m_pNode) m_pNode = m_pNode->pPre;
				return temp;
			}

			// �� ������
			bool operator==(const iterator& other) const { return m_pNode == other.m_pNode; }
			bool operator!=(const iterator& other) const { return m_pNode != other.m_pNode; }
		};
		// ==================================================================


		// list�� ������: ��Ƽ�� ��� ���� �� ũ�⸸ŭ �ʱ�ȭ
		list(int size = 0) : nSize(0)
		{
			// 2. ��Ƽ�� ���(���� ���/����) ���� �� ����
			m_pHead = new SNode<Type>();
			m_pTail = new SNode<Type>();
			m_pHead->pNext = m_pTail;
			m_pTail->pPre = m_pHead;

			if (size > 0) {
				resize(size);
			}
		}

		// �Ҹ���: ��� ��� �޸� ����
		~list()
		{
			clear();
			delete m_pHead;
			delete m_pTail;
		}

		// ù ��° ���� ���Ҹ� ����Ű�� ���ͷ����� ��ȯ
		iterator begin()
		{
			return iterator(m_pHead->pNext);
		}

		// ������ ������ '����' ��ġ(���� ��Ƽ��)�� ����Ű�� ���ͷ����� ��ȯ
		iterator end()
		{
			return iterator(m_pTail);
		}

		// ����Ʈ ũ�� ���� (����/���� ��� ����)
		void resize(int size)
		{
			while (nSize < size) {
				push_back(Type()); // �⺻��(0)���� ��� �߰�
			}
			while (nSize > size) {
				// --end()�� ������ ���Ҹ� ����Ű�� ���ͷ�����
				erase(--end());
			}
		}

		// 3. pPre �����ͱ��� �Ϻ��ϰ� ó���ϴ� push_back
		void push_back(Type data)
		{
			// end() ������ �����ϸ� �� �ڿ� �߰���
			insert(end(), data);
		}

		// 4. pPre �����ͱ��� �Ϻ��ϰ� ó���ϴ� insert
		iterator insert(iterator where, Type data)
		{
			SNode<Type>* pNextNode = where.m_pNode;
			SNode<Type>* pPrevNode = pNextNode->pPre;

			// �� ��带 pPrevNode�� pNextNode ���̿� ����
			SNode<Type>* pNewNode = new SNode<Type>(data, pPrevNode, pNextNode);

			pPrevNode->pNext = pNewNode;
			pNextNode->pPre = pNewNode;

			nSize++;
			return iterator(pNewNode);
		}

		// 5. pPre, pNext�� ��� �����ϰ� ó���ϴ� erase
		iterator erase(iterator where)
		{
			// end() ���ͷ����ʹ� ���� �Ұ�
			if (where == end() || nSize == 0) {
				return end();
			}

			SNode<Type>* pNodeToErase = where.m_pNode;
			SNode<Type>* pPrevNode = pNodeToErase->pPre;
			SNode<Type>* pNextNode = pNodeToErase->pNext;

			// ���� ���� ���� ��带 ���� ����
			pPrevNode->pNext = pNextNode;
			pNextNode->pPre = pPrevNode;

			delete pNodeToErase;
			nSize--;

			// ������ ������ ���� ���Ҹ� ����Ű�� ���ͷ����� ��ȯ
			return iterator(pNextNode);
		}

		// 6. �ܼ��ϰ� �����ϰ� ������ clear
		void clear()
		{
			while (nSize > 0) {
				erase(begin());
			}
		}
	};

	// ListMain�� ���� ���ο� ���ͷ����Ϳ� ������ list Ŭ������ �� �����մϴ�.
	void ListMain()
	{
		cout << "####### Fake::ListMain ######" << endl;
		list<int> container(1);
		*container.begin() = 10;

		list<int>::iterator it; // ���� �� �ڵ�� ���������� �����ϵ˴ϴ�.

		cout << "PrintPtr:";
		for (it = container.begin(); it != container.end(); ++it)
			cout << "[" << (void*)&(*it) << "]" << *it << ",";
		cout << endl;

		container.resize(3);
		cout << "PrintPtr:";
		for (it = container.begin(); it != container.end(); ++it)
			cout << "[" << (void*)&(*it) << "]" << *it << ",";
		cout << endl;

		container.push_back(20);

		cout << "PrintPtr:";
		for (it = container.begin(); it != container.end(); ++it)
			cout << "[" << (void*)&(*it) << "]" << *it << ",";
		cout << endl;

		list<int>::iterator itFind = container.begin();
		++itFind; // �� ��° ���ҷ� �̵�
		cout << "Find++:" << *itFind << endl;

		container.insert(itFind, 30);

		cout << "PrintPtr[Insert]:";
		for (it = container.begin(); it != container.end(); ++it)
			cout << "[" << (void*)&(*it) << "]" << *it << ",";
		cout << endl;

		// std::find�� ���� �˰����� ����Ϸ��� <algorithm> ����� �ʿ��մϴ�.
		// ���⼭�� �������� Ž���մϴ�.
		itFind = container.begin();
		while (itFind != container.end() && *itFind != 30) {
			++itFind;
		}
		cout << "Find(30):" << *itFind << endl;
		--itFind;
		cout << "Find(30)--:" << *itFind << endl;

		itFind = container.begin();
		++itFind; ++itFind; ++itFind; ++itFind;
		cout << "Find(10)+4:" << *itFind << endl;

		list<int>::iterator itErase = container.begin();
		container.erase(itErase);
		cout << "PrintPtr[Erase]:";
		for (it = container.begin(); it != container.end(); ++it)
			cout << "[" << (void*)&(*it) << "]" << *it << ",";
		cout << endl;

		container.push_back(40);
		cout << "PrintPtr[insert]:";
		for (it = container.begin(); it != container.end(); ++it)
			cout << "[" << (void*)&(*it) << "]" << *it << ",";
		cout << endl;

		container.clear();
		cout << "Clear:";
		for (it = container.begin(); it != container.end(); ++it)
			cout << "[" << (void*)&(*it) << "]" << *it << ",";
		cout << endl;
	}
}