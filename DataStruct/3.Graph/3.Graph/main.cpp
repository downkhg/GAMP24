/*##################################
����Ʈ��(C��� ������) - Modified for BFS Pathfinding
���ϸ�: BinaryTree.cpp
�ۼ��� : ��ȫ��(downkhg@gmail.com)
������ : AI Assistant
������������¥ : 2025.06.04
���� : 1.02
###################################*/
#include <stdio.h>
#include <list>
#include <vector>
#include <queue>
#include <stack>
#include <string> // std::string ��뿡 �ʿ�
#include <iostream> // cout ��� �� �ʿ� (���� ����)

using namespace std;

namespace LinkedList
{
	// BFS ��� Ž���� ���� pPre�� distance�� �����ϵ��� ������ SNode ����ü
	struct SNode {
		char cData;
		SNode* pPre;
		int distance;
		list<SNode*> listAdj;
	};

	SNode* CreateNode(char data)
	{
		SNode* pTemp = new SNode;
		pTemp->cData = data;
		pTemp->pPre = NULL; // pPre �ʱ�ȭ
		pTemp->distance = -1;     // distance �ʱ�ȭ (-1�� ���Ѵ�/�̹湮�� �ǹ��� �� ����)
		return pTemp;
	};

	bool MakeAdjacency(SNode* pParent, SNode* pChild) // �Ű������� ����
	{
		if (pParent == NULL)
			return false;
		pParent->listAdj.push_back(pChild);
		return true;
	};

	// ���ο� SNode ����� �����ϵ��� ������ TraverReset
	void TraverReset(SNode* pNodes[], int size)
	{
		for (int i = 0; i < size; i++)
		{
			if (pNodes[i] != NULL) 
			{ // �迭�� ������ ä������ ���� ��� null ������ Ȯ��
				pNodes[i]->distance = -1; // -1�� ��� Ž������ �̹湮/���Ѵ븦 �ǹ�
			}
		}
	}

	// �ܰ躰 ����� �����ϴ� ���ο� BFS ��� Ž�� �Լ�
	void BFS_FindPathWithSteps(SNode* pStartNode, char targetChar) {
		if (!pStartNode) {
			printf("���� ��尡 NULL�Դϴ�.\n");
			return;
		}

		printf("\nBFS Pathfinding from %c to %c (with steps):\n", pStartNode->cData, targetChar);

		queue<SNode*> queBFS;

		pStartNode->distance = 0;
		// ���⼭�� pStartNode->bVisit�� ������ �ʿ� ����, distance != -1�� �ֿ� Ȯ�� ������.
		queBFS.push(pStartNode);

		SNode* pTargetNodeReached = NULL;

		while (!queBFS.empty()) //T
		{
			SNode* pFront = queBFS.front();
			queBFS.pop();

			if (pTargetNodeReached != NULL && pFront->distance > pTargetNodeReached->distance) {
				// ����ȭ: ��ǥ�� ã�Ҵٸ� �� �� �Ÿ��� ��带 Ž���� �ʿ� ����.
				break;
			}

			string exploration_line_intro = "";
			exploration_line_intro += pFront->cData;
			exploration_line_intro += " -> ";

			string exploration_details = "";
			bool first_neighbor_segment = true;
			bool new_discovery_this_level = false;
			int current_level_cost = pFront->distance + 1;

			for (SNode* pAdj : pFront->listAdj) {
				string segment = "";
				segment += pAdj->cData;
				segment += " 1"; // ���� ����� 1

				if (pAdj->distance == -1) { // �̿� v�� (�Ÿ��� ����) �湮���� �ʾҴٸ�
					pAdj->distance = current_level_cost;
					pAdj->pPre = pFront;
					queBFS.push(pAdj);
					new_discovery_this_level = true;
					// ���ο� �߰߿� ���ؼ��� "X" ǥ�� ����
					if (pAdj->cData == targetChar) {
						if (pTargetNodeReached == NULL || pAdj->distance < pTargetNodeReached->distance) {
							pTargetNodeReached = pAdj;
						}
					}
				}
				else { // �̿� v�� �̹� �湮�Ǿ��ų� ���ų� �� ª�� ��θ� ���� �̹� ť�� ����
					segment += " X";
				}

				if (first_neighbor_segment == false) {
					exploration_details += ", ";
				}
				exploration_details += segment;
				first_neighbor_segment = false;
			}

			if (exploration_details.empty() == false) { // u�� Ž���� �̿��� ������ �־��ٸ�
				printf("%s%s", exploration_line_intro.c_str(), exploration_details.c_str());
				if (new_discovery_this_level) { // �� �������� ���ο� ��尡 �߰ߵ� ��� "=> ���" �߰�
					printf(" => %d", current_level_cost);
				}
				printf("\n");
			}

			if (pTargetNodeReached != NULL && pFront == pTargetNodeReached) {
				// u�� �ڽ� ��带 Ž���Ͽ� ��ǥ�� ã�Ұ�, u�� ���� ������ �ٸ� ��峪 u ��ü�� ��ǥ�� ���
				// u�� �ڽ� ��� ó�� �� �� break ������ �ߴܽ�Ŵ (�ٸ� ��ο� ���õ� �ڽ��� �ִ� ��� ����)
				break;
			}
		}

		// ��� �籸�� �� ���
		if (pTargetNodeReached) {
			printf("\nShortest path to %c is: ", targetChar);
			stack<SNode*> pathStack;
			SNode* pCrawler = pTargetNodeReached;
			while (pCrawler != NULL) { // pPre�� ����Ͽ� ������
				pathStack.push(pCrawler);
				pCrawler = pCrawler->pPre;
			}

			bool first_in_path = true;
			while (!pathStack.empty()) {
				if (!first_in_path) {
					printf(" -> ");
				}
				printf("%c", pathStack.top()->cData);
				pathStack.pop();
				first_in_path = false;
			}
			printf("\nTotal cost: %d\n", pTargetNodeReached->distance);
		}
		else {
			printf("\n%c���� %c���� ��θ� ã�� ���߽��ϴ�.\n", pStartNode->cData, targetChar);
		}
	}


	enum E_NODE { A, B, C, D, E, F, G, H, MAX };
	void main()
	{
		SNode* pNodes[MAX];
		char cNodeData = 'A';
		for (int i = 0; i < MAX; i++)
		{
			pNodes[i] = CreateNode(cNodeData);
			cNodeData++;
		}

		MakeAdjacency(pNodes[A], pNodes[B]);
		MakeAdjacency(pNodes[B], pNodes[D]);
		MakeAdjacency(pNodes[B], pNodes[F]);
		MakeAdjacency(pNodes[C], pNodes[B]); // C -> B
		MakeAdjacency(pNodes[D], pNodes[H]);
		MakeAdjacency(pNodes[E], pNodes[C]); // E -> C
		MakeAdjacency(pNodes[E], pNodes[H]);
		MakeAdjacency(pNodes[F], pNodes[D]);
		MakeAdjacency(pNodes[F], pNodes[G]);
		MakeAdjacency(pNodes[F], pNodes[H]);
		MakeAdjacency(pNodes[G], pNodes[E]);
		MakeAdjacency(pNodes[G], pNodes[F]);
		MakeAdjacency(pNodes[G], pNodes[H]);

		// ���ο� BFS ��� Ž�� �Լ� ȣ��
		BFS_FindPathWithSteps(pNodes[A], 'C'); // A���� C������ ��� Ž��
		TraverReset(pNodes, MAX); // ���� ������ ���� �ʱ�ȭ

		// �޸� ���� (�� ���������� ���� ���������� ���� ������)
		for (int i = 0; i < MAX; i++) {
			delete pNodes[i];
			pNodes[i] = NULL;
		}
	}
} // LinkedList ���ӽ����̽� ��

void main()
{
	LinkedList::main();
}