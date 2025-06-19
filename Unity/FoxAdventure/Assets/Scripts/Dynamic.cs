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
        //�̵����⿡ �̵��ӵ�(��)�� �߰��Ͽ� �÷��̾��� �̵��ӵ��� �����Ѵ�.
        if(Input.GetKey(KeyCode.RightArrow))
            transform.position += Vector3.right * Speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftArrow))
            transform.position += Vector3.left * Speed * Time.deltaTime;

        //if (Input.GetKey(KeyCode.Space))//������ ��Ĺ�߻�� ����� ������ ������ //3.5
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
    //        //Destroy(gameObject); //�÷��̾� �ڱ��ڽ��� ���ӿ�����Ʈ�� ������ //����
    //        Destroy(collision.gameObject); //�÷��̾ �������� ������ �������� �������. //������?
    //    }
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"OnTriggerEnter2D:{collision.gameObject.tag}");
        if (collision.gameObject.tag == "Item")
        {
            Score++;
            //Destroy(gameObject); //�÷��̾� �ڱ��ڽ��� ���ӿ�����Ʈ�� ������ //����
            Destroy(collision.gameObject); //�÷��̾ �������� ������ �������� �������. //������?
        }
    }
}
