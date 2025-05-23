#include <stdio.h>

void Swap(int& a, int& b)
{
	int temp = a;
	a = b;
	b = temp;
}

void InsertSort()
{
	int arrData[] = { 8,1,7,4,2,3,9,5,6 };
	//int arrData[] = { 1,8,7,4,2,3,9,5,6 };
	//-------------------------------
	//배열을 정렬되도록 로직을 짜기
	int idx = 0;
	while (idx < 9)//1
	{
		int minidx = idx;
		for (int selidx = idx + 1; selidx < 9; selidx++)
		{
			//arr[1]:8 >  arr[2]:7 //T
			printf("[%d]:%d <-> [%d]:%d -> (%d)\n", minidx, arrData[minidx], selidx, arrData[selidx], (arrData[minidx] > arrData[selidx]));
			if (arrData[minidx] > arrData[selidx])
			{
				minidx = selidx;
			}
		}
		Swap(arrData[minidx], arrData[idx]);

		printf("Data[%d]:",idx);
		for (int i = 0; i < 9; i++)
			printf("%d,",arrData[i]);
		printf("\n");

		idx++;
	}

	//-------------------------------

	//다음출력에서 1~9까지 순서대로 정렬되도록 만들기
	printf("Data:");
	for (int i = 0; i < 9; i++)
		printf("%d",arrData[i]);
	printf("\n");
}

void main()
{
	InsertSort();
}