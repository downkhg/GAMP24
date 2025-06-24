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

    public float maxTime = 1; //최대시간
    public float curTime = -1; //현재시간 //센티넬: 음수의 값을 설정해서 비활성화 상태를 만드는것.

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
        if (curTime >= 0) //현재타임 0보다 클때 //타이머가 활성화 됬을때
        {
            if (curTime < maxTime) // 현재타임이 최대시간보다 작을때
            {
                curTime += Time.deltaTime; //현재시간에 델타타임값을 더한다.
            }
            else // 현재타임이 최대시간보다 작을때가 아닐때 // 현재시간이 최대시간값 클때,
            {
                isJump = true;
                //현재시간에 -1을 셋팅한다 //타이머를 비활성화한다.
                curTime = -1;
            }
        }
        else //현재타임 0보다 클때 아닐때 //현재타임이 0보다 작을때 //마이너스 //타이머가 비활성화 상태일때
        {
            if(isJumpping == false)
                curTime = 0; //타이머에 0의값을 설정한다 //타이머를 활성화 한다.
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
