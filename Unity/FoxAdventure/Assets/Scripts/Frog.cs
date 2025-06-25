using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Frog : MonoBehaviour
{
    public float Speed = 2;
    public bool isMove = false;

    public bool isJump = false;
    public float JumpPower = 300;
    public bool isJumpping = false;

    public float maxTime = 1; //최대시간
    public bool isTimmer;

    //public UnityEvent TimmerAction;

    public void StartJump() { isJump = true; isMove = true; }
    public void EndJump() { isJump = false; isMove = false; }

    IEnumerator ProccssTimmer()
    {
        isTimmer = true;
        yield return new WaitForSeconds(maxTime);
        //TimmerAction.Invoke();
        Jump();
        StartJump();
        //yield return new WaitForSeconds(maxTime);
        //isMove = true ;
        isTimmer = false;
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
        GetComponent<Rigidbody2D>().AddForce(Vector3.up * JumpPower);
    }

    // Update is called once per frame
    void Update()
    {
        if (isJump == false)
        {
            if (isTimmer == false)
            {
                SetTimmer();
            }
        }

        Move();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        EndJump();
        Debug.Log($"OnCollisionEnter2D:{collision.gameObject.name}");
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log($"OnCollisionEnter2D:{collision.gameObject.name}");
    }
}
