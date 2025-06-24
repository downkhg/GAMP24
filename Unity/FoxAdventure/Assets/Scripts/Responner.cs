using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Responner : MonoBehaviour
{
    public float maxTime; //�ִ�ð�
    public float curTime = -1; //����ð� //��Ƽ��: ������ ���� �����ؼ� ��Ȱ��ȭ ���¸� ����°�.

    public GameObject prefabPlayer; //������ �÷��̾��� ������
    public GameObject objPlayer; //���Ӿȿ��� ��� �ִ� �÷��̾�

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (curTime >= 0) //����Ÿ�� 0���� Ŭ�� //Ÿ�̸Ӱ� Ȱ��ȭ ������
        {
            if (curTime < maxTime) // ����Ÿ���� �ִ�ð����� ������
            {
                curTime += Time.deltaTime; //����ð��� ��ŸŸ�Ӱ��� ���Ѵ�.
            }
            else // ����Ÿ���� �ִ�ð����� �������� �ƴҶ� // ����ð��� �ִ�ð��� Ŭ��,
            {
                //�÷��̾��� �������� �����Ѵ�. ���������� ��ġ��, ȸ������ ����.
                objPlayer = Instantiate(prefabPlayer, transform.position, Quaternion.identity);
                //����ð��� -1�� �����Ѵ� //Ÿ�̸Ӹ� ��Ȱ��ȭ�Ѵ�.
                curTime = -1;
            }
        }
        else //����Ÿ�� 0���� Ŭ�� �ƴҶ� //����Ÿ���� 0���� ������ //���̳ʽ� //Ÿ�̸Ӱ� ��Ȱ��ȭ �����϶�
        {
            if (objPlayer == null) //�÷��̾ ������ //�÷��̾ ���������
            {
                curTime = 0; //Ÿ�̸ӿ� 0�ǰ��� �����Ѵ� //Ÿ�̸Ӹ� Ȱ��ȭ �Ѵ�.
            }
        }
    }
}
