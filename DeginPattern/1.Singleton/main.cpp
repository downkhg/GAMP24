#include <iostream>

using namespace std;

class CSingleObject
{
private:
	static CSingleObject* m_pInstance;
//public: //�����ڰ� public�̸� ��ü�� ���������ϹǷ�, ��ü�� 1���̻� �����Ҽ��ְԵȴ�. �׷��Ƿ� �̱��� Ŭ������ ������ private�̿��Ѵ�.
	CSingleObject() { m_pInstance = NULL; };
	~CSingleObject() { /* Release(); */ } //�Ҹ��ڸ� priavete�ϸ� ��ü�ܺο��� delete�� �Ҽ�����. �Ҹ��ڸ� ȣ���Ҽ������Ƿ� � �ڵ带 �ۼ��ص� �۵��Ҽ�����.
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
//C++�� ������ Ŭ������ ��ü�� �����Ǳ����� �Ҵ簡���� �޸𸮰� ���������̹Ƿ�, �ɹ������� ��������ó�� �����ؾ��Ѵ�.
CSingleObject* CSingleObject::m_pInstance = NULL;

int main()
{
	//CSingleObject cSigleObject; //��ü �ν��Ͻ�
	//CSingleObject cSigleObject[2];
	CSingleObject* pSingleObject = NULL;
	pSingleObject = CSingleObject::GetInstance(); //��ü�� �����Ǳ����� ���ٰ����Ѱ��� ��ü�� �̸��̹Ƿ� �̷��� ������ �ؾ��Ѵ�.
	//pSingleObject = pSingleObject->GetInstance(); //��ü�� ������������ ��ü�� �����ϴ°��� �Ұ����ϹǷ� �����Ͽ����� �߻��Ѵ�.
	pSingleObject->ShowMessage();
	//delete pSingleObject; //����å�ӿ�Ģ�� ���� ��ü���� �Ҵ�� �޸𸮴� �� ��ü ���ο��� �����ϴ� ���� ��Ģ�̴�.
	pSingleObject->Release();
	return 0;
}