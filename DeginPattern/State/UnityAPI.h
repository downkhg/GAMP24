#pragma once

#include <iostream>
#include <unordered_set>

class CGameObejct;

class CUnityEditor; //클래스의 선언: 클래스의 실체가 존재하지않지만 나중이 정의할것을 미리 알리는것.
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

//클래스함수의 선언: 클래스의 멤버와 함수의 종류를 알수있게 선언하여 나중에 정의할 함수를 알린다.
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