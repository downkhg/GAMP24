using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dynamic : MonoBehaviour
{
    public float JumpPower;
    public float Speed;
    public int Score;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //이동방향에 이동속도(량)을 추가하여 플레이어의 이동속도를 조절한다.
        if(Input.GetKey(KeyCode.RightArrow))
            transform.position += Vector3.right * Speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftArrow))
            transform.position += Vector3.left * Speed * Time.deltaTime;

        //if (Input.GetKey(KeyCode.Space))//점프가 로캣발사와 비슷한 느낌이 느껴짐 //3.5
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            GetComponent<Rigidbody2D>().AddForce(Vector3.up * JumpPower);
        }
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(0, 0, 100, 20), $"Score:{Score}");
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    Debug.Log($"OnCollisionEnter2D:{collision.gameObject.name}");
    //    if (collision.gameObject.name == "cherry")
    //    {
    //        Score++;
    //        //Destroy(gameObject); //플레이어 자기자신의 게임오브젝트를 삭제함 //함정
    //        Destroy(collision.gameObject); //플레이어가 아이템을 먹으면 아이템이 사라진다. //아이템?
    //    }
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"OnTriggerEnter2D:{collision.gameObject.tag}");
        if (collision.gameObject.tag == "Item")
        {
            Score++;
            //Destroy(gameObject); //플레이어 자기자신의 게임오브젝트를 삭제함 //함정
            Destroy(collision.gameObject); //플레이어가 아이템을 먹으면 아이템이 사라진다. //아이템?
        }
    }
}
