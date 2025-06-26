using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eagle : MonoBehaviour
{
    public GameObject objTarget;
    public float Speed = 2;

    public Transform trResponPoint;
    // Start is called before the first frame update
    void Start()
    {
        //Time.timeScale = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (objTarget != null)
        {
            Vector3 vPos = this.transform.position;
            Vector3 vTargetPos = objTarget.transform.position;
            Vector3 vDist = vTargetPos - vPos;
            Vector3 vDir = vDist.normalized;
            float fDist = vDist.magnitude;

            Debug.DrawLine(vPos, vTargetPos, Color.red);

            if (fDist >= Time.deltaTime)
                transform.position += vDir * Speed * Time.deltaTime;
        }
        else
            objTarget = trResponPoint.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
            objTarget = collision.gameObject;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
            Destroy(collision.gameObject);
    }
}
