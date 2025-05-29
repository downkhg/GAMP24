/*##################################
����Ʈ��(C��� ������)
���ϸ�: BinaryTree.cpp
�ۼ��� : ��ȫ��(downkhg@gmail.com)
������������¥ : 2022.03.04 (����� �� ���� �� Ʈ�� ���� ��� �߰� ����: 2025.05.29-Gemini 2.5)
���� : 1.03
###################################*/
#include <stdio.h>
#include <queue>      // GetParent �Լ����� std::queue ���
#include <algorithm>  // SwapNodeData���� std::swap ��� ���� (���⼭�� ���� ����)
// #include <cmath>   // ���� log, floor ���� �ʿ��ߴٸ� ���� (���� ���������� ���ʿ�)

using namespace std; // ���� �ڵ� ��Ÿ�� ����

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

// ������ ���� ��ȸ ��� �Լ�
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
// ##      �� ���� ���� �Լ�        ##
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
// ##  ����� �� ������ ���� �Լ� �� ���� �Լ�   ##
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
// ##    Ʈ�� ���� ��� �Լ� �߰�     ##
// ##################################
#define LEVEL_INDENT_SPACES 5 // �� ���� �� �鿩������ ���� ��

// Ʈ���� �ð������� ����ϴ� ��� ���� �Լ�
// node: ���� �湮 ���� ���
// level: ���� ����� ���� (��Ʈ�� 0)
void printTreeStructureRecursive(SNode* node, int level) {
    if (node == NULL) {
        return;
    }

    // 1. ������ �ڽ� ���� ��� ȣ�� (������ �ڽ��� �ð������� ���� ǥ�õ�)
    printTreeStructureRecursive(node->pRight, level + 1);

    // 2. ���� ��� ��� (�鿩���� ����)
    for (int i = 0; i < level; ++i) {
        for (int j = 0; j < LEVEL_INDENT_SPACES; ++j) {
            printf(" "); // �� �������� ������ ���� ���� ���
        }
    }
    printf("%d\n", node->nData);

    // 3. ���� �ڽ� ��� ȣ�� (���� �ڽ��� �ð������� �Ʒ��� ǥ�õ�)
    printTreeStructureRecursive(node->pLeft, level + 1);
}

// Ʈ�� ���� ����� ���� ���� �Լ�
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
// ##        main �Լ�             ##
// ##################################
int main()
{
    SNode* heapRoot = NULL;

    printf("Building Min-Heap using RECURSIVE insert:\n");

    InsertMinHeapRecursive(&heapRoot, 100);
    Print(heapRoot); // ���� ��ȸ ���
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
    Print(heapRoot); // ���� �� ���� ��ȸ

    // ���� �߰��� Ʈ�� ���� ��� �Լ� ȣ��
    PrintTreeStructure(heapRoot);

    return 0;
}