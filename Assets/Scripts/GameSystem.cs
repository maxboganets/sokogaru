using UnityEngine.InputSystem;
using UnityEngine;

public class Player
{
    private PlayerInput player;
    private int playerIndex;

    public Player(GameObject playerPrefab, int playerIndex, string controlScheme, InputDevice controlDevice)
    {
        this.playerIndex = playerIndex;

        player = PlayerInput.Instantiate(
            playerPrefab,
            -1,
            controlScheme,
            -1,
            controlDevice
        );

        player.GetComponent<Rigidbody2D>().transform.position = GameObject.Find("Player"+ playerIndex +"Spawn").transform.position;
    }

    public int GetIndex()
    {
        return this.playerIndex;
    }

    public PlayerInput GetPlayerInput()
    {
        return this.player;
    }
}

public class GameSystem : MonoBehaviour
{
    public static GameSystem Instance { get; private set; }

    private int healthBarOffsetX = 40;
    private int healthBarOffsetY = 40;

    private void Start()
    {
        // Create Players
        var player1 = new Player(Resources.Load("LichKing") as GameObject, 1, "KeyboardWASD", Keyboard.current);
        var player2 = new Player(Resources.Load("ArcaneArcher") as GameObject, 2, "KeyboardArrows", Keyboard.current);
        // Get GUI canvas rect
        RectTransform canvasTransform = GameObject.Find("GUI").GetComponent<RectTransform>();
        // Create Health Bars and attach them to GUI
        var healthBarPrefab = Resources.Load("HealthBar") as GameObject;
        var healthBarRealWidth = healthBarPrefab.GetComponent<RectTransform>().sizeDelta.x
            * healthBarPrefab.GetComponent<RectTransform>().localScale.x;
        var healthBar1 = Instantiate(
            healthBarPrefab,
            new Vector2(
                healthBarOffsetX + (int)(healthBarRealWidth / 2),
                canvasTransform.rect.height - healthBarOffsetY
            ),
            Quaternion.identity,
            GameObject.Find("GUI").transform
        );
        var healthBar2 = Instantiate(
            healthBarPrefab,
            new Vector2(
                canvasTransform.rect.width - (int)(healthBarRealWidth / 2) - healthBarOffsetX,
                canvasTransform.rect.height - healthBarOffsetY
            ),
            Quaternion.identity,
            GameObject.Find("GUI").transform
        );
        // Assign Health Bars to the players
        player1.GetPlayerInput().GetComponent<PlayerController>().AssignHealthBar(healthBar1.GetComponent<HealthBar>());
        player2.GetPlayerInput().GetComponent<PlayerController>().AssignHealthBar(healthBar2.GetComponent<HealthBar>());
        // Flip Players if needed
        player2.GetPlayerInput().gameObject.GetComponent<SpriteRenderer>().flipX = true;
        Physics2D.IgnoreCollision(player1.GetPlayerInput().GetComponent<Collider2D>(), player2.GetPlayerInput().GetComponent<Collider2D>());
    }

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one instance!");
            return;
        }

        Instance = this;
    }

    void Update()
    {
        // global game update logic goes here
    }

    void OnGui()
    {
        // common GUI code goes here
    }
}
