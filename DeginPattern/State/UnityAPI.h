#pragma once

#include <iostream>
#include <unordered_set>

class CGameObejct;

class CUnityEditor; //Ŭ������ ����: Ŭ������ ��ü�� �������������� ������ �����Ұ��� �̸� �˸��°�.
class CTitleScene;
class CGameScene;
class CEndScene;

class CUnityScene
{
	std::unordered_set<CGameObejct*> m_gameObjects;
public:
	CUnityScene();
	~CUnityScene();
	virtual void LoadScene(CUnityEditor* pEditor) = 0;
};

class CUnityEditor
{
	CUnityScene* m_pCurrentScene = 0;
public:
	~CUnityEditor() { if (m_pCurrentScene) delete m_pCurrentScene; }

	void SetScene(CUnityScene* newScene);
	void NextScene();
};

//Ŭ�����Լ��� ����: Ŭ������ ����� �Լ��� ������ �˼��ְ� �����Ͽ� ���߿� ������ �Լ��� �˸���.
class CTitleScene : public CUnityScene
{
public:
	CTitleScene();
	~CTitleScene();
	void LoadScene(CUnityEditor* pEditor);
};

class CGameScene : public CUnityScene
{
public:
	CGameScene();
	~CGameScene();
	void LoadScene(CUnityEditor* pEditor);
};

class CEndScene : public CUnityScene
{
public:
	CEndScene();
	~CEndScene();
	void LoadScene(CUnityEditor* pEditer);
};