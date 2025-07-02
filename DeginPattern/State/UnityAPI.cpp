#include "UnityAPI.h"

#include <iostream>

using namespace std;

CUnityScene::CUnityScene() { cout << typeid(*this).name() <<":"<< this << endl; }
CUnityScene::~CUnityScene() { cout << "~" << typeid(*this).name() << ":" << this << endl; }

CTitleScene::CTitleScene() { cout << typeid(*this).name() << ":" << this << endl; }
CTitleScene::~CTitleScene() { cout << "~" << typeid(*this).name() << ":" << this << endl; }

CGameScene::CGameScene() { cout << typeid(*this).name() << ":" << this << endl; }
CGameScene::~CGameScene() { cout << "~" << typeid(*this).name() << ":" << this << endl; }

CEndScene::CEndScene() { cout << typeid(*this).name() << ":" << this << endl; }
CEndScene::~CEndScene() { cout << "~" << typeid(*this).name() << ":" << this << endl; }

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