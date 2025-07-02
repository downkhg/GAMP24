#include <iostream>

using namespace std;

class CUnityEditor; //Ŭ������ ����: Ŭ������ ��ü�� �������������� ������ �����Ұ��� �̸� �˸��°�.
class CTitleScene;
class CGameScene;
class CEndScene;

class CUnityScene
{
public:
	CUnityScene() { cout << typeid(*this).name() << endl; }
	~CUnityScene() { cout << "~" << typeid(*this).name() << endl; }
	virtual void LoadScene(CUnityEditor* pEditor) = 0;
};

class CUnityEditor
{
	CUnityScene* m_pCurrentScene = NULL;
public:
	~CUnityEditor() { if (m_pCurrentScene) delete m_pCurrentScene; }

	void SetScene(CUnityScene* newScene);
	void NextScene();
};

//Ŭ�����Լ��� ����: Ŭ������ ����� �Լ��� ������ �˼��ְ� �����Ͽ� ���߿� ������ �Լ��� �˸���.
class CTitleScene : public CUnityScene
{
public:
	void LoadScene(CUnityEditor* pEditor);
};

class CGameScene : public CUnityScene
{
public:
	void LoadScene(CUnityEditor* pEditor);
};

class CEndScene: public CUnityScene
{
public:
	void LoadScene(CUnityEditor* pEditer);
};

void CTitleScene::LoadScene(CUnityEditor* pEditor)
{
	pEditor->SetScene(new CGameScene);
}

void CGameScene::LoadScene(CUnityEditor* pEditor)
{
	pEditor->SetScene(new CEndScene);
}

void CEndScene::LoadScene(CUnityEditor* pEditor)
{
	pEditor->SetScene(new CTitleScene);
}

void CUnityEditor::SetScene(CUnityScene* newScene)
{
	if (m_pCurrentScene) delete m_pCurrentScene;
	m_pCurrentScene = newScene;
}

void CUnityEditor::NextScene()
{
	m_pCurrentScene->LoadScene(this);
}

int main()
{
	CUnityEditor cUnityEditor;
	cUnityEditor.SetScene(new CTitleScene());
	cUnityEditor.NextScene();
	cUnityEditor.NextScene();
	cUnityEditor.NextScene();

	return 0;
}