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
                keyBindings.Add("fire", KeyCode.Greater);
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

public class LichKing : MonoBehaviour
{
    [SerializeField] float playerRunXOffset = 0.05F;
    [SerializeField] float playerJumpYOffset = 3.0F;
    [SerializeField] int forceMultiplier = 100;
    [SerializeField] int jumpsInAirAllowed = 2;
    [SerializeField] int playerControlScheme = 1;

    private GameObject playerObject;
    private Rigidbody2D playerRigidBody2D;

    private string floorTag = "Floor";
    private string interactiveObjectTag = "InteractiveObject";
    private bool standOnFloor = false;
    private int jumpInAirCurrent = 0;
    private ControlSheme controlScheme;
    private string playerFacing = "right";
    private int animationStepFrameCount = 2;
    private bool projectileFired = false;

    // Start is called before the first frame update
    void Start()
    {
        // Init variables
        playerObject = gameObject;
        playerRigidBody2D = GetComponent<Rigidbody2D>();
        // Configure object
        playerRigidBody2D.freezeRotation = true;
        controlScheme = new ControlSheme(playerControlScheme);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(controlScheme.GetControlKey("up")))
        {
            Scene scene = SceneManager.GetActiveScene();
            Debug.Log("Active Scene is '" + scene.name + "'.");
            if (standOnFloor || jumpInAirCurrent < jumpsInAirAllowed)
            {
                this.doJump(playerObject);
                jumpInAirCurrent++;
            }
        }
        if (Input.GetKey(controlScheme.GetControlKey("left")))
        {
            UpdateFacing("left");
            this.changePlayerPosition(playerObject, -playerRunXOffset, 0);
        }
        if (Input.GetKey(controlScheme.GetControlKey("right")))
        {
            UpdateFacing("right");
            this.changePlayerPosition(playerObject, playerRunXOffset, 0);
        }
        if (Input.GetKey(controlScheme.GetControlKey("fire")))
        {
            if (!projectileFired)
            {
                // Create Projectile
                createProjectile();
            }
        }
    }

    void createProjectile()
    {
        GameObject projectile = GameObject.Find("Projectile");
        GameObject projectileClone = Instantiate(projectile, transform.position, transform.rotation);
        projectileClone.transform.position += Vector3.forward * 100 * Time.deltaTime;
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

    void changePlayerPosition(GameObject playerObj, float xOffset, float yOffset)
    {
        Vector2 playerPosition = playerObj.transform.position;
        Vector2 direction = new Vector2(playerPosition.x + xOffset, playerPosition.y + yOffset) - playerPosition;
        playerObj.GetComponent<Rigidbody2D>().AddForce(direction * forceMultiplier);
    }

    void doJump(GameObject playerObj)
    {
        Vector2 playerPosition = playerObj.transform.position;
        Vector2 direction = new Vector2(playerPosition.x, playerPosition.y + playerJumpYOffset) - playerPosition;
        playerRigidBody2D.AddForce(direction * forceMultiplier);
    }
}
