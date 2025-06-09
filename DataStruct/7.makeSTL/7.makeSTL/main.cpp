#include <iostream>
#include <list>

#include "list.h"

using namespace std;

void ListMain()
{
	cout << "####### ListMain ######" << endl;
	//���ø�: �ڷ����� �����Ҽ��ְ���.
	list<int> container(1);//�����̳ʻ����� ũ�⸦ ���������ϴ�.
	*container.begin() = 10;
	list<int>::iterator it;
	cout << "PrintPtr:";
	for (it = container.begin(); it != container.end(); it++)
		cout << "[" << (int)&*it << "]" << *it << ",";
	cout << endl;
	container.resize(3); //�迭�� ũ�⸦ �����Ѵ�.
	cout << "PrintPtr:";
	for (it = container.begin(); it != container.end(); it++)
		cout << "[" << (int)&*it << "]" << *it << ",";
	cout << endl;
	//1.�߰� 2.���� 3.���� 4.��λ���

	cout << "PrintPtr:";
	for (it = container.begin(); it != container.end(); it++)
		cout << "[" << (int)&*it << "]" << *it << ",";
	cout << endl;

	container.push_back(20);

	cout << "PrintPtr:";
	for (it = container.begin(); it != container.end(); it++)
		cout << "[" << (int)&*it << "]" << *it << ",";
	cout << endl;
	list<int>::iterator itFind = container.begin();
	itFind++;
	cout << "Find++:" << *itFind << endl;

	container.insert(itFind, 30);

	cout << "PrintPtr[Insert]:";
	for (it = container.begin(); it != container.end(); it++)
		cout << "[" << (int)&*it << "]" << *it << ",";
	cout << endl;


	itFind = container.begin();// find(container.begin(), container.end(), 30);

	cout << "Find(30):" << *itFind << endl;
	itFind--;
	cout << "Find(30)--:" << *itFind << endl;

	itFind = container.begin();//find(container.begin(), container.end(), 10);

	itFind++;
	itFind++;
	itFind++;
	itFind++;
	cout << "Find(10)+4:" << *itFind << endl;

	auto itErase = container.begin();
	container.erase(itErase);
	cout << "PrintPtr[Erase]:";
	for (it = container.begin(); it != container.end(); it++)
		cout << "[" << (int)&*it << "]" << *it << ",";
	cout << endl;

	container.push_back(40);
	cout << "PrintPtr[insert]:";
	for (it = container.begin(); it != container.end(); it++)
		cout << "[" << (int)&*it << "]" << *it << ",";
	cout << endl;

	container.clear(); //��λ���
	cout << "Clear:";
	for (it = container.begin(); it != container.end(); it++)
		cout << "[" << &*it << "]" << *it << ",";
	cout << endl;
}

int main()
{
	ListMain();
	Fake::ListMain();
	return 0;
}