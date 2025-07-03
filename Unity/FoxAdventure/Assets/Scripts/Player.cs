using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int atk;
    public int hp;
    public int exp;
    public int maxExp = 100;
    public int lv = 1;

    public void LvUp()
    {
        if(exp >= maxExp)
        {
            lv++;
            atk += 5;
            hp += 10;
            exp -= maxExp;
        }
    }

    public void StillExp(Player player)
    {
        this.exp += player.exp;
    }

    public void Attack(Player target)
    {
        target.hp = target.hp - this.atk; // 100 - 10 = 90
    }
    
    public bool Death()
    {
        if (hp > 0)
            return false;
        else
            return true;
    }

    private void Update()
    {
        LvUp();
    }

    public int idx = 0;
    private void OnGUI()
    {
        GUI.Box(new Rect(100*idx, 0, 100, 20), $"{gameObject.name}[{lv}]");
        GUI.Box(new Rect(100 * idx, 20, 100, 20), $"Exp:{exp}/{maxExp}");
        GUI.Box(new Rect(100 * idx, 40, 100, 20), $"Atk:{atk}");
        GUI.Box(new Rect(100 * idx, 60, 100, 20), $"HP:{hp}");
    }
}
