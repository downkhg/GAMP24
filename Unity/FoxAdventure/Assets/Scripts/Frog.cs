using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Frog : MonoBehaviour
{
    public float Speed = 2;
    public bool isMove = false;

    public bool isJump = false;
    public float JumpPower = 300;
    public bool isJumpping = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public float maxTime = 1; //�ִ�ð�
    public float curTime = -1; //����ð� //��Ƽ��: ������ ���� �����ؼ� ��Ȱ��ȭ ���¸� ����°�.

    public bool isGound;

    public void Move()
    {
        if (isMove)
            transform.position += Vector3.left * Speed * Time.deltaTime;
    }

    public void Jump()
    {
        if (isJump && isJumpping == false)
        {
            GetComponent<Rigidbody2D>().AddForce(Vector3.up * JumpPower);
            isJumpping = true;
            isMove = true;
        }
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
                isJump = true;
                //����ð��� -1�� �����Ѵ� //Ÿ�̸Ӹ� ��Ȱ��ȭ�Ѵ�.
                curTime = -1;
            }
        }
        else //����Ÿ�� 0���� Ŭ�� �ƴҶ� //����Ÿ���� 0���� ������ //���̳ʽ� //Ÿ�̸Ӱ� ��Ȱ��ȭ �����϶�
        {
            if(isJumpping == false)
                curTime = 0; //Ÿ�̸ӿ� 0�ǰ��� �����Ѵ� //Ÿ�̸Ӹ� Ȱ��ȭ �Ѵ�.
        }

        Jump();
        Move();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isJumpping = false;
        isJump = false;
        isMove = false;
        Debug.Log($"OnCollisionEnter2D:{collision.gameObject.name}");
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGound = false;
        Debug.Log($"OnCollisionEnter2D:{collision.gameObject.name}");
    }
}
