#include <iostream>
#include <ostream>
#include <stdexcept> // 예외 처리를 위해 추가

using namespace std;
namespace Fake
{
	// SNode 구조체는 그대로 사용합니다.
	template<typename Type>
	struct SNode {
		Type nData;
		SNode* pNext;
		SNode* pPre;

		// 생성자를 추가하여 노드 생성을 편리하게 합니다.
		SNode(Type data = Type(), SNode* prev = nullptr, SNode* next = nullptr)
			: nData(data), pPre(prev), pNext(next) {
		}
	};

	// SNode 자체를 출력하는 연산자 (디버깅용)
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
		SNode<Type>* m_pHead; // 리스트의 시작을 가리키는 더미 노드 (센티널)
		SNode<Type>* m_pTail; // 리스트의 끝을 가리키는 더미 노드 (센티널)
		int nSize;

	public:
		// ==================================================================
		// 1. list 클래스 내부에 이터레이터 클래스 정의
		// ==================================================================
		class iterator
		{
		private:
			SNode<Type>* m_pNode;
			// list 클래스가 이터레이터의 private 멤버에 접근할 수 있도록 friend 선언
			friend class list<Type>;

			// list 클래스만 이 생성자를 호출할 수 있음
			explicit iterator(SNode<Type>* pNode) : m_pNode(pNode) {}

		public:
			iterator() : m_pNode(nullptr) {}

			// 역참조 연산자 (*it): 노드의 데이터에 접근
			Type& operator*() const {
				if (m_pNode == nullptr) throw std::runtime_error("Dereferencing null iterator");
				return m_pNode->nData;
			}

			// 멤버 접근 연산자 (it->...): 포인터처럼 사용
			SNode<Type>* operator->() const {
				return m_pNode;
			}

			// 전위 증가 (++it): 다음 노드로 이동
			iterator& operator++() {
				if (m_pNode) m_pNode = m_pNode->pNext;
				return *this;
			}

			// 후위 증가 (it++): 이동 전의 이터레이터를 반환
			iterator operator++(int) {
				iterator temp = *this;
				if (m_pNode) m_pNode = m_pNode->pNext;
				return temp;
			}

			// 전위 감소 (--it): 이전 노드로 이동
			iterator& operator--() {
				if (m_pNode) m_pNode = m_pNode->pPre;
				return *this;
			}

			// 후위 감소 (it--): 이동 전의 이터레이터를 반환
			iterator operator--(int) {
				iterator temp = *this;
				if (m_pNode) m_pNode = m_pNode->pPre;
				return temp;
			}

			// 비교 연산자
			bool operator==(const iterator& other) const { return m_pNode == other.m_pNode; }
			bool operator!=(const iterator& other) const { return m_pNode != other.m_pNode; }
		};
		// ==================================================================


		// list의 생성자: 센티널 노드 설정 및 크기만큼 초기화
		list(int size = 0) : nSize(0)
		{
			// 2. 센티널 노드(더미 헤드/테일) 생성 및 연결
			m_pHead = new SNode<Type>();
			m_pTail = new SNode<Type>();
			m_pHead->pNext = m_pTail;
			m_pTail->pPre = m_pHead;

			if (size > 0) {
				resize(size);
			}
		}

		// 소멸자: 모든 노드 메모리 해제
		~list()
		{
			clear();
			delete m_pHead;
			delete m_pTail;
		}

		// 첫 번째 실제 원소를 가리키는 이터레이터 반환
		iterator begin()
		{
			return iterator(m_pHead->pNext);
		}

		// 마지막 원소의 '다음' 위치(테일 센티널)를 가리키는 이터레이터 반환
		iterator end()
		{
			return iterator(m_pTail);
		}

		// 리스트 크기 조절 (증가/감소 모두 지원)
		void resize(int size)
		{
			while (nSize < size) {
				push_back(Type()); // 기본값(0)으로 노드 추가
			}
			while (nSize > size) {
				// --end()는 마지막 원소를 가리키는 이터레이터
				erase(--end());
			}
		}

		// 3. pPre 포인터까지 완벽하게 처리하는 push_back
		void push_back(Type data)
		{
			// end() 이전에 삽입하면 맨 뒤에 추가됨
			insert(end(), data);
		}

		// 4. pPre 포인터까지 완벽하게 처리하는 insert
		iterator insert(iterator where, Type data)
		{
			SNode<Type>* pNextNode = where.m_pNode;
			SNode<Type>* pPrevNode = pNextNode->pPre;

			// 새 노드를 pPrevNode와 pNextNode 사이에 삽입
			SNode<Type>* pNewNode = new SNode<Type>(data, pPrevNode, pNextNode);

			pPrevNode->pNext = pNewNode;
			pNextNode->pPre = pNewNode;

			nSize++;
			return iterator(pNewNode);
		}

		// 5. pPre, pNext를 모두 안전하게 처리하는 erase
		iterator erase(iterator where)
		{
			// end() 이터레이터는 삭제 불가
			if (where == end() || nSize == 0) {
				return end();
			}

			SNode<Type>* pNodeToErase = where.m_pNode;
			SNode<Type>* pPrevNode = pNodeToErase->pPre;
			SNode<Type>* pNextNode = pNodeToErase->pNext;

			// 이전 노드와 다음 노드를 서로 연결
			pPrevNode->pNext = pNextNode;
			pNextNode->pPre = pPrevNode;

			delete pNodeToErase;
			nSize--;

			// 삭제된 원소의 다음 원소를 가리키는 이터레이터 반환
			return iterator(pNextNode);
		}

		// 6. 단순하고 안전하게 구현된 clear
		void clear()
		{
			while (nSize > 0) {
				erase(begin());
			}
		}
	};

	// ListMain은 이제 새로운 이터레이터와 수정된 list 클래스로 잘 동작합니다.
	void ListMain()
	{
		cout << "####### Fake::ListMain ######" << endl;
		list<int> container(1);
		*container.begin() = 10;

		list<int>::iterator it; // 이제 이 코드는 정상적으로 컴파일됩니다.

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
		++itFind; // 두 번째 원소로 이동
		cout << "Find++:" << *itFind << endl;

		container.insert(itFind, 30);

		cout << "PrintPtr[Insert]:";
		for (it = container.begin(); it != container.end(); ++it)
			cout << "[" << (void*)&(*it) << "]" << *it << ",";
		cout << endl;

		// std::find와 같은 알고리즘을 사용하려면 <algorithm> 헤더가 필요합니다.
		// 여기서는 수동으로 탐색합니다.
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