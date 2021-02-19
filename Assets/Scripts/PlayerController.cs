using System.Collections;
using System.Collections.Generic;
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

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] bool animateFlip = false;
    [SerializeField] float playerRunForce = 10;
    [SerializeField] float playerJumpForce = 200;
    [SerializeField] int projectileVelocitySpeed = 15;
    [SerializeField] int jumpsInAirAllowed = 2;
    [SerializeField] int FlipAnimationStepInFrames = 2;
    [SerializeField] float projectileStartOffsetX = 1;
    [SerializeField] float projectileLifeTime = .5F;
    [SerializeField] float delayBetweenProjectiles = .5F;

    private GameObject playerObject;
    private Rigidbody2D playerRigidBody2D;
    private Animator playerAnimator;
    private CharacterController playerController;
    private enum PlayerState {
        idle,
        running,
        jumping,
        casting
    };

    private Vector2 movementInput = Vector2.zero;
    private bool jumpTriggered = false;
    private bool fireProjectileTriggered = false;
    private PlayerState playerState = PlayerState.idle;
    private string groundTag = "Floor";
    private string interactiveObjectTag = "InteractiveObject";
    private bool isGrounded = false;
    private int jumpInAirCurrent = 0;
    private string playerFacing = "right";
    private bool canFireProjectile = true;

    // Callback function for OnMove
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    // Callback function for OnJump
    public void OnJump(InputAction.CallbackContext context)
    {
        jumpTriggered = context.ReadValueAsButton();
    }

    // Callback function for OnFireProjectile
    public void OnFireProjectile(InputAction.CallbackContext context)
    {
        fireProjectileTriggered = context.ReadValueAsButton();
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
        playerFacing = gameObject.GetComponent<SpriteRenderer>().flipX ? "left" : "right";
        // save <CharacterController> as private variable
        playerController = playerObject.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (jumpTriggered)
        {
            if (isGrounded || jumpInAirCurrent < jumpsInAirAllowed)
            {
                this.doJump();
                jumpInAirCurrent++;
            }
            jumpTriggered = false;
        }
        if (movementInput.x < 0)
        {
            UpdateFacing("left");
            this.doWalk();
            playerAnimator.SetBool("running", true);
        }
        else if (movementInput.x > 0)
        {
            UpdateFacing("right");
            this.doWalk();
            playerAnimator.SetBool("running", true);
        }
        else
        {
            playerAnimator.SetBool("running", false);
        }
        if (fireProjectileTriggered)
        {
            if (CanFireProjectile())
            {
                StartCoroutine(CreateProjectile());
            }
            fireProjectileTriggered = false;
        }
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
        //GameObject projectile = GameObject.Find("Projectile");
        GameObject projectile = Instantiate(Resources.Load("Projectile")) as GameObject;
        Vector3 projectileStartPositionOffset = new Vector3(projectileStartOffsetX * (playerFacing == "left" ? -1 : 1), 0, 0);
        Vector3 projectileVelocity = new Vector3(projectileVelocitySpeed * (playerFacing == "left" ? -1 : 1), 0, 0);
        GameObject projectileClone = Instantiate(projectile, transform.position + projectileStartPositionOffset, transform.rotation);
        // Let projectile moving
        projectileClone.GetComponent<Rigidbody2D>().velocity = projectileVelocity;
        // Destroy Projectile after delay
        Destroy(projectileClone, projectileLifeTime);
        // Wait some time and allow fire again
        yield return new WaitForSeconds(delayBetweenProjectiles);
        setCanFireProjectileState(true);
    }

    void UpdateFacing(string newDirection)
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

    private bool collideWithGround(Collision2D gameObject)
    {
        return (gameObject.gameObject.tag == groundTag || gameObject.gameObject.tag == interactiveObjectTag)
            ? true
            : false;
    }

    void OnCollisionEnter2D(Collision2D otherObj)
    {
        if (collideWithGround(otherObj))
        {
            isGrounded = true;
            jumpInAirCurrent = 0;
        }
        if (otherObj.collider.gameObject.tag == interactiveObjectTag)
        {
            otherObj.rigidbody.AddExplosionForce(100, this.transform.position, 5);
        }
    }

    void OnCollisionExit2D(Collision2D otherObj)
    {
        if (collideWithGround(otherObj))
        {
            isGrounded = false;
        }
    }

    void doWalk()
    {
        float forceWithDirection = playerFacing == "right" ? playerRunForce : -playerRunForce;
        Vector2 forceVector = new Vector2(forceWithDirection, 0);
        playerRigidBody2D.AddForce(forceVector);
    }

    void doJump()
    {
        Vector2 forceVector = new Vector2(0, playerJumpForce);
        playerRigidBody2D.AddForce(forceVector);
    }
}
