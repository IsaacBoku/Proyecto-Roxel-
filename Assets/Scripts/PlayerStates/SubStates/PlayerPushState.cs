using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerPushState : PlayerGroundedState
{
    public bool isPush = true;
    public PlayerPushState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();

        
    }

    public override void Enter()
    {
        base.Enter();


    }

    public override void Exit()
    {
        base.Exit();
        
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

    }
    public void DesConexion()
    {
        player.box.GetComponent<FixedJoint2D>().enabled = false;
        isPush = true;
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();


    }
    public bool CanPush()
    {
            Physics2D.queriesStartInColliders = false;
            RaycastHit2D hit = Physics2D.Raycast(player.pushCheck.transform.position, Vector2.right * player.FacingDirection, playerData.distance, playerData.boxMask);

            if (hit.collider != null && hit.collider.gameObject.tag == "pushable"&&isPush)
            {
                player.box = hit.collider.gameObject;

                player.box.GetComponent<FixedJoint2D>().enabled = true;
                player.box.GetComponent<BoxPull>().beingPushed = true;
                player.box.GetComponent<FixedJoint2D>().connectedBody = player.GetComponent<Rigidbody2D>();
                isPush = false;
                Debug.Log("Agarro");
                player.SetVelocityX(playerData.movementVeclocity * xInput);

                 return true;
            }
            else 
            {   
                player.box.GetComponent<FixedJoint2D>().enabled = false;
                player.box.GetComponent<BoxPull>().beingPushed = false;

                isPush = true;
                return false;
                
            }
    }

}
