using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dynamic : MonoBehaviour
{
    public float JumpPower;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.RightArrow))
            transform.position += Vector3.right * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftArrow))
            transform.position += Vector3.left * Time.deltaTime;

        //if (Input.GetKey(KeyCode.Space))//Á¡ÇÁ°¡ ·ÎÄ¹¹ß»ç¿Í ºñ½ÁÇÑ ´À³¦ÀÌ ´À²¸Áü //3.5
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            GetComponent<Rigidbody2D>().AddForce(Vector3.up * JumpPower);
        }
    }
}
