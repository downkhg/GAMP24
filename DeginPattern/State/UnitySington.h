#pragma once
#include <list>
#include <string>

using namespace std;

class CMonoBehavier
{
	bool m_isDestory = true;
public:
	void SetDestory(bool destry) { m_isDestory = destry; }
	bool GetDestory() { return m_isDestory; }

	virtual void Start() = 0;
	virtual void Update() = 0;
};

class CGameManager : public CMonoBehavier
{
public:
	static CGameManager* m_pInstacne;
	
	void Start() { m_pInstacne = this; }
	void Update() {};
};

CGameManager* CGameManager::m_pInstacne = 0;

class CGameObject
{
	list<CMonoBehavier*> components;
public:
	~CGameObject()
	{
		for (auto it : components)
		{
			if (it->GetDestory())
				delete it;
		}
	}

	void Start()
	{
		for (auto it : components)
		{
			it->Start();
		}
	}

	void Update()
	{
		for (auto it : components)
		{
			it->Update();
		}
	}
};