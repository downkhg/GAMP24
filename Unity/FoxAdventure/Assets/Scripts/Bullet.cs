using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Player playerMaster;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Attack()
    {
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        Collider2D collision = Physics2D.OverlapCircle(this.transform.position, circleCollider.radius, 1 << LayerMask.NameToLayer("Monster"));

        if (collision)
        {
            Player target = collision.gameObject.GetComponent<Player>();

            if (target && playerMaster)
            {
                SuperMode superMode = collision.gameObject.GetComponent<SuperMode>();
                if (superMode != null)
                {
                    if (superMode.isUse == false)
                    {
                        playerMaster.Attack(target);
                        superMode.Use();
                        Debug.Log("Attack!");
                        if (target.Death())
                        {
                            playerMaster.StillExp(target);
                            playerMaster.LvUp();
                            Destroy(collision.gameObject);
                        }
                        Destroy(gameObject);
                    }
                }
            };
        }
    }

    private void FixedUpdate()
    {
        Attack();
    }
}
