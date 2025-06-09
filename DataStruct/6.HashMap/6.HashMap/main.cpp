/*##################################
OpenSSL해시함수(C언어 수업용)
파일명: hashfunction.cpp
작성자 : 김홍규(downkhg@gmail.com)
마지막수정날짜 : 2022.03.15
버전 : 1.01
###################################*/
#include <stdio.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <openssl/md5.h> //OpenSSL라이브러리를 설정해야 사용할수있다.(라이브러리설치방법은 구글링-Openssl_1.1.1d) 
#include <iostream>
#include <map>

using namespace std;

unsigned char result[MD5_DIGEST_LENGTH];

// Print the MD5 sum as hex-digits.
void print_md5_sum(unsigned char* md) {
    int i;
    for (i = 0; i < MD5_DIGEST_LENGTH; i++) {
        printf("%02x", md[i]);
    }
    printf("\n");
}

void DM5TestMain()
{
    const int SIZE = 1024;
    unsigned char buffer[SIZE];
    unsigned char dm5_buffer[SIZE];

    for (int i = 0; i < SIZE; i++)
        buffer[i] = i;
    memcpy_s(buffer, SIZE, "The quick brown fox jumps over the lazy dog.", SIZE);
    printf("%s\n", buffer);
    unsigned char* dm5_result = MD5((unsigned char*)buffer, SIZE, result);
    memcpy_s(dm5_buffer, SIZE, dm5_result, SIZE);

    print_md5_sum(dm5_buffer);
}

union HASHDATA
{
    unsigned int nData;
    unsigned char buffer[4];
    HASHDATA(int data = 0) { nData = data; }
};

HASHDATA DM5_HASHDATA(HASHDATA uData)
{
    HASHDATA uResult;

    unsigned char* dm5_result = MD5(uData.buffer, sizeof(HASHDATA), result);
    memcpy_s((void*)uResult.buffer, sizeof(HASHDATA), dm5_result, sizeof(HASHDATA));

    return uResult;
}

unsigned int DM5HashFucntion(unsigned int data)
{
    HASHDATA uData(data);
    HASHDATA uResult = DM5_HASHDATA(uData);
    return uResult.nData;
}

void UHASHDATATestMNain()
{
    HASHDATA uHashKey(2);
    print_md5_sum(uHashKey.buffer);
    HASHDATA uData = DM5_HASHDATA(uHashKey);
    print_md5_sum(uData.buffer);
    printf("[%u]:%u\n", uHashKey.nData, uData.nData);
}
//맵은 중복값을 저장하지않으므로 암화된 값을 가져와 저장하므로, 맵에 저장된 갯수는 중복 안된 갯수다.
void BirthDayTestMain()
{
    unsigned int SIZE = 99999;
    map<unsigned int, unsigned int> map;

    for (unsigned int i = 0; i < SIZE; i++)
    {
        unsigned int nHashValue = DM5HashFucntion(i);
        printf("[%u]:%u\n", nHashValue, i);
        map[nHashValue] = i;
        if (i % 1000 == 0)printf("%u\n", i);
    }

    printf("mapSize:%u\n", map.size());
    printf(":%u / %f\n", SIZE - map.size(), (float)(SIZE - map.size()) / (float)SIZE);
}

int main(int argc, char* argv[])
{
    //DM5TestMain();
    //UHASHDATATestMNain();
    BirthDayTestMain();

    return 0;
}