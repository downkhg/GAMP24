/*##################################
이진트리(C언어 수업용)
파일명: BinaryTree.cpp
작성자 : 김홍규(downkhg@gmail.com)
마지막수정날짜 : 2022.03.04 (재귀적 힙 삽입 및 트리 구조 출력 추가 버전: 2025.05.29-Gemini 2.5)
버전 : 1.03
###################################*/
#include <stdio.h>
#include <queue>      // GetParent 함수에서 std::queue 사용
#include <algorithm>  // SwapNodeData에서 std::swap 사용 가능 (여기서는 수동 구현)
// #include <cmath>   // 만약 log, floor 등이 필요했다면 포함 (현재 로직에서는 불필요)

using namespace std; // 원본 코드 스타일 유지

struct SNode {
    int nData;
    SNode* pLeft;
    SNode* pRight;
};

SNode* CreateNode(int data)
{
    SNode* pTemp = new SNode;
    pTemp->nData = data;
    pTemp->pLeft = NULL;
    pTemp->pRight = NULL;
    return pTemp;
};
bool MakeLeft(SNode* pParent, SNode* pChild)
{
    if (pParent == NULL)
        return false;
    pParent->pLeft = pChild;
    return true;
};
bool MakeRight(SNode* pParent, SNode* pChild)
{
    if (pParent == NULL)
        return false;
    pParent->pRight = pChild;
    return true;
};

// 기존의 전위 순회 출력 함수
void Traverse(SNode* pNode)
{
    if (pNode == NULL) return;
    printf("%d\n", pNode->nData);
    Traverse(pNode->pLeft);
    Traverse(pNode->pRight);
}

void Print(SNode* pSeed)
{
    printf("Pre-order Traversal:\n");
    Traverse(pSeed);
    printf("-----\n");
}

// ##################################
// ##      힙 관련 보조 함수        ##
// ##################################

void SwapNodeData(SNode* node1, SNode* node2) {
    if (node1 && node2) {
        int temp = node1->nData;
        node1->nData = node2->nData;
        node2->nData = temp;
    }
}

SNode* GetParent(SNode* root, SNode* node) {
    if (root == NULL || node == NULL || root == node) {
        return NULL;
    }
    std::queue<SNode*> q;
    q.push(root);
    while (!q.empty()) {
        SNode* current = q.front();
        q.pop();
        if (current->pLeft == node || current->pRight == node) {
            return current;
        }
        if (current->pLeft != NULL) {
            q.push(current->pLeft);
        }
        if (current->pRight != NULL) {
            q.push(current->pRight);
        }
    }
    return NULL;
}

void HeapifyUp(SNode* root, SNode* nodeToHeapify) {
    if (nodeToHeapify == NULL || nodeToHeapify == root) {
        return;
    }
    SNode* current = nodeToHeapify;
    SNode* parent = GetParent(root, current);
    while (parent != NULL && current->nData < parent->nData) {
        SwapNodeData(current, parent);
        current = parent;
        if (current == root) break;
        parent = GetParent(root, current);
    }
}

// ####################################################
// ##  재귀적 힙 삽입을 위한 함수 및 보조 함수   ##
// ####################################################

int countTreeNodes(SNode* node) {
    if (node == NULL) {
        return 0;
    }
    return 1 + countTreeNodes(node->pLeft) + countTreeNodes(node->pRight);
}

void find_parent_and_attach_recursive(SNode* current_node,
    int target_parent_idx,
    int current_node_idx,
    int new_node_final_idx,
    SNode* new_node_to_insert) {
    if (current_node == NULL) {
        return;
    }
    if (current_node_idx == target_parent_idx) {
        if (new_node_final_idx == (2 * current_node_idx + 1)) {
            MakeLeft(current_node, new_node_to_insert);
        }
        else {
            MakeRight(current_node, new_node_to_insert);
        }
        return;
    }
    int left_child_idx = 2 * current_node_idx + 1;
    int right_child_idx = 2 * current_node_idx + 2;
    int path_determining_child_idx = target_parent_idx;
    if (path_determining_child_idx == 0 && current_node_idx != 0) {
        return;
    }
    while (path_determining_child_idx != 0 && (path_determining_child_idx - 1) / 2 != current_node_idx) {
        path_determining_child_idx = (path_determining_child_idx - 1) / 2;
        if (path_determining_child_idx == 0 && current_node_idx != 0) return;
    }

    if (path_determining_child_idx == left_child_idx) {
        if (current_node->pLeft != NULL) {
            find_parent_and_attach_recursive(current_node->pLeft, target_parent_idx, left_child_idx, new_node_final_idx, new_node_to_insert);
        }
    }
    else if (path_determining_child_idx == right_child_idx) {
        if (current_node->pRight != NULL) {
            find_parent_and_attach_recursive(current_node->pRight, target_parent_idx, right_child_idx, new_node_final_idx, new_node_to_insert);
        }
    }
}

void InsertMinHeapRecursive(SNode** pRoot, int data) {
    SNode* newNode = CreateNode(data);
    if (*pRoot == NULL) {
        *pRoot = newNode;
        return;
    }
    int n_nodes_before_insertion = countTreeNodes(*pRoot);
    int new_node_target_idx = n_nodes_before_insertion;
    if (new_node_target_idx > 0) {
        int parent_idx = (new_node_target_idx - 1) / 2;
        find_parent_and_attach_recursive(*pRoot, parent_idx, 0, new_node_target_idx, newNode);
    }
    else {
        if (*pRoot == NULL) *pRoot = newNode;
    }
    HeapifyUp(*pRoot, newNode);
}

// ##################################
// ##    트리 구조 출력 함수 추가     ##
// ##################################
#define LEVEL_INDENT_SPACES 5 // 각 레벨 당 들여쓰기할 공백 수

// 트리를 시각적으로 출력하는 재귀 헬퍼 함수
// node: 현재 방문 중인 노드
// level: 현재 노드의 깊이 (루트는 0)
void printTreeStructureRecursive(SNode* node, int level) {
    if (node == NULL) {
        return;
    }

    // 1. 오른쪽 자식 먼저 재귀 호출 (오른쪽 자식이 시각적으로 위에 표시됨)
    printTreeStructureRecursive(node->pRight, level + 1);

    // 2. 현재 노드 출력 (들여쓰기 포함)
    for (int i = 0; i < level; ++i) {
        for (int j = 0; j < LEVEL_INDENT_SPACES; ++j) {
            printf(" "); // 각 레벨마다 정해진 수의 공백 출력
        }
    }
    printf("%d\n", node->nData);

    // 3. 왼쪽 자식 재귀 호출 (왼쪽 자식이 시각적으로 아래에 표시됨)
    printTreeStructureRecursive(node->pLeft, level + 1);
}

// 트리 구조 출력을 위한 래퍼 함수
void PrintTreeStructure(SNode* root) {
    printf("\nVisual Tree Structure (Root on left, tree grows to the right; right children above, left children below):\n");
    if (root == NULL) {
        printf("Tree is empty.\n");
    }
    else {
        printTreeStructureRecursive(root, 0);
    }
    printf("-----------------------------------------------------------------------------------------------\n");
}


// ##################################
// ##        main 함수             ##
// ##################################
int main()
{
    SNode* heapRoot = NULL;

    printf("Building Min-Heap using RECURSIVE insert:\n");

    InsertMinHeapRecursive(&heapRoot, 100);
    Print(heapRoot); // 전위 순회 출력
    InsertMinHeapRecursive(&heapRoot, 19);
    Print(heapRoot);
    InsertMinHeapRecursive(&heapRoot, 36);
    Print(heapRoot);
    InsertMinHeapRecursive(&heapRoot, 17);
    Print(heapRoot);
    InsertMinHeapRecursive(&heapRoot, 3);
    Print(heapRoot);
    InsertMinHeapRecursive(&heapRoot, 25);
    Print(heapRoot);
    InsertMinHeapRecursive(&heapRoot, 1);
    Print(heapRoot);

    printf("\nFinal Min-Heap (Pre-order Traversal):\n");
    Print(heapRoot); // 최종 힙 전위 순회

    // 새로 추가된 트리 구조 출력 함수 호출
    PrintTreeStructure(heapRoot);

    return 0;
}