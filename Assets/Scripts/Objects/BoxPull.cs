using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxPull : MonoBehaviour
{
    public bool beingPushed;
    float xPos;

    private void Start()
    {
        xPos = transform.position.x;
    }
    private void Update()
    {
        if(beingPushed == false)
        {
            transform.position = new Vector3(xPos,transform.position.y);
        }
        else
        {
            xPos = transform.position.x;
        }
    }
}
