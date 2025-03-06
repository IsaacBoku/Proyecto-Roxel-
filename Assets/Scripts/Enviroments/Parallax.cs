using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Parallax : MonoBehaviour
{

    public Transform cam;
    public float relativeMove = .3f;
    private Vector3 previousCameraPosition;
    private float spriteWidth, startPosition;


    private void Start()
    {
        cam = Camera.main.transform;
        previousCameraPosition = cam.position;
        spriteWidth = GetComponent<TilemapRenderer>().bounds.size.x;
        startPosition = transform.position.x;
    }
    private void LateUpdate()
    {
        float deltaX = (cam.position.x - previousCameraPosition.x) * relativeMove;
        float moveAmount = cam.position.x * (1 - relativeMove);
        transform.Translate(new Vector3(deltaX, 0, 0));
        previousCameraPosition = cam.position;
        
        if(moveAmount > startPosition + spriteWidth)
        {
            transform.Translate(new Vector3(spriteWidth, 0, 0));
            startPosition += spriteWidth;
        }
        else if(moveAmount < startPosition - spriteWidth)
        {
            transform.Translate(new Vector3(-spriteWidth, 0, 0));
        }
    }
}
