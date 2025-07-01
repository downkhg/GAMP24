#include <iostream>
#include <list>
#include <string>

using namespace std;

class CUnit
{
public:
	int x;
	int y;
	CUnit() { cout << typeid(*this).name() << endl; }
	virtual void Move(int x, int y) = 0
	{
		cout << typeid(this).name() << ":Move(" << x << "," << y << ")" << endl;
	}
	virtual void Attack(CUnit* target) = 0
	{
		cout << typeid(this).name() << ":Attack(" << typeid(*target).name()<< ")" << endl;
	}
};

class CMarin : public CUnit
{
public:
	CMarin() { cout << typeid(*this).name() << endl; }
	void Move(int x, int y)
	{
		cout << typeid(this).name() << ":Move(" << x << "," << y << ")" << endl;
	}

	virtual void Attack(CUnit* target)
	{
		cout << typeid(this).name() << ":Attack(" << typeid(*target).name() << ")" << endl;
	}
};

class CMedic : public CUnit
{
public:
	CMedic() { cout << typeid(*this).name() << endl; }
	void Move(int x, int y)
	{
		cout << typeid(this).name() << ":Move(" << x << "," << y << ")" << endl;
	}

	virtual void Attack(CUnit* target)
	{
		Move(target->x, target->y);
	}
};

class CCommender
{
	list<CUnit*> m_listUnits;

public:
	void SelectUnit(CUnit* pUnit)
	{
		m_listUnits.push_back(pUnit);
	}

	void DeselectUnit(CUnit* pUnit)
	{
		m_listUnits.remove(pUnit);
	}

	void Move(int x, int y)
	{
		for (auto it = m_listUnits.begin(); it != m_listUnits.end(); it++)
		{
			(*it)->Move(x, y);
		}
	}

	void Attack(CUnit* pTarget)
	{
		for (auto it = m_listUnits.begin(); it != m_listUnits.end(); it++)
		{
			(*it)->Attack(pTarget);
		}
	}
};

void main()
{
	CCommender cCommeder;
	//CUnit  cUnit; //추상클래스는 객체화할수없다.
	CMedic cMedic[4];
	CMarin cMarin[8];

	for (auto unit : cMarin)
	{
		cCommeder.SelectUnit(&unit);
	}

	for (auto unit : cMedic)
	{
		cCommeder.SelectUnit(&unit);
	}

	cCommeder.Move(10, 20);
	cCommeder.Attack(&cMarin[0]);
}