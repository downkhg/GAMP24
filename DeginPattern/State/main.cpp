#include <iostream>
#include "UnityAPI.h"

using namespace std;

int main()
{
	CUnityEditor cUnityEditor;
	cUnityEditor.SetScene(new CTitleScene());
	cUnityEditor.NextScene();
	cUnityEditor.NextScene();
	cUnityEditor.NextScene();

	return 0;
}