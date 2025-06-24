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

    public float maxTime = 1; //최대시간
    IEnumerator ProccssTimmer()
    {
        yield return new WaitForSeconds(maxTime);
        isJump = true;
    }

    void SetTimmer()
    {
        StartCoroutine(ProccssTimmer());
    }

    // Start is called before the first frame update
    void Start()
    {
        SetTimmer();
    }

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
        //UpdateTimmer();

        if(isJumpping == false && isJump == false)
        {
            SetTimmer();
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
        Debug.Log($"OnCollisionEnter2D:{collision.gameObject.name}");
    }
}
