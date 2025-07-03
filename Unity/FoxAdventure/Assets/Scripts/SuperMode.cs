using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperMode : MonoBehaviour
{
    public float time = 1;
    public bool isUse = false;
    public SpriteRenderer spriteRenderer;

    public void Use()
    {
        StartCoroutine(UseTimmer());
    }

    IEnumerator UseTimmer()
    {
        isUse = true;
        yield return new WaitForSeconds(time);
        spriteRenderer.color = Color.white;
        isUse = false;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (isUse) Use();
    }

    private void Update()
    {
        if (isUse)
        {
            if (spriteRenderer.color == Color.white)
                spriteRenderer.color = Color.clear;
            else if (spriteRenderer.color == Color.clear)
                spriteRenderer.color = Color.white;
        }
    }
}
