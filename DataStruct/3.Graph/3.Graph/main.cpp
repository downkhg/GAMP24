/*##################################
이진트리(C언어 수업용) - Modified for BFS Pathfinding
파일명: BinaryTree.cpp
작성자 : 김홍규(downkhg@gmail.com)
수정자 : AI Assistant
마지막수정날짜 : 2025.06.04
버전 : 1.02
###################################*/
#include <stdio.h>
#include <list>
#include <vector>
#include <queue>
#include <stack>
#include <string> // std::string 사용에 필요
#include <iostream> // cout 사용 시 필요 (선택 사항)

using namespace std;

namespace LinkedList
{
	// BFS 경로 탐색을 위해 pPre와 distance를 포함하도록 수정된 SNode 구조체
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
		pTemp->pPre = NULL; // pPre 초기화
		pTemp->distance = -1;     // distance 초기화 (-1은 무한대/미방문을 의미할 수 있음)
		return pTemp;
	};

	bool MakeAdjacency(SNode* pParent, SNode* pChild) // 매개변수명 수정
	{
		if (pParent == NULL)
			return false;
		pParent->listAdj.push_back(pChild);
		return true;
	};

	// 새로운 SNode 멤버를 포함하도록 수정된 TraverReset
	void TraverReset(SNode* pNodes[], int size)
	{
		for (int i = 0; i < size; i++)
		{
			if (pNodes[i] != NULL) 
			{ // 배열이 완전히 채워지지 않은 경우 null 포인터 확인
				pNodes[i]->distance = -1; // -1은 경로 탐색에서 미방문/무한대를 의미
			}
		}
	}

	// 단계별 출력을 포함하는 새로운 BFS 경로 탐색 함수
	void BFS_FindPathWithSteps(SNode* pStartNode, char targetChar) {
		if (!pStartNode) {
			printf("시작 노드가 NULL입니다.\n");
			return;
		}

		printf("\nBFS Pathfinding from %c to %c (with steps):\n", pStartNode->cData, targetChar);

		queue<SNode*> queBFS;

		pStartNode->distance = 0;
		// 여기서는 pStartNode->bVisit를 설정할 필요 없음, distance != -1이 주요 확인 조건임.
		queBFS.push(pStartNode);

		SNode* pTargetNodeReached = NULL;

		while (!queBFS.empty()) //T
		{
			SNode* pFront = queBFS.front();
			queBFS.pop();

			if (pTargetNodeReached != NULL && pFront->distance > pTargetNodeReached->distance) {
				// 최적화: 목표를 찾았다면 더 먼 거리의 노드를 탐색할 필요 없음.
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
				segment += " 1"; // 간선 비용은 1

				if (pAdj->distance == -1) { // 이웃 v가 (거리에 의해) 방문되지 않았다면
					pAdj->distance = current_level_cost;
					pAdj->pPre = pFront;
					queBFS.push(pAdj);
					new_discovery_this_level = true;
					// 새로운 발견에 대해서는 "X" 표시 없음
					if (pAdj->cData == targetChar) {
						if (pTargetNodeReached == NULL || pAdj->distance < pTargetNodeReached->distance) {
							pTargetNodeReached = pAdj;
						}
					}
				}
				else { // 이웃 v는 이미 방문되었거나 같거나 더 짧은 경로를 통해 이미 큐에 있음
					segment += " X";
				}

				if (first_neighbor_segment == false) {
					exploration_details += ", ";
				}
				exploration_details += segment;
				first_neighbor_segment = false;
			}

			if (exploration_details.empty() == false) { // u가 탐색할 이웃을 가지고 있었다면
				printf("%s%s", exploration_line_intro.c_str(), exploration_details.c_str());
				if (new_discovery_this_level) { // 이 레벨에서 새로운 노드가 발견된 경우 "=> 비용" 추가
					printf(" => %d", current_level_cost);
				}
				printf("\n");
			}

			if (pTargetNodeReached != NULL && pFront == pTargetNodeReached) {
				// u의 자식 노드를 탐색하여 목표를 찾았고, u와 같은 레벨의 다른 노드나 u 자체가 목표인 경우
				// u의 자식 노드 처리 후 이 break 조건이 중단시킴 (다른 경로와 관련된 자식이 있는 경우 중지)
				break;
			}
		}

		// 경로 재구성 및 출력
		if (pTargetNodeReached) {
			printf("\nShortest path to %c is: ", targetChar);
			stack<SNode*> pathStack;
			SNode* pCrawler = pTargetNodeReached;
			while (pCrawler != NULL) { // pPre를 사용하여 역추적
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
			printf("\n%c에서 %c로의 경로를 찾지 못했습니다.\n", pStartNode->cData, targetChar);
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

		// 새로운 BFS 경로 탐색 함수 호출
		BFS_FindPathWithSteps(pNodes[A], 'C'); // A에서 C까지의 경로 탐색
		TraverReset(pNodes, MAX); // 이후 연산을 위해 초기화

		// 메모리 정리 (이 예제에서는 선택 사항이지만 좋은 습관임)
		for (int i = 0; i < MAX; i++) {
			delete pNodes[i];
			pNodes[i] = NULL;
		}
	}
} // LinkedList 네임스페이스 끝

void main()
{
	LinkedList::main();
}