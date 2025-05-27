/*##################################
���Ͽ��Ḯ��Ʈ(������)
���ϸ�: LinkedList_empty.cpp
�ۼ���: ��ȫ�� (downkhg@gmail.com)
������������¥: 2019.04.12
����: 1.10
###################################*/

#include <stdio.h>
#include <stdlib.h> //�޸� �����Ҵ� ���
#include <crtdbg.h> //�޸� ���� Ž�� ���
//#include  "linkedlistClass.h"

struct SNode {
	int nData;
	SNode* pNext;
};

SNode* CreateNode(SNode* pNode, int data); //��带 �����Ͽ� �����Ѵ�.
SNode* FindNodeData(SNode* pStart, int data); //�ش� �����͸� ���� ��带 ã�´�.
SNode* InsertNodeData(SNode* pStart, int data, int insert); //�ش� �����͸� ���� ��� �ڿ� ��带 �߰��Ѵ�.
void DeleteNodeData(SNode* pStart, int del); //�ش絥���͸� ���� ��带 �����Ѵ�.
void PrintLinkedList(SNode* pStart); //��带 ��ȸ�ϸ� ���������� ����Ѵ�.
void DeleteLinkedList(SNode* pStart); //��带 ��ȸ�ϸ� ��絥���͸� �����Ѵ�.
SNode* ReverseLinkedList(SNode* &pStart); //
//SNode* ReverseLinkedList(SNode** pStart); //
//���Ḯ��Ʈ �������� �Է¹ޱ�.(�����Ҵ� �����)
void InputAdd();

//�����۵� �׽�Ʈ�� ���ؼ�, ������ ���� �⺻���� ������ ������ Ȯ���Ѵ�.
//�� �ҽ��� ��� ���װ� �����Ѵ�.
//�� �ڵ尡 �����۵� �� �� �߰��غ���!
//main()�Լ� �� �ڵ�� �߰��� ���������� ������ ��������!
void main()
{
	_CrtSetBreakAlloc(84); //�޸� ������ ��ȣ�� ������ �Ҵ��ϴ� ��ġ�� �극��ũ ����Ʈ�� �Ǵ�.
	_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF); //�޸� ���� �˻� 

	SNode* pBegin = NULL;
	SNode* pEnd = NULL;

	//��� �߰� �׽�Ʈ
	pEnd = CreateNode(pEnd, 10);
	pBegin = pEnd; //������ ��带 �˾ƾ� �˻��� �����ϹǷ� �����صд�.

	pEnd = CreateNode(pEnd, 20);
	pEnd = CreateNode(pEnd, 30);
	pEnd = CreateNode(pEnd, 40);
	pEnd = CreateNode(pEnd, 50);

	PrintLinkedList(pBegin);

	SNode* pFind = FindNodeData(pBegin, 40);
	if (pFind != NULL)//0x04 == N -> !F -> T
		printf("Find:%d\n", pFind->nData);

	SNode* pInsert = InsertNodeData(pBegin, 30, 60);//��� ����
	if (pInsert != NULL)//0x06 == N -> !F -> T
		printf("Insert:%d\n", pInsert->nData);

	PrintLinkedList(pBegin);

	DeleteNodeData(pBegin, 60);//��� ����

	PrintLinkedList(pBegin);

	DeleteNodeData(pBegin, 10);//��� ����

	PrintLinkedList(pBegin);

	DeleteLinkedList(pBegin); //�������� - �� �Լ��� ȣ������������ �޸𸮰� ������.
}

//���⼭ ���� ����� �����Ѵ�.
//�����ڵ�� �մ�������, �߰��� �Ͽ� �� ���α׷� ���� �۵��ϵ����Ұ�.
SNode* CreateNode(SNode* pNode, int data)
{
	SNode* pTemp = NULL;

	pTemp = new SNode();
	pTemp->nData = data;
	if(pNode != NULL) //0x04 != NULL > T
		pNode->pNext = pTemp;
	//pTemp->pNext = pNode;
	return  pTemp;
}

SNode* FindNodeData(SNode* pStart, int data)
{
	SNode* pNode = pStart;
	//if (pStart->nData != data)
	//	pNode = pStart->pNext;
	//if (pNode->nData != data)
	//	pNode = pNode->pNext;
	//while (true) //���� ���̽��� �׽�Ʈ�Ѵ�.
	while (pNode != NULL)
	{
		if (pNode->nData != data)
			pNode = pNode->pNext;
		else
			break;
	}
	//if (pNode->nData == data)

	return pNode;
}

SNode* InsertNodeData(SNode* pStart, int data, int insert)
{
	SNode* pNode = pStart;
	SNode* pInsert = NULL;

	pNode = FindNodeData(pStart, data);

	pInsert = new SNode();
	pInsert->nData = insert;
	pInsert->pNext = pNode->pNext;
	pNode->pNext = pInsert;

	return pNode;
}

SNode* DeleteNodeData(SNode* &pStart, int del)
{
	SNode* pPre = NULL;
	SNode* pNode = pStart;

	//if (pNode->nData != del) //10
	//	pNode = pStart->pNext;
	//if (pNode->nData != del) //20
	//{
	//	pNode = pNode->pNext;
	//	pPre = pStart->pNext;
	//}
	while (pNode != NULL)
	{
		if (pNode->nData != del)//0x01->nData:10 != 10 // 10 == 10 -> T // !T //F
		{
			pPre = pNode;
			pNode = pNode->pNext;
		}
		else //if (pNode->nData == del)//60 == 60
		{
			if (*pStart == pNode)
				*pStart = pNode->pNext;

			if (pPre != NULL)//N == N -> T -> !T ->F
				pPre->pNext = pNode->pNext;

			delete pNode;
			break;
		}
	}
	//���� �ݺ��Ұ��� Ȯ���ϴٸ� ������ �ڵ带 ��Ȱ���ϰ� �Ϻ� �������Ǵºκп��� ������ �ʿ䰡 �ִ�.
	/*while (pNode != NULL)
	{
		if (pNode->nData != del)
			pNode = pNode->pNext;
		else
			break;
	}*/
}


SNode* DeleteNodeDataPtr(SNode** pStart, int del)
{
	SNode* pPre = NULL;
	SNode* pNode = *pStart;

	//if (pNode->nData != del) //10
	//	pNode = pStart->pNext;
	//if (pNode->nData != del) //20
	//{
	//	pNode = pNode->pNext;
	//	pPre = pStart->pNext;
	//}
	while (pNode != NULL)
	{
		if (pNode->nData != del)//0x01->nData:10 != 10 // 10 == 10 -> T // !T //F
		{
			pPre = pNode;
			pNode = pNode->pNext;
		}
		else //if (pNode->nData == del)//60 == 60
		{
			if (*pStart == pNode)
				*pStart = pNode->pNext;

			if (pPre != NULL)//N == N -> T -> !T ->F
				pPre->pNext = pNode->pNext;
			
			delete pNode;
			break;
		}
	}
	//���� �ݺ��Ұ��� Ȯ���ϴٸ� ������ �ڵ带 ��Ȱ���ϰ� �Ϻ� �������Ǵºκп��� ������ �ʿ䰡 �ִ�.
	/*while (pNode != NULL)
	{
		if (pNode->nData != del)
			pNode = pNode->pNext;
		else
			break;
	}*/
}

void PrintLinkedList(SNode* pStart)
{
	SNode* pNode = pStart;
	printf("data:");
	while (pNode)
	{
		printf("%d", pNode->nData);
		pNode = pNode->pNext; //�������� �̵��Ѵ�.

		if (pNode != NULL)
			printf(",");
	}
	printf("\n");
}

void DeleteLinkedList(SNode* pStart)
{
	SNode* pNode = pStart;
	SNode* pDel = NULL;

	while (pNode != NULL)
	{
		//pDel = pNode;
		pNode = pNode->pNext;
		delete pDel;
	}
}

void InputAdd()
{
	SNode* pStart = NULL;
	SNode* pNode = NULL;
	int nData = 0;

	//�����Ҵ��� �ϸ� ���α׷��� ����ڿ� ���ؼ� ���Ǵ� �޸𸮰� �����ȴ�.
	//���Ը��ؼ�, �����ϴܰ迡�� 100���� ����� ���ٸ�, 
	//��������ʴ��� 100���� �޸𸮸� ����Ҽ��ۿ�����.
	//�׸���, 100�� �̻��� �޸𸮵� ����Ҽ�����.
	//�׷���, �����Ҵ��� �ϸ� ����ڰ� �߰��� �޸𸮸�ŭ�� �޸𸮰� ���ǰ� 
	//�޸𸮿뷮�� ����ϴ� �� �߰��� �ȴ�.
	while (nData != -1)
	{
		scanf("%d", &nData);
		pNode = CreateNode(pNode, nData);

		if (pNode == NULL)
		{
			printf("�� �̻� ����Ҽ� �ִ� �޸𸮰� �����ϴ�!");
		}

		if (pStart == NULL)
			pStart = pNode;

		PrintLinkedList(pStart);
	}

	DeleteLinkedList(pStart); //�������� - �� �Լ��� ȣ������������ �޸𸮰� ������.
}