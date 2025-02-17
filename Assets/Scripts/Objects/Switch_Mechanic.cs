using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch_Mechanic : MonoBehaviour
{
    public Door_Mechanic[] door;

    Animator ani;
    public bool sticks;

    private void Start()
    {
        ani = GetComponent<Animator>();
    }
    private void OnTriggerStay2D()
    {
        ani.SetBool("goDown", true);

        foreach(Door_Mechanic trigger in door)
        {
            trigger.Toggle(true);
        }
    }
    private void OnTriggerExit2D()
    {
        if (sticks)
            return;
        ani.SetBool("goDown", false);

        foreach (Door_Mechanic trigger in door)
        {
            trigger.Toggle(false);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (Door_Mechanic trigger in door)
        {
            Gizmos.DrawLine(transform.position, trigger.transform.position);
        }
    }
}
