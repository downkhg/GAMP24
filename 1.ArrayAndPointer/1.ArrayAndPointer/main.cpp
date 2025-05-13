#include <stdio.h>
//문제: 데이터의 값이 전위연산자 후위연산자의 차이점을 확인하는 프로그램
//후위연산자: 변수뒤에 붙는 증감연산자, 전위연산자: 변수앞에 붙는 연산자
//(첫번째 변수)에 값을 넣고, (두번째변수)에 연산자의 증감값을 대입하고, 각 변수의 값을 출력하여 연산결과를 확인한다.
//데이터: 첫번째변수, 두번째변수
//알고리즘:
//1. 첫번째변수와 두번째변수를 선언한다. -> 정수형 변수로 첫번째와 두번째 변수를 만든다. -> 이름은 nDataA, nDataB라고한다. ->n: 정수형. DataA 첫번째 데이터, DataB 두번째 데이터
//2. 첫번째변수에 후위연산을 하고 그값을 두번째변수에 넣는다.
//3. 첫번째변수와 두번째변수의 값을 출력하여 결과를 확인한다.
//4. 첫번재변수에 전위연산을 하고 그값을 두번째 변수에 넣는다.
//5. 첫번째변수와 두번째변수를 출력한다.
//void PrefixApostfixOperatorsMain()
//{
//	int nDataA = 0, nDataB = 0;
//	nDataB = nDataA++; //후위연산자는 값을 대입 후 값이 증가한다.
//	printf("DataA:%d, DataB:%d\n", nDataA, nDataB); //알고리즘을 출력할수있는 것까지 첫번재로 코딩이 끝났다면, 출력을 해서 정상작동을 하는지 확인한다.
//	nDataB = ++nDataA;
//	printf("DataA:%d, DataB:%d\n", nDataA, nDataB);
//}
void PrefixApostfixOperatorsMain()
{
	int nDataA = 0, nDataB = 0;
	nDataB = nDataA++; //후위연산자는 값을 대입 후 값이 증가한다.
	printf("DataA:%d, DataB:%d\n", nDataA, nDataB); //알고리즘을 출력할수있는 것까지 첫번재로 코딩이 끝났다면, 출력을 해서 정상작동을 하는지 확인한다.
	nDataB = ++nDataA;
	printf("DataA:%d, DataB:%d\n", nDataA, nDataB);
}
//문제: 포인터와 변수의 관계를 이해하고 확인하는 프로그램 
//포인터: 변수의 주소값을 저장하는 변수 (&변수: 주소값 가져오기)
//포인터를 출력 -> &변수의 값을 출력하면 주소값이 나온다. ->%x: 16진수,%d: 10진수
//변수를 만들어 포인터변수에 저장한다. 
// 포인터변수에서 주소값과 참조하는 값을 가져와 출력하여 결과를 확인한다. -> 포인터변수에서 참조하는 방법 -> 
// 포인터선언: int* pDataA = &nData -> 포인터변수가 참조하는 변수의 값에 접근할때:*pData
//데이터: 정수형 값을 저장하는 변수., 변수의 주소값을 저장하는 변수 -> int nDataA, int* pDataA
//알고리즘: 값을 저장하는 변수의 주소값을 포인터에 저장한다. 
//값을 저장하는 변수의 값과 주소값을 출력한다.
//포인터의 참조하는 변수의 값과 포인터의 값을 출력한다.

void PointerAndValueCheckMain()
{
	printf("PointerAndValueCheckMain\n");
	int nDataA;
	int* pDataA;

	pDataA = &nDataA;

	printf("DataA:%d/%x\n", nDataA, &nDataA);
	printf("pDataA:%d/%x\n", *pDataA, pDataA);
}

void main()
{
	//PrefixApostfixOperatorsMain();
	PointerAndValueCheckMain();
}