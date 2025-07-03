using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject prefabBullet;
    public float shotPower;
    public Transform trMozzle;

    public void Shot(Player player)
    {
        //objBullet.GetComponent<Rigidbody2D>().AddForce(Vector3.right * shotPower);
        GameObject objBullet = Instantiate(prefabBullet, trMozzle.position, Quaternion.identity);
        objBullet.GetComponent<Rigidbody2D>().AddForce(Vector3.right * shotPower);
        Bullet bullet = objBullet.GetComponent<Bullet>();
        bullet.playerMaster = player;
        Destroy(objBullet, 1);
    }
}
