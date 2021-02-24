using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;

public static class WaitFor
{
    public static IEnumerator Frames(int frameCount)
    {
        while (frameCount > 0)
        {
            frameCount--;
            yield return null;
        }
    }
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject rangeProjectilePrefab;
    [SerializeField] Color onHitColor = Color.red;
    [SerializeField] int hp = 10;
    [SerializeField] int meleePower = 0;
    [SerializeField] int rangePower = 0;
    [SerializeField] float moveSpeed = 3.5F;
    [SerializeField] float jumpSpeed = 6.5F;
    [SerializeField] int projectileSpeed = 15;
    [SerializeField] int jumpsInAirAllowed = 2;
    [SerializeField] float projectileStartOffsetX = 0.3F;
    [SerializeField] float projectileLifeTime = .5F;
    [SerializeField] float delayBetweenProjectiles = .5F;
    [SerializeField] bool animateFlip = false;

    private GameObject playerObject;
    private Rigidbody2D playerRigidBody2D;
    private Animator playerAnimator;
    private enum PlayerFacing
    {
        right,
        left
    };
    private enum ControlAction
    {
        none,
        jump,
        meeleeAttack,
        rangeAttack,
        ultimateAbility
    }
    private enum PlayerState {
        idle,
        running,
        jumping,
        falling,
        die,
        attackingMeelee,
        attackingFromRange,
        ultimateAbility
    };

    private Vector2 movementInput = Vector2.zero;
    private ControlAction actionTriggered = ControlAction.none;
    private PlayerState playerState = PlayerState.idle;
    private int FlipAnimationStepInFrames = 2;
    private string groundTag = "Floor";
    private string interactiveObjectTag = "InteractiveObject";
    private string projectileTag = "Projectile";
    private bool isGrounded = false;
    private int jumpInAirCurrent = 0;
    private PlayerFacing playerFacing;
    private bool canFireProjectile = true;
    private const float stunTimeAfterHit = 0.3F;
    private float ramainedStunTime = 0;
    private const float invulnerabilityTime = 2F;
    private float ramainedInvulnerableTime = 0;
    private float onHitAnimationTime = 0.2F;
    private float dieAnimationTime = 1F;

    // Callback function for OnMove
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    // Callback function for OnJump
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            this.SetActionTriggered(ControlAction.jump);

        }
    }

    // Callback function for onRangeAttack
    public void onRangeAttack(InputAction.CallbackContext context)
    {
        if (this.HasRangeAttack() && context.ReadValueAsButton())
        {
            this.SetActionTriggered(ControlAction.rangeAttack);

        }
    }

    // Callback function for onUltimateAbility
    public void onUltimateAbility(InputAction.CallbackContext context)
    {
        if (this.HasRangeAttack() && context.ReadValueAsButton())
        {
            this.SetActionTriggered(ControlAction.ultimateAbility);

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Init variables
        playerObject = gameObject;
        playerRigidBody2D = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        // Configure object
        playerRigidBody2D.freezeRotation = true;
        // Set facing direction based on flipX value of the game object
        playerFacing = gameObject.GetComponent<SpriteRenderer>().flipX ? PlayerFacing.left : PlayerFacing.right;
    }

    // Update is called once per frame
    void Update()
    {
        // If Not Stunned
        if (!this.IsStunned())
        {
            this.ExecuteControlActions();
        }
        this.StunState();
        this.VelocityState();
        playerAnimator.SetInteger("state", (int)this.GetPlayerState());
    }

    private void ExecuteControlActions()
    {
        ControlAction cAction = this.GetActionTriggered();
        if (cAction == ControlAction.jump)
        {
            if (isGrounded || jumpInAirCurrent < jumpsInAirAllowed)
            {
                this.SetPlayerState(PlayerState.jumping);
                this.doJump();
                jumpInAirCurrent++;
            }
            this.SetActionTriggered(ControlAction.none);
        }
        if (movementInput.x != 0)
        {
            if (this.GetPlayerState() != PlayerState.jumping && isGrounded)
            {
                this.SetPlayerState(PlayerState.running);
            }
            UpdateFacing(movementInput.x > 0 ? PlayerFacing.right : PlayerFacing.left);
            this.doWalk();
        }
        if (cAction == ControlAction.rangeAttack)
        {
            if (CanFireProjectile())
            {
                StartCoroutine(CreateProjectile());
            }
            this.SetActionTriggered(ControlAction.none);
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
            if (playerRigidBody2D.velocity.y < .1F)
            {
                this.SetPlayerState(PlayerState.falling);
            }
        } else if (state == PlayerState.falling)
        {
            if (isGrounded)
            {
                this.SetPlayerState(PlayerState.idle);
            }
        } else if (Mathf.Abs(playerRigidBody2D.velocity.x) > 1F)
        {
            this.SetPlayerState(PlayerState.running);
        } else
        {
            this.SetPlayerState(PlayerState.idle);
        }
    }

    private void StunState()
    {
        if (ramainedStunTime > 0)
        {
            ramainedStunTime -= Time.deltaTime;
        }
    }

    private bool IsStunned()
    {
        return ramainedStunTime > 0;
    }

    private void SetStunState(float stunTime = stunTimeAfterHit)
    {
        ramainedStunTime = stunTime;
    }

    private void InvulnerabilityState()
    {
        if (ramainedInvulnerableTime > 0)
        {
            ramainedInvulnerableTime -= Time.deltaTime;
        }
    }

    private bool IsInvulnerable()
    {
        return ramainedInvulnerableTime > 0;
    }

    private void SetInvulnerableState(float invulnerableTime = invulnerabilityTime)
    {
        ramainedInvulnerableTime = invulnerableTime;
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

    private int GetHealth()
    {
        return this.hp;
    }

    private bool HasMeleeAttack()
    {
        return this.meleePower > 0;
    }

    private bool HasRangeAttack()
    {
        return this.rangePower > 0;
    }

    private void UpdateHealth(int healthDelta)
    {
        this.hp += healthDelta;
    }

    private bool CanFireProjectile()
    {
        return canFireProjectile;
    }

    private void setCanFireProjectileState(bool newState)
    {
        canFireProjectile = newState;
    }

    private IEnumerator CreateProjectile()
    {
        setCanFireProjectileState(false);
        Vector3 projectileStartPositionOffset = new Vector3(projectileStartOffsetX * (playerFacing == PlayerFacing.left ? -1 : 1), 0, 0);
        Vector3 projectileVelocity = new Vector3(projectileSpeed * (playerFacing == PlayerFacing.left ? -1 : 1), 0, 0);
        GameObject projectileClone = Instantiate(rangeProjectilePrefab, transform.position + projectileStartPositionOffset, transform.rotation);
        projectileClone.GetComponent<SpriteRenderer>().flipX = (playerFacing == PlayerFacing.left);
        // Ignore collisions between hero & projectile
        Physics2D.IgnoreCollision(projectileClone.GetComponent<Collider2D>(), this.playerObject.GetComponent<Collider2D>());
        // Let projectile moving
        projectileClone.GetComponent<Rigidbody2D>().velocity = projectileVelocity;
        // Set Projectile's Attack Power
        projectileClone.GetComponent<WeaponController>().SetAttackPower(this.rangePower);
        // Destroy Projectile after delay
        Destroy(projectileClone, projectileLifeTime);
        // Wait some time and allow fire again
        yield return new WaitForSeconds(delayBetweenProjectiles);
        setCanFireProjectileState(true);
    }

    void UpdateFacing(PlayerFacing newDirection)
    {
        if (playerFacing != newDirection)
        {
            playerFacing = newDirection;
            if (animateFlip)
            {
                StartCoroutine(AnimateChangeFacing());
            } else {
                ChangeFacingImmediate();
            }
        }
    }

    private void ChangePlayerObjectLocalScaleX(float multiplier)
    {
        Vector3 currentScale = playerObject.transform.localScale;
        currentScale.x *= multiplier;
        playerObject.transform.localScale = currentScale;
    }

    private IEnumerator AnimateChangeFacing()
    {
        int stepsScaleChange = 4;
        for (int i = 0; i < stepsScaleChange; i++)
        {
            ChangePlayerObjectLocalScaleX(0.5F);
            yield return StartCoroutine(WaitFor.Frames(FlipAnimationStepInFrames));
        }
        ChangePlayerObjectLocalScaleX(-1);
        yield return StartCoroutine(WaitFor.Frames(FlipAnimationStepInFrames));
        for (int i = 0; i < stepsScaleChange; i++)
        {
            ChangePlayerObjectLocalScaleX(2);
            yield return StartCoroutine(WaitFor.Frames(FlipAnimationStepInFrames));
        }
    }

    private void ChangeFacingImmediate()
    {
        ChangePlayerObjectLocalScaleX(-1);
    }

    private bool CollideWithGround(Collision2D gameObject)
    {
        return (gameObject.gameObject.tag == groundTag || gameObject.gameObject.tag == interactiveObjectTag)
            ? true
            : false;
    }

    private bool GotHit(Collision2D collisionObject)
    {
        // It's not a weapon and can't hit hard
        if (this.IsInvulnerable() || !collisionObject.gameObject.GetComponent<WeaponController>())
        {
            return false;
        }
        return (collisionObject.gameObject.tag == projectileTag)
            ? true
            : false;
    }

    private IEnumerator AnimateOnHit()
    {
        Color originalColor = playerObject.GetComponent<Renderer>().material.color;
        playerObject.GetComponent<Renderer>().material.color = this.onHitColor;
        yield return new WaitForSeconds(onHitAnimationTime);
        playerObject.GetComponent<Renderer>().material.color = originalColor;
    }

    void OnCollisionEnter2D(Collision2D otherObj)
    {
        if (this.GotHit(otherObj))
        {
            // Decrease hp, stun, maybe die
            int attackPower = otherObj.gameObject.GetComponent<WeaponController>().GetAttackPower();
            this.UpdateHealth(-attackPower);
            StartCoroutine(this.AnimateOnHit());
            this.SetStunState();
            this.DoKnockBack(otherObj);
            if (this.GetHealth() <= 0)
            {
                StartCoroutine(this.DoDie());
            }
        }
        if (this.CollideWithGround(otherObj))
        {
            isGrounded = true;
            jumpInAirCurrent = 0;
        }
        if (otherObj.collider.gameObject.tag == interactiveObjectTag)
        {
            otherObj.rigidbody.AddExplosionForce(100, this.transform.position, 5);
        }
    }

    void DoKnockBack(Collision2D otherObj)
    {
        Vector2 moveDirectionPush = playerRigidBody2D.transform.position - otherObj.gameObject.GetComponent<Rigidbody2D>().transform.position;
        playerRigidBody2D.AddForce(moveDirectionPush.normalized * 100);
    }

    void OnCollisionExit2D(Collision2D otherObj)
    {
        if (this.CollideWithGround(otherObj))
        {
            isGrounded = false;
        }
    }

    void doWalk()
    {
        float moveVelocity = playerFacing == PlayerFacing.right ? this.moveSpeed : -this.moveSpeed;
        playerRigidBody2D.velocity = new Vector2(moveVelocity, playerRigidBody2D.velocity.y);
    }

    void doJump()
    {
        playerRigidBody2D.velocity = new Vector2(playerRigidBody2D.velocity.x, this.jumpSpeed);
    }

    private IEnumerator DoDie()
    {
        this.SetStunState(dieAnimationTime);
        this.SetPlayerState(PlayerState.die);
        yield return new WaitForSeconds(dieAnimationTime);
        Destroy(this.playerObject);
    }
}
