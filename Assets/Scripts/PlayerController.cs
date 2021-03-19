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

    // Class Constants
    private string groundTag = "Floor";
    private string interactiveObjectTag = "InteractiveObject";
    private string slidePlatformTag = "SlidePlatform";
    private string projectileTag = "Projectile";

    // Class Variables
    [SyncVar] private Vector2 movementInput = Vector2.zero;
    private ControlAction actionTriggered = ControlAction.none;
    private PlayerState playerState = PlayerState.idle;
    private PlayerFacing playerFacing;
    private Color originalTintColor;
    private int jumpInAirCurrent = 0;
    [SyncVar] private bool isGrounded = false;

    private void Start()
    {
        rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        rigidbody2D.freezeRotation = true;
        this.originalTintColor = gameObject.GetComponent<SpriteRenderer>().color;
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
        if (cAction != ControlAction.none)
        {
            this.DoCmdOnServer(cAction);
        }
    }

    void Update()
    {
        if (isServer)
        {
            this.VelocityState();
            this.ClientUpdateState(this.GetPlayerState());
        }
        if (isClient)
        {
            if (hasAuthority)
            {
                this.ExecuteControlActions();
            }
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
            if (cAction == ControlAction.jump)
            {
                if (isGrounded || jumpInAirCurrent < jumpsInAirAllowed)
                {
                    this.SetPlayerState(PlayerState.jumping);
                    this.doJump();
                    jumpInAirCurrent++;
                }
                this.SetActionTriggered(ControlAction.none);
            } else if (cAction == ControlAction.rangeAttack)
            {
                //if (CanFireProjectile())
                //{
                //    StartCoroutine(CreateProjectile());
                //}
                this.SetActionTriggered(ControlAction.none);
            } else if (cAction == ControlAction.walkLeft || cAction == ControlAction.walkRight)
            {
                if (this.GetPlayerState() != PlayerState.jumping && isGrounded)
                {
                    this.SetPlayerState(PlayerState.running);
                }
                this.doWalk(cAction);
                UpdateFacing(cAction == ControlAction.walkRight ? PlayerFacing.right : PlayerFacing.left);
            }
            else if (cAction == ControlAction.walkStop && isGrounded)
            {
                if (this.GetPlayerState() == PlayerState.running)
                {
                    rigidbody2D.velocity = new Vector2(0, rigidbody2D.velocity.y);
                }
            }
        }
    }

    private void VelocityState()
    {
        PlayerState state = this.GetPlayerState();
        if (state == PlayerState.idle || state == PlayerState.die)
        {
            return;
        }

        if (state == PlayerState.jumping)
        {
            if (rigidbody2D.velocity.y < .1F)
            {
                this.SetPlayerState(PlayerState.falling);
            }
        }
        else if (state == PlayerState.falling)
        {
            if (isGrounded)
            {
                this.SetPlayerState(PlayerState.idle);
            }
        }
        else if (Mathf.Abs(rigidbody2D.velocity.x) > 1F)
        {
            this.SetPlayerState(PlayerState.running);
        }
        else
        {
            this.SetPlayerState(PlayerState.idle);
        }
    }

    [ClientRpc]
    void ClientUpdateState(PlayerState playerState)
    {
        gameObject.GetComponent<Animator>().SetInteger("state", (int)playerState);
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

    private PlayerState GetPlayerState()
    {
        return playerState;
    }

    private void SetPlayerState(PlayerState newState)
    {
        playerState = newState;
    }





    private bool CollideWithGround(Collision2D gameObject)
    {
        return (
            gameObject.gameObject.tag == groundTag ||
            gameObject.gameObject.tag == interactiveObjectTag ||
            gameObject.gameObject.tag == slidePlatformTag
        )
            ? true
            : false;
    }

    void OnCollisionEnter2D(Collision2D otherObj)
    {
        //int hitPower = this.GetDamageByCollision(otherObj);
        //if (hitPower > 0)
        //{
        //    this.doHit(hitPower);
        //    this.DoKnockBack(otherObj);
        //}
        if (this.CollideWithGround(otherObj))
        {
            isGrounded = true;
            jumpInAirCurrent = 0;
        }
    }

    void OnCollisionExit2D(Collision2D otherObj)
    {
        if (this.CollideWithGround(otherObj))
        {
            isGrounded = false;
        }
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
