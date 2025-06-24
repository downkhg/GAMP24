using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Responner : MonoBehaviour
{
    public float maxTime; //최대시간
    public float curTime = -1; //현재시간 //센티넬: 음수의 값을 설정해서 비활성화 상태를 만드는것.

    public GameObject prefabPlayer; //복제할 플레이어의 프리팹
    public GameObject objPlayer; //게임안에서 살아 있는 플레이어

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (curTime >= 0) //현재타임 0보다 클때 //타이머가 활성화 됬을때
        {
            if (curTime < maxTime) // 현재타임이 최대시간보다 작을때
            {
                curTime += Time.deltaTime; //현재시간에 델타타임값을 더한다.
            }
            else // 현재타임이 최대시간보다 작을때가 아닐때 // 현재시간이 최대시간값 클때,
            {
                //플레이어의 프리팹을 복제한다. 리스포너의 위치에, 회전값은 없음.
                objPlayer = Instantiate(prefabPlayer, transform.position, Quaternion.identity);
                //현재시간에 -1을 셋팅한다 //타이머를 비활성화한다.
                curTime = -1;
            }
        }
        else //현재타임 0보다 클때 아닐때 //현재타임이 0보다 작을때 //마이너스 //타이머가 비활성화 상태일때
        {
            if (objPlayer == null) //플레이어가 없을때 //플레이어가 사망했을때
            {
                curTime = 0; //타이머에 0의값을 설정한다 //타이머를 활성화 한다.
            }
        }
    }
}
