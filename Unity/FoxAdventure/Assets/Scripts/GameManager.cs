using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Responner responnerPlayer;
    public Responner responnerEagle;
    public CameraTracker tracker;


    public void UpdateCameraTracker()
    {
        if (tracker != null && tracker.objTarget == null)
        {
            tracker.objTarget = responnerPlayer.objPlayer;
        }
    }

    public void UpdateEagle()
    {
        if (responnerEagle.objPlayer)
        {
            Eagle eagle = responnerEagle.objPlayer.GetComponent<Eagle>();
            if (eagle != null)
            {
                eagle.trResponPoint = responnerEagle.transform;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEagle();
        UpdateCameraTracker();
    }
}
