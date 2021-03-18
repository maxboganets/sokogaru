using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    private enum ControlAction
    {
        none,
        jump,
        walkLeft,
        walkRight,
        walkStop,
        meeleeAttack,
        rangeAttack,
        ultimateAbility
    }
    private enum PlayerState
    {
        idle,
        running,
        jumping,
        falling,
        die,
        attackingMeelee,
        attackingFromRange,
        ultimateAbility
    };
    private enum PlayerFacing
    {
        right,
        left
    };

    Rigidbody2D rigidbody2D;

    private Vector2 movementInput = Vector2.zero;
    private ControlAction actionTriggered = ControlAction.none;
    private PlayerState playerState = PlayerState.idle;
    private PlayerFacing playerFacing;

    private void Start()
    {
        rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
    }

    // Callback function for OnMove
    public void OnMove(InputAction.CallbackContext context)
    {
        if (hasAuthority) {
            movementInput = context.ReadValue<Vector2>();
            ControlAction cAction;
            if (movementInput.x > 0)
            {
                cAction = ControlAction.walkRight;
            }
            else if (movementInput.x < 0)
            {
                cAction = ControlAction.walkLeft;
            }
            else
            {
                cAction = ControlAction.walkStop;
            }
            this.SetActionTriggered(cAction);
        }
    }

    // Callback function for OnJump
    public void OnJump(InputAction.CallbackContext context)
    {
        if (hasAuthority && context.ReadValueAsButton())
        {
            Debug.Log($"<color=blue>JUMP PRESSED</color>");
            this.SetActionTriggered(ControlAction.jump);
        }
    }

    // Callback function for onRangeAttack
    public void onRangeAttack(InputAction.CallbackContext context)
    {
        if (hasAuthority && context.ReadValueAsButton())
        {
            this.SetActionTriggered(ControlAction.rangeAttack);
        }
    }

    // Callback function for onUltimateAbility
    public void onUltimateAbility(InputAction.CallbackContext context)
    {
        if (hasAuthority && context.ReadValueAsButton())
        {
            this.SetActionTriggered(ControlAction.ultimateAbility);

        }
    }

    private void ExecuteControlActions()
    {
        ControlAction cAction = this.GetActionTriggered();
        //if (cAction == ControlAction.jump)
        //{
        //    //if (isGrounded || jumpInAirCurrent < jumpsInAirAllowed)
        //    //{
        //    //    this.SetPlayerState(PlayerState.jumping);
        //    this.doJump();
        //    //    jumpInAirCurrent++;
        //    //}
        //    this.SetActionTriggered(ControlAction.none);
        //}
        //if (movementInput.x != 0)
        //{
        //    //if (this.GetPlayerState() != PlayerState.jumping && isGrounded)
        //    //{
        //    //    this.SetPlayerState(PlayerState.running);
        //    //}
        //    UpdateFacing(movementInput.x > 0 ? PlayerFacing.right : PlayerFacing.left);
        //    //this.doWalk();
        //    this.DoCmdOnServer(ControlAction.jump);
        //}
        //else
        //{
        //    if (this.GetPlayerState() == PlayerState.running)
        //    {
        //        playerRigidBody2D.velocity = new Vector2(0, playerRigidBody2D.velocity.y);
        //    }
        //}
        //if (cAction == ControlAction.rangeAttack)
        //{
        //    if (CanFireProjectile())
        //    {
        //        StartCoroutine(CreateProjectile());
        //    }
        //    this.SetActionTriggered(ControlAction.none);
        //}
        if (cAction != ControlAction.none)
        {
            this.DoCmdOnServer(cAction);
        }
        //this.SetActionTriggered(ControlAction.none);
    }

    [Client]
    void Update()
    {
        if (hasAuthority)
        {
            this.ExecuteControlActions();
        }
    }

    public override void OnStartServer()
    {
        Debug.Log("Player Spawned on the server!");
    }

    [Command]
    private void DoCmdOnServer(ControlAction cAction)
    {
        if (cAction != ControlAction.none) {
            //this.RpcCdmOnClient(cAction);
            if (cAction == ControlAction.jump)
            {
                this.doJump();
            }
            if (cAction == ControlAction.walkLeft || cAction == ControlAction.walkRight || cAction == ControlAction.walkStop)
            {
                this.doWalk(cAction);
            }
        }
    }

    //[ClientRpc]
    //private void RpcCdmOnClient(ControlAction cAction)
    //{
    //    if (cAction == ControlAction.jump)
    //    {
    //        this.doJump();
    //    }
    //    if (cAction == ControlAction.walkLeft || cAction == ControlAction.walkRight || cAction == ControlAction.walkStop)
    //    {
    //        this.doWalk(cAction);
    //    }
    //}

    void UpdateFacing(PlayerFacing newDirection)
    {
        if (this.playerFacing != newDirection)
        {
            this.playerFacing = newDirection;
            this.ChangeFacingImmediate();
        }
    }

    private void ChangePlayerObjectLocalScaleX(float multiplier)
    {
        Vector3 currentScale = transform.localScale;
        currentScale.x *= multiplier;
        transform.localScale = currentScale;
    }

    private void ChangeFacingImmediate()
    {
        ChangePlayerObjectLocalScaleX(-1);
    }

    private ControlAction GetActionTriggered()
    {
        return actionTriggered;
    }

    private void SetActionTriggered(ControlAction newAction)
    {
        actionTriggered = newAction;
    }

    void doJump()
    {
        //transform.position = transform.position + new Vector3(0, 0.5f, 0);
        rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 6);
    }

    void doWalk(ControlAction cAction)
    {
        //transform.position = transform.position + new Vector3(0.1f * (this.playerFacing == PlayerFacing.right ? 1 : -1), 0, 0);
        float moveVelocity = 0;
        if (cAction == ControlAction.walkRight)
        {
            moveVelocity = 5;
        } else if (cAction == ControlAction.walkLeft)
        {
            moveVelocity = -5;
        }
        rigidbody2D.velocity = new Vector2(moveVelocity, rigidbody2D.velocity.y);
    }
}
