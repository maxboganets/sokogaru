using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] GameObject rangeProjectilePrefab;
    [SerializeField] Color onHitColor = Color.red;
    [SerializeField] int maxHealth = 10;
    [SerializeField] int meleePower = 2;
    [SerializeField] int rangePower = 2;
    [SerializeField] float moveSpeed = 3.5F;
    [SerializeField] float jumpSpeed = 6.5F;
    [SerializeField] int projectileSpeed = 15;
    [SerializeField] int jumpsInAirAllowed = 2;

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
            if (cAction == ControlAction.walkLeft || cAction == ControlAction.walkRight || cAction == ControlAction.walkStop)
            {
                this.doWalk(cAction);
            } else
            {
                if (cAction == ControlAction.jump)
                {
                    this.doJump();
                }
                this.TargetResetAction(ControlAction.none);
            }
        }
    }

    [TargetRpc]
    void TargetResetAction(ControlAction cAction)
    {
        this.SetActionTriggered(cAction);
    }

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
        rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, this.jumpSpeed);
    }

    void doWalk(ControlAction cAction)
    {
        float moveVelocity = 0;
        if (cAction == ControlAction.walkRight)
        {
            moveVelocity = this.moveSpeed;
        } else if (cAction == ControlAction.walkLeft)
        {
            moveVelocity = -this.moveSpeed;
        }
        rigidbody2D.velocity = new Vector2(moveVelocity, rigidbody2D.velocity.y);
    }
}
