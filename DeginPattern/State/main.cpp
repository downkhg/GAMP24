#include <iostream>

using namespace std;

class CUnityScene
{
public:
	CUnityScene() { cout << typeid(*this).name() << endl; }
	~CUnityScene() { cout <<"~" << typeid(*this).name() << endl; }
	virtual void LoadScene(CUnityEditor* pEditor);
};

class CTitleScene : public CUnityScene
{
public:
	void LoadScene(CUnityEditor* pEditor)
	{
		pEditor->SetScene(new CGameScene());
	}
};


class CGameScene : public CUnityScene
{
public:
	void LoadScene(CUnityEditor* pEditor)
	{
		pEditor->SetScene(new CEndScene());
	}
};

class CEndScene : public CUnityScene
{
public:
	void LoadScene(CUnityEditor* pEditor)
	{
		pEditor->SetScene(new CTitleScene);
	}
};


class CTitleScene: public CUnityScene
{
public:
	void LoadScene(CUnityEditor* pEditer)
	{
		pEditer->SetScene(new CGameScene());
	}
};

class CUnityEditor
{
	CUnityScene* m_pCurrentScene = NULL;
public:
	~CUnityEditor() { if(m_pCurrentScene) delete m_pCurrentScene; }

	void SetScene(CUnityScene* newScene)
	{
		if (m_pCurrentScene) delete m_pCurrentScene;
		m_pCurrentScene = newScene;
	}

	void NextScene()
	{
		m_pCurrentScene->LoadScene(this);
	}
};

int main()
{
	CUnityEditor cUnityEditor;
	cUnityEditor.SetScene(new CTitleScene());
	cUnityEditor.NextScene();
	cUnityEditor.NextScene();
	cUnityEditor.NextScene();

	return 0;
}