using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch_Mechanic : MonoBehaviour
{
    [SerializeField] private Door_Mechanic[] doorOpen;
    [SerializeField] private Door_Mechanic[] doorClosed;


    Animator ani;
    public bool sticks;

    private void Start()
    {
        ani = GetComponent<Animator>();
    }
    private void OnTriggerStay2D()
    {
        ani.SetBool("goDown", true);

        foreach(Door_Mechanic trigger in doorOpen)
        {
            trigger.Toggle(true);
        }

        foreach(Door_Mechanic closed in doorClosed)
        {
            closed.ignoreTrigger = true;
            closed.Toggle(false);
        }
    }
    private void OnTriggerExit2D()
    {
        if (sticks)
            return;
        ani.SetBool("goDown", false);

        foreach (Door_Mechanic trigger in doorOpen)
        {
            trigger.Toggle(false);
        }
        foreach (Door_Mechanic closed in doorClosed)
        {
            closed.ignoreTrigger = false;
            closed.Toggle(true);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (Door_Mechanic trigger in doorOpen)
        {
            Gizmos.DrawLine(transform.position, trigger.transform.position);
        }
        Gizmos.color = Color.yellow;
        foreach (Door_Mechanic closed in doorClosed)
        {
            Gizmos.DrawLine(transform.position, closed.transform.position);
        }
    }
}
