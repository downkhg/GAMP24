using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int Score = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"OnTriggerEnter2D[{collision.gameObject.tag}]:{collision.gameObject.name}");
        if (collision.gameObject.tag == "Player")
        {
            collision.GetComponent<Dynamic>().Score += Score;
            Destroy(gameObject);
        }
    }
}
