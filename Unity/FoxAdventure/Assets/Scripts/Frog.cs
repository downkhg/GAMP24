using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frog : MonoBehaviour
{
    public float Speed = 2;
    public bool isMove = false;

    public bool isGround = false;
    public float JumpPower = 300;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isGround)
        {
            GetComponent<Rigidbody2D>().AddForce(Vector3.up * JumpPower);
        }


        if (isMove)
            transform.position += Vector3.left * Speed * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"OnCollisionEnter2D:{collision.gameObject.name}");
        isGround = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log($"OnCollisionEnter2D:{collision.gameObject.name}");
        isGround = false;
    }
}
