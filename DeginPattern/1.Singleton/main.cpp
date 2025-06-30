#include <iostream>

using namespace std;

class CSingleObject
{
private:
	static CSingleObject* m_pInstance;
//public: //생성자가 public이면 객체가 생성가능하므로, 객체가 1개이상 생성할수있게된다. 그러므로 싱글톤 클래스는 생성자 private이여한다.
	CSingleObject() { m_pInstance = NULL; };
	~CSingleObject() { /* Release(); */ } //소멸자를 priavete하면 객체외부에서 delete를 할수없다. 소멸자를 호출할수없으므로 어떤 코드를 작성해도 작동할수없다.
public:
	static CSingleObject* GetInstance() 
	{
		if (m_pInstance == NULL)
		{
			m_pInstance = new CSingleObject();
		}
		return m_pInstance; 
	}
	void ShowMessage()
	{
		cout << "ShowMessage" << endl;
	}
	void Release()
	{
		delete m_pInstance;
	}
};
//C++의 문법상 클래스의 객체가 생성되기전에 할당가능한 메모리가 전역변수이므로, 맴버변수를 전역변수처렴 선언해야한다.
CSingleObject* CSingleObject::m_pInstance = NULL;

int main()
{
	//CSingleObject cSigleObject; //객체 인스턴스
	//CSingleObject cSigleObject[2];
	CSingleObject* pSingleObject = NULL;
	pSingleObject = CSingleObject::GetInstance(); //객체가 생성되기전에 접근가능한것은 객체의 이름이므로 이렇게 접근을 해야한다.
	//pSingleObject = pSingleObject->GetInstance(); //객체가 생성되지않은 객체에 접근하는것은 불가능하므로 컴파일에러가 발생한다.
	pSingleObject->ShowMessage();
	//delete pSingleObject; //단일책임원칙에 따라 객체내의 할당된 메모리는 각 객체 내부에서 삭제하는 것이 원칙이다.
	pSingleObject->Release();
	return 0;
}