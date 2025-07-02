#include <iostream>

using namespace std;

class CUnityEditor; //클래스의 선언: 클래스의 실체가 존재하지않지만 나중이 정의할것을 미리 알리는것.
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

//클래스함수의 선언: 클래스의 멤버와 함수의 종류를 알수있게 선언하여 나중에 정의할 함수를 알린다.
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