using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

class ControlSheme
{
    private Dictionary<string, UnityEngine.KeyCode> keyBindings =
        new Dictionary<string, UnityEngine.KeyCode>();

    public ControlSheme(int playerControlScheme)
    {
        switch (playerControlScheme)
        {
            case 1:
                keyBindings.Add("up", KeyCode.UpArrow);
                keyBindings.Add("left", KeyCode.LeftArrow);
                keyBindings.Add("right", KeyCode.RightArrow);
                keyBindings.Add("fire", KeyCode.Period);
                break;
            case 2:
                keyBindings.Add("up", KeyCode.W);
                keyBindings.Add("left", KeyCode.A);
                keyBindings.Add("right", KeyCode.D);
                keyBindings.Add("fire", KeyCode.Alpha1);
                break;
        }
    }

    public UnityEngine.KeyCode GetControlKey(string keyName)
    {
        if (keyBindings.ContainsKey(keyName))
        {
            return keyBindings[keyName];
        }
        return KeyCode.None;
    }
}

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
    [SerializeField] float playerRunXOffset = 0.1F;
    [SerializeField] float playerJumpYOffset = 3.0F;
    [SerializeField] int forceMultiplier = 100;
    [SerializeField] int projectileVelocitySpeed = 15;
    [SerializeField] int jumpsInAirAllowed = 2;
    [SerializeField] int playerControlScheme = 1;
    [SerializeField] float delayBetweenProjectiles = .5F;

    private GameObject playerObject;
    private Rigidbody2D playerRigidBody2D;
    private Animator playerAnimator;
    private enum playerState {
        idle,
        running,
        jumping,
        casting
    };

    private string floorTag = "Floor";
    private string interactiveObjectTag = "InteractiveObject";
    private bool standOnFloor = false;
    private int jumpInAirCurrent = 0;
    private ControlSheme controlScheme;
    private string playerFacing = "right";
    private int animationStepFrameCount = 2;
    private bool canFireProjectile = true;
    private int projectileStartOffsetX = 1;
    private float projectileLifeTime = .5F;

    // Start is called before the first frame update
    void Start()
    {
        // Init variables
        playerObject = gameObject;
        playerRigidBody2D = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        // Configure object
        playerRigidBody2D.freezeRotation = true;
        controlScheme = new ControlSheme(playerControlScheme);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(controlScheme.GetControlKey("up")))
        {
            if (standOnFloor || jumpInAirCurrent < jumpsInAirAllowed)
            {
                this.doJump();
                jumpInAirCurrent++;
            }
        }
        if (Input.GetKey(controlScheme.GetControlKey("left")))
        {
            UpdateFacing("left");
            this.changePlayerPosition(-playerRunXOffset, 0);
            playerAnimator.SetBool("running", true);
        }
        else if (Input.GetKey(controlScheme.GetControlKey("right")))
        {
            UpdateFacing("right");
            this.changePlayerPosition(playerRunXOffset, 0);
            playerAnimator.SetBool("running", true);
        }
        else
        {
            playerAnimator.SetBool("running", false);
        }
        if (Input.GetKeyDown(controlScheme.GetControlKey("fire")))
        {
            if (CanFireProjectile())
            {
                StartCoroutine(CreateProjectile());
            }
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
        GameObject projectile = GameObject.Find("Projectile");
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
            StartCoroutine(AnimateChangeFacing());
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
            yield return StartCoroutine(WaitFor.Frames(animationStepFrameCount));
        }
        ChangePlayerObjectLocalScaleX(-1);
        yield return StartCoroutine(WaitFor.Frames(animationStepFrameCount));
        for (int i = 0; i < stepsScaleChange; i++)
        {
            ChangePlayerObjectLocalScaleX(2);
            yield return StartCoroutine(WaitFor.Frames(animationStepFrameCount));
        }
    }

    private bool collideWithFloor(Collision2D gameObject)
    {
        return (gameObject.gameObject.tag == floorTag || gameObject.gameObject.tag == interactiveObjectTag)
            ? true
            : false;
    }

    void OnCollisionEnter2D(Collision2D otherObj)
    {
        if (collideWithFloor(otherObj))
        {
            standOnFloor = true;
            jumpInAirCurrent = 0;
        }
    }

    void OnCollisionExit2D(Collision2D otherObj)
    {
        if (collideWithFloor(otherObj))
        {
            standOnFloor = false;
        }
    }

    void changePlayerPosition(float xOffset, float yOffset)
    {
        Vector2 playerPosition = playerObject.transform.position;
        Vector2 direction = new Vector2(playerPosition.x + xOffset, playerPosition.y + yOffset) - playerPosition;
        playerRigidBody2D.AddForce(direction * forceMultiplier);
    }

    void doJump()
    {
        Vector2 playerPosition = playerObject.transform.position;
        Vector2 direction = new Vector2(playerPosition.x, playerPosition.y + playerJumpYOffset) - playerPosition;
        playerRigidBody2D.AddForce(direction * forceMultiplier);
    }
}
