using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject prefabBullet;
    public float ShotPower;
    public Transform trMozzle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            GameObject objBullet = Instantiate(prefabBullet,trMozzle.position,Quaternion.identity);
            Rigidbody2D rigidbody = objBullet.GetComponent<Rigidbody2D>();
            if (rigidbody != null)
            {
                rigidbody.AddForce(Vector3.right * ShotPower);
                Destroy(objBullet,1);
            }
        }
    }
}
